using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleBusinessFramework.Example.Test
{ 

	public class TestPrefabAsyncObjectLoad : MonoBehaviour
	{
		private GameObject m_Obj;
		private void Awake()
		{
			AssetBundleManager.Instance.LoadAssetBundleConfig();
			ResourceManager.Instance.Init(this);
			ObjectManager.Instance.Init(transform.Find("RecyclePoolTrans"), transform.Find("SceneTrans"));

		}
		// Start is called before the first frame update
		void Start()
		{
			ObjectManager.Instance.InstantiateObjectAsync("Assets/GameData/Prefabs/Attack.prefab", OnLoadFinish, LoadResPriority.RES_HIGHT, true);

			ObjectManager.Instance.PreloadGameObject("Assets/GameData/Prefabs/Attack.prefab",5); // 预加载演示
		}

		void OnLoadFinish(string path, Object obj,
		object param1 = null, object param2 = null, object param3 = null) {
			Debug.Log("OnLoadFinish == ");
			m_Obj = obj as GameObject;
		}

		// Update is called once per frame
		void Update()
		{
			if (Input.GetKeyDown(KeyCode.A))
			{
				ObjectManager.Instance.ReleaseObject(m_Obj);
				m_Obj = null;

			}
			else if (Input.GetKeyDown(KeyCode.D))
			{
				m_Obj = ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab", true);

			}
			else if (Input.GetKeyDown(KeyCode.S))
			{
				ObjectManager.Instance.ReleaseObject(m_Obj, 0, true);
				m_Obj = null;

			}
		}
	}
}
