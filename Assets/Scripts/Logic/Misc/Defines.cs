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
    ClientReceivedData
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
    OnePlayerInRoom,
    ForcedByMasterClient,
    Disconnected
}

public enum SimulationEventNotificationPriority
{
    Normal,
    High
}

namespace ITCompanySimulation.Character
{
    /// <summary>
    /// This class defines possible movement of character. Animation names
    /// should match names in this enum.
    /// </summary>
    public enum CharacterMovement
    {
        WalkS,
        WalkW,
        WalkE,
        WalkN,
        StandS,
        StandW,
        StandE,
        StandN
    }

    public enum WorkerAttribute
    {
        Salary,
        ExpierienceTime,
        ProjectTechnology
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

    public enum Gender
    {
        Male,
        Female
    }
}

/*Classes*/

/// <summary>
/// This class maps enums to strings
/// </summary>
public class EnumToString
{
    public static IReadOnlyDictionary<ProjectTechnology, string> ProjectTechnologiesStrings = new Dictionary<ProjectTechnology, string>()
    {
        { ProjectTechnology.C,"C" },
        { ProjectTechnology.Cpp,"C++" },
        { ProjectTechnology.CSharp,"C#" },
        { ProjectTechnology.JavaScript,"JavaScript" },
        { ProjectTechnology.PHP,"PHP" },
        { ProjectTechnology.Python,"Python" },
        { ProjectTechnology.Java,"Java" }
    };

    public static IReadOnlyDictionary<FullScreenMode, string> FullScreenModeStrings = new Dictionary<FullScreenMode, string>()
    {
        { FullScreenMode.ExclusiveFullScreen,"Exclusive fullscreen" },
        { FullScreenMode.FullScreenWindow,"Fullscreen window" },
        { FullScreenMode.MaximizedWindow,"Maximized window" },
        { FullScreenMode.Windowed,"Windowed" }
    };
}

/// <summary>
/// Class that holds data that is used when creating new worker
/// </summary>
public static class WorkerData
{
    public static IReadOnlyList<string> MaleNames = new List<string>()
    {
        "John",
        "Adam",
        "David",
        "Kenny",
        "Michael",
        "Peter",
        "Roger",
        "Charles",
        "Harper",
        "Jack",
        "Carter",
        "Easton",
        "Robert"
    };

    public static IReadOnlyList<string> FemaleNames = new List<string>()
    {
        "Kathie",
        "Kate",
        "Anna",
        "Alice",
        "Evelyn",
        "Ella",
        "Scarlett",
        "Madison",
        "Lily",
        "Lucy"
    };

    public static IReadOnlyList<string> Surenames = new List<string>()
    {
        "Smith",
        "Crew",
        "Kendrick",
        "Sanny",
        "Sanchez",
        "Brown",
        "Hughes",
        "Warren",
        "Cempy",
        "Kowalik"
    };
}

/// <summary>
/// Class that holds data that is used when creating new project
/// </summary>
public static class ProjectData
{
    public static IReadOnlyList<string> Names = new List<string>()
    {
        "Automotive embedded",
        "Telecommunications embedded",
        "Website",
        "Desktop application"
    };
}

namespace ITCompanySimulation.Multiplayer
{
    public static class NetworkingData
    {
        /// <summary>
        /// Bytes codes used for sending custom type data between clients
        /// </summary>
        public const byte SHARED_WORKER_BYTE_CODE = 0;
        public const byte LOCAL_WORKER_BYTE_CODE = 1;
        public const byte PROJECT_BYTE_CODE = 2;
        public const byte SIMULATION_SETTINGS_BYTE_CODE = 3;
        public const byte PLAYER_DATA_BYTE_CODE = 4;
        public const byte SIMULATION_STATS_BYTE_CODE = 5;
    }

    /// <summary>
    /// This enum defines keys to access photon player custom properties
    /// </summary>
    public enum PlayerCustomPropertiesKey
    {
        RoomLobbyPlayerState
    }

    /// <summary>
    /// This enum defines keys to access room custom properties
    /// </summary>
    public enum RoomCustomPropertiesKey
    {
        SettingsOfSimulationMinimalBalance,
        SettingsOfSimulationInitialBalance,
        SettingsOfSimulationTargetBalance
    }
}

/*Delegates*/

namespace ITCompanySimulation.Project
{
    public delegate void SharedProjectAction(SharedProject proj);
    public delegate void LocalProjectAction(LocalProject proj);
    public delegate void ScrumAtion(Scrum scrumObj);
}

namespace ITCompanySimulation.UI
{
    public delegate void ParentChangeAction(GameObject obj, GameObject newParent);
}

namespace ITCompanySimulation.Character
{
    public delegate void LocalWorkerAction(LocalWorker worker);
    public delegate void SharedWorkerAction(SharedWorker worker);
    public delegate void WorkerAbilityAction(SharedWorker worker, ProjectTechnology workerAbility, float workerAbilityValue);
    public delegate void MultiplayerWorkerAction(SharedWorker worker, PhotonPlayer player);
}

namespace ITCompanySimulation.Multiplayer
{
    /// <summary>
    /// Delagate for event fired when photon chat message from other client is received
    /// </summary>
    /// <param name="senderNickname">Nickname of player that sent message</param>
    /// <param name="message">String containing message</param>
    public delegate void PhotonChatMessageAction(string senderNickname, string message);
    public delegate void PhotonPlayerAction(PhotonPlayer player);
}

namespace ITCompanySimulation.Core
{
    public delegate void SimulationFinishAction(int winnerPhotonPlayerID, SimulationFinishReason finishReason);
}

namespace ITCompanySimulation.Company
{
    /// <summary>
    /// Delegate used when company balance is changed
    /// </summary>
    /// <param name="newBalance">New comapny's balance</param>
    /// <param name="balanceDelta">Difference between current and previous balance</param>
    public delegate void BalanceChangeAction(int newBalance, int balanceDelta);
}

