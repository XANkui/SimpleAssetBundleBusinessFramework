using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleBusinessFramework { 

	public class UIOfflineData : OfflineData
	{
		public Vector2[] AnchorMax;
		public Vector2[] AnchorMin;
		public Vector2[] Pivot;
		public Vector2[] SizeDelta;
		public Vector3[] AnchorPos;
		public ParticleSystem[] Particle;

        public override void ResetProp()
        {
            int allPointCount = AllPoint.Length;
            for (int i = 0; i < allPointCount; i++)
            {
                RectTransform tempTrs = AllPoint[i] as RectTransform;
                if (tempTrs != null)
                {
                    tempTrs.localPosition = Pos[i];
                    tempTrs.localRotation = Rot[i];
                    tempTrs.localScale = Scale[i];
                    tempTrs.anchorMax = AnchorMax[i];
                    tempTrs.anchorMin = AnchorMin[i];
                    tempTrs.pivot = Pivot[i];
                    tempTrs.sizeDelta = SizeDelta[i];
                    tempTrs.anchoredPosition3D = AnchorPos[i];
                }
            }

            int particleCount = Particle.Length;
            for (int i = 0; i < particleCount; i++)
            {
                Particle[i].Clear(true);
                Particle[i].Play();

            }
        }

        public override void BindData()
        {
            Transform[] allTrs = gameObject.GetComponentsInChildren<Transform>();
            int allTrsCount = allTrs.Length;
            for (int i = 0; i < allTrsCount; i++)
            {
                if ((allTrs[i] is RectTransform)==false)
                {
                    allTrs[i].gameObject.AddComponent<RectTransform>();
                }
            }

            AllPoint = gameObject.GetComponentsInChildren<RectTransform>(true);
            Particle = gameObject.GetComponentsInChildren<ParticleSystem>(true);
            int allPointCount = AllPoint.Length;
            AllPointChildCount = new int[allPointCount];
            AllPointActive = new bool[allPointCount];
            Pos = new Vector3[allPointCount];
            Rot = new Quaternion[allPointCount];
            Scale = new Vector3[allPointCount];
            Pivot = new Vector2[allPointCount];
            AnchorMax = new Vector2[allPointCount];
            AnchorMin = new Vector2[allPointCount];
            SizeDelta = new Vector2[allPointCount];
            AnchorPos = new Vector3[allPointCount];
            for (int i = 0; i < allPointCount; i++)
            {
                RectTransform temp = AllPoint[i] as RectTransform;
                AllPointChildCount[i] = temp.childCount;
                AllPointActive[i] = temp.gameObject.activeSelf;
                Pos[i] = temp.localPosition;
                Rot[i] = temp.localRotation;
                Scale[i] = temp.localScale;
                Pivot[i] = temp.pivot;
                AnchorMax[i] = temp.anchorMax;
                AnchorMin[i] = temp.anchorMin;
                SizeDelta[i] = temp.sizeDelta;
                AnchorPos[i] = temp.anchoredPosition3D;
            }

        }
    }
}
