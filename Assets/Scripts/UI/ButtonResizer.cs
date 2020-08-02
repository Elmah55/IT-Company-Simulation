using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
[RequireComponent(typeof(RectTransform))]
public class ButtonResizer : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    private EventTrigger ButtonEventTrigger;
    private RectTransform ButtonTransform;
    private Vector2 NormalSize;
    private bool CheckPointerPostion;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    private void OnPointerEnter(BaseEventData data)
    {
        NormalSize = ButtonTransform.sizeDelta;
        float newWidth = NormalSize.x + (0.2f * NormalSize.x);
        float newHeigth = NormalSize.y + (0.2f * NormalSize.y);
        Vector2 newSize = new Vector2(newWidth, newHeigth);
        ButtonTransform.sizeDelta = newSize;
    }

    private void OnPointerExit(BaseEventData data)
    {
        ButtonTransform.sizeDelta = NormalSize;
    }

    private void Update()
    {
        
    }

    /*Public methods*/

    // Start is called before the first frame update
    void Start()
    {
        ButtonEventTrigger = GetComponent<EventTrigger>();
        ButtonTransform = GetComponent<RectTransform>();

        EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener(OnPointerEnter);
        ButtonEventTrigger.triggers.Add(pointerEnter);

        EventTrigger.Entry pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener(OnPointerExit);
        ButtonEventTrigger.triggers.Add(pointerExit);

    }
}
