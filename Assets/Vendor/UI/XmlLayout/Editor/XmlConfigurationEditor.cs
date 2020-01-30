using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;

namespace UI.Xml.Configuration
{
    [CustomEditor(typeof(XmlLayoutConfiguration))]
    public class XmlConfigurationEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var config = target as XmlLayoutConfiguration;

            var comprehensiveAssemblySearch = config.ComprehensiveCustomElementAndAttributeCheck;

            base.OnInspectorGUI();

            // if we've changed this, we need to update symbols/etc.
            if (comprehensiveAssemblySearch != config.ComprehensiveCustomElementAndAttributeCheck)
            {
                XmlLayoutAssemblyHelper.ClearAssemblyNames();
                XmlLayoutPluginProcessor.ProcessInstalledPlugins();
                XmlLayoutSchemaProcessor.ProcessXmlSchema();
            }

#if UNITY_2018_1_OR_NEWER
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

            if (GUILayout.Button("Generate Assembly Definition Files"))
            {
                XmlLayoutPluginProcessor.GenerateAssemblyDefinitionFiles();
            }

            if (GUILayout.Button("Delete Assembly Definition Files"))
            {
                if (config.ManageAssemblyDefinitionsAutomatically)
                {
                    Debug.LogWarning("[XmlLayout] Please note: as 'Manage Assembly Definitions Automatically' is enabled, XmlLayout will automatically recreate assembly definition files immediately.");
                }

                XmlLayoutPluginProcessor.DeleteAssemblyDefinitionFiles();
            }
#endif

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (GUILayout.Button("Regenerate XSD file Now"))
            {
                XmlLayoutSchemaProcessor.ProcessXmlSchema(true);
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (GUILayout.Button("Edit XSD file in Visual Studio"))
            {
                AssetDatabase.OpenAsset(((XmlLayoutConfiguration)target).XSDFile);
            }

            if (GUILayout.Button("Edit XSD file in MonoDevelop"))
            {
                UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(AssetDatabase.GetAssetPath(((XmlLayoutConfiguration)target).XSDFile), 0);
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

#if MVVM_ENABLED
            if (GUILayout.Button("Disable MVVM Functionality"))
            {
                XmlLayoutEditorUtilities.RemoveSymbol("MVVM_ENABLED");
            }
#else

#if UNITY_2019_1_OR_NEWER
            if (PlayerSettings.GetApiCompatibilityLevel(EditorUserBuildSettings.selectedBuildTargetGroup) != ApiCompatibilityLevel.NET_4_6)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Enable MVVM Functionality (Requires .NET 3.5 or greater)");
                EditorGUI.EndDisabledGroup();
            }
            else
#endif
            if (GUILayout.Button("Enable MVVM Functionality"))
            {
                XmlLayoutEditorUtilities.AddSymbol("MVVM_ENABLED");
            }
#endif

#if MVVM_DISABLED
            XmlLayoutEditorUtilities.RemoveSymbol("MVVM_DISABLED");
#endif
        }
    }
}
