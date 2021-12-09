using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleBusinessFramework.Example.Test
{ 

	public class TestAudioSyncObjectLoad : MonoBehaviour
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
			m_Obj = ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab",true);
		}

		// Update is called once per frame
		void Update()
		{
            if (Input.GetKeyDown(KeyCode.A))
            {
				ObjectManager.Instance.ReleaseObject(m_Obj);
				m_Obj = null;

			}else if (Input.GetKeyDown(KeyCode.D))
			{
				m_Obj = ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab", true);

			}else if (Input.GetKeyDown(KeyCode.S))
			{
				ObjectManager.Instance.ReleaseObject(m_Obj,0,true);
				m_Obj = null;

			}
		}
	}
}
