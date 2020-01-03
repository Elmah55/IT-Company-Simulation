using System.Collections.Generic;

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
public class WorkerData
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

/*Delegates*/
public delegate void ProjectAction(Project proj);
public delegate void LoanAction(BankLoan load);
public delegate void WorkerAction(Worker companyWorker);
