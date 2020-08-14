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

    private void Start()
    {
        Transform = GetComponent<RectTransform>();
        ParentTransform = Transform.parent.gameObject.GetComponent<RectTransform>();
        SetTooltipSize();
    }

    private void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector2 tooltipPostion = new Vector2(mousePos.x + 30f, mousePos.y);
        Vector2 finalPostion;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(ParentTransform, tooltipPostion, null, out finalPostion))
        {
            Transform.localPosition = finalPostion;
            
        }
        else
        {
            Debug.Log("FAIL");
        }
    }

    /*Public methods*/
}
