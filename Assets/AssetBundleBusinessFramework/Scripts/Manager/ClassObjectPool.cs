using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleBusinessFramework { 

	/// <summary>
	/// 类对象池类
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ClassObjectPool<T> where T : class,new()
	{
		// 对象池
		protected Stack<T> m_Pool = new Stack<T>();

		// 最大对象个数 ，<=0 则为不限制个数
		protected int m_MaxCount = 0;
		// 没有回收的对象个数
		protected int m_NoRecycleCount = 0;

		public ClassObjectPool(int maxCount) {
			m_MaxCount = maxCount;

			// 预载
            for (int i = 0; i < maxCount; i++)
            {
				m_Pool.Push(new T());
            }
		}

		/// <summary>
		/// 对象池中取对象
		/// </summary>
		/// <param name="createIfPoolEmpty">如果取不到，是否创建</param>
		/// <returns>返回对象</returns>
		public T Spawn(bool createIfPoolEmpty) {

			if (m_Pool.Count > 0)
			{
				T rtn = m_Pool.Pop();
				if (rtn == null)
				{
					// 创建
					if (createIfPoolEmpty)
					{
						rtn = new T();
					}
				}

				m_NoRecycleCount++;
				return rtn;
			}
			else {
				// 创建
				if (createIfPoolEmpty)
				{
					T rtn = new T();
					m_NoRecycleCount++;
					return rtn;
				}
			}

			return null;
		}

		/// <summary>
		/// 回收
		/// </summary>
		/// <param name="obj">对象</param>
		/// <returns>返回是否回收成功</returns>
		public bool Recycle(T obj) {
			// 对象为空
            if (obj == null)
            {
				return false;
            }

			m_NoRecycleCount--;

			// 对象回收已满
            if (m_Pool.Count>=m_MaxCount && m_MaxCount >0)
            {
				obj = null;
				return false;
            }

			m_Pool.Push(obj);
			return true;
		}
	}
}
