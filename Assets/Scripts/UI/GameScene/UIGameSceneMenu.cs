﻿using UnityEngine;
using ITCompanySimulation.Core;
using UnityEngine.Events;

namespace ITCompanySimulation.UI
{
    public class UIGameSceneMenu : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        private InfoWindow InfoWindowComponent;
        private ApplicationManager ApplicationManagerComponent;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Start()
        {
            InfoWindowComponent = InfoWindow.Instance;
            ApplicationManagerComponent = ApplicationManager.Instance;
        }

        /*Public methods*/

        public void OnExitToMenuButtonClick()
        {
            string infoWindowText = "Do you really want to exit to main menu ?";

            UnityAction okAction = () =>
            {
                ApplicationManagerComponent.FinishSession();
                ApplicationManagerComponent.LoadScene(SceneIndex.Menu);
            };

            InfoWindowComponent.ShowOkCancel(infoWindowText, okAction, null);
        }
    } 
}
