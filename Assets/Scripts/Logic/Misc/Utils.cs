using UnityEngine;
using System.Linq;
using System.Reflection;

namespace ITCompanySimulation.Utilities
{
    public static class Utils
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        /*Public methods*/

        /// <summary>
        /// Maps one range of values to another range of values
        /// </summary>
        /// <param name="input">Value that will be mapped to another range</param>
        /// <param name="inputMin">Minimum value of input range</param>
        /// <param name="inputMax">Maximum value of input range</param>
        /// <param name="outputMin">Minimum value of output range</param>
        /// <param name="outputMax">Maximum value of output range</param>
        /// <returns></returns>
        public static float MapRange(float input, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return outputMin + ((outputMax - outputMin) / (inputMax - inputMin)) * (input - inputMin);
        }

        /// <summary>
        /// Maps linear value to dB log scale
        /// </summary>
        /// <param name="value">Linear value (should be greater than 0)</param>
        /// <returns>Value mapped to dB. If provided input value 
        /// was out of range (value less or equal to 0) returns float.NaN</returns>
        public static float MapLinearTodB(float value)
        {
            float dBValue = float.NaN;

            if (value > 0f)
            {
                dBValue = Mathf.Log10(value) * 20f;
            }

            return dBValue;
        }

        /// <summary>
        /// Maps dB log scale to linear value
        /// </summary>
        /// <param name="dBVolume"></param>
        /// <returns>Value mapped to linear scale (range 0.1 - 1)</returns>
        public static float MapDbToLinear(float dBVolume)
        {
            float linearValue = dBVolume / 20f;
            linearValue = Mathf.Pow(10f, linearValue);
            return linearValue;
        }

        /// <summary>
        /// Checks if mouse pointer is inside screen bounds
        /// </summary>
        /// <returns>True if pointer is inside screen bounds, false otherwise</returns>
        public static bool MouseInsideScreen()
        {
            Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
            bool result = screenRect.Contains(Input.mousePosition);
            return result;
        }

        /// <summary>
        /// Returns photon player from photon player ID. If player with given ID does
        /// not exists returns null.
        /// </summary>
        public static PhotonPlayer PhotonPlayerFromID(int playerID)
        {
            PhotonPlayer result = PhotonNetwork.playerList?.FirstOrDefault(player => player.ID == playerID);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (default(PhotonPlayer) == result)
            {
                string warningMsg = string.Format("[{0}] {1} - could not find photon player with ID ({2})",
                                                  typeof(Utils).Name,
                                                  MethodBase.GetCurrentMethod().Name,
                                                  playerID);
                Debug.LogWarning(warningMsg);
            }
#endif
            return result;
        }
    }
}
