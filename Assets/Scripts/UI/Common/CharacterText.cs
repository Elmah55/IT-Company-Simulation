using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ITCompanySimulation.Utilities;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// This script allows to display text above character. Script should be
    /// placed on object with transform that will define text's position
    /// </summary>
    public class CharacterText : MonoBehaviour
    {
        /*Private consts fields*/

        /// <summary>
        /// If distance of mouse pointer from character is greater than this value
        /// then text will not be displayed (color alpha will be 0)
        /// </summary>
        private const float TEXT_DISPLAY_DISTANCE = 1.2f;
        /// <summary>
        /// If distance of mouse pointer from character is less than this value
        /// then text color's alpha will be 1
        /// </summary>
        private const float TEXT_COLOR_FULL_ALPHA_DISTANCE = 0.5f;

        /*Private fields*/

        [SerializeField]
        private GameObject TextObjectPrefab;
        private GameObject TextObject;
        private Image BackgroundImage;
        private TextMeshProUGUI TextComponent;

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
                SetSize();
            }
        }

        /*Private methods*/

        private void SetSize()
        {
            RectTransform textObjectTransform = TextObject.GetComponent<RectTransform>();
            Vector2 newParentSize = new Vector2(TextComponent.preferredWidth, textObjectTransform.sizeDelta.y);
            textObjectTransform.sizeDelta = newParentSize;
            textObjectTransform.localScale = Vector3.one;
        }

        private void OnDisable()
        {
            if (null != TextObject)
            {
                TextObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (null != TextObject)
            {
                TextObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            GameObject.Destroy(TextObject);
        }

        private void Update()
        {
            Vector2 textPostion = Camera.main.WorldToScreenPoint(transform.position);
            TextObject.transform.position = textPostion;

            //Change text alpha based on mouse pointer distance from character
            float mousePtrToCharacterDist = Vector2.Distance(
                Camera.main.ScreenToWorldPoint(Input.mousePosition),
                transform.parent.position);
            mousePtrToCharacterDist = Mathf.Clamp(mousePtrToCharacterDist, 0f, TEXT_DISPLAY_DISTANCE);
            //-x + 1 linear function
            float colorAlpha =
                -Utils.MapRange(mousePtrToCharacterDist, TEXT_COLOR_FULL_ALPHA_DISTANCE, TEXT_DISPLAY_DISTANCE, 0f, 1f) + 1;

            if (colorAlpha > 0f)
            {
                //Change background image alpha
                Color newColor = BackgroundImage.color;
                newColor.a = colorAlpha;
                BackgroundImage.color = newColor;
                //Now change alpha of text component
                newColor = TextComponent.color;
                newColor.a = colorAlpha;
                TextComponent.color = newColor;
                TextObject.SetActive(true);
            }
            else
            {
                TextObject.SetActive(false);
            }
        }

        private void Awake()
        {
            TextObject = GameObject.Instantiate(TextObjectPrefab);
            TextObject.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform);
            TextObject.transform.SetAsFirstSibling();
            TextComponent = TextObject.GetComponentInChildren<TextMeshProUGUI>();
            BackgroundImage = TextObject.GetComponent<Image>();
            SetSize();
        }

        /*Public methods*/
    }
}
