using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleBusinessFramework { 

	/// <summary>
	/// 主要用来在实例化资源回收的时候，回复实例化体的初始形态
	/// </summary>
	public class OfflineData : MonoBehaviour
	{
		public Rigidbody Rigidbody;
		public Collider Collider;
		public Transform[] AllPoint;
		public int[] AllPointChildCount;
		public bool[] AllPointActive;
		public Vector3[] Pos;
		public Vector3[] Scale;
		public Quaternion[] Rot;

		/// <summary>
		/// 还原属性
		/// </summary>
		public virtual void ResetProp() {
			int allPointCount = AllPoint.Length;
            for (int i = 0; i < allPointCount; i++)
            {
				Transform tempTrs = AllPoint[i];
                if (tempTrs!=null)
                {
					tempTrs.localPosition = Pos[i];
					tempTrs.localRotation = Rot[i];
					tempTrs.localScale = Scale[i];

					// 是否激活显示
                    if (AllPointActive[i])
                    {
						if (tempTrs.gameObject.activeSelf == false)
						{
							tempTrs.gameObject.SetActive(true);

						}
						
                    }
					else
					{
						if (tempTrs.gameObject.activeSelf == true)
						{
							tempTrs.gameObject.SetActive(false);

						}
					}

                    if (tempTrs.childCount > AllPointChildCount[i])
                    {
						int childCount = tempTrs.childCount;
                        for (int j = AllPointChildCount[i]; j < childCount; j++)
                        {
							GameObject tempObj = tempTrs.GetChild(j).gameObject;
                            if (ObjectManager.Instance.IsObjectManagerCreate(tempObj)==false)
                            {
								GameObject.Destroy(tempObj);
                            }
                        }
                    }
				}
            }
		
		}

		/// <summary>
		/// 编辑器下保存的初始户数
		/// </summary>
		public virtual void BindData() {
			Collider = gameObject.GetComponentInChildren<Collider>(true);
			Rigidbody = gameObject.GetComponentInChildren<Rigidbody>(true);
			AllPoint = gameObject.GetComponentsInChildren<Transform>(true);
			int allPointCount = AllPoint.Length;
			AllPointChildCount = new int[allPointCount];
			AllPointActive = new bool[allPointCount];
			Pos = new Vector3[allPointCount];
			Rot = new Quaternion[allPointCount];
			Scale = new Vector3[allPointCount];
            for (int i = 0; i < allPointCount; i++)
            {
				Transform temp = AllPoint[i] ;
				AllPointChildCount[i] = temp.childCount;
				AllPointActive[i] = temp.gameObject.activeSelf;
				Pos[i] = temp.localPosition;
				Rot[i] = temp.localRotation;
				Scale[i] = temp.localScale;

            }
		}
	}
}
