using AssetBundleBusinessFramework.Tools;
using System.Collections;
using UnityEngine;

namespace AssetBundleBusinessFramework.Scenes { 

	public class GameScenesManager : Singleton<GameScenesManager>
	{
		public string CurrentSceneName { get; set; }
		public static int LoadingProgress = 0;
		private MonoBehaviour m_Mono;

		public void Init(MonoBehaviour mono) {
			m_Mono = mono;
		}

		public void LoadScene(string name) {
			LoadingProgress = 0;
			m_Mono.StartCoroutine(LoadSceneAsync(name));
		}

		IEnumerator LoadSceneAsync(string name) {
			yield return null;
		}
	}
}
