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
	}
}
