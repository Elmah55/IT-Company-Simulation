using System;
using UnityEngine.Events;

public class SimulationEventNotification
{
    /*Private consts fields*/

    /*Private fields*/

    /*Public consts fields*/

    /*Public fields*/

    public string Text { get; private set; }
    public SimulationEventNotificationPriority Priority { get; private set; }
    public DateTime Timestamp { get; private set; }

    /*Private methods*/

    /*Public methods*/

    public SimulationEventNotification(string txt, SimulationEventNotificationPriority prio, DateTime timestamp)
    {
        this.Text = txt;
        this.Priority = prio;
        this.Timestamp = timestamp;
    }
}
