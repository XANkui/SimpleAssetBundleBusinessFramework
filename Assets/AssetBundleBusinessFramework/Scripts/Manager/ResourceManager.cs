using AssetBundleBusinessFramework.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleBusinessFramework {

	/// <summary>
	/// Resource 管理
	/// Resource 并非 Unity Resources 类
	/// </summary>
	public class ResourceManager : Singleton<ResourceManager>
	{
		// 是否从 AB 中加载
		private readonly bool IS_LOAD_ASSET_FROM_ASSETBUNDLE = true;

		// 缓存使用的资源列表
		public Dictionary<uint, ResourceItem> AssetDict { get; set; } = new Dictionary<uint, ResourceItem>();
		// 缓存应用计数为零的资源列表，达到缓存最大的时候，释放列表里面最早的没有应用的资源
		protected CMapList<ResourceItem> m_NoRefrenceAssetMapList = new CMapList<ResourceItem>();

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
		/// 释放不需要实例化的资源
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
		void WashOutCacheResource() { }

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

			if (AssetDict.Remove(item.Crc) == false) // 移除失败
			{
				return;
			}

			if (isDestoryCache==false)
            {
				m_NoRefrenceAssetMapList.InsertToHead(item);
				return;
            }

			// 释放 AssetBundle引用
			AssetBundleManager.Instance.ReleaseAsset(item);

            if (item.Obj!=null)
            {
				item.Obj = null;

			}
		}

#if UNITY_EDITOR

		protected T LoadAssetByEditor<T>(string path)where T : UnityEngine.Object{
			return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
		}
#endif

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
