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

        public static ListViewElementWorker CreateWorkerListViewElement(SharedWorker worker, ListViewElementWorker prefab, Tooltip tooltipComponent = null)
        {
            ListViewElementWorker el = GameObject.Instantiate<ListViewElementWorker>(prefab);

            if (null != tooltipComponent)
            {
                MousePointerEvents mousePtrEvt = el.gameObject.AddComponent<MousePointerEvents>();

                mousePtrEvt.PointerEntered += () =>
                {
                    tooltipComponent.gameObject.SetActive(true);
                    tooltipComponent.Text = GetWorkerAbilitiesString(worker);
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
            return string.Format("{0} {1}\n{2} days of expierience\nSalary {3} $ / Month",
                worker.Name, worker.Surename, worker.ExperienceTime, worker.Salary);
        }

        public static string GetWorkerAbilitiesString(SharedWorker worker)
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

        public static string GetWorkerNameString(SharedWorker worker)
        {
            return string.Format("Name: {0} {1}",
                worker.Name, worker.Surename);
        }

        public static string GetWorkerExpierienceString(SharedWorker worker)
        {
            return string.Format("Expierience: {0} days",
                worker.ExperienceTime);
        }

        public static string GetWorkerSalaryString(SharedWorker worker)
        {
            return string.Format("Salary: {0} $",
                worker.Salary);
        }

        public static string GetWorkerSatisfactionString(LocalWorker worker)
        {
            return string.Format("Satisfaction: {0} %",
                 worker.Satiscation.ToString("0.00"));
        }

        public static string GetWorkerDaysInCompanyString(LocalWorker worker)
        {
            return string.Format("Days in company: {0} days",
                worker.DaysInCompany);
        }

        /// <summary>
        /// Returns list view element representing worker
        /// </summary>
        public static ListViewElementWorker WorkerToListViewElement(SharedWorker worker, ControlListView listView)
        {
            ListViewElementWorker element = listView.Controls.Find(
                x => x.GetComponent<ListViewElementWorker>().Worker == worker).GetComponent<ListViewElementWorker>();

            return element;
        }
    }
}
