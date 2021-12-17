using AssetBundleBusinessFramework.Common;
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
