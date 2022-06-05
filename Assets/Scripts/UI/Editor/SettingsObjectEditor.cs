using UnityEditor;
using ITCompanySimulation.Settings;
using UnityEngine;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// Custom editor for SettingsObject
    /// </summary>
    [CustomEditor(typeof(SettingsObject))]
    public class SettingsObjectEditor : Editor
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        /*Public methods*/

        public override void OnInspectorGUI()
        {
            SettingsObject inspectedSettings = (SettingsObject)target;

            EditorGUILayout.LabelField("Application Settings");
            EditorGUI.indentLevel = 1;
            inspectedSettings.UseRoom = EditorGUILayout.Toggle("Use Room", inspectedSettings.UseRoom);
            inspectedSettings.OfflineMode = EditorGUILayout.Toggle("Use Offline Mode", inspectedSettings.OfflineMode);
            EditorGUI.indentLevel = 0;

            GUILayout.Space(10f);

            EditorGUILayout.LabelField("Simulation Settings");
            EditorGUI.indentLevel = 1;
            inspectedSettings.TargetBalance = EditorGUILayout.IntSlider("Target Balance",
                                                                        inspectedSettings.TargetBalance,
                                                                        SimulationSettings.MIN_TARGET_BALANCE,
                                                                        SimulationSettings.MAX_TARGET_BALANCE);
            inspectedSettings.InitialBalance = EditorGUILayout.IntSlider("Initial Balance",
                                                                        inspectedSettings.InitialBalance,
                                                                        SimulationSettings.MIN_INITIAL_BALANCE,
                                                                        SimulationSettings.MAX_INITIAL_BALANCE);
            inspectedSettings.MinimalBalance = EditorGUILayout.IntSlider("Minimal Balance",
                                                                         inspectedSettings.MinimalBalance,
                                                                         SimulationSettings.MIN_MINIMAL_BALANCE,
                                                                         SimulationSettings.MAX_MINIMAL_BALANCE);
            EditorGUI.indentLevel = 0;

            GUILayout.Space(30f);

            //Set default settings values used for release version
            //of build
            if (GUILayout.Button("Set default values"))
            {
                inspectedSettings.UseRoom = true;
                inspectedSettings.OfflineMode = false;
                inspectedSettings.TargetBalance = 500000;
                inspectedSettings.InitialBalance = 100000;
                inspectedSettings.MinimalBalance = 0;
                inspectedSettings.TargetBalance =
                    Mathf.Clamp(inspectedSettings.TargetBalance, SimulationSettings.MIN_TARGET_BALANCE, SimulationSettings.MAX_INITIAL_BALANCE);
                inspectedSettings.MinimalBalance =
                    Mathf.Clamp(inspectedSettings.MinimalBalance, SimulationSettings.MIN_MINIMAL_BALANCE, SimulationSettings.MAX_MINIMAL_BALANCE);
                inspectedSettings.InitialBalance =
                    Mathf.Clamp(inspectedSettings.InitialBalance, SimulationSettings.MIN_INITIAL_BALANCE, SimulationSettings.MAX_INITIAL_BALANCE);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
