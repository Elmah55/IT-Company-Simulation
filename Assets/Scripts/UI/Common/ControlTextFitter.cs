using UnityEngine;
using TMPro;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// Resizes control to match text size
    /// </summary>
    public class ControlTextFitter : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        [Tooltip("Text that control size will match")]
        public TextMeshProUGUI Text;
        public bool MatchWidth;
        public bool MatchHeight;

        /*Private methods*/

        private void Update()
        {
            RectTransform rect = (RectTransform)this.gameObject.transform;
            float width = MatchWidth ? Text.preferredWidth : rect.sizeDelta.x;
            float heigth = MatchHeight ? Text.preferredHeight : rect.sizeDelta.y;
            Vector2 rectSize = new Vector2(width, heigth);
            rect.sizeDelta = rectSize;
        }

        /*Public methods*/
    }
}
