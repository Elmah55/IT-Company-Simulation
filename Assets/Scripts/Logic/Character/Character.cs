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

        public string Name { get; private set; }
        public string Surename { get; private set; }
        public Gender Gender { get; private set; }

        /*Private methods*/

        /*Public methods*/

        public Character(string name, string surename, Gender gender)
        {
            this.Name = name;
            this.Surename = surename;
            this.Gender = Gender;
        }
    }
}
