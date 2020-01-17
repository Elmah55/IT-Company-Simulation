﻿using System;
using UnityEngine;

/// <summary>
/// This is core class for all aspects of gameplay that will
/// happen during running simulation (like ending game if gameplay
/// target is reached)
/// </summary>
public class MainSimulationManager : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    private SimulationSettings SimulationSettingsComponent;

    /*Public consts fields*/

    /*Public fields*/

    public PlayerCompany ControlledCompany { get; private set; }

    /*Private methods*/

    private void CreateCompany()
    {
        //Testing scrum only for now
        ControlledCompany = new PlayerCompany("TEST COMPANY", gameObject);
        Worker workerA = new Worker("Jan", "Kowalski");
        Worker workerB = new Worker("Adam", "Nowak");
        workerA.WorkingCompany = ControlledCompany;
        workerB.WorkingCompany = ControlledCompany;
        workerA.Abilites = new System.Collections.Generic.Dictionary<ProjectTechnology, float>();
        workerB.Abilites = new System.Collections.Generic.Dictionary<ProjectTechnology, float>();
        ControlledCompany.Workers.Add(workerA);
        ControlledCompany.Workers.Add(workerB);

        Project testProject = new Project("TEST");
        testProject.UsedTechnologies.Add(ProjectTechnology.C);
        testProject.UsedTechnologies.Add(ProjectTechnology.Cpp);
        testProject.DaysSinceStart = 10;
        Scrum testScrum = gameObject.AddComponent(typeof(Scrum)) as Scrum;
        ControlledCompany.ScrumProcesses.Add(testScrum);
        testScrum.BindedProject = testProject;

        Project testProject2 = new Project("TEST 2222");
        testProject2.UsedTechnologies.Add(ProjectTechnology.Java);
        testProject2.UsedTechnologies.Add(ProjectTechnology.Python);
        testProject2.DaysSinceStart = 134;
        Scrum testScrum2 = gameObject.AddComponent(typeof(Scrum)) as Scrum;
        ControlledCompany.ScrumProcesses.Add(testScrum2);
        testScrum2.BindedProject = testProject2;

        ControlledCompany.BalanceChanged += OnControlledCompanyBalanceChanged;
        ControlledCompany.Balance = 1000000000;
    }

    private void OnControlledCompanyBalanceChanged(int newBalance)
    {
        if (newBalance >= SimulationSettingsComponent.TargetBalance)
        {
            FinishGame();
        }
    }

    private void FinishGame()
    {
        //TODO Add implementation of this methed
        //(sending info of finished game to other players,
        //updating GUI, etc.)

        //Stop time so events in game are no longer updated
        Time.timeScale = 0.0f;
        Debug.Log("Game finished !");
    }

    /*Public methods*/

    public void Start()
    {
        //Obtain refence to game manager object wich was created in
        //menu scene
        GameObject gameManagerObject = GameObject.Find("GameManager");
        SimulationSettingsComponent = gameManagerObject.GetComponent<SimulationSettings>();

        //TEST
        CreateCompany();
    }
}
