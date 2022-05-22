using UnityEngine;

namespace ITCompanySimulation.Character
{
    /// <summary>
    /// Contains data needed for creating new worker.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterGenerationData", menuName = "ITCompanySimulation/Character/Generation Data")]
    public class WorkerGenerationData : ScriptableObject
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        public string[] MaleNames;
        public string[] FemaleNames;
        public string[] Surenames;
        public Sprite[] MaleCharactersAvatars;
        public Sprite[] FemaleCharactersAvatars;

        /*Private methods*/

        /*Public methods*/
    } 
}
