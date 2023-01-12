﻿using UnityEngine;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// Script that will resize control's size by specified percentage.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ResizeControlAnimation : UIAnimation
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
        /// <summary>
        /// Specifies whether this instance of component
        /// has been started and initialized
        /// </summary>
        private bool Initialized;

        /*Public consts fields*/

        /*Public fields*/

        [Tooltip("Specifies by how many % control should grow")]
        public float ResizePercentage;

        /*Private methods*/

        private void Awake()
        {
            LocalTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            OrignalSize = LocalTransform.localScale;
            Initialized = true;
        }

        private void OnDisable()
        {
            if (true == Initialized)
            {
                RestoreOriginalSize();
            }
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
                LeanTween.scale(LocalTransform, newScale, AnimationTime).setIgnoreTimeScale(true);
            }
        }

        /// <summary>
        /// Restores element's size back to its original value.
        /// </summary>
        public void RestoreOriginalSize()
        {
            LeanTween.scale(LocalTransform, OrignalSize, AnimationTime).setIgnoreTimeScale(true);
        }
    }
}
