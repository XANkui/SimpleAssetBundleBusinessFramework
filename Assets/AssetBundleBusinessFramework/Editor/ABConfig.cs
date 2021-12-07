using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleBusinessFramework
{ 
	[CreateAssetMenu(fileName ="ABConfig",menuName ="MyAssetBundleTool/CreateABConfig",order =0)]
	public class ABConfig : ScriptableObject
	{
		/// <summary>
		/// 单个文件所在文件夹路径，会遍历这个文件夹下面所有 prefabs
		/// 注意：所有 prefabs 的名字不能重复，必须保证名字的唯一性
		/// </summary>
		public List<string> m_AllPrefabPath = new List<string>();
		public List<FileDirABName> m_AllFileDirAB = new List<FileDirABName>();

		[System.Serializable]
		public struct FileDirABName {
			public string ABName;
			public string Path;
		}
	}
}
