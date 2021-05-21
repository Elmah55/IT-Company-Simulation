using UnityEngine;
using TMPro;
using ITCompanySimulation.Settings;

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
        /// <summary>
        /// How long is this text active
        /// </summary>
        private float TextCredentialsSavedActiveTime;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Start()
        {
            InputFieldCompanyName.text = PlayerInfo.CompanyName;
            InputFieldCompanyName.characterLimit = PlayerInfo.COMPANY_NAME_MAX_LENGHT;
            InputFieldPlayerNickName.text = PlayerInfo.Nickname;
            InputFieldPlayerNickName.characterLimit = PlayerInfo.PLAYER_NICKNAME_MAX_LENGTH;
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
            PlayerInfo.Apply(InputFieldCompanyName.text, InputFieldPlayerNickName.text);
            TextCredentialsSaved.gameObject.SetActive(true);
            TextCredentialsSavedActiveTime = 0f;
        }
    }
}
