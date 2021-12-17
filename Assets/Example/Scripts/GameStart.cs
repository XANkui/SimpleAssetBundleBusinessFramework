using AssetBundleBusinessFramework.UI;
using AssetBundleBusinessFramework.UI.Test;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
            UIManager.Instance.Init(transform.Find("UIRoot") as RectTransform,
                transform.Find("UIRoot/WndRoot") as RectTransform,
                transform.Find("UIRoot/UICamera").GetComponent<Camera>(),
                transform.Find("UIRoot/EventSystem").GetComponent<EventSystem>());
            RegisterUI();

            UIManager.Instance.PopUpWindow("MenuPanel.prefab");


        }

        void RegisterUI() {
            UIManager.Instance.Register<MenuUI>("MenuPanel.prefab");
        }
       
    }
}
