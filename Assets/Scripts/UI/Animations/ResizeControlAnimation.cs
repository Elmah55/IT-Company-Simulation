using UnityEngine;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// Script that will resize control's size by specified percentage.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ResizeControlAnimation : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        /// <summary>
        /// This game object's transform.
        /// </summary>
        private RectTransform LocalTransform;
        /// <summary>
        /// Scale of transform before resizing.
        /// </summary>
        private Vector2 OrignalSize;

        /*Public consts fields*/

        /*Public fields*/

        [Tooltip("Specifies by how many % control should grow")]
        public float ResizePercentage;
        [Tooltip("How many seconds it should take to resize control")]
        public float ResizeTime;

        /*Private methods*/

        private void Awake()
        {
            LocalTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            OrignalSize = LocalTransform.localScale;
        }

        private void OnDisable()
        {
            RestoreOriginalSize();
        }

        /*Public methods*/

        /// <summary>
        /// Increases element's size by specified percentage.
        /// </summary>
        public void IncreaseSize()
        {
            if (false == LeanTween.isTweening(this.gameObject))
            {
                OrignalSize = LocalTransform.localScale;
                //Conver to percent value
                float resizePercentValue = ResizePercentage / 100f;
                float newScaleX = OrignalSize.x * (1f + resizePercentValue);
                float newScaleY = OrignalSize.y * (1f + resizePercentValue);
                Vector2 newScale = new Vector2(newScaleX, newScaleY);
                LeanTween.scale(LocalTransform, newScale, ResizeTime).setIgnoreTimeScale(true);
            }
        }

        /// <summary>
        /// Restores element's size back to its original value.
        /// </summary>
        public void RestoreOriginalSize()
        {
            LeanTween.scale(LocalTransform, OrignalSize, ResizeTime).setIgnoreTimeScale(true);
        }
    }
}
