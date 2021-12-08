using AssetBundleBusinessFramework.Tools;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace AssetBundleBusinessFramework {

	/// <summary>
	/// 资源管理Item
	/// </summary>
	public class ResourceItem {
		// 资源路径crc
		public uint Crc = 0;
		// 该资源的文件名
		public string AssetName = string.Empty;
		// 该资源的AB包名
		public string ABName = string.Empty;
		// 该资源的依赖的AB包名
		public List<string> ABDependce = null;
		// 该资源的AB 包
		public AssetBundle AssetBundle = null;
	}

	/// <summary>
	/// AB 包的应用计数管理 Item
	/// </summary>
	public class AssetBundleItem {
		public AssetBundle AssetBundle = null;
		public int RefCount = 0;

		public void Reset() {
			AssetBundle = null;
			RefCount = 0;
		}
	}

	public class AssetBundleManager : Singleton<AssetBundleManager>
	{
		// key: crc,value : 资源 Item
		private Dictionary<uint, ResourceItem> m_ResourceItemsDIct = new Dictionary<uint, ResourceItem>();
		// key: crc,value : AB 资源的引用计数Item 
		private Dictionary<uint, AssetBundleItem> m_AssetBundleItemRefDict = new Dictionary<uint, AssetBundleItem>();
		// AssetBundleItem 类对象池
		private ClassObjectPool<AssetBundleItem> m_AssetBundleItemPool = ObjectManager.Instance.GetOrCreateClassPool<AssetBundleItem>(500);
		
		/// <summary>
		/// 加载二进制配置文件
		/// </summary>
		/// <returns></returns>
		public bool LoadAssetBundleConfig() {
			Debug.LogWarning("LoadAssetBundleConfig ==========");
			string configPath = Application.streamingAssetsPath + "/assetbundleconfig";
			AssetBundle configAB = AssetBundle.LoadFromFile(configPath);
			TextAsset textAsset = configAB.LoadAsset<TextAsset>("assetbundleconfig");
            if (textAsset==null)
            {
				Debug.LogError("AssetBundleConfig 不存在");
				return false;
            }

			MemoryStream stream = new MemoryStream(textAsset.bytes);
			BinaryFormatter bf = new BinaryFormatter();
			AssetBundleXMLConfig config = (AssetBundleXMLConfig)bf.Deserialize(stream);
			stream.Close();

            for (int i = 0; i < config.ABList.Count; i++)
            {
				ABBase abBase = config.ABList[i];
				ResourceItem item = new ResourceItem();
				item.Crc = abBase.Crc;
				item.AssetName = abBase.AssetName;
				item.ABName = abBase.ABName;
				item.ABDependce = abBase.ABDependce;
				if (m_ResourceItemsDIct.ContainsKey(item.Crc) == true)
				{
					Debug.LogError($"重复的 Crc 资源名 {item.AssetName} AB 包名为 {item.ABName}");
				}
				else {
					m_ResourceItemsDIct.Add(item.Crc,item);
				}
            }

			return true;
		}

		public ResourceItem LoadResourceAssetBundle(uint crc) {
			ResourceItem item = null;

			if (m_ResourceItemsDIct.TryGetValue(crc,out item)
				|| item ==null)
            {
				Debug.LogError($"LoadResourceAssetBundle error : can not find crc {crc} in AssetBundleCOnfig");
				return item;
			}

            if (item.AssetBundle !=null)
            {
				return item;
            }

			item.AssetBundle = LoadAssetBundle(item.ABName);
            if (item.ABDependce!=null)
            {
                for (int i = 0; i < item.ABDependce.Count; i++)
                {
					LoadAssetBundle(item.ABDependce[i]);
                }
            }

			return item;
		}

		/// <summary>
		/// 根据名字加载单个 AssetBundle
		/// </summary>
		/// <param name="abName"></param>
		/// <returns></returns>
		private AssetBundle LoadAssetBundle(string abName) {
			AssetBundleItem refItem = null;
			uint crc = Crc32.GetCrc32(abName); // 根据ABName重新生成的 crc
			if (m_AssetBundleItemRefDict.TryGetValue(crc, out refItem) == false)
			{
				AssetBundle assetBundle = null;
				string fullPath = Application.streamingAssetsPath + "/" + abName;
				if (File.Exists(fullPath) == true)
				{
					assetBundle = AssetBundle.LoadFromFile(fullPath);
				}

				if (assetBundle == null)
				{
					Debug.LogError($" Load AssetBundle Error , path : {fullPath} ");
				}
				refItem = m_AssetBundleItemPool.Spawn(true);
				refItem.AssetBundle = assetBundle;
				refItem.RefCount++;
				m_AssetBundleItemRefDict.Add(crc, refItem);
			}
			else {
				refItem.RefCount++;
			}

			
			return refItem.AssetBundle;
		}

		/// <summary>
		/// 根据 ResourceItem 卸载资源
		/// </summary>
		/// <param name="item"></param>
		public void ReleaseAsset(ResourceItem item) {
            if (item == null)
            {
				return;
            }

            if (item.ABDependce!=null && item.ABDependce.Count>0)
            {
                for (int i = 0; i < item.ABDependce.Count; i++)
                {
					UnLoadAssetBundle(item.ABDependce[i]);					
				}
            }
			UnLoadAssetBundle(item.ABName);
		}

		/// <summary>
		/// 判断卸载AB资源
		/// </summary>
		/// <param name="abName"></param>
		private void UnLoadAssetBundle(string abName) {
			AssetBundleItem item = null;
			uint crc = Crc32.GetCrc32(abName);
			if (m_AssetBundleItemRefDict.TryGetValue(crc,out item)==true
				&& item !=null)
            {
				item.RefCount--;
                if (item.RefCount<=0 && item.AssetBundle!=null)
                {
					item.AssetBundle.Unload(true);
					item.Reset();
					m_AssetBundleItemPool.Recycle(item);
					m_AssetBundleItemRefDict.Remove(crc);
                }
            }
		}

		/// <summary>
		/// 根据crc查找 ResourceItem
		/// </summary>
		/// <param name="crc"></param>
		/// <returns></returns>
		public ResourceItem FindResourceItem(uint crc) {
			return m_ResourceItemsDIct[crc];
		}
	}
}
