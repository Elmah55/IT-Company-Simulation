using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ITCompanySimulation.Utilities;
using ITCompanySimulation.Character;
using Random = UnityEngine.Random;
using System.Collections;
using ITCompanySimulation.Project;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// This script allows to display text above character. Script should be
    /// placed on object with transform that will define text's position
    /// </summary>
    public class WorkerText : MonoBehaviour
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
        private GameObject TextWorkerNamePrefab;
        [SerializeField]
        private TextMeshProUGUI TextWorkerAbilityPrefab;
        private GameObject TextObject;
        private Image TextWorkerNameBackgroundImage;
        /// <summary>
        /// Text with name displayed above character.
        /// </summary>
        private TextMeshProUGUI TextWorkerName;
        /// <summary>
        /// Text displayed when worker ability is updated.
        /// </summary>
        private TextMeshProUGUI TextWorkerAbility;
        private Camera MainCamera;
        [SerializeField]
        private GameObject AbilityTextPlaceholder;
        /// <summary>
        /// Used for storing local position of placeholder for
        /// ability text. Needed for setting postion when
        /// seting again character object as parent of placeholder object.
        /// </summary>
        private Vector2 AbilityTextPlaceholderLocalPostion;
        private bool AbilityTextAnimationActive;

        /*Public consts fields*/

        /*Public fields*/

        public string Text
        {
            get
            {
                return TextWorkerName.text;
            }

            set
            {
                TextWorkerName.text = value;
            }
        }
        /// <summary>
        /// Worker that this instance of CharacterText component belongs to.
        /// </summary>
        public LocalWorker Worker { get; set; }
        [Tooltip("How much ability text should move up when playing animation")]
        public float AbilityTxtMoveAnimationYAmount;
        public float AbilityTxtMoveAnimationSpeed;
        public Canvas CanvasComponent;
        public Transform ScriptsObjectTransform;

        /*Private methods*/

        private void OnDisable()
        {
            if (null != TextObject)
            {
                TextObject.SetActive(false);
            }

            if (null != TextWorkerAbility)
            {
                TextWorkerAbility.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            GameObject.Destroy(TextObject);
            GameObject.Destroy(TextWorkerAbility);
        }

        private void Update()
        {
            //Change text alpha based on mouse pointer distance from character
            float mousePtrToCharacterDist = Vector2.Distance(
                MainCamera.ScreenToWorldPoint(Input.mousePosition),
                transform.parent.position);
            mousePtrToCharacterDist = Mathf.Clamp(mousePtrToCharacterDist, 0f, TEXT_DISPLAY_DISTANCE);
            //-x + 1 linear function
            float colorAlpha =
                -Utils.MapRange(mousePtrToCharacterDist, TEXT_COLOR_FULL_ALPHA_DISTANCE, TEXT_DISPLAY_DISTANCE, 0f, 1f) + 1;
            //Activate text object only when mouse is close enough to worker's character
            bool textObjectActive = colorAlpha > 0f;

            if (true == textObjectActive)
            {
                //Change background image alpha
                Color newColor = TextWorkerNameBackgroundImage.color;
                newColor.a = colorAlpha;
                TextWorkerNameBackgroundImage.color = newColor;
                //Now change alpha of text component
                newColor = TextWorkerName.color;
                newColor.a = colorAlpha;
                TextWorkerName.color = newColor;
            }

            TextObject.SetActive(textObjectActive);
        }

        private void LateUpdate()
        {
            /* Calculate onscreen position in late update to move UI controls to they target
               position before they are visible. It is needed because on screen controls position 
               is calculated only when they are active. Using late update control can be actived 
               in coroutine and moved to proper onscreen position during one frame.
            */
            Vector2 textScreenPositon;

            if (true == TextObject.activeInHierarchy)
            {
                textScreenPositon = MainCamera.WorldToScreenPoint(transform.position);
                TextObject.transform.position = textScreenPositon;
            }

            if (true == TextWorkerAbility.gameObject.activeInHierarchy)
            {
                textScreenPositon = MainCamera.WorldToScreenPoint(AbilityTextPlaceholder.transform.position);
                TextWorkerAbility.gameObject.transform.position = textScreenPositon;
            }
        }

        private void Awake()
        {
            Transform canvasTransform = CanvasComponent.transform;
            TextObject = GameObject.Instantiate(TextWorkerNamePrefab, canvasTransform);
            TextWorkerAbility = GameObject.Instantiate(TextWorkerAbilityPrefab, canvasTransform);

            TextObject.transform.SetParent(canvasTransform);
            TextObject.transform.SetAsFirstSibling();
            TextWorkerAbility.transform.SetParent(canvasTransform);
            TextWorkerAbility.transform.SetAsFirstSibling();
            TextWorkerAbility.gameObject.SetActive(false);
            TextWorkerName = TextObject.GetComponentInChildren<TextMeshProUGUI>();
            TextWorkerNameBackgroundImage = TextObject.GetComponent<Image>();
            MainCamera = Camera.main;
            AbilityTextPlaceholderLocalPostion = AbilityTextPlaceholder.transform.localPosition;
        }

        private void Start()
        {
            Worker.AbilityUpdated += OnWorkerAbilityUpdated;
        }

        private IEnumerator AbilityTextAnimation()
        {
            AbilityTextAnimationActive = true;
            Color abilityTextColor = TextWorkerAbility.color;
            abilityTextColor.a = 1f;
            TextWorkerAbility.color = abilityTextColor;
            float animationDelay = Random.Range(0f, 10f);
            float animationStartTime = Time.time + animationDelay;

            while (Time.time < animationStartTime)
            {
                yield return null;
            }

            //Unparent text placeholder from character object so it doesn't follow character during animation
            AbilityTextPlaceholder.transform.SetParent(ScriptsObjectTransform);
            Vector3 initialPosition = AbilityTextPlaceholder.transform.position;
            Vector3 targetPositon = new Vector3(initialPosition.x,
                                                initialPosition.y + AbilityTxtMoveAnimationYAmount,
                                                initialPosition.z);
            TextWorkerAbility.gameObject.SetActive(true);

            float lerpStep = 0f;

            while (lerpStep < 1f)
            {
                Vector3 currentPositon = Vector3.Lerp(initialPosition, targetPositon, lerpStep);
                currentPositon.x += Mathf.Sin(3f * Time.time) / 5f; //Get effect of moving left and right
                AbilityTextPlaceholder.transform.position = currentPositon;

                if (lerpStep >= 0.5f)
                {
                    //If text has reached half way to its target position start fading it
                    float alpha = 1 - Utils.MapRange(lerpStep, 0.5f, 1f, 0f, 1f);
                    abilityTextColor.a = alpha;
                    TextWorkerAbility.color = abilityTextColor;
                }

                lerpStep += AbilityTxtMoveAnimationSpeed * Time.deltaTime;
                yield return null;
            }

            TextWorkerAbility.gameObject.SetActive(false);
            //Set text placeholder's parent back to character's object
            AbilityTextPlaceholder.transform.SetParent(transform.parent);
            AbilityTextPlaceholder.transform.localPosition = AbilityTextPlaceholderLocalPostion;
            AbilityTextAnimationActive = false;
        }

        private void OnWorkerAbilityUpdated(SharedWorker worker, ProjectTechnology workerAbility, float workerAbilityValue)
        {
            if (false == AbilityTextAnimationActive)
            {
                TextWorkerAbility.text = "+ " + EnumToString.GetString(workerAbility);
                StartCoroutine(AbilityTextAnimation());
            }
        }

        /*Public methods*/
    }
}
