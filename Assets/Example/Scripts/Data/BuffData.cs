using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace AssetBundleBusinessFramework
{

	[System.Serializable]
	public class BuffData : ExcelBase
	{
        public override void Construction()
        {
        }

        public override void Init()
        {
        }

        [XmlElement("AllBuffList")]
        public List<BuffBase> AllBuffList { get; set; }

        [XmlElement("MonsterBuffList")]
        public List<BuffBase> MonsterBuffList { get; set; }
    }

    public enum BuffEnum { 
        None=0,
        RanShao,
        BingDong,
        Du,
    }

    [System.Serializable]
    public class BuffBase {
        [XmlAttribute("Id")]
        public int Id { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("OutLook")]
        public string OutLook { get; set; }

        [XmlAttribute("Time")]
        public float Time { get; set; }
        [XmlAttribute("BuffType")]
        public BuffEnum BuffType { get; set; }
    }
}
