using ITCompanySimulation.Developing;
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
            return string.Format("{0}\nCompletion bonus: {1} $\nProgress: {2} %",
                                 proj.Name,
                                 proj.CompleteBonus,
                                 proj.Progress.ToString("0.00"));
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

        protected void SubscribeToMouseEventPointers(MousePointerEvents events, SharedProject proj, Tooltip tooltipComponent)
        {
            events.PointerEntered += () =>
            {
                tooltipComponent.gameObject.SetActive(true);
                string tooltipText = string.Format("Used technologies:\n{0}",
                    GetProjectTechnologiesString(proj));
                tooltipComponent.Text = tooltipText;
            };

            events.PointerExited += () =>
            {
                tooltipComponent.gameObject.SetActive(false);
            };
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
                events.RemoveAllListeners();
                SubscribeToMouseEventPointers(events, proj, TooltipComponent);
            }

            newElement.Project = proj;
            newElement.gameObject.SetActive(true);

            return newElement;
        }

        /*Public methods*/
    }
}
