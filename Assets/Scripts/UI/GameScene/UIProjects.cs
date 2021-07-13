using ITCompanySimulation.Project;
using ITCompanySimulation.Utilities;
using System.Text;
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
        private ListViewElementProject ListViewElementPrefab;
        [SerializeField]
        protected Tooltip TooltipComponent;

        /*Public consts fields*/

        /*Public fields*/

        protected static IObjectPool<ListViewElementProject> ListViewElementPool;

        /*Private methods*/

        protected string GetProjectTechnologiesString(SharedProject proj)
        {
            StrBuilder.Clear();

            for (int i = 0; i < proj.UsedTechnologies.Count; i++)
            {
                ProjectTechnology pt = proj.UsedTechnologies[i];
                StrBuilder.Append(EnumToString.ProjectTechnologiesStrings[pt]);

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
        /// Returns list view element associated with given project
        /// </summary>
        /// <param name="listView">List view element will be searched in this list view</param>
        protected ListViewElementProject GetProjectListViewElement(ControlListView listView, SharedProject proj)
        {
            GameObject elementObject = null;
            ListViewElementProject element = null;

            elementObject = listView.Controls.Find(x =>
            {
                return x.GetComponent<ListViewElementProject>().Project == proj;
            });

            if (null != elementObject)
            {
                element = elementObject.GetComponent<ListViewElementProject>();
            }

            return element;
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

        protected ListViewElementProject CreateListViewElement(SharedProject proj)
        {
            ListViewElementProject newElement = null;

            if (null != ListViewElementPool)
            {
                newElement = ListViewElementPool.GetObject();
            }

            if (null == newElement)
            {
                newElement = GameObject.Instantiate<ListViewElementProject>(ListViewElementPrefab);
                MousePointerEvents events = newElement.GetComponent<MousePointerEvents>();
                SubscribeToMouseEventPointers(events, proj, TooltipComponent);
            }
            else
            {
                MousePointerEvents events = newElement.GetComponent<MousePointerEvents>();
                events.PointerEntered.RemoveAllListeners();
                events.PointerExited.RemoveAllListeners();
                SubscribeToMouseEventPointers(events, proj, TooltipComponent);
            }

            newElement.FrontImage.sprite = proj.Icon;
            newElement.Project = proj;
            newElement.gameObject.SetActive(true);

            return newElement;
        }

        protected string GetEstimatedCompletionTimeText(int days)
        {
            string estimatedCompletionStr = string.Empty;

            if (-1 != days)
            {
                estimatedCompletionStr = string.Format("{0} days",
                                                       days);
            }

            return estimatedCompletionStr;
        }

        /*Public methods*/
    }
}
