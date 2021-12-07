using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace AssetBundleBusinessFramework
{ 
	/// <summary>
	/// AB Xml 配置表文件格式
	/// </summary>
	[System.Serializable]
	public class AssetBundleXMLConfig 
	{
		[XmlElement("ABList")]
		public List<ABBase> ABList { get; set; }
	}

	[System.Serializable]
	public class ABBase { 
		[XmlAttribute("Path")]
		public string Path { get; set; }
		[XmlAttribute("Crc")]
		public uint Crc { get; set; }
		[XmlAttribute("ABName")]
		public string ABName { get; set; }
		[XmlAttribute("AssetName")]
		public string AssetName { get; set; }
		[XmlElement("ABDependce")]
		public List<string> ABDependce { get; set; }
	}
}
