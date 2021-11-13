using UnityEngine;
using TMPro;
using ITCompanySimulation.Core;
using UnityEngine.UI;

namespace ITCompanySimulation.UI
{
    public class UIStats : Photon.PunBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        private TextMeshProUGUI TextOtherPlayerStats;
        [SerializeField]
        private TextMeshProUGUI TextThisPlayerStats;
        [SerializeField]
        private TextMeshProUGUI TextOtherPlayerStatsTitle;
        [SerializeField]
        private ListViewElement PlayerListListViewElementPrefab;
        [SerializeField]
        private ControlListView PlayerListListView;
        private SimulationManager SimulationManagerComponent;
        private IButtonSelector ButtonSelectorPlayerList = new ButtonSelector();
        /// <summary>
        /// Player that is currently selected form players' list view.
        /// Null if none player is selected.
        /// </summary>
        private PhotonPlayer SelectedPlayer;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        /// <summary>
        /// Returns string with simulation statistics listed.
        /// </summary>
        private string GetStatsTxt(SharedSimulationStats stats)
        {
            string statsTxt = string.Format(
                                "Money earned: {0} $\n" +
                                "Money spent: {1} $\n" +
                                "Workers hired: {2}\n" +
                                "Other players' workers hired: {3}\n" +
                                "Workers that left company: {4}\n" +
                                "Projects completed: {5}\n" +
                                "Balance: {6} $",
                                stats.MoneyEarned,
                                stats.MoneySpent,
                                stats.WorkersHired,
                                stats.OtherPlayersWorkersHired,
                                stats.WorkersLeftCompany,
                                stats.ProjectsCompleted,
                                stats.CompanyBalance);

            return statsTxt;
        }

        /// <summary>
        /// Called when stats of local player are updated.
        /// </summary>
        private void OnThisPlayerStatsUpdated()
        {
            TextThisPlayerStats.text = GetStatsTxt(SimulationManagerComponent.Stats);
        }

        /// <summary>
        /// Called when stats of other player are updated.
        /// </summary>
        /// <param name="otherPlayer"></param>
        private void OnOtherPlayerStatsUpdated(PhotonPlayer otherPlayer)
        {
            if (null != SelectedPlayer && SelectedPlayer.ID == otherPlayer.ID)
            {
                TextOtherPlayerStats.text =
                    GetStatsTxt(SimulationManagerComponent.PlayerDataMap[otherPlayer.ID].Stats);
                TextOtherPlayerStatsTitle.text = string.Format("{0}'s stats", otherPlayer.NickName);
            }
        }

        private void Awake()
        {
            SimulationManagerComponent =
                GameObject.FindGameObjectWithTag("ScriptsGameObject").GetComponent<SimulationManager>();
        }

        private void Start()
        {
            SimulationManagerComponent.Stats.StatsUpdated += OnThisPlayerStatsUpdated;
            ButtonSelectorPlayerList.SelectedButtonChanged += OnPlayerListSelectedButtonChanged;

            foreach (var data in SimulationManagerComponent.PlayerDataMap)
            {
                data.Value.StatsUpdated += OnOtherPlayerStatsUpdated;
            }

            foreach (PhotonPlayer player in PhotonNetwork.otherPlayers)
            {
                ListViewElement newListViewElement =
                    GameObject.Instantiate(PlayerListListViewElementPrefab);
                newListViewElement.RepresentedObject = player;
                newListViewElement.Text.text = player.NickName;
                ButtonSelectorPlayerList.AddButton(newListViewElement.Button);
                PlayerListListView.AddControl(newListViewElement.gameObject);
            }

            TextOtherPlayerStatsTitle.text = "Other player's stats";
            //Init stats text so stats are displayed without waiting
            //for update
            OnThisPlayerStatsUpdated();
        }

        private void OnPlayerListSelectedButtonChanged(Button btn)
        {
            if (null != btn)
            {
                ListViewElement listViewElement = btn.GetComponent<ListViewElement>();
                SelectedPlayer = (PhotonPlayer)listViewElement.RepresentedObject;
                OnOtherPlayerStatsUpdated(SelectedPlayer);
            }
            else
            {
                SelectedPlayer = null;
            }
        }

        /*Public methods*/

        public override void OnPhotonPlayerDisconnected(PhotonPlayer disconnectedPlayer)
        {
            base.OnPhotonPlayerDisconnected(disconnectedPlayer);

            ListViewElement disconnectedPlayerElement = PlayerListListView.FindElement(disconnectedPlayer);
            Button elementButton = disconnectedPlayerElement.GetComponent<Button>();
            ButtonSelectorPlayerList.RemoveButton(elementButton);
            PlayerListListView.RemoveControl(disconnectedPlayerElement.gameObject);
        }
    }
}
