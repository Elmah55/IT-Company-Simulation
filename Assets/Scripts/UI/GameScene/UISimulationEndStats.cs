using UnityEngine;
using ITCompanySimulation.Core;
using TMPro;

namespace ITCompanySimulation.UI
{
    public class UISimulationEndStats : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        private ApplicationManager ApplicationManagerComponent;
        [SerializeField]
        private SimulationManager SimulationManagerComponent;
        [SerializeField]
        private TextMeshProUGUI TextStats;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Awake()
        {
            ApplicationManagerComponent =
                GameObject.FindGameObjectWithTag("ApplicationManager").GetComponent<ApplicationManager>();
        }

        private void OnEnable()
        {
            string statsString = string.Format(
                "Simulation running time: {0}h {1}m\n" +
                "Days since start: {2}\n" +
                "Money earned: {3} $\n" +
                "Money spent: {4} $\n" +
                "Workers hired: {5}\n" +
                "Other players' workers hired: {6}\n" +
                "Workers that left company: {7}\n" +
                "Projects completed: {8}",
                SimulationManagerComponent.Stats.SimulationRunningTime.Hours,
                SimulationManagerComponent.Stats.SimulationRunningTime.Minutes,
                SimulationManagerComponent.Stats.DaysSinceStart,
                SimulationManagerComponent.Stats.MoneyEarned,
                SimulationManagerComponent.Stats.MoneySpent,
                SimulationManagerComponent.Stats.WorkersHired,
                SimulationManagerComponent.Stats.OtherPlayersWorkersHired,
                SimulationManagerComponent.Stats.WorkersLeftCompany,
                SimulationManagerComponent.Stats.ProjectsCompleted);

            TextStats.text = statsString;
        }

        /*Public methods*/

        public void OnContinueButtonClicked()
        {
            ApplicationManagerComponent.LoadScene(SceneIndex.Menu);
        }
    }

}