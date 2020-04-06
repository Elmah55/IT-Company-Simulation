using System;
using System.Collections;
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
    /// How often game time should be updated. After time
    /// specified by this value passes next day in game world
    /// occurs. This value is seconds in game time (scaled time)
    /// </summary>
    private const float TIME_UPDATE_FREQUENCY = 6.0f;

    /*Private fields*/

    private PhotonView PhotonViewComponent;

    /*Public consts fields*/

    /*Public fields*/

    public DateTime CurrentTime { get; private set; }
    /// <summary>
    /// How many days (game time) have passed since start of game
    /// </summary>
    public int DaysSinceStart { get; private set; }
    public event Action DayChanged;
    public event Action MonthChanged;
    public event Action YearChanged;

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

    private void StartTime()
    {
        CurrentTime = DateTime.Now;
        StartCoroutine(UpdateGameTime());
    }

    [PunRPC]
    private void StartTime(int day, int month, int year)
    {
        CurrentTime = new DateTime(year, month, day);
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
            PhotonViewComponent.RPC("StartTime", PhotonTargets.Others,
                CurrentTime.Day, CurrentTime.Month, CurrentTime.Year);
        }
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        base.OnPhotonPlayerConnected(newPlayer);

        //Synchronize connected client's time with master client's time
        if (true == PhotonNetwork.isMasterClient)
        {
            PhotonViewComponent.RPC("StartTime", newPlayer,
                CurrentTime.Day, CurrentTime.Month, CurrentTime.Year);
        }
    }
}
