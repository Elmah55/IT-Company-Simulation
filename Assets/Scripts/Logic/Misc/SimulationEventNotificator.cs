using ITCompanySimulation.Core;
using System;
using UnityEngine.Events;

/// <summary>
/// This component allows to send notification messages that can be displayed to player
/// </summary>
public class SimulationEventNotificator
{
    /*Private consts fields*/

    /*Private fields*/

    private GameTime GameTimeComponent;

    /*Public consts fields*/

    /*Public fields*/

    public event UnityAction<SimulationEventNotification> NotificationReceived;

    /*Private methods*/

    /*Public methods*/

    public SimulationEventNotificator(GameTime gameTimeComponent)
    {
        this.GameTimeComponent = gameTimeComponent;
    }

    public void Notify(string txt, SimulationEventNotificationPriority prio = SimulationEventNotificationPriority.Normal)
    {
        DateTime timestamp = GameTimeComponent.CurrentTime;
        SimulationEventNotification newNotification = new SimulationEventNotification(txt, prio, timestamp);
        NotificationReceived?.Invoke(newNotification);
    }
}
