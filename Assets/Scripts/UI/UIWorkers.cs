using ITCompanySimulation.Character;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// This is base class for displaying worker info
    /// </summary>
    public static class UIWorkers
    {
        /*Private consts fields*/

        /*Private fields*/

        private static StringBuilder StrBuilder = new StringBuilder();

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        /*Public methods*/

        public static ListViewElement CreateWorkerListViewElement(SharedWorker worker, ListViewElement prefab, Tooltip tooltipComponent = null)
        {
            ListViewElement el = GameObject.Instantiate<ListViewElement>(prefab);

            if (null != tooltipComponent)
            {
                MousePointerEvents mousePtrEvt = el.gameObject.AddComponent<MousePointerEvents>();

                mousePtrEvt.PointerEntered += () =>
                {
                    tooltipComponent.gameObject.SetActive(true);
                    tooltipComponent.Text = GetWorkerAbilitiesText(worker);
                };

                mousePtrEvt.PointerExited += () =>
                {
                    tooltipComponent.gameObject.SetActive(false);
                };
            }

            el.Text.text = GetWorkerListViewElementText(worker);

            return el;
        }

        public static string GetWorkerListViewElementText(SharedWorker worker)
        {
            return string.Format("{0} {1}\n{2} days of expierience\n{3} $ / Month",
                worker.Name, worker.Surename, worker.ExperienceTime, worker.Salary);
        }

        public static string GetWorkerAbilitiesText(SharedWorker worker)
        {
            StrBuilder.Clear();
            StrBuilder.Append("Abilities:\n");

            for (int i = 0; i < worker.Abilites.Count; i++)
            {
                KeyValuePair<ProjectTechnology, float> ability = worker.Abilites.ElementAt(i);

                StrBuilder.AppendFormat("{0} {1}\n",
                    EnumToString.ProjectTechnologiesStrings[ability.Key],
                    ability.Value.ToString("0.00"));
            }

            return StrBuilder.ToString();
        }
    }
}
