using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetBundleBusinessFramework
{ 
	[CreateAssetMenu(fileName ="DataConfig",menuName ="CreateDataConfig",order =0)]
	public class DataConfig : ScriptableObject
	{
		// 打包时生成AB包配置表的二进制路径
		public string ABBytePath;

		// 打包时默认包名
		public string APPName;

		// xml 文件夹路径
		public string XmlPath ;
		// 二进制文件夹路径
		public string BinaryPath ;
		// 脚本文件夹路径
		public string ScriptsPath ;
	}

	/// <summary>
	/// DataConfig 自定义 Inspector 显示面板
	/// </summary>
	[CustomEditor(typeof(DataConfig))]
	public class DataConfigInspectot : Editor {
		// 打包时生成AB包配置表的二进制路径
		public SerializedProperty ABBytePath;

		// 打包时默认包名
		public SerializedProperty APPName;

		// xml 文件夹路径
		public SerializedProperty XmlPath;
		// 二进制文件夹路径
		public SerializedProperty BinaryPath;
		// 脚本文件夹路径
		public SerializedProperty ScriptsPath;

        private void OnEnable()
        {
			ABBytePath = serializedObject.FindProperty("ABBytePath");
			APPName = serializedObject.FindProperty("APPName");
			XmlPath = serializedObject.FindProperty("XmlPath");
			BinaryPath = serializedObject.FindProperty("BinaryPath");
			ScriptsPath = serializedObject.FindProperty("ScriptsPath");
        }

        public override void OnInspectorGUI()
        {
			serializedObject.Update();
			EditorGUILayout.PropertyField(ABBytePath,new GUIContent("打包时生成AB包配置表的二进制路径"));
			GUILayout.Space(5);
			EditorGUILayout.PropertyField(APPName, new GUIContent("打包时默认包名"));
			GUILayout.Space(5);
			EditorGUILayout.PropertyField(XmlPath, new GUIContent("xml 文件夹路径"));
			GUILayout.Space(5);
			EditorGUILayout.PropertyField(BinaryPath, new GUIContent("二进制文件夹路径"));
			GUILayout.Space(5);
			EditorGUILayout.PropertyField(ScriptsPath, new GUIContent("脚本文件夹路径"));
			GUILayout.Space(5);
			serializedObject.ApplyModifiedProperties();
		}
    }

	public class ReadDataConfig {
		private const string DATA_CONFIG_PATH = "Assets/AssetBundleBusinessFramework/Editor/Config/DataConfig.asset";
		public static DataConfig GetDataConfig() {
			DataConfig dataConfig = AssetDatabase.LoadAssetAtPath<DataConfig>(DATA_CONFIG_PATH);
			return dataConfig;
		}
	}
}
