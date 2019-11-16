using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWorkers : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    /*Public consts fields*/

    /*Public fields*/

    public GameObject ListViewButtonPrefab;
    public UIControlListView MarketWorkersListView;
    public UIControlListView CompanyWorkersListView;
    public MainSimulationManager SimulationManagerComponent;
    public WorkersMarket WorkersMarketComponent;

    /*Private methods*/

    private void InitializeWorkersListView(UIControlListView listView, List<Worker> workers)
    {
        foreach (Worker singleWorker in workers)
        {
            GameObject newListViewButton = GameObject.Instantiate(ListViewButtonPrefab);
            Button buttonComponent = newListViewButton.GetComponent<Button>();

            Text buttonTextComponent = buttonComponent.GetComponentInChildren<Text>();
            string buttonText = string.Format("{0} {1} / {2} days / {3} $",
                singleWorker.Name, singleWorker.Surename, singleWorker.ExperienceTime, singleWorker.Salary);
            buttonTextComponent.text = buttonText;

            listView.AddControl(newListViewButton);
        }
    }

    private void Start()
    {
        InitializeWorkersListView(MarketWorkersListView, WorkersMarketComponent.Workers);
        InitializeWorkersListView(CompanyWorkersListView, SimulationManagerComponent.ControlledCompany.Workers);
    }

    /*Public methods*/
}
