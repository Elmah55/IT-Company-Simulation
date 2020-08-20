using UnityEngine;
using TMPro;

namespace ITCompanySimulation.UI
{
    public class UISettingsCredentials : MonoBehaviour
    {
        /*Private consts fields*/

        /// <summary>
        /// How long text should be displayed after clicking save button
        /// </summary>
        private const float TextCredentialsSavedDisplayTime = 2f;

        /*Private fields*/

        [SerializeField]
        private TMP_InputField InputFieldCompanyName;
        [SerializeField]
        private TMP_InputField InputFieldPlayerNickName;
        [SerializeField]
        private TextMeshProUGUI TextCredentialsSaved;
        [SerializeField]
        private PlayerInfo PlayerInfoComponent;
        /// <summary>
        /// How long is this text active
        /// </summary>
        private float TextCredentialsSavedActiveTime;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Start()
        {
            InputFieldCompanyName.text = PlayerInfoComponent.CompanyName;
            InputFieldPlayerNickName.text = PlayerInfoComponent.Nickname;
            TextCredentialsSaved.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (true == TextCredentialsSaved.gameObject.GetActive())
            {
                TextCredentialsSavedActiveTime += Time.deltaTime;

                if (TextCredentialsSavedActiveTime >= TextCredentialsSavedDisplayTime)
                {
                    TextCredentialsSaved.gameObject.SetActive(false);
                }
            }
        }

        private void OnDisable()
        {
            TextCredentialsSaved.gameObject.SetActive(false);
        }

        /*Public methods*/

        public void OnButtonSaveCliked()
        {
            PlayerInfoComponent.Apply(InputFieldCompanyName.text, InputFieldPlayerNickName.text);
            TextCredentialsSaved.gameObject.SetActive(true);
            TextCredentialsSavedActiveTime = 0f;
        }
    }
}
