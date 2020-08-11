using ITCompanySimulation.Developing;
using System.Text;
using UnityEngine;

namespace ITCompanySimulation.UI
{
    public abstract class UIProjects : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        protected IButtonSelector ButtonSelectorProjects;

        /*Public consts fields*/

        /*Public fields*/

        protected static StringBuilder StrBuilder = new StringBuilder();

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

        protected ListViewElement CreateProjectListViewElement(LocalProject proj, ListViewElement prefab)
        {
            ListViewElement newElement = GameObject.Instantiate<ListViewElement>(prefab);
            newElement.Text.text = GetProjectListViewElementText(proj);

            return newElement;
        }

        protected string GetProjectListViewElementText(LocalProject proj)
        {
            return string.Format("{0}\nCompletion bonus: {1} $\nProgress: {2} %",
                                 proj.Name,
                                 proj.CompleteBonus,
                                 proj.Progress.ToString("0.00"));
        }

        /*Public methods*/
    }
}
