using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleBusinessFramework { 

	/// <summary>
	/// 主要用来在实例化资源回收的时候，回复实例化体的初始形态
	/// </summary>
	public class OfflineData 
	{
		public Rigidbody Rigidbody;
		public Collider Collider;
		public Transform[] AllPoint;
		public int[] AllPointChildCount;
		public bool[] AllPointActive;
		public Vector3[] Pos;
		public Vector3[] Scale;
		public Quaternion[] ROt;

		/// <summary>
		/// 还原属性
		/// </summary>
		public virtual void ResetProp() { }

		public virtual void BindData() { }
	}
}
