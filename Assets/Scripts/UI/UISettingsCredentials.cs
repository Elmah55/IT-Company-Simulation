using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISettingsCredentials : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    [SerializeField]
    private InputField InputFieldCompanyName;
    [SerializeField]
    private InputField InputFieldPlayerNickName;
    [SerializeField]
    private PlayerInfo PlayerInfoComponent;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    private void Start()
    {
        InputFieldCompanyName.text = PlayerInfoComponent.CompanyName;
        InputFieldPlayerNickName.text = PlayerInfoComponent.Nickname;
    }

    /*Public methods*/

    public void OnButtonSaveCliked()
    {
        PlayerInfoComponent.Apply(InputFieldCompanyName.text, InputFieldPlayerNickName.text);
    }
}
