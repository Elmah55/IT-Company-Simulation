using UnityEngine;
using ITCompanySimulation.UI;
using ITCompanySimulation.Core;

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

        [Tooltip("Male characters prefabs that will be spawn onto game world")]
        [SerializeField]
        private GameObject[] MaleCharactersPrefabs;
        [Tooltip("Female characters prefabs that will be spawn onto game world")]
        [SerializeField]
        private GameObject[] FemaleCharactersPrefabs;
        private SimulationManager SimulationManagerComponent;
        [SerializeField]
        private Transform[] SpawnPoints;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void OnControlledCompanyWorkerRemoved(LocalWorker companyWorker)
        {
            RemoveCharacter(companyWorker);
        }

        private void OnControlledCompanyWorkerAdded(LocalWorker companyWorker)
        {
            SpawnCharacter(companyWorker);
        }

        private void RemoveCharacter(LocalWorker companyWorker)
        {
            companyWorker.AbsenceStarted -= OnWorkerAbsenceStarted;
            companyWorker.AbsenceFinished -= OnWorkerAbsenceFinished;
            GameObject.Destroy(companyWorker.PhysicalCharacter);
            companyWorker.PhysicalCharacter = null;
        }

        private void SpawnCharacter(LocalWorker companyWorker)
        {
            int randomIndex = Random.Range(0, SpawnPoints.Length);
            Transform randomTransform = SpawnPoints[randomIndex];
            Vector2 spawnPos = randomTransform.position;
            GameObject[] characterPrefabs = null;

            switch (companyWorker.Gender)
            {
                case Gender.Male:
                    characterPrefabs = MaleCharactersPrefabs;
                    break;
                case Gender.Female:
                    characterPrefabs = FemaleCharactersPrefabs;
                    break;
                default:
                    break;
            }

            randomIndex = Random.Range(0, characterPrefabs.Length);
            GameObject newCharacter = GameObject.Instantiate(characterPrefabs[randomIndex], spawnPos, Quaternion.identity, transform);

            CharacterText textComponent = newCharacter.GetComponentInChildren<CharacterText>();
            textComponent.Text = string.Format("{0} {1}", companyWorker.Name, companyWorker.Surename);

            companyWorker.AbsenceStarted += OnWorkerAbsenceStarted;
            companyWorker.AbsenceFinished += OnWorkerAbsenceFinished;
            companyWorker.PhysicalCharacter = newCharacter;
        }

        private void OnWorkerAbsenceStarted(LocalWorker worker)
        {
            worker.PhysicalCharacter.gameObject.SetActive(false);
        }

        private void OnWorkerAbsenceFinished(LocalWorker worker)
        {
            worker.PhysicalCharacter.gameObject.SetActive(true);
        }

        private void Awake()
        {
            this.SimulationManagerComponent = GetComponent<SimulationManager>();
        }

        private void Start()
        {
            SimulationManagerComponent.ControlledCompany.WorkerAdded += OnControlledCompanyWorkerAdded;
            SimulationManagerComponent.ControlledCompany.WorkerRemoved += OnControlledCompanyWorkerRemoved;
        }

        /*Public methods*/
    }
}
