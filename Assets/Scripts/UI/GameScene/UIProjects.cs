using ITCompanySimulation.Project;
using System.Text;
using TMPro;
using UnityEngine;

namespace ITCompanySimulation.UI
{
    public abstract class UIProjects : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        protected IButtonSelector ButtonSelectorProjects;
        private static StringBuilder StrBuilder = new StringBuilder();
        [SerializeField]
        private ListViewElement ListViewElementPrefab;
        [SerializeField]
        protected Tooltip TooltipComponent;
        [SerializeField]
        protected TextMeshProUGUI TextProjectName;
        [SerializeField]
        protected TextMeshProUGUI TextCompleteBonus;
        [SerializeField]
        protected TextMeshProUGUI TextPenalty;
        [SerializeField]
        protected TextMeshProUGUI TextUsedTechnologies;
        [SerializeField]
        protected TextMeshProUGUI TextCompletionTime;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        protected string GetProjectTechnologiesString(SharedProject proj)
        {
            StrBuilder.Clear();

            for (int i = 0; i < proj.UsedTechnologies.Count; i++)
            {
                ProjectTechnology pt = proj.UsedTechnologies[i];
                StrBuilder.Append(EnumToString.GetString(pt));

                if (i != proj.UsedTechnologies.Count - 1)
                {
                    StrBuilder.Append(" / ");
                }
            }

            return StrBuilder.ToString();
        }

        protected string GetProjectListViewElementText(LocalProject proj)
        {
            return string.Format("{0}\nCompletion bonus: {1} $\nProgress: {2} %\nCompletion time: {3} days",
                                 proj.Name,
                                 proj.CompletionBonus,
                                 proj.Progress.ToString("0.00"),
                                 proj.CompletionTime);
        }

        /// <summary>
        /// Used to display text in tooltip component
        /// </summary>
        protected void SubscribeToMouseEventPointers(MousePointerEvents events, SharedProject proj, Tooltip tooltipComponent)
        {
            events.PointerEntered.AddListener(() =>
           {
               tooltipComponent.gameObject.SetActive(true);
               string tooltipText = string.Format("Used technologies:\n{0}",
                   GetProjectTechnologiesString(proj));
               tooltipComponent.Text = tooltipText;
           });

            events.PointerExited.AddListener(() =>
            {
                tooltipComponent.gameObject.SetActive(false);
            });
        }

        protected ListViewElement CreateListViewElement(SharedProject proj)
        {
            ListViewElement newElement = GameObject.Instantiate(ListViewElementPrefab);
            MousePointerEvents events = newElement.GetComponent<MousePointerEvents>();
            SubscribeToMouseEventPointers(events, proj, TooltipComponent);

            newElement.FrontImage.sprite = proj.Icon;
            newElement.RepresentedObject = proj;
            newElement.gameObject.SetActive(true);

            return newElement;
        }

        /*Public methods*/
    }
}
