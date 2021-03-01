using System;
using System.Collections;
using UnityEngine;
using ITCompanySimulation.Utilities;
using UnityEngine.Events;
using ITCompanySimulation.Core;

/// <summary>
/// This class handles updating time in game world. Time
/// measured here is based on const values and might not be
/// equal to scaled time
/// </summary>
public class GameTime : Photon.PunBehaviour, IDataReceiver
{
    /*Private consts fields*/

    /// <summary>
    /// How often game time should be updated. After time
    /// specified by this value passes next day in game world
    /// occurs. This value is seconds in game time (scaled time)
    /// </summary>
    private const float TIME_UPDATE_FREQUENCY = 6f;

    /*Private fields*/

    private MainSimulationManager SimulationManagerComponent;

    /*Public consts fields*/

    /*Public fields*/

    public DateTime CurrentTime { get; private set; }
    /// <summary>
    /// How many days (game time) have passed since start of game
    /// </summary>
    public int DaysSinceStart { get; private set; }
    /// <summary>
    /// Sets the time scale that simulation should be run with. 1.0 is default scale
    /// </summary>
    [Range(0.1f, 10.0f)]
    public float Scale;
    /// <summary>
    /// Returns number of seconds in one day (scaled time)
    /// </summary>
    public static float SecondsInDay
    {
        get
        {
            return TIME_UPDATE_FREQUENCY;
        }
    }
    public bool IsDataReceived { get; private set; }
    public bool IsTimeStarted { get; private set; }

    public event Action DayChanged;
    public event Action MonthChanged;
    public event Action YearChanged;
    public event UnityAction DataReceived;

    /*Private methods*/

    private IEnumerator UpdateGameTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(TIME_UPDATE_FREQUENCY);

            //Next days occurs when time specified by
            //TIME_UPDATE_FREQUENCY passes
            DateTime newTime = CurrentTime.AddDays(1);

            ++DaysSinceStart;
            DayChanged?.Invoke();

            if (newTime.Month != CurrentTime.Month)
            {
                MonthChanged?.Invoke();
            }

            if (newTime.Year != CurrentTime.Year)
            {
                YearChanged?.Invoke();
            }

            CurrentTime = newTime;
        }
    }

    [PunRPC]
    private void SetTimeRPC(int day, int month, int year)
    {
        CurrentTime = new DateTime(year, month, day);
        //Inform all subscribers right after receiving new date from master client
        if (false == IsDataReceived)
        {
            IsDataReceived = true;
            DataReceived?.Invoke();
        }
        DayChanged?.Invoke();
    }

    private void Awake()
    {
        DaysSinceStart = 1;
        Scale = 1f;

        //Only master client shoud start timer with default time.
        //In other cases client will receive current time from
        //master client at joining game so time in game of all
        //clients is synchronized
        if (true == PhotonNetwork.isMasterClient)
        {
            CurrentTime = DateTime.Now;
            this.photonView.RPC("SetTimeRPC", PhotonTargets.Others,
                CurrentTime.Day, CurrentTime.Month, CurrentTime.Year);
        }
    }

    private void Update()
    {
        if (true == IsTimeStarted)
        {
            Time.timeScale = Scale;
        }
    }

    /*Public methods*/

    public void StartTime()
    {
        if (false == IsTimeStarted)
        {
            StartCoroutine(UpdateGameTime());
            IsTimeStarted = true;
        }
    }

    public void StopTime()
    {
        if (true == IsTimeStarted)
        {
            StopAllCoroutines();
            IsTimeStarted = false;
            Time.timeScale = 0f;
        }
    }
}
