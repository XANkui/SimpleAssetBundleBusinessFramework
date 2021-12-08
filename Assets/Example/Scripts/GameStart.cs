using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleBusinessFramework.Example { 

	public class GameStart : MonoBehaviour
	{
        private void Awake()
        {
			AssetBundleManager.Instance.LoadAssetBundleConfig();
        }

        // Start is called before the first frame update
        void Start()
		{
			
		}

       
    }
}
