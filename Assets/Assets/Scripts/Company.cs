using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Company
{
    public string Name { get; set; }
    public List<Worker> Workers { get; set; }
    /// <summary>
    /// Money balance of company
    /// </summary>
    public float Balance { get; set; }

    private const int MAX_WORKERS_PER_COMPANY = 10;
}
