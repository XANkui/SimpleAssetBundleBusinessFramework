using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace AssetBundleBusinessFramework { 

	public class DataEditor 
	{
        private static string XmlPath = "Assets/GameData/Data/Xml/";
        private static string BinaryPath = "Assets/GameData/Data/Binary/";
        private static string ScriptsPath = "Assets/Example/Scripts/Data/";

        // 注意 Data 可能带有命名空间
        private static string DATA_NAMESPACE = "AssetBundleBusinessFramework.";

        [MenuItem("Assets/类转 xml")]
        public static void AssetClassToXml() {
            UnityEngine.Object[] objs = Selection.objects;
            for (int i = 0; i < objs.Length; i++)
            {
                EditorUtility.DisplayProgressBar("文件下的类转xml","正在扫描"+objs[i].name+" ... ...",1.0f/objs.Length * i);
                ClassToXml(objs[i].name);
            }

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        [MenuItem("Assets/ xml 转 Binary")]
        public static void AssetXmlToBinary()
        {
            UnityEngine.Object[] objs = Selection.objects;
            for (int i = 0; i < objs.Length; i++)
            {
                EditorUtility.DisplayProgressBar("文件下的 xml 转 二进制", "正在扫描" + objs[i].name + " ... ...", 1.0f / objs.Length * i);
                XmlToBinary(objs[i].name);
            }

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        [MenuItem("MyTools/Xml/Xml转成二进制")]
        public static void AllXmlToBinary() {
            string path = Application.dataPath.Replace("Assets","")+XmlPath;
            string[] filesPath = Directory.GetFiles(path,"*.*",SearchOption.AllDirectories);
            for (int i = 0; i < filesPath.Length; i++)
            {
                EditorUtility.DisplayProgressBar("查找文件夹下面的 Xml","正在扫描"+filesPath[i]+" ... ... ",1.0f/filesPath.Length*i);
                if (filesPath[i].EndsWith(".xml")==true)
                {
                    string tempPath = filesPath[i].Substring(filesPath[i].LastIndexOf("/")+1);
                    tempPath = tempPath.Replace(".xml","");
                    XmlToBinary(tempPath);
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        [MenuItem("MyTools/测试读取 reg类型.xml")]
        public static void TextReadXml() {
            string xmlPath = Application.dataPath + "/../Data/Reg/MonsterData.xml";
            XmlReader reader = null;
            try
            {
                XmlDocument xml = new XmlDocument();
                reader = XmlReader.Create(xmlPath);
                xml.Load(reader);
                XmlNode xn = xml.SelectSingleNode("data");
                XmlElement xe = (XmlElement)xn;
                string className = xe.GetAttribute("name");
                string xmlName = xe.GetAttribute("to");
                string excelName = xe.GetAttribute("from");
                reader.Close();
                Debug.Log($"{className} {xmlName} {excelName}");
            }
            catch (Exception e)
            {

                if (reader!=null)
                {
                    reader.Close();
                }

                Debug.LogError(e);
            }
        }

        /// <summary>
        /// 实际的类转 XML
        /// </summary>
        /// <param name="name"></param>
		private static void ClassToXml(string name) {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            try
            {
                name = DATA_NAMESPACE + name;
                Debug.Log($" {name} 类转 xml ... ");
                Type type = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type tempType = asm.GetType(name);
                    if (tempType != null)
                    {
                        type = tempType;
                        break;
                    }
                }

                if (type != null)
                {
                    var temp = Activator.CreateInstance(type);
                    if (temp is ExcelBase)
                    {
                        (temp as ExcelBase).Construction();
                    }
                    string xmlPath = XmlPath + name + ".xml";
                    BinarySerializeOpt.XmlSerialize(xmlPath, temp);
                    Debug.Log($"{name} 类转 xml 成功，xml 路径为：{xmlPath}");
                }
            }
            catch (Exception)
            {

                Debug.LogError($" {name} 类转 xml 失败");
            }
			
		}

        /// <summary>
        /// xml 转 二进制
        /// </summary>
        /// <param name="name"></param>
        private static void XmlToBinary(string name) {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            try
            {
                Debug.Log($" {name} 类转 xml ... ");
                Type type = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type tempType = asm.GetType(name);
                    if (tempType != null)
                    {
                        type = tempType;
                        break;
                    }
                }

                if (type != null)
                {
                    string xmlPath = XmlPath + name + ".xml";
                    string binaryPath = BinaryPath + name + ".bytes";
                    object obj = BinarySerializeOpt.XmlDeserialize(xmlPath, type);
                    BinarySerializeOpt.BinarySeralize(binaryPath, obj);

                    Debug.Log($" {name} xml 转 二进制 成功 ：{binaryPath}");
                }
            }
            catch (Exception)
            {

                Debug.LogError($" {name} xml 转 二进制 失败");
            }
        }
	}
}
