using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace AssetBundleBusinessFramework { 

    [System.Serializable]
	public class MonsterData : ExcelBase
	{
        /// <summary>
        /// 模拟数据加载
        /// </summary>
        public override void Construction()
        {
            AllMonster = new List<MonsterBase>();
            for (int i = 0; i < 5; i++)
            {
                MonsterBase monster = new MonsterBase();
                monster.Id = i + 1;
                monster.Name = i + "sq";
                monster.OutLook = "Assets/GameData/Prefabs/Attack.prefab";
                monster.Rare = 2;
                monster.Height = 2 + i;
                AllMonster.Add(monster);
            }
        }

        /// <summary>
        /// 数据初始化
        /// </summary>
        public override void Init()
        {
            m_AllMonsterDict.Clear();

            foreach (MonsterBase monster in AllMonster)
            {
                if (m_AllMonsterDict.ContainsKey(monster.Id) == true)
                {
                    Debug.LogError($" {monster.Name} 有重复 ID");
                }
                else {
                    m_AllMonsterDict.Add(monster.Id, monster);
                }
            }
        }

        /// <summary>
        /// 根据 ID 查找 MOnster 数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MonsterBase FindMonsterById(int id) {
            return m_AllMonsterDict[id];
        }

        [XmlIgnore] // xml 序列化时忽略
        public Dictionary<int, MonsterBase> m_AllMonsterDict = new Dictionary<int, MonsterBase>();

        [XmlElement("AllMonster")]
        public List<MonsterBase> AllMonster { get; set; }
    }

    [System.Serializable]
    public class MonsterBase { 
        // ID
        [XmlAttribute("Id")]
        public int Id { get; set; }

        // Name
        [XmlAttribute("Name")]
        public string Name { get; set; }

        // 预制路径
        [XmlAttribute("OutLook")]
        public string OutLook { get; set; }

        // 怪物等级
        [XmlAttribute("Level")]
        public int Level { get; set; }

        // 怪物稀有度
        [XmlAttribute("Rare")]
        public int Rare { get; set; }

        // 怪物高度
        [XmlAttribute("Height")]
        public float Height { get; set; }
    }
}
