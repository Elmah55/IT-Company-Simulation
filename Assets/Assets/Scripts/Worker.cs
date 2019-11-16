using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worker
{
    /*Private consts fields*/

    /*Private fields*/

    /*Public consts fields*/

    public const int BASE_SALARY = 800;
    /// <summary>
    /// The maximum value of one ability that worker
    /// can have
    /// </summary>
    public const float MAX_ABILITY_VALUE = 10.0f;

    /*Public fields*/

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
            float score = 1.0f;

            foreach (ProjectTechnology workerAbility in Abilites.Keys)
            {
                foreach (ProjectTechnology technologyInProject in AssignedProject.UsedTechnologies)
                {
                    if (workerAbility == technologyInProject)
                    {
                        score += Abilites[workerAbility] * 0.1f;
                    }
                }
            }

            score += ExperienceTime * 0.3f;

            return score;
        }
    }
    /// <summary>
    /// List of abilites that worker has. Each ability can be on different level
    /// depending on worker experience. Ability level is indicated by value assigned
    /// to each of abilities key
    /// </summary>
    public Dictionary<ProjectTechnology, float> Abilites { get; set; }
    public Project AssignedProject { get; set; }
    /// <summary>
    /// Shows how much of time of experience working in project worker has.
    /// This values is days in game time
    /// </summary>
    public int ExperienceTime { get; set; }
    public int Salary { get; set; }
    /// <summary>
    /// Salary that needs to be offered to worker to hire him
    /// from other player's company
    /// </summary>
    public int HireSalary
    {
        get
        {
            return (int)(Salary * 1.25f);
        }
    }

    /*Private methods*/

    /*Public methods*/

    public Worker(string name, string surename)
    {
        this.Name = name;
        this.Surename = surename;
        this.Salary = BASE_SALARY;
    }
}
