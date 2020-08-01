using UnityEngine;

namespace ITCompanySimulation.Character
{
    /// <summary>
    /// Represents character in simulation world
    /// </summary>
    public abstract class Character
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        public string Name { get; protected set; }
        public string Surename { get; protected set; }
        /// <summary>
        /// Representation of character in game world
        /// </summary>
        public GameObject PhysicalCharacter { get; set; }

        /*Private methods*/

        /*Public methods*/
    } 
}
