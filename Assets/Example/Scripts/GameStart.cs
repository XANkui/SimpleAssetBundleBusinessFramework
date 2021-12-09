using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleBusinessFramework.Example { 

	public class GameStart : MonoBehaviour
	{
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
			AssetBundleManager.Instance.LoadAssetBundleConfig();
            ResourceManager.Instance.Init(this);
            ObjectManager.Instance.Init(transform.Find("RecyclePoolTrans"), transform.Find("SceneTrans"));
        }

        // Start is called before the first frame update
        void Start()
		{
			
		}

       
    }
}
