using AssetBundleBusinessFramework.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleBusinessFramework {

	/// <summary>
	/// 资源加载优先级别
	/// </summary>
	public enum LoadResPriority { 
		RES_HIGHT=0,// 最高优先级别
		RES_MIDDLE,// 一般优先级别
		RES_SLOW,// 较低优先级别
		RES_NUM,// 作为级别计数使用

	}

	/// <summary>
	/// 注意和 ResourceItem区分开来
	/// 
	/// </summary>
	public class ResourceObj {
		public uint Crc = 0;    // 路径对应的 Crc
		public ResourceItem ResItem=null;	// 用于实例化的预制体载体（可从此获取预制体，来实例化）
		public GameObject CloneObj = null;	// 实例化出来的 GameObject
		public bool IsClear = true; //是否清除资源
		public long GUID = 0;   // Object 的标识，便于查找该对象
		public bool Already = false;    // 是否已经被释放过

		//---------------------
		public bool SetSceneParent = false;    // 是否放在场景节点下面
		public OnAsyncObjFinish DealFinish = null; // 实例化资源记载完成后的回调
		public object Param1, Param2, Param3 = null;    // 异步参数
		public OfflineData OfflineData = null;	// 离线数据
		public void Reset() {
			Crc = 0;
			ResItem = null;
			CloneObj = null;
			IsClear = true;
			GUID = 0;
			Already = false;
			SetSceneParent = false;
			DealFinish = null;
			Param1 = Param2 = Param3 = null;
			OfflineData = null;
		}
	}


	public class AsyncLoadResParam {
		public List<AsyncCallback> CallbackList = new List<AsyncCallback>();
		public uint Crc;
		public string Path;
		public bool IsSprite = false; // 注意：因为 Unity 中 Object as Sprite 是不行的，所以添加这个，好提前处理
		public LoadResPriority Priority = LoadResPriority.RES_SLOW;

		public void Reset() {
			CallbackList.Clear();
			Crc = 0;
			Path = "";
			IsSprite = false;
			Priority = LoadResPriority.RES_SLOW;
		}
	}

	public class AsyncCallback {
		// 加载完成的回调 (针对 ObjectManager 实例化异步加载)
		public OnAsyncResObjFinish DealResObjFinish = null;
		// ObjectManager 对应的中间
		public ResourceObj ResObj = null;
		// -----------------

		// 加载完成的回调
		public OnAsyncObjFinish DealObjFinish = null;
		// 回调参数
		public object Param1 = null, Param2 = null, Param3 = null;

		public void Reset() {
			DealResObjFinish = null;
			ResObj = null;
			DealObjFinish = null;			
			Param1 = null;
			Param2 = null;
			Param3 = null;
		}
	}

	/// <summary>
	/// 异步加载完成的委托
	/// </summary>
	/// <param name="path"></param>
	/// <param name="obj"></param>
	/// <param name="param1"></param>
	/// <param name="param2"></param>
	/// <param name="param3"></param>
	public delegate void OnAsyncObjFinish(string path, Object obj,
		object param1=null, object param2 = null, object param3 = null);

	/// <summary>
	/// 实例化对象异步加载完成的委托
	/// </summary>
	/// <param name="path"></param>
	/// <param name="resObj"></param>
	/// <param name="param1"></param>
	/// <param name="param2"></param>
	/// <param name="param3"></param>
	public delegate void OnAsyncResObjFinish(string path, ResourceObj resObj,
		object param1 = null, object param2 = null, object param3 = null);

	/// <summary>
	/// Resource 管理
	/// Resource 并非 Unity Resources 类
	/// </summary>
	public class ResourceManager : Singleton<ResourceManager>
	{
		// guid
		protected long m_Guid = 0;
		// 是否从 AB 中加载
		private readonly bool IS_LOAD_ASSET_FROM_ASSETBUNDLE = true;
		// 最长连续卡着加载资源的时间，单位微秒
		private const long MAX_LOAD_RESET_TIME = 200000;
		// 缓存使用的资源列表
		public Dictionary<uint, ResourceItem> AssetDict { get; set; } = new Dictionary<uint, ResourceItem>();
		// 缓存应用计数为零的资源列表，达到缓存最大的时候，释放列表里面最早的没有应用的资源
		protected CMapList<ResourceItem> m_NoRefrenceAssetMapList = new CMapList<ResourceItem>();

		// 中间类，回调类的类对象池
		protected ClassObjectPool<AsyncLoadResParam> m_AsyncLoadResParamPool = new ClassObjectPool<AsyncLoadResParam>(50);
		protected ClassObjectPool<AsyncCallback> m_AsyncCallbackPool = new ClassObjectPool<AsyncCallback>(100);
		
		// Mono 脚本，主要为了协程的使用
		protected MonoBehaviour m_StartMono;
		// 异步加载的有加载优先级别的资源列表
		protected List<AsyncLoadResParam>[] m_LoadingAssetList = new List<AsyncLoadResParam>[(int)LoadResPriority.RES_NUM];
		// 正在异步加载的Dict 
		protected Dictionary<uint, AsyncLoadResParam> m_LoadingAssetDict = new Dictionary<uint, AsyncLoadResParam>();

		/// <summary>
		/// 初始化
		/// 主要是为了添加 MonoBehaviour 
		/// </summary>
		/// <param name="mono"></param>
		public void Init(MonoBehaviour mono) {

            for (int i = 0; i < (int)LoadResPriority.RES_NUM; i++)
            {
				m_LoadingAssetList[i] = new List<AsyncLoadResParam>();

			}
			m_StartMono = mono;
			m_StartMono.StartCoroutine(AsyncLoadCor());
		}

		/// <summary>
		/// 创建唯一 guid
		/// </summary>
		/// <returns></returns>
		public long CreateGuid() {
			return m_Guid++;
		}

		/// <summary>
		/// 清空缓存
		/// 跳转场景的时候可能需要
		/// </summary>
		public void ClearCache() {
			List<ResourceItem> tempList = new List<ResourceItem>();
            foreach (ResourceItem item in AssetDict.Values)
            {
                if (item.IsClear==true)
                {
					tempList.Add(item);
                }
            }

            foreach (ResourceItem item in tempList)
            {
				DestoryResourceItem(item,true);
            }
			tempList.Clear();

		}

		/// <summary>
		/// 取消正在进行的异步加载
		/// </summary>
		/// <param name="resObj"></param>
		/// <returns></returns>
		public bool CancelAsyncLoad(ResourceObj resObj) {
			AsyncLoadResParam para = null;
            if (m_LoadingAssetDict.TryGetValue(resObj.Crc,out para)==true && m_LoadingAssetList[(int)para.Priority].Contains(para))
            {
                for (int i = para.CallbackList.Count; i >=0; i--)
                {
					AsyncCallback tempCallback = para.CallbackList[i];
                    if (tempCallback !=null && resObj == tempCallback.ResObj)
                    {
						tempCallback.Reset();
						m_AsyncCallbackPool.Recycle(tempCallback);
						para.CallbackList.Remove(tempCallback);
                    }
                }

                if (para.CallbackList.Count <=0)
                {
					para.Reset();
					m_LoadingAssetList[(int)para.Priority].Remove(para);
					m_AsyncLoadResParamPool.Recycle(para);
					m_LoadingAssetDict.Remove(resObj.Crc);
					return true;
				}
            }

			return false;
		}

		/// <summary>
		/// 预加载指定资源
		/// </summary>
		/// <param name="path"></param>
		public void PreLoad(string path) {
            if (string.IsNullOrEmpty(path)==true)
            {
				return;
            }
			uint crc = Crc32.GetCrc32(path);

			ResourceItem item = GetCacheResourceItem(crc);
			if (item != null)
			{
				return;
			}

			Object obj = null;
#if UNITY_EDITOR
			if (IS_LOAD_ASSET_FROM_ASSETBUNDLE == false)
			{

				item = AssetBundleManager.Instance.FindResourceItem(crc);
				if (item.Obj != null)
				{
					obj = item.Obj;
				}
				else
				{
					obj = LoadAssetByEditor<Object>(path);
				}
			}
#endif

			if (obj == null)
			{
				item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
				if (item != null && item.AssetBundle != null)
				{

					if (item.Obj != null)
					{
						obj = item.Obj;
					}
					else
					{
						obj = item.AssetBundle.LoadAsset<Object>(item.AssetName);
					}
				}
			}

			CacheResource(path, ref item, crc, obj);

			// 预加载，为了跳转场景不清缓存
			item.IsClear = false;
			ReleaseResource(obj, false);
		}

		/// <summary>
		/// 同步加载资源，针对ObjectManager的接口
		/// 需要实例化的资源
		/// </summary>
		/// <param name="path"></param>
		/// <param name="resObj"></param>
		/// <returns></returns>
		public ResourceObj LoadResource(string path,ResourceObj resObj) {
            if (resObj == null)
            {
				return null;
            }

			uint crc = resObj.Crc == 0 ? Crc32.GetCrc32(path) : resObj.Crc;
			ResourceItem item = GetCacheResourceItem(crc);
            if (item!=null)
            {
				resObj.ResItem = item;
				return resObj;
            }
			Object obj = null;
#if UNITY_EDITOR
            if (IS_LOAD_ASSET_FROM_ASSETBUNDLE==false)
            {
				item = AssetBundleManager.Instance.FindResourceItem(crc);
				if (item!=null && item.Obj != null)
				{
					obj = item.Obj as Object;
				}
				else {

                    if (item ==null)
                    {
						item = new ResourceItem();
						item.Crc = crc;
                    }

					obj = LoadAssetByEditor<Object>(path);
				}
			}
#endif
			if (obj == null)
			{
				item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
				if (item != null && item.AssetBundle != null)
				{

					if (item.Obj != null)
					{
						obj = item.Obj as Object;
					}
					else
					{
						obj = item.AssetBundle.LoadAsset<Object>(item.AssetName);
					}
				}
			}

			CacheResource(path, ref item, crc, obj);
			resObj.ResItem = item;
			item.IsClear = resObj.IsClear;
			return resObj;
		}

		/// <summary>
		/// 同步资源加载，外部直接调用
		/// 仅加载不需要实例化的资源，例如 Texture,Sound 等等
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <returns></returns>
		public T LoadResource<T>(string path) where T : UnityEngine.Object {
			if (string.IsNullOrEmpty(path))
			{
				return null;
			}

			uint crc = Crc32.GetCrc32(path);

			ResourceItem item = GetCacheResourceItem(crc);
			if (item != null)
			{
				return item.Obj as T;
			}

			T obj = null;
#if UNITY_EDITOR
			if (IS_LOAD_ASSET_FROM_ASSETBUNDLE == false)
			{
				
				item = AssetBundleManager.Instance.FindResourceItem(crc);
				if (item.Obj != null)
				{
					obj = (T)item.Obj;
				}
				else {
					obj = LoadAssetByEditor<T>(path);
				}
			}
#endif

            if (obj==null)
            {
				item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
                if (item!=null && item.AssetBundle!=null)
                {

					if (item.Obj != null)
					{
						obj = (T)item.Obj;
					}
					else { 
						obj = item.AssetBundle.LoadAsset<T>(item.AssetName);
					}
				}
            }

			CacheResource(path,ref item,crc,obj);

			return obj;
		}

		/// <summary>
		/// 卸载实例化过的资源（ResourceObj）
		/// </summary>
		/// <param name="resObj"></param>
		/// <param name="destroyObj"></param>
		/// <returns></returns>
		public bool ReleaseResource(ResourceObj resObj,bool destroyObj =false) {
            if (resObj==null)
            {
				return false;
            }

			ResourceItem item = null;
			if (AssetDict.TryGetValue(resObj.Crc,out item)==false || item==null )
            {
				Debug.LogError($"AssetDict 不存在该资源：{resObj.CloneObj.name},或者释放了多次");
            }

			GameObject.Destroy(resObj.CloneObj);
			item.RefCount--;
			DestoryResourceItem(item,destroyObj);
			return true;
		}

		/// <summary>
		/// 释放不需要实例化的资源,根据 Object
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="isDestroyObj"></param>
		/// <returns></returns>
		public bool ReleaseResource(Object obj, bool isDestroyObj=false) {
			if (obj == null)
            {
				return false;
            }

			ResourceItem item = null;
            foreach (ResourceItem res in AssetDict.Values)
            {
                if (res.GUID == obj.GetInstanceID())
                {
					item = res;
					break;
                }
            }

            if (item == null)
            {
				Debug.LogError($"AssetDict has no Obj ：{obj.name}, Could have been released multiple times");
				return false;
			}

			item.RefCount--;
			DestoryResourceItem(item,isDestroyObj);
			return true;
		}

		/// <summary>
		/// 释放不需要实例化的资源,根据 路径
		/// </summary>
		/// <param name="path"></param>
		/// <param name="isDestroyObj"></param>
		/// <returns></returns>
		public bool ReleaseResource(string path, bool isDestroyObj = false) {
			if (string.IsNullOrEmpty(path) == true)
			{
				return false;
			}

			uint crc = Crc32.GetCrc32(path);

			ResourceItem item = null;

            if (AssetDict.TryGetValue(crc,out item)==false || null == item)
            {
				Debug.LogError($"AssetDict has no Obj ：{path}, Could have been released multiple time");

			}

			item.RefCount--;
			DestoryResourceItem(item, isDestroyObj);
			return true;
		}

		/// <summary>
		/// 根据 ResourceObj 增加引用计数 
		/// </summary>
		/// <param name="resObj"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public int IncreaseResourceRef(ResourceObj resObj,int count =1) {
			return resObj != null ? IncreaseResourceRef(resObj.Crc, count):0;
		}

		/// <summary>
		/// 根据 crc(path) 增加引用计数 
		/// </summary>
		/// <param name="crc"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public int IncreaseResourceRef(uint crc, int count = 1)
		{
			ResourceItem item = null;
            if (AssetDict.TryGetValue(crc,out item)==false || item==null)
            {
				return 0;
            }

			item.RefCount += count;
			item.LastUserTime = Time.realtimeSinceStartup;

			return item.RefCount;
		}


		/// <summary>
		/// 根据 ResourceObj 减少引用计数 
		/// </summary>
		/// <param name="resObj"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public int DecreaseResourceRef(ResourceObj resObj, int count = 1)
		{
			return resObj != null ? DecreaseResourceRef(resObj.Crc, count) : 0;
		}

		/// <summary>
		/// 根据 crc(path) 减少引用计数 
		/// </summary>
		/// <param name="crc"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public int DecreaseResourceRef(uint crc, int count = 1)
		{
			ResourceItem item = null;
			if (AssetDict.TryGetValue(crc, out item) == false || item == null)
			{
				return 0;
			}

			item.RefCount -= count;

			return item.RefCount;
		}

		/// <summary>
		/// 缓存资源
		/// </summary>
		/// <param name="path"></param>
		/// <param name="item"></param>
		/// <param name="crc"></param>
		/// <param name="obj"></param>
		/// <param name="addRefCount"></param>
		void CacheResource(string path,ref ResourceItem item,uint crc,Object obj, int addRefCount=1) {

			// 缓存太傅哦，清除最早没有使用的资源
			WashOutCacheResource();
			
			if (item==null)
            {
				Debug.LogError($"ResoureItem is null,path : {path}");
            }

            if (obj==null)
            {
				Debug.LogError($"Resource Load fail, path : {path}");
            }

			item.Obj = obj;
			item.GUID = obj.GetInstanceID();
			item.LastUserTime = Time.realtimeSinceStartup;
			item.RefCount += addRefCount;
			ResourceItem oldItem = null;
			if (AssetDict.TryGetValue(item.Crc, out oldItem) == true)
			{
				AssetDict[item.Crc] = item;
			}
			else {
				AssetDict.Add(item.Crc,item);
			}
		}

		/// <summary>
		/// 清理过多的缓存资源
		/// </summary>
		void WashOutCacheResource() {
			// 当前内存使用大于 80%,进行清除最早没用的资源


			//{
   //             if (m_NoRefrenceAssetMapList.Size()<=0)
   //             {
			//		break;
   //             }

			//	ResourceItem item = m_NoRefrenceAssetMapList.Back();
			//	DestoryResourceItem(item,true);
			//	m_NoRefrenceAssetMapList.Pop();
			//}
		
		}

		/// <summary>
		/// 回收资源
		/// </summary>
		/// <param name="item"></param>
		/// <param name="isDestoryCache"></param>
		protected void DestoryResourceItem(ResourceItem item,bool isDestoryCache =false) {
            if (item==null || item.RefCount>0)
            {
				return;
            }

			if (isDestoryCache==false)
            {
				m_NoRefrenceAssetMapList.InsertToHead(item);
				return;
            }

			if (AssetDict.Remove(item.Crc) == false) // 移除失败
			{
				return;
			}

			m_NoRefrenceAssetMapList.Remove(item);

			// 释放 AssetBundle引用
			AssetBundleManager.Instance.ReleaseAsset(item);

			// 清空资源对应的对象池
			ObjectManager.Instance.ClearPoolObject(item.Crc);

            if (item.Obj!=null)
            {
				item.Obj = null;
#if UNITY_EDITOR
				Resources.UnloadUnusedAssets();
#endif
			}
		}

#if UNITY_EDITOR

		protected T LoadAssetByEditor<T>(string path)where T : UnityEngine.Object{
			return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
		}
#endif

		/// <summary>
		/// 从资源池获取缓存资源
		/// </summary>
		/// <param name="crc"></param>
		/// <param name="addRefCount"></param>
		/// <returns></returns>
		ResourceItem GetCacheResourceItem(uint crc, int addRefCount =1) {
			ResourceItem item = null;
            if (AssetDict.TryGetValue(crc,out item))
            {
                if (item!=null)
                {
					item.RefCount += addRefCount;
					item.LastUserTime = Time.realtimeSinceStartup;
                    
					//if (item.RefCount<-1)
     //               {
					//	m_NoRefrenceAssetMapList.Remove(item);
     //               }
					
                }
            }

			return item;
		}

		public void AsyncLoadResource(string path,OnAsyncObjFinish dealFinish,LoadResPriority priority,
			object param1=null, object param2=null, object param3=null,uint crc =0) {
            if (crc==0)
            {
				crc = Crc32.GetCrc32(path);
            }

			ResourceItem item = GetCacheResourceItem(crc);
            if (item!=null)
            {
                if (dealFinish!=null)
                {
					dealFinish(path,item.Obj, param1, param2, param3);
                }

				return;
            }

			// 判断是否在加载中
			AsyncLoadResParam para = null;
            if (m_LoadingAssetDict.TryGetValue(crc,out para)==false|| para==null)
            {
				para = m_AsyncLoadResParamPool.Spawn(true);
				para.Crc = crc;
				para.Path = path;
				para.Priority = priority;
				m_LoadingAssetDict.Add(crc,para);
				m_LoadingAssetList[(int)priority].Add(para);
            }

			// 往回调列表里面加回调
			AsyncCallback callback = m_AsyncCallbackPool.Spawn(true);
			callback.DealObjFinish = dealFinish;
			callback.Param1 = param1;
			callback.Param2 = param2;
			callback.Param3 = param3;
			para.CallbackList.Add(callback);
		}

		/// <summary>
		/// 异步加载针对实例化资源
		/// </summary>
		/// <param name="path"></param>
		/// <param name="resObj"></param>
		/// <param name="dealFinish"></param>
		/// <param name="priority"></param>
		public void AsyncLoadResource(string path, ResourceObj resObj, OnAsyncResObjFinish dealFinish, LoadResPriority priority)
		{
			ResourceItem item = GetCacheResourceItem(resObj.Crc);
			if (item != null)
			{
				resObj.ResItem = item;
				if (dealFinish != null)
				{
					dealFinish(path, resObj);
				}

				return;
			}

			// 判断是否在加载中
			AsyncLoadResParam para = null;
			if (m_LoadingAssetDict.TryGetValue(resObj.Crc, out para) == false || para == null)
			{
				para = m_AsyncLoadResParamPool.Spawn(true);
				para.Crc = resObj.Crc;
				para.Path = path;
				para.Priority = priority;
				m_LoadingAssetDict.Add(resObj.Crc, para);
				m_LoadingAssetList[(int)priority].Add(para);
			}

			// 往回调列表里面加回调
			AsyncCallback callback = m_AsyncCallbackPool.Spawn(true);
			callback.DealResObjFinish = dealFinish;
			callback.ResObj = resObj;
			para.CallbackList.Add(callback);
		}

		/// <summary>
		/// 协程异步加载资源
		/// </summary>
		/// <returns></returns>
		System.Collections.IEnumerator AsyncLoadCor() {
			List<AsyncCallback> callbackList = null;
			// 上次 yield 的时间
			long lastYieldTime = System.DateTime.Now.Ticks;
			// 判断时候 yield 过
			bool IsHasYield = false;
			while (true) {

				
				IsHasYield = false;
				
                for (int i = 0; i < (int)LoadResPriority.RES_NUM; i++)
                {
					List<AsyncLoadResParam> loadingList = m_LoadingAssetList[i];
                    if (loadingList.Count<=0)
                    {
						continue;
                    }

					AsyncLoadResParam loadingItem = loadingList[0];
					loadingList.RemoveAt(0);
					callbackList = loadingItem.CallbackList;

					Object obj = null;
					ResourceItem item = null;

#if UNITY_EDITOR
                    if (IS_LOAD_ASSET_FROM_ASSETBUNDLE ==false)
                    {
						obj = LoadAssetByEditor<Object>(loadingItem.Path);
						// 模拟异步加载
						yield return new WaitForSeconds(0.5f);

						item = AssetBundleManager.Instance.FindResourceItem(loadingItem.Crc);

						if (item == null)
						{
							item = new ResourceItem();
							item.Crc = loadingItem.Crc;
						}
					}
#endif
                    if (obj==null)
                    {
						item = AssetBundleManager.Instance.LoadResourceAssetBundle(loadingItem.Crc);
                        if (item!=null && item.AssetBundle!=null)
                        {
							AssetBundleRequest abRequest = null;
							if (loadingItem.IsSprite == true)
							{
								abRequest = item.AssetBundle.LoadAssetAsync<Sprite>(item.AssetName);
							}
							else {
								abRequest = item.AssetBundle.LoadAssetAsync(item.AssetName);
							}
							yield return abRequest;
                            if (abRequest.isDone)
                            {
								obj = abRequest.asset;
                            }

							lastYieldTime = System.DateTime.Now.Ticks;
						}
                    }

					CacheResource(loadingItem.Path,ref item,loadingItem.Crc,obj,callbackList.Count);

					// 执行回调
                    for (int j = 0; j < callbackList.Count; j++)
                    {
						AsyncCallback callback = callbackList[j];
						if (callback!=null && callback.DealResObjFinish!=null && callback.ResObj!=null)
                        {
							ResourceObj tempResObj = callback.ResObj;
							tempResObj.ResItem = item;
							callback.DealResObjFinish(loadingItem.Path,tempResObj,tempResObj.Param1,tempResObj.Param2,tempResObj.Param3);
							callback.DealResObjFinish = null;
							tempResObj = null;
                        }

                        if (callback!=null && callback.DealObjFinish!=null)
                        {
							callback.DealObjFinish(loadingItem.Path,obj,callback.Param1,callback.Param2,callback.Param3);
							callback.DealObjFinish = null;
						}

						// 回收
						callback.Reset();
						m_AsyncCallbackPool.Recycle(callback);
                    }

					// 善后处理
					obj = null;
					callbackList.Clear();
					m_LoadingAssetDict.Remove(loadingItem.Crc);

					// 回收
					loadingItem.Reset();
					m_AsyncLoadResParamPool.Recycle(loadingItem);

					if (System.DateTime.Now.Ticks - lastYieldTime > MAX_LOAD_RESET_TIME)
					{
						yield return null;
						lastYieldTime = System.DateTime.Now.Ticks;
						IsHasYield = true;
					}
				}

                if (IsHasYield == false || System.DateTime.Now.Ticks - lastYieldTime > MAX_LOAD_RESET_TIME)
                {
					lastYieldTime = System.DateTime.Now.Ticks;
					yield return null;
				}
				
			}
		}
	}

	public class DoubleLinkedListNode<T> where T : class, new() {
		// 前一个节点
		public DoubleLinkedListNode<T> Prev = null;
		// 后一个节点
		public DoubleLinkedListNode<T> Next = null;
		// 当前节点
		public T Current = null;
	}

	public class DoubleLinkedList<T> where T : class, new()
    {
		// 表头
		public DoubleLinkedListNode<T> Head = null;
		// 表尾
		public DoubleLinkedListNode<T> Tail = null;
		// 双向链表结构类对象池
		protected ClassObjectPool<DoubleLinkedListNode<T>> m_DoubleLinkedNodePool = ObjectManager.Instance.GetOrCreateClassPool<DoubleLinkedListNode<T>>(500);
		// 个数
		protected int m_Count = 0;
		public int Count {
			get { return m_Count; }
		}

		/// <summary>
		/// 添加节点到头部
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public DoubleLinkedListNode<T> AddToHeader(T t) {
			DoubleLinkedListNode<T> pList = m_DoubleLinkedNodePool.Spawn(true);
			pList.Next = null;
			pList.Prev = null; ;
			pList.Current = t;

			return AddToHeader(pList);
		}

		/// <summary>
		/// 添加节点到头部
		/// </summary>
		/// <param name="pNode"></param>
		/// <returns></returns>
		public DoubleLinkedListNode<T> AddToHeader(DoubleLinkedListNode<T> pNode) {
            if (pNode==null)
            {
				return null;
            }

			pNode.Prev = null;
			if (Head == null)
			{
				Head = Tail = pNode;
			}
			else {
				pNode.Next = Head;
				Head.Prev = pNode;
				Head = pNode;
			}

			m_Count++;

			return Head;
		}

		/// <summary>
		/// 添加节点到尾部
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public DoubleLinkedListNode<T> AddToTail(T t)
		{
			DoubleLinkedListNode<T> pList = m_DoubleLinkedNodePool.Spawn(true);
			pList.Next = null;
			pList.Prev = null; ;
			pList.Current = t;

			return AddToTail(pList);
		}

		/// <summary>
		/// 添加节点到尾部
		/// </summary>
		/// <param name="pNode"></param>
		/// <returns></returns>
		public DoubleLinkedListNode<T> AddToTail(DoubleLinkedListNode<T> pNode)
		{
			if (pNode == null)
			{
				return null;
			}

			pNode.Prev = null;
			if (Tail == null)
			{
				Head = Tail = pNode;
			}
			else
			{
				pNode.Prev = Tail;
				Tail.Next = pNode;
				Tail = pNode;
			}

			m_Count++;

			return Tail;
		}

		/// <summary>
		/// 移除某个节点
		/// </summary>
		/// <param name="pNode"></param>
		public void RemoveNode(DoubleLinkedListNode<T> pNode) {
            if (pNode == null)
            {
				return;
            }
            if (pNode == Head)
            {
				Head = pNode.Next;
            }
            if (pNode==Tail)
            {
				Tail = pNode.Prev;
            }

            if (pNode.Prev != null)
            {
				pNode.Prev.Next = pNode.Next;
            }

            if (pNode.Next!=null)
            {
				pNode.Next.Prev = pNode.Prev;
            }

			pNode.Next = pNode.Prev = null;
			pNode.Current = null;
			m_DoubleLinkedNodePool.Recycle(pNode);
			m_Count--;
		}

		/// <summary>
		/// 移动某个节点到头部
		/// </summary>
		/// <param name="pNode"></param>
		public void MoveToHead(DoubleLinkedListNode<T> pNode) {
            if (pNode == null || pNode == Head)
            {
				return;
            }

            if (pNode.Prev == null && pNode.Next == null)
            {
				return;
            }

            if (pNode==Tail)
            {
				Tail = pNode.Prev;
            }

            if (pNode.Prev!=null)
            {
				pNode.Prev.Next = pNode.Next;
            }
			if (pNode.Next != null)
			{
				pNode.Next.Prev = pNode.Prev;
			}

			pNode.Prev = null;
			pNode.Next = Head;
			Head.Prev = pNode;
			Head = pNode;
            if (Tail==null) // 只有两个的特殊情况
            {
				Tail = Head;
            }
		}

	}

	public class CMapList<T> where T : class, new() {
		DoubleLinkedList<T> m_DLink = new DoubleLinkedList<T>();
		Dictionary<T, DoubleLinkedListNode<T>> m_FindMap = new Dictionary<T, DoubleLinkedListNode<T>>();

		// 析构函数
		~CMapList() {
			Clear();
		}

		/// <summary>
		/// 情况列表
		/// </summary>
		public void Clear() {
            while (m_DLink.Tail!=null)
            {
				Remove(m_DLink.Tail.Current);
            }
		}

		/// <summary>
		/// 插入一个节点到表头
		/// </summary>
		/// <param name="t"></param>
		public void InsertToHead(T t) {
			DoubleLinkedListNode<T> node = null;
            if (m_FindMap.TryGetValue(t,out node) && node !=null)
            {
				m_DLink.AddToHeader(node);
				return;
            }

			m_DLink.AddToHeader(t);
			m_FindMap.Add(t,m_DLink.Head);
		}

		/// <summary>
		/// 从表尾弹出一个节点
		/// </summary>
		public void Pop() {
            if (m_DLink.Tail !=null)
            {
				Remove(m_DLink.Tail.Current);
            }
		}

		/// <summary>
		/// 删除某个节点
		/// </summary>
		/// <param name="t"></param>
		public void Remove(T t) {
			DoubleLinkedListNode<T> node = null;
            if (m_FindMap.TryGetValue(t, out node)==false || node == null)
            {
				return;
            }

			m_DLink.RemoveNode(node);
			m_FindMap.Remove(t);
		}

		/// <summary>
		/// 返回结尾节点
		/// </summary>
		/// <returns></returns>
		public T Back() {
			return m_DLink.Tail == null ? null : m_DLink.Tail.Current;
		}

		/// <summary>
		/// 返回节点个数
		/// </summary>
		/// <returns></returns>
		public int Size() {
			return m_FindMap.Count;
		}

		/// <summary>
		/// 判断某个节点是否存在
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public bool Find(T t) {
			DoubleLinkedListNode<T> node = null;
            if (m_FindMap.TryGetValue(t,out node)==false || node ==null)
            {
				return false;
            }
			return true;
		}

		/// <summary>
		/// 刷新链表，把指定节点移动到链表头部
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public bool Refresh(T t) {
			DoubleLinkedListNode<T> node = null;
            if (m_FindMap.TryGetValue(t,out node)|| node ==null)
            {
				return false;
            }

			m_DLink.MoveToHead(node);
			return true;
		}
	}	
}
