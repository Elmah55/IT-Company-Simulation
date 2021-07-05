using UnityEngine;
using UnityEditor;


namespace ITCompanySimulation.UI
{
    /// <summary>
    /// Main class for creating custom menu items.
    /// </summary>
    public sealed class ITCompanySimulationEditorWindow : EditorWindow
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        /*Public methods*/

        [MenuItem("Window/ITCompanySimulation/Settings", false, 1)]
        private static void OnMenuItemSettingsSelected()
        {
            Object settingsObject = Resources.Load("Settings");
            Selection.objects = new Object[] { settingsObject };
            EditorGUIUtility.PingObject(settingsObject);
        }
    } 
}
