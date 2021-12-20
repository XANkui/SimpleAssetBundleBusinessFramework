using OfficeOpenXml;
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

                foreach (XmlNode node in xe.ChildNodes)
                {
                    XmlElement tempXE = (XmlElement)node;
                    string name = tempXE.GetAttribute("name");
                    string type = tempXE.GetAttribute("type");
                    Debug.Log($"{name} {type}");
                    XmlNode listNode = tempXE.FirstChild;
                    XmlElement listElement = (XmlElement)listNode;
                    string listName = listElement.GetAttribute("name");
                    string sheetName = listElement.GetAttribute("sheetname");
                    string mainkey = listElement.GetAttribute("mainkey");
                    Debug.Log($"{listName} {sheetName} {mainkey}");

                    foreach (XmlNode nd in listElement.ChildNodes)
                    {
                        XmlElement txe = (XmlElement)nd;
                        Debug.Log($"{txe.GetAttribute("name")} {txe.GetAttribute("col")} {txe.GetAttribute("type")}");
                    }
                }
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

        [MenuItem("MyTools/测试写入Excel")]
        public static void TextWriteExcel() {
            string xlsxPath = Application.dataPath + "/../Data/Excel/G怪物.xlsx";
            FileInfo xlsxFile = new FileInfo(xlsxPath);
            if (xlsxFile.Exists ==true)
            {
                xlsxFile.Delete();
                xlsxFile = new FileInfo(xlsxPath);
            }
            using (ExcelPackage package = new ExcelPackage(xlsxFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("怪物配置");

                //worksheet.DefaultColWidth = 10;     // sheet 页面默认的行宽
                //worksheet.DefaultRowHeight = 10;    // sheet 页面默认的列高
                //worksheet.Cells.Style.WrapText = true;  // 设置所有单元格默认自动换行
                //worksheet.InsertColumn(1,2);            // 从某行开始插入若干行
                //worksheet.InsertRow(1,2);               // 从某列开始插入若干列
                //worksheet.DeleteColumn(1,2);            // 从某行开始删除若干行
                //worksheet.DeleteRow(1,2);               // 从某列开始删除若干列
                //worksheet.Column(1).Width =10;          // 设置第几行宽度
                //worksheet.Row(1).Height =10;            // 设置第几列高度
                //worksheet.Column(1).Hidden =true;       // 设置第几行隐藏
                //worksheet.Row(1).Hidden =true;          // 设置第几列隐藏
                //worksheet.Column(1).Style.Locked =true; // 设置第几行锁定
                //worksheet.Row(1).Style.Locked =true;    // 设置第几列锁定
                //worksheet.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;// 设置所有单元格对齐方式
                worksheet.Cells.AutoFitColumns();   

                ExcelRange range = worksheet.Cells[1,1];
                range.Value = " Testt";
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.DarkDown; // 设置单元格填充方式
                //range.Style.Fill.BackgroundColor.SetColor(); // 设置单元格填充颜色
                //range.Style.Font.Color.SetColor();  // 设置单元格字体颜色
                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center; // 设置单元格对齐方式
                range.AutoFitColumns();
                range.Style.WrapText = true;


                package.Save();
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
