using OfficeOpenXml;
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

    public class SheetClass {
        // 所属父级 VarClass
        public VarClass ParentVar { get; set; }
        // 存下该节点的深度值
        public int Depth { get; set; }
        // 类名
        public string Name { get; set; }
        // 类对应的 sheet 名
        public string SheetName { get; set; }
        // 主键
        public string MainKey { get; set; }
        // 分隔符
        public string SplitStr { get; set; }
        // 所包含的变量
        public List<VarClass> VarList = new List<VarClass>();
    }

    //原类
    public class VarClass { 
        // 原类里面的变量的名称
        public string Name { get; set; }
        // 原类里面的变量的类型
        public string Type { get; set; }
        // 原类里面的变量对应的Excel列
        public string Col { get; set; }
        // 原类里面的变量的默认值
        public string DefaultValue { get; set; }
        // 原类里面的变量是list的话，外联部分列
        public string Foreign { get; set; }
        // 原类里面的分隔符
        public string SplitStr { get; set; }
        // 如果自己是list，对应的 list 类名
        public string ListName { get;set; }
        // 如果自己是list，对应的 sheet 名
        public string ListSheetName { get; set; }
    }

    public class SheetData {
        public List<string> AllName = new List<string>();
        public List<string> AllType = new List<string>();
        public List<RowData> AllData = new List<RowData>();
    }

    public class RowData {
        public string ParentValue = "";
        public Dictionary<string, string> RowDataDict = new Dictionary<string, string>();
    }

    #region 测试反射功能使用
    public enum TestEnum
    {
        None = 0,
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
    #endregion

    public class DataEditor 
	{
        private static string XmlPath = ReadDataConfig.GetDataConfig().XmlPath;
        private static string BinaryPath = ReadDataConfig.GetDataConfig().BinaryPath;
        private static string ScriptsPath = ReadDataConfig.GetDataConfig().ScriptsPath;

        private static string ExcelPath = Application.dataPath+"/../Data/Excel/";
        private static string RegPath = Application.dataPath+"/../Data/Reg/";
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

        [MenuItem("Assets/ xml 转 Excel")]
        public static void AssetXmlToExcel()
        {
            UnityEngine.Object[] objs = Selection.objects;
            for (int i = 0; i < objs.Length; i++)
            {
                EditorUtility.DisplayProgressBar("文件下的 xml 转 Excel", "正在扫描" + objs[i].name + " ... ...", 1.0f / objs.Length * i);
                XmlToEcxel(objs[i].name);
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

        [MenuItem("MyTools/Xml/Excel转Xml")]
        public static void AllExcelToXml() {
            string[] filePath = Directory.GetFiles(RegPath,"*",SearchOption.AllDirectories);
            for (int i = 0; i < filePath.Length; i++)
            {
                if (filePath[i].EndsWith(".xml")==false)
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar("查找文件夹下的类","正在扫描路径"+filePath[i]+"... ...",
                    1.0f/filePath.Length * i);
                string path = filePath[i].Substring(filePath[i].LastIndexOf("/")+1);
                ExcelToXml(path.Replace(".xml", ""));
            }

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        private static void ExcelToXml(string name ) {
            string className = "";
            string xmlName = "";
            string excelName = "";
            // 第一步 读取 Reg文件，确定类的结构
            Dictionary<string, SheetClass> allSheetClassDict = ReadReg(name, ref className, ref xmlName, ref excelName); // 注意可能的命名空间

            // 第二步 读取 excel 里面的数据
            string excelPath = ExcelPath + excelName;
            Dictionary<string, SheetData> sheetDataDict = new Dictionary<string, SheetData>();
            try
            {
                using (FileStream stream = new FileStream(excelPath,FileMode.Open,FileAccess.Read,FileShare.ReadWrite))
                {
                    using (ExcelPackage package = new ExcelPackage(stream))
                    {
                        ExcelWorksheets workSheetArray = package.Workbook.Worksheets;
                        for (int i = 0; i < workSheetArray.Count; i++)
                        {
                            SheetData sheetData = new SheetData();
                            ExcelWorksheet workSheet = workSheetArray[i+1];
                            SheetClass sheetClass = allSheetClassDict[workSheet.Name];
                            int colCount = workSheet.Dimension.End.Column;
                            int rowCount = workSheet.Dimension.End.Row;

                            for (int n = 0; n < sheetClass.VarList.Count; n++)
                            {
                                sheetData.AllName.Add(sheetClass.VarList[n].Name);
                                sheetData.AllType.Add(sheetClass.VarList[n].Type);
                            }

                            for (int m = 1; m < rowCount; m++) // Excel 第一行是标题栏
                            {
                                RowData rowData = new RowData();
                                int n = 0;
                                if (string.IsNullOrEmpty(sheetClass.SplitStr)
                                    && sheetClass.ParentVar !=null 
                                    && string.IsNullOrEmpty(sheetClass.ParentVar.Foreign)==false)
                                {
                                    rowData.ParentValue = workSheet.Cells[m + 1, 1].Value.ToString().Trim();
                                    n = 1;
                                }
                                for (; n < colCount; n++)
                                {
                                    ExcelRange range = workSheet.Cells[m+1,n+1]; // 索引是从1开始，而非 0
                                    string value = "";
                                    if (range.Value!=null)
                                    {
                                        value = range.Value.ToString().Trim();
                                    }
                                    
                                    string colValue = workSheet.Cells[1, n + 1].Value.ToString();
                                    rowData.RowDataDict.Add(GetNameFromCol(sheetClass.VarList, colValue),
                                        value);
                                }

                                sheetData.AllData.Add(rowData);
                            }

                            sheetDataDict.Add(workSheet.Name,sheetData);
                        }
                    }
                }
            }
            catch (Exception e)
            {

                Debug.LogError($"{ name },{className},{xmlName},{excelName}:{e}");
                return;
            }

            // 根据类的结构，创建类，并且给每个变量赋值（从Excel里读取的值）
            object objClass = CreateClass(className);
            List<string> outKeyList = new List<string>();
            foreach (string str in allSheetClassDict.Keys)
            {
                SheetClass sheetClass = allSheetClassDict[str];
                if (sheetClass.Depth == 1)
                {
                    outKeyList.Add(str);
                }
            }

            for (int i = 0; i < outKeyList.Count; i++)
            {
                ReadDataToClass(objClass,allSheetClassDict[outKeyList[i]],sheetDataDict[outKeyList[i]],
                    allSheetClassDict,sheetDataDict,null);
            }

            BinarySerializeOpt.XmlSerialize(XmlPath+xmlName,objClass);
            // 转为二进制
           // BinarySerializeOpt.BinarySeralize(BinaryPath+className+".bytes",objClass);
            Debug.Log($"{excelName} 表导入 Unity 完成");
            AssetDatabase.Refresh();
        }

        private static void ReadDataToClass(object objClass, SheetClass sheetClass,SheetData sheetData,
            Dictionary<string, SheetClass> allSheetClassDict, Dictionary<string, SheetData> sheetDataDict,
            object keyValue) {
            object item = CreateClass(DATA_NAMESPACE+ sheetClass.Name); // 只是为了得到变量类型
            object list = CreateList(item.GetType());
            for (int i = 0; i < sheetData.AllData.Count; i++)
            {
                if (keyValue!=null && string.IsNullOrEmpty(sheetData.AllData[i].ParentValue)==false)
                {
                    if (sheetData.AllData[i].ParentValue !=keyValue.ToString())
                    {
                        continue;
                    }
                }

                object addItem = CreateClass(DATA_NAMESPACE + sheetClass.Name);
                for (int j = 0; j < sheetClass.VarList.Count; j++)
                {
                    VarClass varClass = sheetClass.VarList[j];
                    if (varClass.Type == "list" && string.IsNullOrEmpty(varClass.SplitStr)==true)
                    {
                        ReadDataToClass(addItem, allSheetClassDict[varClass.ListSheetName], sheetDataDict[varClass.ListSheetName], allSheetClassDict,sheetDataDict,
                            GetMemberValue(addItem,sheetClass.MainKey));
                    }
                    else if (varClass.Type == "list")
                    {
                        string vale = sheetData.AllData[i].RowDataDict[sheetData.AllName[j]];
                        SetSplitClass(addItem,allSheetClassDict[varClass.ListSheetName],vale);
                    }
                    else if (varClass.Type == "listStr" || varClass.Type == "listFloat"
                        || varClass.Type == "listInt" || varClass.Type == "listBool")
                    {
                        string vale = sheetData.AllData[i].RowDataDict[sheetData.AllName[j]] ;
                        SetSplitBaseClass(addItem, varClass,vale);
                    }
                    else {
                        string value = sheetData.AllData[i].RowDataDict[sheetData.AllName[j]];
                        if (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(varClass.DefaultValue)==false)
                        {
                            value = varClass.DefaultValue;
                        }
                        if (string.IsNullOrEmpty(value)==true)
                        {
                            Debug.LogError($"表格中有空数据，或者 Reg 文件未配置 defaultValue 值， {sheetData.AllName[j]}");
                            continue;
                        }
                        SetValue(addItem.GetType().GetProperty(sheetData.AllName[j]),addItem,value,
                            sheetData.AllType[j]);
                    }
                }

                list.GetType().InvokeMember("Add",BindingFlags.Default|BindingFlags.InvokeMethod,null,list,
                    new object[] { addItem});
            }

            objClass.GetType().GetProperty(sheetClass.ParentVar.Name).SetValue(objClass,list);
        }

        /// <summary>
        /// 自定义类 List 赋值
        /// </summary>
        /// <param name="objClass"></param>
        /// <param name="sheetClass"></param>
        /// <param name="value"></param>
        private static void SetSplitClass(object objClass,SheetClass sheetClass, string value) {
            object item = CreateClass(sheetClass.Name);
            object list = CreateList(item.GetType());

            if (string.IsNullOrEmpty(value)==true)
            {
                Debug.Log($"Excel 里面自定义 list 的列表中有空值：{sheetClass.Name}");
                return;
            }

            string splitStr = sheetClass.ParentVar.SplitStr.Replace("\\n","\n").Replace("\\r","\r");
            string[] rowArray = value.Split(new string[] { splitStr},StringSplitOptions.None);
            for (int i = 0; i < rowArray.Length; i++)
            {
                object addItem = CreateClass(sheetClass.Name);
                string[] valueList = rowArray[i].Trim().Split(new string[] { sheetClass.SplitStr },StringSplitOptions.None);
                for (int j = 0; j < rowArray.Length; j++)
                {
                    SetValue(addItem.GetType().GetProperty(sheetClass.VarList[j].Name),
                        addItem,valueList[j].Trim(), sheetClass.VarList[j].Type);
                }

                list.GetType().InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod,
                    null, list, new object[] { addItem });
            }
        }

        /// <summary>
        /// 基础 list 赋值
        /// </summary>
        /// <param name="objClass"></param>
        /// <param name="varClass"></param>
        /// <param name="value"></param>
        private static void SetSplitBaseClass(object objClass,VarClass varClass, string value) {
            Type type = null;

            if (varClass.Type =="listStr")
            {
                type = typeof(string);
            }
            else if (varClass.Type == "listFloat")
            {
                type = typeof(float);
            }
            else if (varClass.Type == "listInt")
            {
                type = typeof(int);
            }
            else if (varClass.Type == "listBool")
            {
                type = typeof(bool);
            }
            object list = CreateList(type);
            string[] rowArray = value.Split(new string[] { varClass.SplitStr},StringSplitOptions.None);
            for (int i = 0; i < rowArray.Length; i++)
            {
                object addItem = rowArray[i].Trim();
                try
                {
                    list.GetType().InvokeMember("Add",BindingFlags.Default| BindingFlags.InvokeMethod,null,
                        list,new object[] { addItem});
                }
                catch (Exception)
                {

                    Debug.Log($"{ varClass.ListSheetName} 里 {varClass.Name} 列表添加失败，具体数据值：{addItem}");
                }

            }
            objClass.GetType().GetProperty(varClass.Name).SetValue(objClass,list);
        }

        /// <summary>
        /// 根据列名获取变量名
        /// </summary>
        /// <param name="varList"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        private static string GetNameFromCol(List<VarClass> varList,string col) {
            foreach (VarClass varClass in varList)
            {
                if (varClass.Col == col)
                {
                    return varClass.Name;
                }
            }

            return null;
        }

        private static void XmlToEcxel(string name) {

            // 去除可能的带命名空间
            string[] splitNames = name.Split('.');
            name = splitNames[splitNames.Length-1];


            string className = "";
            string xmlName = "";
            string excelName = "";
            Dictionary<string, SheetClass> allSheetClassDict = ReadReg(name,ref className,ref xmlName,ref excelName); // 注意可能的命名空间

            object data = GetObjectFromXml(className);

            // Excel sheet 表单数据存储
            Dictionary<string, SheetData> sheetDataDict = new Dictionary<string, SheetData>();
            List<SheetClass> outSheetClassList = new List<SheetClass>();
            foreach (SheetClass sheetClass in allSheetClassDict.Values)
            {
                if (sheetClass.Depth==1)
                {
                    outSheetClassList.Add(sheetClass);
                }
            }

            for (int i = 0; i < outSheetClassList.Count; i++)
            {
                ReadData(data,outSheetClassList[i],allSheetClassDict,sheetDataDict,"");
            }

            string xlsxPath =ExcelPath+excelName;
            if (FileIsUsed(xlsxPath) ==true)
            {
                Debug.LogError($"文件被占用：{xlsxPath}");
                return;
            }

            try
            {
                FileInfo xlsxFile = new FileInfo(xlsxPath);
                if (xlsxFile.Exists == true)
                {
                    xlsxFile.Delete();
                    xlsxFile = new FileInfo(xlsxPath);
                }

                using (ExcelPackage package = new ExcelPackage(xlsxFile))
                {
                    foreach (string str in sheetDataDict.Keys)
                    {
                        ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(str);                        
                        SheetData sheetData = sheetDataDict[str];
                        for (int i = 0; i < sheetData.AllName.Count; i++)
                        {
                            ExcelRange range = workSheet.Cells[1, i + 1];
                            range.Value = sheetData.AllName[i];
                            range.AutoFitColumns();
                        }

                        for (int i = 0; i < sheetData.AllData.Count; i++)
                        {
                            RowData rowData = sheetData.AllData[i];
                            for (int j = 0; j < sheetData.AllData[i].RowDataDict.Count; j++)
                            {
                                ExcelRange range = workSheet.Cells[i + 2, j + 1];
                                string value = rowData.RowDataDict[sheetData.AllName[j]];
                                range.Value = value;
                                range.AutoFitColumns();
                                // 可能的换行处理
                                if (value.Contains("\n") || value.Contains("\r\n"))
                                {
                                    range.Style.WrapText = true;
                                }
                            }
                        }
                        workSheet.Cells.AutoFitColumns();
                    }

                    package.Save();
                }
            }
            catch (Exception e)
            {

                Debug.LogError(e);
                Debug.LogError($"生成 {xlsxPath} 失败");
                return;
            }
            

            Debug.Log($"生成 {xlsxPath} 成功");
        }

        /// <summary>
        /// 读取 Excel 表单结构
        /// </summary>
        /// <param name="name"></param>
        /// <param name="className"></param>
        /// <param name="xmlName"></param>
        /// <param name="excelName"></param>
        /// <returns></returns>
        private static Dictionary<string, SheetClass> ReadReg(string name,ref string className,ref string xmlName,ref string excelName) {
            string regPath = RegPath + name + ".xml";

            if (File.Exists(regPath) == false)
            {
                Debug.LogError($"此数据不存在配置文件xml: {regPath}");
                return null;
            }

            XmlDocument xml = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true; // 忽略xml中注释
            XmlReader reader = XmlReader.Create(regPath, settings);
            xml.Load(reader);

            XmlNode xn = xml.SelectSingleNode("data");
            XmlElement xe = (XmlElement)xn;
            className = xe.GetAttribute("name");
            className = DATA_NAMESPACE + className; // 如果类有命名空间，注意添加上命名空间
            xmlName = xe.GetAttribute("to");
            excelName = xe.GetAttribute("from");

            // 储存所有变量的表
            Dictionary<string, SheetClass> allSheetClassDict = new Dictionary<string, SheetClass>();
            ReadXmlNode(xe, allSheetClassDict, 0);

            reader.Close();

            return allSheetClassDict;
        }

        /// <summary>
        /// 反序列化 xml 到类
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        private static object GetObjectFromXml(string className) {
            object data = null;
            Type type = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type tempType = asm.GetType(className); // 如果类有命名空间，注意添加上命名空间
                if (tempType != null)
                {
                    type = tempType;
                    break;
                }
            }

            if (type != null)
            {
                string xmlPath = XmlPath + className + ".xml";
                return data = BinarySerializeOpt.XmlDeserialize(xmlPath, type);
            }

            return null;
        }

        /// <summary>
        /// 递归读取类里面的数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sheetClass"></param>
        /// <param name="allSheetClassDict"></param>
        /// <param name="sheetDataDict"></param>
        private static void ReadData(object data, SheetClass sheetClass,Dictionary<string,SheetClass> allSheetClassDict,
           Dictionary<string, SheetData> sheetDataDict, string mainKey) {

            List<VarClass> varList = sheetClass.VarList;
            VarClass varClass = sheetClass.ParentVar;
            object dataList = GetMemberValue(data,varClass.Name);

            int listCount = System.Convert.ToInt32(dataList.GetType().InvokeMember("get_Count",
                BindingFlags.Default | BindingFlags.InvokeMethod, null,dataList,new object[] { }));

            SheetData sheetData = new SheetData();

            if (string.IsNullOrEmpty(varClass.Foreign)==false)
            {
                sheetData.AllName.Add(varClass.Foreign);
                sheetData.AllType.Add(varClass.Type);
            }
            
            for (int i = 0; i < varList.Count; i++)
            {
                if (string.IsNullOrEmpty(varList[i].Col)==false)
                {
                    sheetData.AllName.Add(varList[i].Col);
                    sheetData.AllType.Add(varList[i].Type);
                }
            }

            string tempKey = mainKey;
            for (int i = 0; i < listCount; i++)
            {
                object item = dataList.GetType().InvokeMember("get_Item",
                BindingFlags.Default | BindingFlags.InvokeMethod, null, dataList, new object[] { i});

                RowData rowData = new RowData();

                if (string.IsNullOrEmpty(varClass.Foreign)==false && string.IsNullOrEmpty(tempKey)==false)
                {
                    rowData.RowDataDict.Add(varClass.Foreign,tempKey);
                }

                if (string.IsNullOrEmpty(sheetClass.MainKey)==false)
                {
                    mainKey = GetMemberValue(item,sheetClass.MainKey).ToString();
                }

                for (int j = 0; j < varList.Count; j++)
                {
                    if (varList[j].Type == "list" && string.IsNullOrEmpty(varList[j].SplitStr)==true)
                    {
                        SheetClass tempSheetClass = allSheetClassDict[varList[j].ListSheetName];
                        ReadData(item, tempSheetClass, allSheetClassDict, sheetDataDict,mainKey);
                    }
                    else if (varList[j].Type == "list")
                    {
                        SheetClass tempSheetClass = allSheetClassDict[varList[j].ListSheetName];
                        string value = GetSplitStrList(item,varList[j], tempSheetClass);
                        rowData.RowDataDict.Add(varList[j].Col, value);
                    }
                    else if (varList[j].Type == "listStr" || varList[j].Type=="listFloat"
                        || varList[j].Type == "listInt" || varList[j].Type == "listBool")
                    {
                        string value = GetSplitbaseList(item,varList[j]);
                        rowData.RowDataDict.Add(varList[j].Col,value);
                    }
                    else {
                        object value = GetMemberValue(item,varList[j].Name);
                        if (varList != null)
                        {
                            rowData.RowDataDict.Add(varList[j].Col, value.ToString());
                        }
                        else {
                            Debug.LogError(varList[j].Name+"反射出来为空");
                        }
                    }
                }

                string key = varClass.ListSheetName;
                if (sheetDataDict.ContainsKey(key))
                {
                    sheetDataDict[key].AllData.Add(rowData);
                }
                else {
                    sheetData.AllData.Add(rowData);
                    sheetDataDict.Add(key,sheetData);
                }
            }
            
        }

        /// <summary>
        /// 获取本身是一个类列表,但是数据比较少(没有办法确定父级结构的)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="varClass"></param>
        /// <param name="sheetClass"></param>
        /// <returns></returns>
        private static string GetSplitStrList(object data, VarClass varClass,SheetClass sheetClass) {
            string split = varClass.SplitStr;
            string classSplit = sheetClass.SplitStr;
            string str = "";

            if (string.IsNullOrEmpty(split)|| string.IsNullOrEmpty(classSplit))
            {
                Debug.LogError("类的列类分隔符或变量分隔符为空！！！");
                return str;
            }
            object dataList = GetMemberValue(data,varClass.Name);
            int listCount = System.Convert.ToInt32(dataList.GetType().InvokeMember("get_Count", BindingFlags.Default
               | BindingFlags.InvokeMethod, null, dataList, new object[] { }));
            for (int i = 0; i < listCount; i++)
            {
                object item = dataList.GetType().InvokeMember("get_Item",BindingFlags.Default
                    |BindingFlags.InvokeMethod, null,dataList,new object[] { i});

                for (int j = 0; j < sheetClass.VarList.Count; j++)
                {
                    object value = GetMemberValue(item,sheetClass.VarList[j].Name);
                    str += value.ToString();
                    if (j!=sheetClass.VarList.Count-1)
                    {
                        str += classSplit.Replace("\\n","\n").Replace("\\r","\r");//注意换行转义处理
                    }
                }
                if (i!=listCount-1)
                {
                    str += split.Replace("\\n", "\n").Replace("\\r", "\r"); //注意换行转义处理
                }
            }
            return str;
        }


        /// <summary>
        /// 获取基础 List 里面的所有值
        /// </summary>
        /// <param name="data"></param>
        /// <param name="varClass"></param>
        /// <returns></returns>
        private static string GetSplitbaseList(object data, VarClass varClass) {
            string str = "";

            if (string.IsNullOrEmpty(varClass.SplitStr)==true)
            {
                Debug.LogError("基础的 list 的分隔符为空");
                return str;
            }

            object dataList = GetMemberValue(data,varClass.Name);
            int listCount = System.Convert.ToInt32(dataList.GetType().InvokeMember("get_Count",BindingFlags.Default
                |BindingFlags.InvokeMethod, null,dataList,new object[] { }));

            for (int i = 0; i < listCount; i++)
            {
                object item = dataList.GetType().InvokeMember("get_Item", BindingFlags.Default
                | BindingFlags.InvokeMethod, null, dataList, new object[] { i });
                str += item.ToString();
                if (i!=listCount-1)
                {
                    str += varClass.SplitStr.Replace("\\n", "\n").Replace("\\r", "\r"); //注意换行转义处理
                }
            }

            return str;
        }

        /// <summary>
        /// 递归读取配置文件
        /// </summary>
        /// <param name="xmlElement"></param>
        private static void ReadXmlNode(XmlElement xmlElement, Dictionary<string, SheetClass> allSheetClassDict, int depth) {
            depth++;
            foreach (XmlNode node in xmlElement.ChildNodes)
            {
                
                XmlElement xe = (XmlElement)node;
                if (xe.GetAttribute("type")=="list")
                {
                    XmlElement listEle = (XmlElement)node.FirstChild;
                    VarClass parentVar = new VarClass() {
                        Name = xe.GetAttribute("name"),
                        Type = xe.GetAttribute("type"),
                        Col = xe.GetAttribute("col"),
                        DefaultValue = xe.GetAttribute("defaultValue"),
                        Foreign = xe.GetAttribute("foreign"),
                        SplitStr = xe.GetAttribute("split"),
                    };

                    if (parentVar.Type == "list")
                    {
                        parentVar.ListName = ((XmlElement)xe.FirstChild).GetAttribute("name");
                        parentVar.ListSheetName = ((XmlElement)xe.FirstChild).GetAttribute("sheetname");
                    }

                    SheetClass sheetClass = new SheetClass() {
                        Name = listEle.GetAttribute("name"),
                        SheetName = listEle.GetAttribute("sheetname"),
                        SplitStr = listEle.GetAttribute("split"),
                        MainKey = listEle.GetAttribute("mainKey"),
                        ParentVar= parentVar,
                        Depth=depth
                    };

                    if (string.IsNullOrEmpty(sheetClass.SheetName)==false)
                    {
                        if (allSheetClassDict.ContainsKey(sheetClass.SheetName)==false)
                        {
                            // 获取该类下面所有变量
                            foreach (XmlNode insideNode in listEle.ChildNodes)
                            {
                                XmlElement insideXe = (XmlElement)insideNode;

                                VarClass varClass = new VarClass()
                                {
                                    Name = insideXe.GetAttribute("name"),
                                    Type = insideXe.GetAttribute("type"),
                                    Col = insideXe.GetAttribute("col"),
                                    DefaultValue = insideXe.GetAttribute("defaultValue"),
                                    Foreign = insideXe.GetAttribute("foreign"),
                                    SplitStr = insideXe.GetAttribute("split"),
                                };

                                if (varClass.Type == "list")
                                {
                                    varClass.ListName = ((XmlElement)insideXe.FirstChild).GetAttribute("name");
                                    varClass.ListSheetName = ((XmlElement)insideXe.FirstChild).GetAttribute("sheetname");
                                }

                                sheetClass.VarList.Add(varClass);
                            }

                            allSheetClassDict.Add(sheetClass.SheetName,sheetClass);
                        }
                    }

                    ReadXmlNode(listEle,allSheetClassDict, depth);
                }
            }
        }

        /// <summary>
        /// 判断文件是否占用
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool FileIsUsed(string path) {
            bool result = false;
            if (File.Exists(path) == false)
            {
                result = false;
            }
            else {
                FileStream fileStream = null;
                try
                {
                    fileStream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    result = false;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    result = true;
                }
                finally {
                    if (fileStream!=null)
                    {
                        fileStream.Close();
                    }
                }
            }

            return result;
        }

        #region 测试
        [MenuItem("MyTools/测试/读取 reg类型.xml")]
        public static void TestTextReadXml()
        {
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

                if (reader != null)
                {
                    reader.Close();
                }

                Debug.LogError(e);
            }
        }

        [MenuItem("MyTools/测试/写入Excel")]
        public static void TestTextWriteExcel()
        {
            string xlsxPath = Application.dataPath + "/../Data/Excel/G怪物.xlsx";
            FileInfo xlsxFile = new FileInfo(xlsxPath);
            if (xlsxFile.Exists == true)
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

                ExcelRange range = worksheet.Cells[1, 1];
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
        #endregion


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
