using AssetBundleBusinessFramework.Common;
using AssetBundleBusinessFramework.Scenes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleBusinessFramework.UI.Test { 

	public class LoadingUI : Window
	{
		private LoadingPanel m_LoadingPanel;
		private string m_SceneName;

        public override void Awake(params object[] paraList)
        {
            m_LoadingPanel = GameObject.GetComponent<LoadingPanel>();
            m_SceneName = (string)paraList[0];
        }

        public override void OnUpdate()
        {
            if (m_LoadingPanel==null)
            {
                return;
            }

            m_LoadingPanel.ProgressSlider.value = GameScenesManager.LoadingProgress / 100.0f;
            m_LoadingPanel.ProgressText.text = $"{GameScenesManager.LoadingProgress}%";
            if (GameScenesManager.LoadingProgress>=100)
            {
                LoadOtherScene();
            }
        }

        /// <summary>
        /// 加载对应场景第一个 UI
        /// </summary>
        private void LoadOtherScene()
        {
            // 根据场景名字打开对应场景的第一个界面
            if (m_SceneName==ConStr.MENU_SCENE)
            {
                UIManager.Instance.PopUpWindow(ConStr.MENU_PANEL);
            }

            UIManager.Instance.CloseWnd(ConStr.LOADING_PANEL);
        }
    }
}
