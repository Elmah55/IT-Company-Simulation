using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class UIProgressBarText : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    private Text TextComponent;
    private Slider SliderComponent;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    /*Public methods*/

    public void OnProgressBarUpdate(float value)
    {
        TextComponent.text = string.Format("{0} %", value.ToString("0.00"));
    }

    public void Start()
    {
        TextComponent = GetComponentInChildren<Text>();
        SliderComponent = GetComponent<Slider>();
        SliderComponent.onValueChanged.AddListener(OnProgressBarUpdate);
        TextComponent.text = string.Format("{0} %", SliderComponent.value.ToString("0.00 "));
    }
}
