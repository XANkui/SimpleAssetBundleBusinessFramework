using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleBusinessFramework.Example.Test { 

	public class TestAudioSyncLoad : MonoBehaviour
	{
        public AudioSource AudioSourceObj;
        private AudioClip clip;
        private void Awake()
        {
            AssetBundleManager.Instance.LoadAssetBundleConfig();
        }

        // Start is called before the first frame update
        void Start()
        {
            clip = ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameData/Sounds/senlin.mp3");
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
        }

        
    }
}
