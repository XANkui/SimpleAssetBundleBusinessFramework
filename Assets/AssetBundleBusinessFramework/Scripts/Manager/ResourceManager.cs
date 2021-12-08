using AssetBundleBusinessFramework.Tools;
using System.Collections.Generic;

namespace AssetBundleBusinessFramework {

	/// <summary>
	/// Resource 管理
	/// Resource 并非 Unity Resources 类
	/// </summary>
	public class ResourceManager : Singleton<ResourceManager>
	{
		
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
