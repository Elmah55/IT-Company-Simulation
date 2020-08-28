using TMPro;
using UnityEngine;

/// <summary>
/// This class allows to display UI tooltip
/// </summary>
public class Tooltip : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    [SerializeField]
    private TextMeshProUGUI TextComponent;
    private RectTransform Transform;
    private RectTransform ParentTransform;

    /*Public consts fields*/

    /*Public fields*/

    public string Text
    {
        get
        {
            return TextComponent.text;
        }

        set
        {
            TextComponent.text = value;
            SetTooltipSize();
        }
    }

    /*Private methods*/

    private void SetTooltipSize()
    {
        if (null != Transform)
        {
            Vector2 newSize = new Vector2(TextComponent.preferredWidth, TextComponent.preferredHeight);
            Transform.sizeDelta = newSize;
        }
    }

    private void SetTooltipPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector2 tooltipPostion = new Vector2(mousePos.x + 30f, mousePos.y);
        Vector2 finalPostion;
        bool result = RectTransformUtility.ScreenPointToLocalPointInRectangle(ParentTransform, tooltipPostion, null, out finalPostion);

        if (true == result)
        {
            Transform.localPosition = finalPostion;

        }
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        else
        {
            string debugMsg = string.Format("[{0}] RectTransformUtility.ScreenPointToLocalPointInRectangle could not find point",
                this.GetType().Name);
            Debug.LogWarning(debugMsg);
        }
#endif
    }

    private void Start()
    {
        Transform = GetComponent<RectTransform>();
        ParentTransform = Transform.parent.gameObject.GetComponent<RectTransform>();
        SetTooltipSize();
    }

    private void OnEnable()
    {
        SetTooltipPosition();
    }

    private void Update()
    {
        SetTooltipPosition();
    }

    /*Public methods*/
}
