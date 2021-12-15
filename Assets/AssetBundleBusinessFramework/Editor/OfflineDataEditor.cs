using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AssetBundleBusinessFramework { 

	/// <summary>
	/// 生成离线数据编辑器类
	/// </summary>
	public class OfflineDataEditor 
	{
		[MenuItem("Assets/生成离线数据")]
		public static void AssetCreateOfflineData() {
			GameObject[] objects = Selection.gameObjects;
            for (int i = 0; i < objects.Length; i++)
            {
				EditorUtility.DisplayProgressBar("添加离线数据", "正在修改："+objects[i] +"……", 1.0f/objects.Length);
				CreateOfflineData(objects[i]);
			}

			EditorUtility.ClearProgressBar();
		}

		/// <summary>
		/// 给指定预制体创建离线数据
		/// </summary>
		/// <param name="obj"></param>
		private static void CreateOfflineData(GameObject obj) {
			OfflineData offlineData = obj.GetComponent<OfflineData>();
            if (offlineData==null)
            {
				offlineData = obj.AddComponent<OfflineData>();
            }

			offlineData.BindData();
			EditorUtility.SetDirty(obj);
			Debug.Log($"修改了 {obj.name} prefab");
			Resources.UnloadUnusedAssets();
			AssetDatabase.Refresh();
		}

		[MenuItem("Assets/生成UI离线数据")]
		public static void AssetCreateUIOfflineData()
		{
			GameObject[] objects = Selection.gameObjects;
			for (int i = 0; i < objects.Length; i++)
			{
				EditorUtility.DisplayProgressBar("添加UI离线数据", "正在修改：" + objects[i] + "……", 1.0f / objects.Length);
				CreateUIOfflineData(objects[i]);
			}

			EditorUtility.ClearProgressBar();
		}

		[MenuItem("离线数据/生成所有 UI prefab 离线数据 (path)")]
		public static void AllAssetCreateUIOfflineData() {
			string[] allStr = AssetDatabase.FindAssets("t:Prefab",new string[] { "Assets/GameData/Prefabs/UGUI"});
            for (int i = 0; i < allStr.Length; i++)
            {
				string prefabPath = AssetDatabase.GUIDToAssetPath(allStr[i]);
				EditorUtility.DisplayProgressBar("添加UI离线数据", "正在扫描路径：" + prefabPath + "……", 1.0f / allStr.Length);
				GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
				if (obj == null)
                {
					continue;
                }

				CreateUIOfflineData(obj);
			}
			Debug.Log("UI 离线数据全部生成完毕！");
			EditorUtility.ClearProgressBar();
		}


		/// <summary>
		/// 给指定UI预制体创建离线数据
		/// </summary>
		/// <param name="obj"></param>
		private static void CreateUIOfflineData(GameObject obj)
		{
			obj.layer = LayerMask.NameToLayer("UI");
			UIOfflineData uiOfflineData = obj.GetComponent<UIOfflineData>();
			if (uiOfflineData == null)
			{
				uiOfflineData = obj.AddComponent<UIOfflineData>();
			}

			uiOfflineData.BindData();
			EditorUtility.SetDirty(obj);
			Debug.Log($"修改了 {obj.name} UI prefab");
			Resources.UnloadUnusedAssets();
			AssetDatabase.Refresh();
		}


		[MenuItem("Assets/生成Effect离线数据")]
		public static void AssetCreateEffectOfflineData()
		{
			GameObject[] objects = Selection.gameObjects;
			for (int i = 0; i < objects.Length; i++)
			{
				EditorUtility.DisplayProgressBar("添加Effect离线数据", "正在修改：" + objects[i] + "……", 1.0f / objects.Length);
				CreateEffectOfflineData(objects[i]);
			}

			EditorUtility.ClearProgressBar();
		}

		[MenuItem("离线数据/生成所有 Effect prefab 离线数据 (path)")]
		public static void AllAssetCreateEffectOfflineData()
		{
			string[] allStr = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/GameData/Prefabs/Effect" });
			for (int i = 0; i < allStr.Length; i++)
			{
				string prefabPath = AssetDatabase.GUIDToAssetPath(allStr[i]);
				EditorUtility.DisplayProgressBar("添加Effect离线数据", "正在扫描路径：" + prefabPath + "……", 1.0f / allStr.Length);
				GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
				if (obj == null)
				{
					continue;
				}

				CreateEffectOfflineData(obj);
			}
			Debug.Log("Effect 离线数据全部生成完毕！");
			EditorUtility.ClearProgressBar();
		}

		/// <summary>
		/// 给指定UI预制体创建离线数据
		/// </summary>
		/// <param name="obj"></param>
		private static void CreateEffectOfflineData(GameObject obj)
		{
			EffectOfflineData effectOfflineData = obj.GetComponent<EffectOfflineData>();
			if (effectOfflineData == null)
			{
				effectOfflineData = obj.AddComponent<EffectOfflineData>();
			}

			effectOfflineData.BindData();
			EditorUtility.SetDirty(obj);
			Debug.Log($"修改了 {obj.name} Effect prefab");
			Resources.UnloadUnusedAssets();
			AssetDatabase.Refresh();
		}
	}

}

