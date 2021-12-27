using AssetBundleBusinessFramework.Common;
using AssetBundleBusinessFramework.Scenes;
using AssetBundleBusinessFramework.Tools;
using AssetBundleBusinessFramework.UI;
using AssetBundleBusinessFramework.UI.Test;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AssetBundleBusinessFramework.Example { 

	public class GameStart : MonoSingleton<GameStart>
    {
        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);
			AssetBundleManager.Instance.LoadAssetBundleConfig();
            ResourceManager.Instance.Init(this);
            ObjectManager.Instance.Init(transform.Find("RecyclePoolTrans"), transform.Find("SceneTrans"));
        }

        // Start is called before the first frame update
        void Start()
		{
            LoadConfiger();

            UIManager.Instance.Init(transform.Find("UIRoot") as RectTransform,
                transform.Find("UIRoot/WndRoot") as RectTransform,
                transform.Find("UIRoot/UICamera").GetComponent<Camera>(),
                transform.Find("UIRoot/EventSystem").GetComponent<EventSystem>());
            RegisterUI();

            GameScenesManager.Instance.Init(this);

            // 预加载 对象
            ObjectManager.Instance.PreloadGameObject(ConStr.PREFAB_ATTACK_PATH,5);


            GameScenesManager.Instance.LoadScene(ConStr.MENU_SCENE);

        }

        void RegisterUI() {
            UIManager.Instance.Register<MenuUI>(ConStr.MENU_PANEL);
            UIManager.Instance.Register<LoadingUI>(ConStr.LOADING_PANEL);
        }

        void LoadConfiger() {
            ConfigManager.Instance.LoadData<MonsterData>(CFG.TABLE_MONSTER);
            ConfigManager.Instance.LoadData<BuffData>(CFG.TABLE_BUFF);
        }

        private void Update()
        {
            UIManager.Instance.OnUpdate();
  
        }

    }
}
