using UnityEngine;

namespace ITCompanySimulation.Utilities
{
    /// <summary>
    /// This script prevents duplicate creation of game objects with "DontDestroyOnLoad" enabled.
    /// </summary>
    public class DontDestroyOnLoadManager : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        [Tooltip("Object that will be not destroyed on scene load")]
        private GameObject[] ObjectsPrefabs;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Awake()
        {
            if (null != ObjectsPrefabs)
            {
                foreach (GameObject obj in ObjectsPrefabs)
                {
                    GameObject foundObject = GameObject.FindGameObjectWithTag(obj.tag);

                    if (null == foundObject)
                    {
                        GameObject.Instantiate(obj);
                    }
                }
            }
        }

        /*Public methods*/
    } 
}
