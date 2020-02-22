using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This is base class for displaying worker info
/// </summary>
public abstract class UIWorkers : Photon.PunBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    [SerializeField]
    protected InputField InputFieldNameAndSurename;
    [SerializeField]
    protected InputField InputFieldExpierienceTime;
    [SerializeField]
    protected InputField InputFieldSalary;
    [SerializeField]
    protected InputField InputFieldDaysInCompany;
    [SerializeField]
    protected ControlListView ListViewWorkerAbilities;
    [SerializeField]
    protected Button ListViewButtonPrefab;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    protected virtual void UpdateWorkerInfo(Worker selectedWorker)
    {
        //Name and surename
        InputFieldNameAndSurename.text = string.Format("{0} {1}", selectedWorker.Name, selectedWorker.Surename);

        //Expierience time
        InputFieldExpierienceTime.text = string.Format("{0} days", selectedWorker.ExperienceTime);

        //Salary
        InputFieldSalary.text = string.Format("{0} $", selectedWorker.Salary);

        //Days in company
        if (null != selectedWorker.WorkingCompany)
        {
            InputFieldDaysInCompany.text = string.Format("{0} days", selectedWorker.DaysInCompany);

        }

        //Abilites
        UpdateWorkerAbilitiesListView(ListViewWorkerAbilities, selectedWorker);
    }

    protected void ClearWorkerInfo()
    {
        InputFieldNameAndSurename.text = InputFieldExpierienceTime.text = InputFieldSalary.text = string.Empty;
        ListViewWorkerAbilities.RemoveAllControls();
    }

    protected void UpdateWorkerAbilitiesListView(ControlListView listView, Worker workerToDisplay)
    {
        listView.RemoveAllControls();

        foreach (KeyValuePair<ProjectTechnology, float> ability in workerToDisplay.Abilites)
        {
            Button workerAbilityButton = CreateWorkerAbilityButton(ability);
            listView.AddControl(workerAbilityButton.gameObject);
        }
    }

    private Button CreateWorkerAbilityButton(KeyValuePair<ProjectTechnology, float> ability)
    {
        Button workerAbilityButton = GameObject.Instantiate<Button>(ListViewButtonPrefab);
        Text buttonTextComponent = workerAbilityButton.gameObject.GetComponentInChildren<Text>(workerAbilityButton);
        string buttonText = string.Format("{0} {1}", ability.Key, ability.Value.ToString("0.00"));
        buttonTextComponent.text = buttonText;

        return workerAbilityButton;
    }

    /*Public methods*/
}
