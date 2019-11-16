using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles updating time in game world. Time
/// measured here is based on const values and might not be
/// equal to scaled time
/// </summary>
public class GameTime : Photon.PunBehaviour
{
    /*Private consts fields*/

    /// <summary>
    /// How often game time should be updated. This value
    /// is seconds in game game time (scaled time)
    /// </summary>
    private const float TIME_UPDATE_FREQUENCY = 1.0f;

    /// <summary>
    /// How many minutes should pass in game time in one
    /// time update
    /// </summary>
    private const float TIME_MINUTES_PER_UPDATE = 30.0f;

    /*Private fields*/

    private PhotonView PhotonViewComponent;
    /// <summary>
    /// Used to count passed days
    /// </summary>
    private int PreviousDayNumber;

    /*Public consts fields*/

    /*Public fields*/

    public DateTime CurrentTime { get; private set; }
    /// <summary>
    /// How many days (game time) have passed since start of game
    /// </summary>
    public int DaysSinceStart { get; private set; }
    public Action DayChanged;

    /*Private methods*/

    private IEnumerator UpdateGameTime()
    {
        while (true)
        {
            CurrentTime = CurrentTime.AddMinutes(TIME_MINUTES_PER_UPDATE);

            if (CurrentTime.Day != PreviousDayNumber)
            {
                ++DaysSinceStart;
                DayChanged?.Invoke();
                PreviousDayNumber = CurrentTime.Day;
            }

            Debug.LogFormat("{0}:{1} {2}", new object[] { CurrentTime.Hour, CurrentTime.Minute, DaysSinceStart });

            yield return new WaitForSeconds(TIME_UPDATE_FREQUENCY);
        }
    }

    private void StartTime()
    {
        CurrentTime = DateTime.Now;
        StartTimeInternal();
    }

    [PunRPC]
    private void StartTime(DateTime timeAtStart)
    {
        CurrentTime = timeAtStart;
        StartTimeInternal();
    }

    private void StartTimeInternal()
    {
        PreviousDayNumber = CurrentTime.Day;
        StartCoroutine(UpdateGameTime());
    }

    /*Public methods*/

    // Start is called before the first frame update
    public void Start()
    {
        DaysSinceStart = 1;
        PhotonViewComponent = GetComponent<PhotonView>();

        //Only master client shoud start timer with default time.
        //In other cases client will receive current time from
        //master client at joining game so time in game of all
        //clients is synchronized
        if (true == PhotonNetwork.isMasterClient)
        {
            StartTime();
        }
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        base.OnPhotonPlayerConnected(newPlayer);

        //Synchronize connected client's time with master client's time
        if (true == PhotonNetwork.isMasterClient)
        {
            PhotonViewComponent.RPC("StartTime", newPlayer, CurrentTime);
        }
    }
}
