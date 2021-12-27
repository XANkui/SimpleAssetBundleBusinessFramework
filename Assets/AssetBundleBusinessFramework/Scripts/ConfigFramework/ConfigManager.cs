using AssetBundleBusinessFramework.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleBusinessFramework {

	public class CFG {
		// 配置表路径
		public const string TABLE_MONSTER = "Assets/GameData/Data/Binary/AssetBundleBusinessFramework.MonsterData.bytes";
		public const string TABLE_BUFF = "Assets/GameData/Data/Binary/AssetBundleBusinessFramework.BuffData.bytes";
	}

	public class ConfigManager : Singleton<ConfigManager>
	{
		/// <summary>
		/// 储存所有已经加载的配置表
		/// </summary>
		protected Dictionary<string, ExcelBase> m_AllExcelData = new Dictionary<string, ExcelBase>();

		public T LoadData<T>(string path) where T:ExcelBase {
            if (string.IsNullOrEmpty(path))
            {
				return null;
            }

            if (m_AllExcelData.ContainsKey(path)==true)
            {
				Debug.LogError($"重复加载相同配置的文件: {path}");
				return m_AllExcelData[path] as T;
            }

			T data = BinarySerializeOpt.BinaryDeserialize<T>(path);

#if UNITY_EDITOR
            if (data ==null)
            {
				Debug.Log($" {path} 不存在，尝试从 xml 加载数据");
				// 注意 二进制文件和xml 文件名一致性
				string xmlPath = path.Replace("Binary","Xml").Replace(".bytes",".xml");
				data = BinarySerializeOpt.XmlDeserialize<T>(xmlPath);
			}
#endif
            if (data !=null)
            {
				data.Init();
            }

			m_AllExcelData.Add(path,data);

			return data;
        }

		public T FindData<T>(string path) where T :ExcelBase {
            if (string.IsNullOrEmpty(path)==true)
            {
				return null;
            }

			ExcelBase excelBase = null;
			if (m_AllExcelData.TryGetValue(path, out excelBase) == true)
			{
				return excelBase as T;
			}
			else {
				excelBase = LoadData<T>(path);
			}

			return excelBase as T;
		}
	}
}
