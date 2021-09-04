using UnityEngine;
using UnityEngine.UI;
using ITCompanySimulation.Utilities;

namespace ITCompanySimulation.UI
{
    [ExecuteAlways]
    public class ProgressBar : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        private RectTransform ForegroundImageTransform;

        /*Public consts fields*/

        /*Public fields*/

        public float MinimumValue;
        public float MaximumValue;
        public float Value;
        [Tooltip("Image that will be updated when progress bar's value changes")]
        [SerializeField]
        private Image ProgressBarImage;

        /*Private methods*/

        private void Start()
        {
            if (Image.Type.Sliced == ProgressBarImage.type)
            {
                ForegroundImageTransform = ProgressBarImage.gameObject.GetComponent<RectTransform>();
            }
        }

        private void Update()
        {
            Value = Mathf.Clamp(Value, MinimumValue, MaximumValue);
            CalculateProgressBarImageSize();
        }

        private void CalculateProgressBarImageSize()
        {
            //Map values range to rect transform size range
            float transformScaleX = Utils.MapRange(Value, MinimumValue, MaximumValue, 0f, 1f);
            //When min and max values are 0 map range will return NaN
            transformScaleX = (true == float.IsNaN(transformScaleX)) ? 0f : transformScaleX;

            //For normal progress bar update image scale,
            //for radial progress bar usage image fill property
            if (Image.Type.Sliced == ProgressBarImage.type)
            {
                Vector3 newScale = new Vector3(transformScaleX, 1f, 1f);
                ForegroundImageTransform.localScale = newScale;
            }
            else if (Image.Type.Filled == ProgressBarImage.type)
            {
                ProgressBarImage.fillAmount = transformScaleX;
            }
        }

        /*Public methods*/
    }
}
