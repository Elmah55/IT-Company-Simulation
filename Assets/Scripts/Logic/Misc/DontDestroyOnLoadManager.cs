using UnityEngine;

namespace ITCompanySimulation.Utilities
{
    /// <summary>
    /// This script prevents duplicate creation of game objects with "DontDestroyOnLoad" enabled.
    /// All scripts that should not be destroyed on load should be instatiated thorugh this script.
    /// </summary>
    public class DontDestroyOnLoadManager : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        [Tooltip("Object that will be not destroyed on scene load")]
        private GameObject[] ObjectsPrefabs;
        private static bool ObjectsInstantiated = false;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Awake()
        {
            if (null != ObjectsPrefabs && false == ObjectsInstantiated)
            {
                foreach (GameObject obj in ObjectsPrefabs)
                {
                    GameObject.Instantiate(obj);
                }

                ObjectsInstantiated = true;
            }
        }

        /*Public methods*/
    }
}
