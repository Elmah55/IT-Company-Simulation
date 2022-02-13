using ITCompanySimulation.Character;
using ITCompanySimulation.Company;
using ITCompanySimulation.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ITCompanySimulation.Project;

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
            ListViewElement element = GameObject.Instantiate<ListViewElement>(prefab);

            if (null != tooltipComponent)
            {
                MousePointerEvents mousePtrEvt = element.gameObject.AddComponent<MousePointerEvents>();

                mousePtrEvt.PointerEntered.AddListener(() =>
               {
                   tooltipComponent.gameObject.SetActive(true);
                   tooltipComponent.Text = GetWorkerAbilitiesString(worker);
               });

                mousePtrEvt.PointerExited.AddListener(() =>
               {
                   tooltipComponent.gameObject.SetActive(false);
               });
            }

            element.Text.text = GetWorkerListViewElementText(worker);
            element.RepresentedObject = worker;
            element.FrontImage.sprite = worker.Avatar;

            return element;
        }

        public static string GetWorkerListViewElementText(SharedWorker worker)
        {
            return string.Format("{0} {1}\n{2} days of expierience\nSalary {3} $ / Month",
                worker.Name, worker.Surename, worker.ExperienceTime, worker.Salary);
        }

        public static string GetWorkerAbilitiesString(SharedWorker worker)
        {
            StrBuilder.Clear();

            for (int i = 0; i < worker.Abilites.Count; i++)
            {
                KeyValuePair<ProjectTechnology, SafeFloat> ability = worker.Abilites.ElementAt(i);

                StrBuilder.AppendFormat("{0} {1}\n",
                    EnumToString.GetString(ability.Key),
                    ability.Value.Value.ToString("0.00"));
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
            return string.Format("{0} days",
                worker.DaysInCompany);
        }

        public static string GetCompanyWorkersListViewString(PlayerCompany company)
        {
            return string.Format("Company workers ({0} / {1})",
                company.Workers.Count,
                PlayerCompany.MAX_WORKERS_PER_COMPANY);
        }
    }
}
