﻿using AssetBundleBusinessFramework.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleBusinessFramework.UI.Test { 

	public class MenuUI : Window
	{
		private MenuPanel m_MenuPanel;
        public override void Awake(params object[] paraList)
        {
            m_MenuPanel = GameObject.GetComponent<MenuPanel>();
            AddButtonClickListener(m_MenuPanel.StartButton,OnClickStart);
            AddButtonClickListener(m_MenuPanel.LoadButton,OnClickLoad);
            AddButtonClickListener(m_MenuPanel.ExitButton,OnClickExit);

            // 测试异步加载图片
            ResourceManager.Instance.AsyncLoadResource("Assets/GameData/UGUI/bird.jpg",OnLoadSpriteTest1,LoadResPriority.RES_SLOW,true);
            ResourceManager.Instance.AsyncLoadResource("Assets/GameData/UGUI/dog.png",OnLoadSpriteTest2,LoadResPriority.RES_HIGHT, true);
        }

        private void OnLoadSpriteTest2(string path, UnityEngine.Object obj, object param1, object param2, object param3)
        {
            if (obj!=null)
            {
                Sprite sp = obj as Sprite;
                m_MenuPanel.Test2Image.sprite = sp;
                Debug.Log("Test12mage");
            }
        }

        private void OnLoadSpriteTest1(string path, UnityEngine.Object obj, object param1, object param2, object param3)
        {
            if (obj != null)
            {
                Sprite sp = obj as Sprite;
                m_MenuPanel.Test1Image.sprite = sp;
                Debug.Log("Test1Image");
            }
        }

        void OnClickStart() {
            Debug.Log("OnClickStart");
        }
        void OnClickLoad() {
            Debug.Log("OnClickLoad");
        }
        void OnClickExit() {

            Debug.Log("OnClickExit");
        }
    }
}
