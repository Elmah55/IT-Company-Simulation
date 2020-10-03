using TMPro;
using UnityEngine;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// This class allows to display UI tooltip
    /// </summary>
    public class Tooltip : MonoBehaviour
    {
        /*Private consts fields*/

        [Tooltip("Tooltip offset from mouse position on X axis")]
        public float TooltipOffsetX;
        [Tooltip("Tooltip offset from mouse position on Y axis")]
        public float TooltipOffsetY;

        /*Private fields*/

        [SerializeField]
        private TextMeshProUGUI TextComponent;
        /// <summary>
        /// This gameobject's transform
        /// </summary>
        private RectTransform ObjectTransform;
        private RectTransform CanvasTransform;
        private Canvas CanvasComponent;

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
            Vector2 newSize = new Vector2(TextComponent.preferredWidth, TextComponent.preferredHeight);
            ObjectTransform.sizeDelta = newSize;

            //Size has to be set twice. For some reason when settings size only once its not perfectly fitted.
            //Possible text mesh pro issue
            newSize = new Vector2(TextComponent.preferredWidth, TextComponent.preferredHeight);
            ObjectTransform.sizeDelta = newSize;
        }

        private void SetTooltipPosition()
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 tooltipPostion =
                new Vector2(mousePos.x + (TooltipOffsetX * CanvasComponent.scaleFactor),
                mousePos.y - (TooltipOffsetY * CanvasComponent.scaleFactor));
            ObjectTransform.position = tooltipPostion;

            //Tooltip reached right edge of screen, move tooltip to left side of mouse pointer
            if (ObjectTransform.anchoredPosition.x + ObjectTransform.rect.width > CanvasTransform.rect.width)
            {
                tooltipPostion =
                    new Vector2(mousePos.x - (ObjectTransform.rect.width + TooltipOffsetX) * CanvasComponent.scaleFactor,
                    tooltipPostion.y);
                ObjectTransform.position = tooltipPostion;
            }

            //Tooltip reached left edge of screen, move tooltip to right side of mouse pointer
            if (ObjectTransform.anchoredPosition.x < 0f)
            {
                tooltipPostion =
                    new Vector2(mousePos.x + ((Mathf.Abs(TooltipOffsetX) - ObjectTransform.rect.width) * CanvasComponent.scaleFactor),
                    tooltipPostion.y);
                ObjectTransform.position = tooltipPostion;
            }

            //Tooltip reached botom edge of screen, move tooltip above mouse pointer
            if (Mathf.Abs(ObjectTransform.anchoredPosition.y - ObjectTransform.rect.width) > CanvasTransform.rect.height)
            {
                tooltipPostion = new Vector2(tooltipPostion.x,
                    mousePos.y + (TooltipOffsetY + ObjectTransform.rect.height) * CanvasComponent.scaleFactor);
                ObjectTransform.position = tooltipPostion;
            }

            //Tooltip reached upper edge of screen, move tooltip below mouse pointer
            if (ObjectTransform.anchoredPosition.y > 0f)
            {
                tooltipPostion = new Vector2(tooltipPostion.x,
                    mousePos.y + ((TooltipOffsetY + ObjectTransform.rect.height) * CanvasComponent.scaleFactor));
                ObjectTransform.position = tooltipPostion;
            }
        }

        private void Awake()
        {
            ObjectTransform = GetComponent<RectTransform>();
            CanvasTransform = GameObject.FindGameObjectWithTag("Canvas").GetComponent<RectTransform>();
            CanvasComponent = CanvasTransform.GetComponent<Canvas>();
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
}
