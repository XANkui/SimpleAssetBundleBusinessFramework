using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleBusinessFramework.Tools { 

	/// <summary>
	/// 简单单例
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Singleton<T> where T : new() 
	{
		private static T m_Instance;
		public static T Instance {
			get {
                if (m_Instance==null)
                {
					m_Instance = new T();
                }

				return m_Instance;
			}
		}
	}
}
