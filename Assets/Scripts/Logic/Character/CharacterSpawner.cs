using System.Collections.Generic;
using UnityEngine;
using ITCompanySimulation.Utilities;

namespace ITCompanySimulation.Character
{
    /// <summary>
    /// This class handles spawing character in game world
    /// </summary>
    public class CharacterSpawner : MonoBehaviour
    {
        /*Private consts fields*/

        /// <summary>
        /// How many retries there will be to spawn character before
        /// spawning fails
        /// </summary>
        private const int MAX_SPAWN_RETRIES = 15;
        private const float SPAWN_POS_MINIMUM_COLLIDER_DISTANCE = 0.3f;

        /*Private fields*/

        private Rect SpawnBounds = new Rect()
        {
            xMin = -4f,
            xMax = 1f,
            yMax = 2.8f,
            yMin = 1.8f
        };
        [SerializeField]
        private GameObject CharacterPrefab;
        private MainSimulationManager SimulationManagerComponent;
        private List<SharedWorker> SpawnedCharacters = new List<SharedWorker>();
        private IObjectPool<GameObject> CharactersPool;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void OnControlledCompanyWorkerRemoved(SharedWorker companyWorker)
        {
            RemoveCharacter(companyWorker);
        }

        private void OnControlledCompanyWorkerAdded(SharedWorker companyWorker)
        {
            SpawnCharacter(companyWorker);
        }

        private void RemoveCharacter(SharedWorker companyWorker)
        {
            SharedWorker removedChar = SpawnedCharacters.Find(x => x == companyWorker);

            if (null == CharactersPool)
            {
                CharactersPool = new ObjectPool<GameObject>();
            }

            removedChar.PhysicalCharacter.gameObject.SetActive(false);
            CharactersPool.AddObject(companyWorker.PhysicalCharacter);
            removedChar.PhysicalCharacter = null;
        }

        private void SpawnCharacter(SharedWorker companyWorker)
        {
            //Spawn new character in game world
            int spawnRetries = 0;
            bool spawnPosCorrect = false;
            Vector2 spawnPos = Vector2.zero;

            while (false == spawnPosCorrect)
            {
                float spawnPosX = Random.Range(SpawnBounds.xMin, SpawnBounds.xMax);
                float spawnPosY = Random.Range(SpawnBounds.yMin, SpawnBounds.yMax);
                spawnPos = new Vector2(spawnPosX, spawnPosY);

                //Check if there's no obstacle at spawn position
                Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPos, SPAWN_POS_MINIMUM_COLLIDER_DISTANCE);
                spawnPosCorrect = (colliders.Length == 0);
                ++spawnRetries;

                if (spawnRetries > MAX_SPAWN_RETRIES)
                {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    string debugInfo =
                        string.Format("[{0}] Failed to spawn character in game world after {1} retries",
                                      this.GetType().Name,
                                      MAX_SPAWN_RETRIES);
                    Debug.Log(debugInfo);
#endif
                    break;
                }
            }

            if (true == spawnPosCorrect)
            {
                GameObject newCharacter = null;

                if (null != CharactersPool)
                {
                    newCharacter = CharactersPool.GetObject();
                }

                if (null == newCharacter)
                {
                    newCharacter = GameObject.Instantiate(CharacterPrefab, spawnPos, Quaternion.identity);
                }

                LocalWorker worker = (LocalWorker)companyWorker;
                worker.AbsenceStarted += OnWorkerAbsenceStarted;
                worker.AbsenceFinished += OnWorkerAbsenceFinished;
                companyWorker.PhysicalCharacter = newCharacter;
                SpawnedCharacters.Add(companyWorker);
            }
        }

        private void OnWorkerAbsenceStarted(SharedWorker worker)
        {
            worker.PhysicalCharacter.gameObject.SetActive(false);
        }

        private void OnWorkerAbsenceFinished(SharedWorker worker)
        {
            worker.PhysicalCharacter.gameObject.SetActive(true);
        }

        private void Start()
        {
            this.SimulationManagerComponent = GetComponent<MainSimulationManager>();
            SimulationManagerComponent.ControlledCompany.WorkerAdded += OnControlledCompanyWorkerAdded;
            SimulationManagerComponent.ControlledCompany.WorkerRemoved += OnControlledCompanyWorkerRemoved;
        }

        /*Public methods*/
    }
}
