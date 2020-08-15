using UnityEngine;
using UnityEngine.UI;

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
        public Image BackgroundImage;
        public Image ForegroundImage;

        /*Private methods*/

        private void Start()
        {
            ForegroundImageTransform = ForegroundImage.gameObject.GetComponent<RectTransform>();
        }

        private void Update()
        {
            Value = Mathf.Clamp(Value, MinimumValue, MaximumValue);
            CalculateForegroundImageSize();
        }

        private void CalculateForegroundImageSize()
        {
            //Map values range to rect transform size range
            float transformScaleX = Utils.MapRange(Value, MinimumValue, MaximumValue, 0f, 1f);
            //When min and max values are 0 map range will return NaN
            transformScaleX = (true == float.IsNaN(transformScaleX)) ? 0f : transformScaleX;
            Vector3 newScale = new Vector3(transformScaleX, 1f, 1f);
            ForegroundImageTransform.localScale = newScale;
        }

        /*Public methods*/
    }
}
