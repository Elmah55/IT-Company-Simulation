using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ITCompanySimulation.Core;
using ITCompanySimulation.Utilities;
using ITCompanySimulation.Settings;
using System;
using System.Globalization;

namespace ITCompanySimulation.UI
{
    public class UIBalanceStats : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        private GameObject ChartBarPrefab;
        [SerializeField]
        private TextMeshProUGUI TextYearPrefab;
        [SerializeField]
        private GameObject BarParent;
        [SerializeField]
        private TextMeshProUGUI TextUpperScale;
        [SerializeField]
        private TextMeshProUGUI TextLowerScale;
        [SerializeField]
        private GameTime GameTimeComponent;
        [SerializeField]
        private SimulationManager SimulationManagerComponent;
        [SerializeField]
        private Color BarColorPositiveBalance;
        [SerializeField]
        private Color BarColorNegativeBalance;
        [SerializeField]
        private Tooltip TooltipComponent;
        [SerializeField]
        private ScrollRect ScrollRectChart;
        [SerializeField]
        private HorizontalLayoutGroup ChartLayoutGroup;
        /// <summary>
        /// Maximum possible heigth of chart bar.
        /// </summary>
        private float BarMaxHeight;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Start()
        {
            GameTimeComponent.MonthChanged += OnMonthChanged;

            TextLowerScale.text = SimulationSettings.MinimalBalance.ToString() + " $";
            TextUpperScale.text = SimulationSettings.TargetBalance.ToString() + " $";

            BarMaxHeight = TextUpperScale.rectTransform.parent.localPosition.y;

            //Add all balance history that was stored before start of this component
            DateTime initialDate = GameTimeComponent.InitialDate;
            var balanceHistory = SimulationManagerComponent.Stats.BalanceHistory;

            for (int i = 0; i < balanceHistory.Count; i++)
            {
                int balance = balanceHistory[i];
                AddChartBar(initialDate, balance);
                initialDate = initialDate.AddMonths(1);
            }
        }

        private void OnMonthChanged()
        {
            DateTime currentDate = GameTimeComponent.CurrentDate;
            int currentBalance = SimulationManagerComponent.ControlledCompany.Balance;
            AddChartBar(currentDate, currentBalance);
        }

        private void AddChartBar(DateTime date, int balance)
        {
            float barHeight = 0f;

            if (balance >= 0)
            {
                barHeight = Utils.MapRange(balance,
                                           0f,
                                           SimulationSettings.TargetBalance,
                                           0f,
                                           BarMaxHeight);
            }
            else
            {
                barHeight = Utils.MapRange(Mathf.Abs(balance),
                                           0f,
                                           Mathf.Abs(SimulationSettings.MinimalBalance),
                                           0f,
                                           BarMaxHeight);
            }

            GameObject newBar = GameObject.Instantiate(ChartBarPrefab, BarParent.transform);
            newBar.gameObject.SetActive(true);

            RectTransform barImageTransform = newBar.GetComponentInChildren<Image>().rectTransform;
            barImageTransform.sizeDelta = new Vector2(barImageTransform.sizeDelta.x, barHeight);
            barImageTransform.pivot = new Vector2(barImageTransform.pivot.x, balance >= 0 ? 0f : 1f);

            Image barImage = newBar.GetComponentInChildren<Image>();
            float colorProgress = Utils.MapRange(
                balance >= 0 ? barHeight : -barHeight, -BarMaxHeight, BarMaxHeight, 0f, 1f);
            Color barColor = Color.Lerp(BarColorNegativeBalance, BarColorPositiveBalance, colorProgress);
            barImage.color = barColor;

            string monthName = date.ToString("MMMM");

            MousePointerEvents mouseEvents = newBar.GetComponentInChildren<MousePointerEvents>();
            mouseEvents.PointerEntered.AddListener(() =>
            {
                float scrollRectVelocity = ScrollRectChart.velocity.magnitude;

                //Show tooltip only when scroll rect speed is low
                if (scrollRectVelocity <= 200f)
                {

                    TooltipComponent.Text = string.Format("{0} {1}\nBalance: {2} $", monthName, date.Year, balance);
                    TooltipComponent.gameObject.SetActive(true);
                }
            });

            mouseEvents.PointerExited.AddListener(() =>
            {
                TooltipComponent.gameObject.SetActive(false);
            });

            if (12 == date.Month)
            {
                TextMeshProUGUI yearText = GameObject.Instantiate(TextYearPrefab, BarParent.transform);
                yearText.text = date.Year.ToString();
                yearText.gameObject.SetActive(true);
            }

            TextMeshProUGUI barText = newBar.GetComponentInChildren<TextMeshProUGUI>();
            barText.text = monthName;
        }

        /*Public methods*/
    }
}
