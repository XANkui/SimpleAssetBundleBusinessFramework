using AssetBundleBusinessFramework.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleBusinessFramework { 

    /// <summary>
    /// 对象池管理类
    /// </summary>
	public class ObjectManager : Singleton<ObjectManager>
	{
        #region 类对象池的使用

        protected Dictionary<Type, object> m_ClassPoolDict = new Dictionary<Type, object>();

        /// <summary>
        /// 创建或者获取类对象池
        /// 使用 类对象池中的 Spawn 取得对象，Recycle 回收对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public ClassObjectPool<T> GetOrCreateClassPool<T>(int maxCount) where T : class, new() {
            Type type = typeof(T);
            object outObj = null;
            // 字典中不存在，或者是为空
            if (m_ClassPoolDict.TryGetValue(type,out outObj)==false || outObj==null)
            {
                ClassObjectPool<T> newPool = new ClassObjectPool<T>(maxCount);
                m_ClassPoolDict.Add(type,newPool);

                return newPool;
            }

            return outObj as ClassObjectPool<T>;
        }

        #endregion
    }
}
