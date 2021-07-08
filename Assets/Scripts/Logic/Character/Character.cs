using ITCompanySimulation.Core;

namespace ITCompanySimulation.Character
{
    /// <summary>
    /// Represents character in simulation world
    /// </summary>
    public abstract class Character : IdentifiableObject
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        public string Name { get; private set; }
        public string Surename { get; private set; }
        public Gender Gender { get; private set; }

        /*Private methods*/

        /*Public methods*/

        public Character(string charName, string charSurename, Gender charGender)
        {
            this.Name = charName;
            this.Surename = charSurename;
            this.Gender = charGender;
        }
    }
}
