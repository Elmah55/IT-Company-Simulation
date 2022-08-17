using UnityEngine;
using TMPro;
using ITCompanySimulation.Core;
using System.Collections.Generic;
using ITCompanySimulation.Settings;

namespace ITCompanySimulation.UI
{
    public class UIStats : Photon.PunBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        private SimulationManager SimulationManagerComponent;
        [SerializeField]
        private TextMeshProUGUI TextMoneyEarned;
        [SerializeField]
        private TextMeshProUGUI TextMoneySpent;
        [SerializeField]
        private TextMeshProUGUI TextWorkersHired;
        [SerializeField]
        private TextMeshProUGUI TextOtherPlayersWorkersHired;
        [SerializeField]
        private TextMeshProUGUI TextWorkersLeftCompany;
        [SerializeField]
        private TextMeshProUGUI TextProjectsCompleted;
        [SerializeField]
        private TextMeshProUGUI TextCompanyBalance;
        [SerializeField]
        private ProgressBar OtherPlayerBalanceProgressBarPrefab;
        [SerializeField]
        private GameObject ProgressBarLayout;
        /// <summary>
        /// Maps ID of photon player to progress bar with balance of his company.
        /// </summary>
        private Dictionary<int, ProgressBar> PhotonPlayerProgressBarMap = new Dictionary<int, ProgressBar>();

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        /// <summary>
        /// Called when stats of player are updated.
        /// </summary>
        private void OnThisPlayerStatsUpdated()
        {
            SimulationStats stats = SimulationManagerComponent.Stats;
            TextMoneyEarned.text = string.Format("Money earned: {0} $", stats.MoneyEarned);
            TextMoneySpent.text = string.Format("Money spent: {0} $", stats.MoneySpent);
            TextWorkersHired.text = string.Format("Workers hired: {0}", stats.WorkersHired);
            TextOtherPlayersWorkersHired.text = string.Format("Other players' workers hired: {0}", stats.OtherPlayersWorkersHired);
            TextWorkersLeftCompany.text = string.Format("Workers that left company: {0}", stats.WorkersLeftCompany);
            TextProjectsCompleted.text = string.Format("Number of completed projects: {0}", stats.ProjectsCompleted);
            TextCompanyBalance.text = string.Format("Company balance: {0} $", stats.CompanyBalance);
        }

        /// <summary>
        /// Called when stats of other player are updated.
        /// </summary>
        /// <param name="otherPlayer"></param>
        private void OnOtherPlayerCompanyBalanceUpdated(PlayerData data)
        {
            ProgressBar companyBalanceProgressBar = PhotonPlayerProgressBarMap[data.Player.ID];
            companyBalanceProgressBar.Value = data.CompanyBalance;
            companyBalanceProgressBar.Text.text = string.Format("{0} {1} / {2} $",
                                                                data.Player.NickName,
                                                                data.CompanyBalance,
                                                                SimulationSettings.TargetBalance);
        }

        private void Awake()
        {
            SimulationManagerComponent =
                GameObject.FindGameObjectWithTag("ScriptsGameObject").GetComponent<SimulationManager>();
        }

        private void Start()
        {
            SimulationManagerComponent.Stats.StatsUpdated += OnThisPlayerStatsUpdated;

            foreach (var data in SimulationManagerComponent.PlayerDataMap)
            {
                ProgressBar playerBalanceProgressBar =
                    GameObject.Instantiate(OtherPlayerBalanceProgressBarPrefab, ProgressBarLayout.transform);
                playerBalanceProgressBar.gameObject.SetActive(true);
                playerBalanceProgressBar.MinimumValue = SimulationSettings.MinimalBalance;
                playerBalanceProgressBar.MaximumValue = SimulationSettings.TargetBalance;
                PhotonPlayerProgressBarMap.Add(data.Value.Player.ID, playerBalanceProgressBar);

                data.Value.CompanyBalanceUpdated += OnOtherPlayerCompanyBalanceUpdated;
                //Init stats so balance is displayed without waiting for update
                OnOtherPlayerCompanyBalanceUpdated(data.Value);
            }

            //Init stats text so stats are displayed without waiting for update
            OnThisPlayerStatsUpdated();
        }

        /*Public methods*/

        public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            base.OnPhotonPlayerDisconnected(otherPlayer);

            ProgressBar companyBalanceProgressBar = PhotonPlayerProgressBarMap[otherPlayer.ID];
            GameObject.Destroy(companyBalanceProgressBar.gameObject);
            PhotonPlayerProgressBarMap.Remove(otherPlayer.ID);
        }
    }
}
