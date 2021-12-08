using AssetBundleBusinessFramework.Tools;


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
}
