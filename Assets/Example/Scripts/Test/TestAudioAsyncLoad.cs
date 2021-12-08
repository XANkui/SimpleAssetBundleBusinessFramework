using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleBusinessFramework.Example.Test
{ 

	public class TestAudioAsyncLoad : MonoBehaviour
	{
        public AudioSource AudioSourceObj;
        private AudioClip clip;
        private void Awake()
        {
            AssetBundleManager.Instance.LoadAssetBundleConfig();
            ResourceManager.Instance.Init(this);
        }

        // Start is called before the first frame update
        void Start()
        {
            long startTime = System.DateTime.Now.Ticks;
            ResourceManager.Instance.AsyncLoadResource("Assets/GameData/Sounds/menusound.mp3", OnLoadFinished,LoadResPriority.RES_MIDDLE);
            Debug.Log($"没有预载的异步加载耗时 {System.DateTime.Now.Ticks - startTime}");
            // 预加载测试
            ResourceManager.Instance.PreLoad("Assets/GameData/Sounds/senlin.mp3");

        }

        private void OnLoadFinished(string path, UnityEngine.Object obj, object param1, object param2, object param3)
        {
            clip = obj as AudioClip;
            AudioSourceObj.clip = clip;
            AudioSourceObj.Play();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                AudioSourceObj.Stop();
                AudioSourceObj.clip = null;
                ResourceManager.Instance.ReleaseResource(clip, true);
                clip = null;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                long startTime = System.DateTime.Now.Ticks;
                AudioSourceObj.clip = ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameData/Sounds/senlin.mp3");
                Debug.Log($"有预载的加载耗时 {System.DateTime.Now.Ticks - startTime}");
                AudioSourceObj.Play();
                
            }
        }

        private void OnApplicationQuit()
        {
# if UNITY_EDITOR
            ResourceManager.Instance.ClearCache();
            Resources.UnloadUnusedAssets();
            Debug.Log(" Clear Assets ");
#endif
        }
    }
}
