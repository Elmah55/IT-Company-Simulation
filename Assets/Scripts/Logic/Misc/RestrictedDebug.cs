using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ITCompanySimulation.Utilities
{
    /// <summary>
    /// Logging helper utility to prevent debug prints in release builds.
    /// </summary>
    public static class RestrictedDebug
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        /*Public methods*/

        /// <summary>
        /// Prints debug message to console.
        /// </summary>
        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void Log(string message, LogType type = LogType.Log)
        {
            //Get calling object info
            StackFrame callingMethodFrame = new StackFrame(1);
            Type callingObjectType = callingMethodFrame.GetMethod().DeclaringType;
            //Append source class name to every debug print
            message = string.Format("[{0}] {1}", callingObjectType.Name, message);

            switch (type)
            {
                case LogType.Error:
                    Debug.LogError(message);
                    break;
                case LogType.Assert:
                    Debug.LogAssertion(message);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogType.Log:
                    Debug.Log(message);
                    break;
                default:
                    Debug.LogError("Unsupported log type: " + type.ToString());
                    break;
            }
        }
    }
}
