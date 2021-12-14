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
        // 对象池节点
        private Transform m_RecyclePoolTrans;
        // 场景节点
        private Transform m_SceneTrans;

        // 对象存放的字典
        protected Dictionary<uint, List<ResourceObj>> m_ObjectPoolDict = new Dictionary<uint, List<ResourceObj>>();
        // 暂存 ResourceObj(int 是 GUID)
        protected Dictionary<int, ResourceObj> m_ResourceObjDict = new Dictionary<int, ResourceObj>();
        // ResourceObj 的类对象池
        protected ClassObjectPool<ResourceObj> m_ResourceObjClassPool = null;
        public void Init(Transform recyclePoolTrans, Transform sceneTrans) {
            m_ResourceObjClassPool = GetOrCreateClassPool<ResourceObj>(1000);
            m_RecyclePoolTrans = recyclePoolTrans;
            m_SceneTrans = sceneTrans;
        }

        /// <summary>
        /// 从对象池中取对象
        /// </summary>
        /// <param name="crc"></param>
        /// <returns></returns>
        protected ResourceObj GetObjectFromPool(uint crc) {
            List<ResourceObj> st = null;
            if (m_ObjectPoolDict.TryGetValue(crc,out st)==true && st !=null && st.Count>0)
            {
                // 引用计数增加
                ResourceManager.Instance.IncreaseResourceRef(crc);

                ResourceObj resObj = st[0];
                st.RemoveAt(0);
                GameObject obj = resObj.CloneObj;
                if (System.Object.ReferenceEquals(obj,null)==false)
                {
                    resObj.Already = false;
#if UNITY_EDITOR
                    if (obj.name.EndsWith("(Recycle)"))
                    {
                        obj.name = obj.name.Replace("(Recycle)", "");
                    }
#endif
                }

                return resObj;
            }

            return null;
        }

        /// <summary>
        /// 预加载实例化资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="count"></param>
        /// <param name="clear"></param>
        public void PreloadGameObject(string path,int count=1,bool clear = false) {
            List<GameObject> tempGameObjectList = new List<GameObject>();
            for (int i = 0; i < count; i++)
            {
                GameObject obj = InstantiateObject(path,false, isClear: clear);
                tempGameObjectList.Add(obj);
            }

            for (int i = 0; i < count; i++)
            {
                GameObject obj = tempGameObjectList[i];
                ReleaseObject(obj); // 释放是为了，不显示，回收后期使用 因为 clear = false
                obj = null;
            }

            tempGameObjectList.Clear();
        }

        /// <summary>
        /// 同步加载对象
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isClear"></param>
        /// <returns></returns>
        public GameObject InstantiateObject(string path,bool setSceneTrans=false,bool isClear = true) {
            uint crc = Crc32.GetCrc32(path);
            ResourceObj resourceObj = GetObjectFromPool(crc);

            if (resourceObj==null)
            {
                resourceObj = m_ResourceObjClassPool.Spawn(true);
                resourceObj.Crc = crc;
                resourceObj.IsClear = isClear;
                // ResourceManager 提供加载方法
                resourceObj = ResourceManager.Instance.LoadResource(path, resourceObj);

                if (resourceObj.ResItem.Obj!=null)
                {
                    resourceObj.CloneObj = GameObject.Instantiate(resourceObj.ResItem.Obj) as GameObject;
                }
            }
            if (setSceneTrans==true)
            {
                resourceObj.CloneObj.transform.SetParent(m_SceneTrans,false);
            }

            int tempID = resourceObj.CloneObj.GetInstanceID();
            if (m_ResourceObjDict.ContainsKey(tempID) ==false)
            {
                m_ResourceObjDict.Add(tempID, resourceObj);
            }

            return resourceObj.CloneObj;
        }

        /// <summary>
        /// 异步加载实例化资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="dealFinish"></param>
        /// <param name="priority"></param>
        /// <param name="setSceneObject"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <param name="param3"></param>
        /// <param name="clear"></param>
        public void InstantiateObjectAsync(string path,OnAsyncObjFinish dealFinish,LoadResPriority priority, bool setSceneObject=false,
            object param1=null,object param2 = null, object param3 = null,bool clear =true) {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            uint crc = Crc32.GetCrc32(path);

            ResourceObj resObj = GetObjectFromPool(crc);
            if (resObj !=null)
            {
                if (setSceneObject)
                {
                    resObj.CloneObj.transform.SetParent(m_SceneTrans,false);
                }

                if (dealFinish!=null)
                {
                    dealFinish(path,resObj.CloneObj,param1,param2,param3);
                }

                return;
            }

            resObj = m_ResourceObjClassPool.Spawn(true);
            resObj.Crc = crc;
            resObj.SetSceneParent = setSceneObject;
            resObj.IsClear = clear;
            resObj.DealFinish = dealFinish;
            resObj.Param1 = param1;
            resObj.Param2 = param2;
            resObj.Param3 = param3;
            // 调用 ResourceManager 的异步加载接口
            ResourceManager.Instance.AsyncLoadResource(path,resObj, OnLoadResourceObjFinish, priority);
        }

        /// <summary>
        /// 资源加载完的回调
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resObj"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <param name="param3"></param>
        void OnLoadResourceObjFinish(string path, ResourceObj resObj, object param1 = null, object param2 = null, object param3 = null) {
            if (resObj==null)
            {
                return;
            }

            if (resObj.ResItem.Obj == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"异步加载资源为空，Path = {path}");
#endif
            }
            else {
                resObj.CloneObj = GameObject.Instantiate(resObj.ResItem.Obj) as GameObject;
            }

            if (resObj.CloneObj !=null && resObj.SetSceneParent)
            {
                resObj.CloneObj.transform.SetParent(m_SceneTrans,false);
            }

            if (resObj.DealFinish!=null)
            {
                int tempID = resObj.CloneObj.GetInstanceID();
                if (m_ResourceObjDict.ContainsKey(tempID)==false)
                {
                    m_ResourceObjDict.Add(tempID,resObj);
                }

                resObj.DealFinish(path,resObj.CloneObj,resObj.Param1,resObj.Param2,resObj.Param3);
            }
        }

        /// <summary>
        /// 对象资源回收
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="maxCacheCount"></param>
        /// <param name="destroyCache"></param>
        /// <param name="recycleParent"></param>
        public void ReleaseObject(GameObject obj, int maxCacheCount =-1,bool destroyCache=false,bool recycleParent = true) {
            if (obj == null)
            {
                return;
            }

            ResourceObj resObj = null;
            int tempID = obj.GetInstanceID();
            if (m_ResourceObjDict.TryGetValue(tempID,out resObj)==false)
            {
                Debug.Log($"{obj.name} 对象不是 ObjectManager创建的");
                return;
            }
            if (resObj==null)
            {
                Debug.LogError("缓存的 ResourceObj 为空");
                return;
            }

            if (resObj.Already==true)
            {
                Debug.LogError($"该对象 {obj.name} 已被释放过，请检查是否还在引用");
                return;
            }

#if UNITY_EDITOR
            obj.name += "(Recycle)";
#endif
            List<ResourceObj> st = null;
            if (maxCacheCount == 0)
            {
                m_ResourceObjDict.Remove(tempID);
                ResourceManager.Instance.ReleaseResource(resObj, destroyCache);
                resObj.Reset();
                m_ResourceObjClassPool.Recycle(resObj);
            }
            else { // 回收到对象池
                if (m_ObjectPoolDict.TryGetValue(resObj.Crc,out st)==false || st ==null)
                {
                    st = new List<ResourceObj>();
                    m_ObjectPoolDict.Add(resObj.Crc, st);
                }

                if (resObj.CloneObj !=null)
                {
                    if (recycleParent == true)
                    {
                        resObj.CloneObj.transform.SetParent(m_RecyclePoolTrans);
                    }
                    else {
                        resObj.CloneObj.SetActive(false);
                    }
                }

                if (maxCacheCount < 0 || st.Count < maxCacheCount)
                {
                    st.Add(resObj);
                    resObj.Already = true;
                    // 引用计数减少
                    ResourceManager.Instance.DecreaseResourceRef(resObj);
                }
                else {
                    m_ResourceObjDict.Remove(tempID);
                    ResourceManager.Instance.ReleaseResource(resObj,destroyCache);
                    resObj.Reset();
                    m_ResourceObjClassPool.Recycle(resObj);
                }
            }
        }

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
