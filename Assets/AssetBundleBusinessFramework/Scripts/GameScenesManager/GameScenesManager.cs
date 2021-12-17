using AssetBundleBusinessFramework.Common;
using AssetBundleBusinessFramework.Tools;
using AssetBundleBusinessFramework.UI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AssetBundleBusinessFramework.Scenes { 

	public class GameScenesManager : Singleton<GameScenesManager>
	{
		// 加载场景完成回调
		public Action LoadSceneOverCallback;
		// 加载场景开始回调
		public Action LoadSceneEnterCallback;

		public string CurrentSceneName { get; set; } // 场景名称
		public bool AlreadyLoadScene { get; set; }	// 场景是否加载完成
		public static int LoadingProgress = 0;		// 切换场景进度条
		private MonoBehaviour m_Mono;

		public void Init(MonoBehaviour mono) {
			m_Mono = mono;
		}

		public void LoadScene(string name) {
			LoadingProgress = 0;
			m_Mono.StartCoroutine(LoadSceneAsync(name));
			UIManager.Instance.PopUpWindow(ConStr.LOADING_PANEL,true,name);
		}

		/// <summary>
		/// 设置场景环境
		/// </summary>
		/// <param name="name"></param>
		void SetSceneSetting(string name) { 
			// 设置各种场景环境，可以格局配置表来，TODO
		}

		/// <summary>
		/// 协程异步加载场景
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		IEnumerator LoadSceneAsync(string name) {
            if (LoadSceneEnterCallback!=null)
            {
				LoadSceneEnterCallback.Invoke();

			}
			ClearCache();
			AlreadyLoadScene = false;
			// 过渡场景
			AsyncOperation unLoadScene = SceneManager.LoadSceneAsync(ConStr.EMPTY_SCENE, LoadSceneMode.Single);
            while (unLoadScene !=null && unLoadScene.isDone==false)
            {
				yield return new WaitForEndOfFrame();
            }

			LoadingProgress = 0;
			int targetProgress = 0;
			AsyncOperation asyncScene = SceneManager.LoadSceneAsync(name);
            if (asyncScene!=null && asyncScene.isDone==false)
            {
				asyncScene.allowSceneActivation = false;
				while (asyncScene.progress <0.9f) {
					targetProgress = (int)(asyncScene.progress * 100);
					yield return new WaitForEndOfFrame();
                    // 平滑过渡
                    while (LoadingProgress < targetProgress)
                    {
						++LoadingProgress;
						yield return new WaitForEndOfFrame();
                    }
				}

				CurrentSceneName = name;
				SetSceneSetting(name);
				// 自行加载剩余的 10%
				targetProgress = 100;
                while (LoadingProgress< targetProgress -2) // 平滑加载需要而已
                {
					++LoadingProgress;
					yield return new WaitForEndOfFrame();
                }
				LoadingProgress = 100;
				asyncScene.allowSceneActivation = true;
				AlreadyLoadScene = true;

				if (LoadSceneEnterCallback!=null)
                {
					LoadSceneEnterCallback.Invoke();

				}
            }

		}

		/// <summary>
		/// 跳转场景需要清除的东西
		/// </summary>
		private void ClearCache() {
			ObjectManager.Instance.ClearCache();
			ResourceManager.Instance.ClearCache();
		}
	}
}
