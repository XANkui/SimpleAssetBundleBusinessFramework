using UnityEngine;

namespace AssetBundleBusinessFramework.Tools {

    /// <summary>
    /// MonoBehaviour 单例类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoSingleton<T> : MonoBehaviour where T :MonoBehaviour
	{
		private static T m_Instance;

		public static T Instance { get => m_Instance; }

        protected virtual void Awake()
        {
            if (m_Instance == null)
            {
                m_Instance = this as T;
            }
            else {
                Debug.LogError($"{this.GetType()} has more than one , Please check !");
            }
        }
    }
}
