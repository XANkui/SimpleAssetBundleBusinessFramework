﻿using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace AssetBundleBusinessFramework {

    public enum TestEnum { 
        None=0,
        VAR1,

    }
    public class TestInfo
    {
        public int Id;
        public string Name;
        public bool IsA;
        public List<string> AllStrList;
        public List<TestInfo2> AllTestInfo2List;
    }

    public class TestInfo2
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsA { get; set; }
    }

    public class TestInfo3
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsA { get; set; }
        public float Height { get; set; }
        public TestEnum TEnum { get; set; }

        public List<string> AllStrList { get; set; }
        public List<TestInfo2> AllTestInfo2List { get; set; }
    }
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

        [MenuItem("MyTools/测试/读取 reg类型.xml")]
        public static void TestTextReadXml() {
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

        [MenuItem("MyTools/测试/写入Excel")]
        public static void TestTextWriteExcel() {
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

        #region 测试反射

       

        [MenuItem("MyTools/测试/测试反射/获取已存在类的值")]
        public static void TestReflection()
        {
            TestInfo testInfo = new TestInfo() {
                Id = 2,
                Name = "测试反射",
                IsA = false,
                AllStrList = new List<string>(),
                AllTestInfo2List = new List<TestInfo2>()
            };
            testInfo.AllStrList.Add("Test 111");
            testInfo.AllStrList.Add("Test 222");
            testInfo.AllStrList.Add("Test 333");

            for (int i = 0; i < 3; i++)
            {
                testInfo.AllTestInfo2List.Add(new TestInfo2()
                {
                    Id = i,
                    Name = "TestInfo2 " + i,
                    IsA = (i % 2 == 0 ? true : false)
                });
            }

            Debug.LogError(GetMemberValue(testInfo,"Name")+" " + GetMemberValue(testInfo, "Id"));

            object list = GetMemberValue(testInfo, "AllStrList");
            int listCount = System.Convert.ToInt32(list.GetType().InvokeMember("get_Count",BindingFlags.Default|BindingFlags.InvokeMethod,null,list,new object[] { }));
            Debug.LogError(listCount);
            for (int i = 0; i < listCount; i++)
            {
                object item = list.GetType().InvokeMember("get_Item",BindingFlags.Default | BindingFlags.InvokeMethod,null,list,new object[] { i});
                Debug.LogError(item);
            }

            object listInfo2 = GetMemberValue(testInfo, "AllTestInfo2List");
            int listInfo2Count = System.Convert.ToInt32(listInfo2.GetType().InvokeMember("get_Count", BindingFlags.Default | BindingFlags.InvokeMethod, null, listInfo2, new object[] { }));
            Debug.LogError(listInfo2Count);
            for (int i = 0; i < listInfo2Count; i++)
            {
                object item = listInfo2.GetType().InvokeMember("get_Item", BindingFlags.Default | BindingFlags.InvokeMethod, null, listInfo2, new object[] { i });

                object name = GetMemberValue(item,"Name");
                object id = GetMemberValue(item,"Id");

                Debug.LogError(id + " "+ name);
            }

        }

        [MenuItem("MyTools/测试/测试反射/根据已有数据进行实例类创建赋值")]
        public static void TestReflection2() {
            //object obj = CreateClass("AssetBundleBusinessFramework.DataEditor.TestInfo2"); // 内部类好似也不可以，所以把类提出去了
            object obj = CreateClass("AssetBundleBusinessFramework.TestInfo3");
            PropertyInfo idInfo = obj.GetType().GetProperty("Id");
            //idInfo.SetValue(obj,System.Convert.ToInt32("20"));
            SetValue(idInfo,obj,"22","int");
            PropertyInfo nameInfo = obj.GetType().GetProperty("Name");
            //nameInfo.SetValue(obj, "Test ha ha");
            SetValue(nameInfo,obj,"Test o o o","string");
            PropertyInfo isAInfo = obj.GetType().GetProperty("IsA");
            //isAInfo.SetValue(obj, System.Convert.ToBoolean("true"));
            SetValue(isAInfo,obj,"false","bool");
            PropertyInfo heightInfo = obj.GetType().GetProperty("Height");
            //heightInfo.SetValue(obj,System.Convert.ToSingle("17.9"));
            SetValue(heightInfo, obj, "11.5", "float");
            PropertyInfo enumInfo = obj.GetType().GetProperty("TEnum");
            //object enumInfoVal = TypeDescriptor.GetConverter(enumInfo.PropertyType).ConvertFromInvariantString("VAR1"); ;
            //enumInfo.SetValue(obj,enumInfoVal);
            SetValue(enumInfo, obj, "VAR1", "enum");

            Type type = typeof(string);
            object list = CreateList(type);  // new 出来这个 list
            for (int i = 0; i < 3; i++)
            {
                object addItem = "Test num "+i;
                list.GetType().InvokeMember("Add",BindingFlags.Default|BindingFlags.InvokeMethod,null,list,new object[] { addItem});

            }
            obj.GetType().GetProperty("AllStrList").SetValue(obj,list);


            object twoList = CreateList(typeof(TestInfo2));
            for (int i = 0; i < 3; i++)
            {
                object addItem = CreateClass("AssetBundleBusinessFramework.TestInfo2");
                PropertyInfo itemIdInfo = addItem.GetType().GetProperty("Id");
                SetValue(itemIdInfo,addItem,"22"+i,"int");
                PropertyInfo itemNameInfo = addItem.GetType().GetProperty("Name");
                SetValue(itemNameInfo, addItem, "Name" + i, "string");
                PropertyInfo itemIsAInfo = addItem.GetType().GetProperty("IsA");
                SetValue(itemIsAInfo, addItem, "true", "bool");
                twoList.GetType().InvokeMember("Add", BindingFlags.Default|BindingFlags.InvokeMethod,null,twoList,new object[] { addItem});
            }
            obj.GetType().GetProperty("AllTestInfo2List").SetValue(obj, twoList);

            TestInfo3 testInfo3 = obj as TestInfo3;
            Debug.LogError($"{testInfo3.Id} {testInfo3.Name} {testInfo3.IsA} {testInfo3.Height} {testInfo3.TEnum}");
            foreach (string str in testInfo3.AllStrList)
            {
                Debug.LogError(str);
            }
            foreach (TestInfo2 item in testInfo3.AllTestInfo2List)
            {
                Debug.LogError($"{item.Id} {item.Name} {item.IsA}");
            }
        }
        /// <summary>
        /// 反射获取存在类的值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="memberName"></param>
        /// <param name="bindingFlags"></param>
        /// <returns></returns>
        private static object GetMemberValue(object obj,string memberName,
            BindingFlags bindingFlags= BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public) {
            Type type = obj.GetType();
            MemberInfo[] members = type.GetMember(memberName, bindingFlags);
            while (members == null || members.Length == 0)
            {
                type = type.BaseType;
                if (type == null)
                {
                    return null;
                }

                members = type.GetMember(memberName, bindingFlags);
            }

            switch (members[0].MemberType)
            {
                
                case MemberTypes.Field:
                    return type.GetField(memberName, bindingFlags).GetValue(obj);
                 
               
                case MemberTypes.Property:
                    return type.GetProperty(memberName, bindingFlags).GetValue(obj);
               
                default:
                    return null;
            }
        }

        /// <summary>
        /// 反射创建实例类
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static object CreateClass(string name) {
            object obj = null;
            Type type = null;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type tempType = assembly.GetType(name);
                if (tempType !=null)
                {
                    type = tempType;
                    break;
                }
            }

            if (type!=null)
            {
                obj = Activator.CreateInstance(type);
            }

            return obj;
        }

        private static void SetValue(PropertyInfo info, object var, string value,string type) {
            object val = value;

            if (type=="int")
            {
                val = System.Convert.ToInt32(val);
            }else if (type == "bool")
            {
                val = System.Convert.ToBoolean(val);
            }
            else if (type == "float")
            {
                val = System.Convert.ToSingle(val);
            }
            else if (type == "enum")
            {
                val = TypeDescriptor.GetConverter(info.PropertyType).ConvertFromInvariantString(val.ToString());
            }

            info.SetValue(var,val);
        }

        /// <summary>
        /// 反射 new 一个 list
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static object CreateList(Type type) {
            Type listType = typeof(List<>);
            Type specType = listType.MakeGenericType(new System.Type[] { type });// 确定 List<T> T的类型
            object list = Activator.CreateInstance(specType, new object[] { });  // new 出来这个 list
            return list;
        }

        #endregion


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
