using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worker:IUpdatable
{
    /*Private consts fields*/

    /*Private fields*/

    /*Public consts fields*/

    /*Public fields*/

    public int ID { get; private set; }
    public string Name { get; private set; }
    public string Surename { get; private set; }
    /// <summary>
    /// Company that this worker is working in
    /// </summary>
    public PlayerCompany WorkingCompany { get; set; }
    /// <summary>
    /// Combination of all player's attributes. It's player's overall score
    /// and indicates how effective worker can work
    /// </summary>
    public float Score
    {
        get
        {
            //TODO: Calculate score based on player abilities
            return 1.0f;
        }
    }

    /*Private methods*/

    /*Public methods*/

    public Worker(int id, string name, string surename)
    {
        this.ID = id;
        this.Name = name;
        this.Surename = surename;
    }

    public void UpdateState()
    {
        throw new System.NotImplementedException();
    }
}
