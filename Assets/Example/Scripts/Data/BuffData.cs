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
            AllBuffList = new List<BuffBase>();
            for (int i = 0; i < 10; i++)
            {
                BuffBase buff = new BuffBase();
                buff.Id = i + 1;
                buff.Name = "全 Buff"+i;
                buff.OutLook = "Assets/GameData/... "+i;
                buff.Time = Random.Range(0.5f,10);
                buff.BuffType = (BuffEnum)Random.Range(0,4);
                buff.AllString = new List<string>();
                buff.AllString.Add("测试"+i);
                buff.AllString.Add("Test"+i);
                buff.AllBuffTest = new List<BuffTest>();
                int count = Random.Range(0,4);
                for (int j = 0; j < count; j++)
                {
                    BuffTest test = new BuffTest();
                    test.Id = j + Random.Range(5,24);
                    test.Name = "Name" + test.Id;
                    buff.AllBuffTest.Add(test);
                }
                AllBuffList.Add(buff);
            }

            MonsterBuffList = new List<BuffBase>();
            for (int i = 0; i < 10; i++)
            {
                BuffBase buff = new BuffBase();
                buff.Id = i + 1;
                buff.Name = "Monster Buff" + i;
                buff.OutLook = "Assets/GameData/... " + i;
                buff.Time = Random.Range(0.5f, 10);
                buff.BuffType = (BuffEnum)Random.Range(0, 4);
                buff.AllString = new List<string>();
                buff.AllString.Add("测试" + i);
                buff.AllBuffTest = new List<BuffTest>();
                int count = Random.Range(0, 4);
                for (int j = 0; j < count; j++)
                {
                    BuffTest test = new BuffTest();
                    test.Id = j + Random.Range(5, 24);
                    test.Name = "Name" + test.Id;
                    buff.AllBuffTest.Add(test);
                }
                MonsterBuffList.Add(buff);
            }
        }

        public override void Init()
        {
            m_AllBuffDict.Clear();
            for (int i = 0; i < AllBuffList.Count; i++)
            {
                m_AllBuffDict.Add(AllBuffList[i].Id,AllBuffList[i]);
            }
        }

        /// <summary>
        /// 根据 id 查找
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BuffBase FindBuffById(int id) {
            return m_AllBuffDict[id];
        }


        [XmlIgnore]
        private Dictionary<int, BuffBase> m_AllBuffDict = new Dictionary<int, BuffBase>();

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
        [XmlAttribute("AllString")]
        public List<string> AllString { get; set; }

        [XmlElement("AllBuffTest")]
        public List<BuffTest> AllBuffTest { get; set; }
    }

    [System.Serializable]
    public class BuffTest {
        [XmlAttribute("Id")]
        public int Id { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }
    }
}
