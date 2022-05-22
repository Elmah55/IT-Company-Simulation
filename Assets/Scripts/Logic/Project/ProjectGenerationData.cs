using UnityEngine;

namespace ITCompanySimulation.Project
{
    /// <summary>
    /// Contains data needed for creating new project.
    /// </summary>
    [CreateAssetMenu(fileName = "ProjectGenerationData", menuName = "ITCompanySimulation/Project/Generation Data")]
    public class ProjectGenerationData : ScriptableObject
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        public string[] Names;
        [Tooltip("Index in this arrays should match index in project names array")]
        public Sprite[] Icons;

        /*Private methods*/

        /*Public methods*/
    } 
}
