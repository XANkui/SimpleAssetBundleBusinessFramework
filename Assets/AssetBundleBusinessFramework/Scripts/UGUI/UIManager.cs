using AssetBundleBusinessFramework.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AssetBundleBusinessFramework.UI {

    /// <summary>
    /// UI 窗口之间的信息传递类型
    /// </summary>
    public enum UIMsgID { 
        
    }

	public class UIManager : Singleton<UIManager>
	{
        // UI 根节点
		public RectTransform UIRoot;
        // Wnd 根节点
		private RectTransform m_WndRoot;
		private Camera m_UICamera;
		private EventSystem m_EventSystem;
		private float m_CavansRate = 0;

        private const string UI_PREFABS_PATH = "Assets/GameData/Prefabs/UGUI/Panel/";

        // 所有打开的窗口
        private Dictionary<string, Window> m_WindowDIc = new Dictionary<string, Window>();
        // 注册窗口类型字典
        private Dictionary<string, System.Type> m_RegisterDic = new Dictionary<string, System.Type>();
        // 打开窗口列表
        private List<Window> m_WindList = new List<Window>();
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="uIRoot"></param>
        /// <param name="wndRoot"></param>
        /// <param name="uICamera"></param>
        /// <param name="eventSystem"></param>
        public void Init(RectTransform uIRoot, RectTransform wndRoot, Camera uICamera, EventSystem eventSystem)
        {
            UIRoot = uIRoot;
            m_WndRoot = wndRoot;
            m_UICamera = uICamera;
            m_EventSystem = eventSystem;
            m_CavansRate = Screen.height / (m_UICamera.orthographicSize *2);
        }

        /// <summary>
        /// 窗口类型注册方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        public void Register<T>(string name) where T : Window {
            m_RegisterDic[name] = typeof(T);
        }

        /// <summary>
        /// 显示或者隐藏所有 UI
        /// </summary>
        /// <param name="isShow"></param>
        public void ShowOrHideUI(bool isShow) {
            if (UIRoot!=null)
            {
                UIRoot.gameObject.SetActive(isShow);
            }
        }

        /// <summary>
        /// 设置默认选择对象
        /// </summary>
        /// <param name="obj"></param>
        public void SetNormalSelectObj(GameObject obj) {
            if (m_EventSystem==null)
            {
                m_EventSystem = EventSystem.current;

            }
            m_EventSystem.firstSelectedGameObject = obj;
        }

        /// <summary>
        /// 更新各个窗口
        /// </summary>
        public void OnUpdate() {
            for (int i = 0; i < m_WindList.Count; i++)
            {
                if (m_WindList[i]!=null)
                {
                    m_WindList[i].OnUpdate();
                }
            }
        }

        /// <summary>
        /// 发送消息给窗口
        /// </summary>
        /// <param name="name"></param>
        /// <param name="msgID"></param>
        /// <param name="paraList"></param>
        /// <returns></returns>
        public bool SendMessageToWnd(string name, UIMsgID msgID, params object[] paraList) {
            Window wnd = FindWndByName<Window>(name);
            if (wnd!=null)
            {
                return wnd.OnMessage(msgID,paraList);
            }

            return false;
        }

        /// <summary>
        /// 根据窗口名查找窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T FindWndByName<T>(string name) where T :Window {
            Window wnd = null;
            if (m_WindowDIc.TryGetValue(name,out wnd))
            {
                return (T)wnd;
            }

            return null;
        }

        public Window PopUpWindow(string wndName,bool isTop =true, params object[] paraList ) {
            Window wnd = FindWndByName<Window>(wndName);
            if (wnd == null)
            {
                System.Type tp = null;
                if (m_RegisterDic.TryGetValue(wndName, out tp))
                {
                    wnd = System.Activator.CreateInstance(tp) as Window;
                }
                else
                {
                    Debug.LogError($"找不到窗口对应的脚本，窗口名是: " + wndName);
                }

                GameObject wndObj = ObjectManager.Instance.InstantiateObject(UI_PREFABS_PATH + wndName, false, false);
                if (wndObj == null)
                {
                    Debug.Log("创建窗口 Prefab 失败:" + wndName);
                    return null;
                }

                if (m_WindowDIc.ContainsKey(wndName) == false)
                {
                    m_WindList.Add(wnd);
                    m_WindowDIc.Add(wndName, wnd);
                }

                wnd.GameObject = wndObj;
                wnd.Transform = wndObj.transform;
                wnd.Name = wndName;
                wnd.Awake(paraList);
                wndObj.transform.SetParent(m_WndRoot, false);

                if (isTop == true)
                {
                    wndObj.transform.SetAsLastSibling();
                }

                wnd.OnShow(paraList);
            }
            else {
                ShowWnd(wndName,isTop,paraList);
            }

            return wnd;
        }

        /// <summary>
        /// 根据窗口名字关闭
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isDestroy"></param>
        public void CloseWnd(string name, bool isDestroy=false) {
            Window wnd = FindWndByName<Window>(name);
            CloseWnd(wnd, isDestroy);
        }

        /// <summary>
        /// 根据窗口对象关闭
        /// </summary>
        /// <param name="wnd"></param>
        /// <param name="isDestroy"></param>
        public void CloseWnd(Window wnd, bool isDestroy = false)
        {
            if (wnd!=null)
            {
                wnd.OnDisable();
                wnd.OnClose();
                if (m_WindowDIc.ContainsKey(wnd.Name))
                {
                    m_WindowDIc.Remove(wnd.Name);
                    m_WindList.Remove(wnd);
                }

                if (isDestroy == true)
                {
                    ObjectManager.Instance.ReleaseObject(wnd.GameObject, 0, true);
                }
                else { 
                    ObjectManager.Instance.ReleaseObject(wnd.GameObject, recycleParent:false);

                }

                wnd.GameObject = null;
                wnd=null;
            }
        }

        /// <summary>
        /// 关闭所有窗口
        /// </summary>
        public void CloseAllWnd() {
            for (int i = m_WindList.Count; i >= 0; i++)
            {
                CloseWnd(m_WindList[i]);
            }
        }

        /// <summary>
        /// 切换到唯一窗口
        /// </summary>
        /// <param name="wndName"></param>
        /// <param name="isTop"></param>
        /// <param name="paraList"></param>
        public void SwitchStateByName(string wndName, bool isTop = true, params object[] paraList)
        {
            CloseAllWnd();
            PopUpWindow(wndName,isTop,paraList);
        }

        /// <summary>
        /// 根据名字隐藏窗口
        /// </summary>
        /// <param name="name"></param>
        public void HideWnd(string name) {
            Window wnd = FindWndByName<Window>(name);
            HideWnd(wnd);
        }

        /// <summary>
        /// 根据窗口对象隐藏窗口
        /// </summary>
        /// <param name="wnd"></param>
        public void HideWnd(Window wnd) {
            if (wnd!=null)
            {
                wnd.GameObject.SetActive(false);
                wnd.OnDisable();
            }
        }

        /// <summary>
        /// 根据窗口名显示
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isTop"></param>
        /// <param name="paraList"></param>
        public void ShowWnd(string name,bool isTop=true, params object[] paraList) {
            Window wnd = FindWndByName<Window>(name);
            ShowWnd(wnd, isTop,paraList);
        }

        /// <summary>
        /// 根据窗口 Window 显示
        /// </summary>
        /// <param name="wnd"></param>
        /// <param name="isTop"></param>
        /// <param name="paraList"></param>
        public void ShowWnd(Window wnd, bool isTop = true, params object[] paraList)
        {
            if (wnd!=null)
            {
                if (wnd.GameObject!=null && wnd.GameObject.activeSelf==false)
                {
                    wnd.GameObject.SetActive(true);
                }

                if (isTop == true)
                {
                    wnd.Transform.SetAsLastSibling();
                }

                wnd.OnShow(paraList);
            }
        }
    }
}
