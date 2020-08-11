﻿using ITCompanySimulation.Character;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This file contains definitons of enum and types used
/// in other classes as well as utlities classes for these
/// definitions
/// </summary>

/*Enums*/

/// <summary>
/// This enum defines technologies that can be used in a single
/// project 
/// </summary>
public enum ProjectTechnology
{
    C,
    Cpp,
    CSharp,
    JavaScript,
    PHP,
    Python,
    Java
}

/// <summary>
/// This enum defines index of scene (as defined in
/// build settings)
/// </summary>
public enum SceneIndex
{
    Menu = 0,
    Game = 1
}

/// <summary>
/// Codes of events used for PhotonNetwork.RaiseEvent
/// </summary>
public enum RaiseEventCode : byte
{
    RoomLobbyPlayerStateChanged,
    ClientReadyToReceiveData
}

/// <summary>
/// Possible states for player in
/// room lobby
/// </summary>
public enum RoomLobbyPlayerState
{
    Ready,
    NotReady
}

public enum SprintStage
{
    Planning,
    Developing,
    Retrospective
}

public enum SimulationFinishReason
{
    PlayerCompanyReachedTargetBalance,
    PlayerCompanyReachedMinimalBalance,
    OnePlayerInRoom
}

public enum SimulationEventNotificationPriority
{
    Normal,
    High
}

namespace ITCompanySimulation.Character
{
    public enum CharacterMovement
    {
        RunS,
        RunW,
        RunE,
        RunN,
        StandS,
        StandW,
        StandE,
        StandN
    }

    public enum WorkerAttribute
    {
        Salary
    }

    /// <summary>
    /// Reason of worker not being able to
    /// work
    /// </summary>
    public enum WorkerAbsenceReason
    {
        Sickness,
        Holiday
    }
}

/*Classes*/

/// <summary>
/// This class maps enums to strings
/// </summary>
public class EnumToString
{
    public static Dictionary<ProjectTechnology, string> ProjectTechnologiesStrings = new Dictionary<ProjectTechnology, string>()
    {
        { ProjectTechnology.C,"C" },
        { ProjectTechnology.Cpp,"C++" },
        { ProjectTechnology.CSharp,"C#" },
        { ProjectTechnology.JavaScript,"JavaScript" },
        { ProjectTechnology.PHP,"PHP" },
        { ProjectTechnology.Python,"Python" },
        { ProjectTechnology.Java,"Java" }
    };
}

/// <summary>
/// Class that holds data that is used when creating new worker
/// </summary>
public static class WorkerData
{
    public static List<string> Names = new List<string>()
    {
        "John",
        "Adam",
        "David",
        "Kenny",
        "Michael",
        "Kathie",
        "Anna",
        "Alice"
    };

    public static List<string> Surenames = new List<string>()
    {
        "Smith",
        "Crew",
        "Kendrick",
        "Sanny"
    };
}

/// <summary>
/// Class that holds data that is used when creating new project
/// </summary>
public static class ProjectData
{
    public static List<string> Names = new List<string>()
    {
        "Automotive embedded",
        "Telecommunications embedded",
        "Website",
        "Desktop application"
    };
}

public static class NetworkingData
{
    /// <summary>
    /// Bytes codes used for sending custom type data between clients
    /// </summary>
    public const byte SHARED_WORKER_BYTE_CODE = 0;
    public const byte LOCAL_WORKER_BYTE_CODE = 1;
    public const byte PROJECT_BYTE_CODE = 2;
}

/*Delegates*/

namespace ITCompanySimulation.Developing
{
    public delegate void SharedProjectAction(SharedProject proj);
    public delegate void LocalProjectAction(LocalProject proj);
    public delegate void ScrumAtion(Scrum scrumObj);
}

namespace ITCompanySimulation.UI
{
    public delegate void ParentChangeAction(GameObject obj, GameObject newParent);
}

public delegate void LoanAction(BankLoan load);
public delegate void WorkerAction(SharedWorker companyWorker);
public delegate void WorkerAbilityAction(SharedWorker companyWorker, ProjectTechnology workerAbility, float workerAbilityValue);
public delegate void MultiplayerWorkerAction(SharedWorker playerWorker, PhotonPlayer player);
public delegate void PhotonPlayerAction(PhotonPlayer player);