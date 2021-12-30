using AssetBundleBusinessFramework.Tools;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace AssetBundleBusinessFramework
{ 

	public class AssetBundleBuildFlowEditor 
	{
		// AB 打包输出路径
		private readonly static string AB_BUILD_OUTPUT_PATH = Application.dataPath+ "/../AssetBundle/" + EditorUserBuildSettings.activeBuildTarget.ToString();
		// AB 配置路径
		private readonly static string ABCONFIG_PATH = "Assets/AssetBundleBusinessFramework/Editor/ABConfig.asset";

		private  readonly static string AB_BYTE_PATH = ReadDataConfig.GetDataConfig().ABBytePath;
		// key ab包名，value 是路径，所有 文件夹 ab 包的 dic 
		private static Dictionary<string, string> m_AllFileDirDict = new Dictionary<string, string>();
		// 记录所有 ab 资源路径的列表，用于过滤使用
		private static List<string> m_AllFileABList = new List<string>();

		// 单个 prefab 的 ab 包数据字典（ prefab名字：依赖的资源路径）
		private static Dictionary<string, List<string>> m_AllPrefabDirDict = new Dictionary<string, List<string>>();

		// 记录所有 ab 资源路径的列表，用于 prefab xml config 过滤使用
		private static List<string> m_XmlConfigFilterList = new List<string>();

		[MenuItem("MyTools/MyAssetBundle/打包 AB 包")]
		public static void Build() {

			// 配置表转为二进制（游戏配置表打包）
			DataEditor.AllExcelToXml();

			// 收集 ABConfig 的所有需要打AB包的资源和依赖资源
			CollectABConfigAllABAndDependenciesInfo();

			// 给对应所有资源打上 AB 标签
			SetAllABNameLabel();

			// 正式 AB 打包
			BuildAssetsToAB();

			// 清空所有资源的 AB 标签
			CleallAllABNameLabel();

			// 刷新 Project 资源  
			AssetDatabase.Refresh(); 
		}

		#region 收集 ABConfig 的所有需要打AB包的资源和依赖资源

		static void CollectABConfigAllABAndDependenciesInfo() {
			Debug.LogWarning(" CollectABConfigAllABAndDependenciesInfo 收集 ABConfig 的所有需要打AB包的资源和依赖资源");
			// 执行前清除之前的记录
			m_AllFileDirDict.Clear();
			m_AllFileABList.Clear();
			m_AllPrefabDirDict.Clear();
			m_XmlConfigFilterList.Clear();

			// 读取AB配置文件信息
			ABConfig abConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIG_PATH);
			foreach (ABConfig.FileDirABName fileDir in abConfig.m_AllFileDirAB)
			{
				if (m_AllFileDirDict.ContainsKey(fileDir.ABName))
				{
					Debug.LogError($"AB 包配置名字 {fileDir.ABName} 重复，请检查");
				}
				else
				{
					m_AllFileDirDict.Add(fileDir.ABName, fileDir.Path);
					m_AllFileABList.Add(fileDir.Path);
					m_XmlConfigFilterList.Add(fileDir.Path);
				}
			}

			// 获取指定prefabs文件夹下所有 prefabs 
			string[] allStr = AssetDatabase.FindAssets("t:Prefab", abConfig.m_AllPrefabPath.ToArray());
			for (int i = 0; i < allStr.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(allStr[i]);
				EditorUtility.DisplayProgressBar("查找 Prefab", "Prefab : " + path, i * 1.0f / allStr.Length);
				m_XmlConfigFilterList.Add(path);
				if (IsContainInAllFileABList(path) == false)
				{
					GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
					// 所有依赖项
					string[] allDepend = AssetDatabase.GetDependencies(path);
					// 所有依赖项路径
					List<string> allDependPath = new List<string>();
					for (int j = 0; j < allDepend.Length; j++)
					{
						Debug.Log(allDepend[j]);
						if (IsContainInAllFileABList(allDepend[j]) == false && allDepend[j].EndsWith(".cs")==false)
						{
							// 添加到列表中，用于过滤使用
							m_AllFileABList.Add(allDepend[j]);
							allDependPath.Add(allDepend[j]);
						}
					}
					if (m_AllPrefabDirDict.ContainsKey(obj.name))
					{
						Debug.LogError($"存在相同名字的prefab, 名字为 = {obj.name}");
					}
					else
					{
						// 把 Prefab 依赖项的路径添加到字典中
						m_AllPrefabDirDict.Add(obj.name, allDependPath);
					}

				}
			}
			// 关闭进度条
			EditorUtility.ClearProgressBar();
		}

        

        /// <summary>
        /// 是否 AB 资源已经包含
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static bool IsContainInAllFileABList(string path) {
            for (int i = 0; i < m_AllFileABList.Count; i++)
            {
                if (path == m_AllFileABList[i]
					// (path.Contains(m_AllFileABList[i]) && (path.Replace(m_AllFileABList[i],"")[0]=='/')) 为了排除类似路径冗余的错误剔除（类似（假包含关系）/GameData/Test 与 /GameData/TestDDD/xx）
					|| (path.Contains(m_AllFileABList[i]) && (path.Replace(m_AllFileABList[i],"")[0]=='/')))
                {
					return true;
                }

				
            }

			return false;
		}

		#endregion

		#region 给对应资源打上 AB 标签

		/// <summary>
		/// 给对应资源打上 AB 标签
		/// </summary>
		static void SetAllABNameLabel() {
			Debug.LogWarning(" SetAllABNameLabel 给对应资源打上 AB 标签");
            foreach (string name in m_AllFileDirDict.Keys)
            {
				SetABNameLabel(name,m_AllFileDirDict[name]);
            }

			foreach (string name in m_AllPrefabDirDict.Keys)
			{
				SetABNameLabel(name, m_AllPrefabDirDict[name]);
			}
		}


		/// <summary>
		/// 给对应资源打上 AB 标签
		/// </summary>
		/// <param name="name"></param>
		/// <param name="path"></param>
		static void SetABNameLabel(string name,string path) {
			AssetImporter assetImporter = AssetImporter.GetAtPath(path);
			if (assetImporter == null)
			{
				Debug.LogError($"不存在此路径文件, path = {path}");
			}
			else {
				Debug.LogWarning($" {path} 打上标签 {name}");
				assetImporter.assetBundleName = name;
			}
		}

		/// <summary>
		/// 给对应资源打上 AB 标签
		/// </summary>
		/// <param name="name"></param>
		/// <param name="paths"></param>
		static void SetABNameLabel(string name, List<string> paths) {
            for (int i = 0; i < paths.Count; i++)
            {
				SetABNameLabel(name,paths[i]);

			}
		}

		#endregion

		#region 把打上 AB 标签的资源打包成 AssetBundle

		/// <summary>
		/// 把打上 AB 标签的资源打包成 AssetBundle
		/// </summary>
		static void BuildAssetsToAB() {
			// 获取打上 AB 标签的资源
			string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
			// key 为全路径， value 包名
			Dictionary<string, string> resPathDict = new Dictionary<string, string>();
            for (int i = 0; i < allBundles.Length; i++)
            {
				Debug.Log($" 此AB包：{allBundles[i]} ======================== ");
				string[] allBundlePath = AssetDatabase.GetAssetPathsFromAssetBundle(allBundles[i]);
                for (int j = 0; j < allBundlePath.Length; j++)
                {
                    if (allBundlePath[j].EndsWith(".cs")==true)
                    {
						continue;
                    }

					Debug.Log($" 此AB包：{allBundles[i]} , 包含资源路径为：{allBundlePath[j]}" );
					resPathDict.Add(allBundlePath[j], allBundles[i]);

				}

				// 判断是否存在该路径文件夹
				if (Directory.Exists(AB_BUILD_OUTPUT_PATH) == false)
				{
					Directory.CreateDirectory(AB_BUILD_OUTPUT_PATH);

				}

				// 删除冗余废弃的AB包
				DeleteObsoleteAB();

				// 生成自己的配置表
				WriteData(resPathDict);
                
                // 把AB包输出到路径文件夹下
                AssetBundleManifest assetBundleManifest = BuildPipeline.BuildAssetBundles(AB_BUILD_OUTPUT_PATH, BuildAssetBundleOptions.ChunkBasedCompression,
					EditorUserBuildSettings.activeBuildTarget);

				if (assetBundleManifest == null)
				{
					Debug.LogError("AssetBundle 打包失败");
				}
				else {
					Debug.Log("AssetBundle 打包成功");
				}
			}
		}

		/// <summary>
		/// 把相关的AB 包资源，生成 XML 、二进制配置表
		/// XML 便于查看 AB 包资源及其依赖关系、二进制配置表 用于加载
		/// </summary>
		/// <param name="resPathDict"></param>
		static void WriteData(Dictionary<string,string> resPathDict) {
			AssetBundleXMLConfig xmlConfig = new AssetBundleXMLConfig();
			xmlConfig.ABList = new List<ABBase>();
            foreach (string path in resPathDict.Keys)
            {
				if (IsValidPathForPrefab(path)==false)
				{
					continue;
                }

				ABBase abBase = new ABBase();
				abBase.Path = path;
				abBase.Crc = Crc32.GetCrc32(path);
				abBase.ABName = resPathDict[path];
				abBase.AssetName = path.Remove(0,path.LastIndexOf("/")+1);
				abBase.ABDependce = new List<string>();
				string[] resDependce = AssetDatabase.GetDependencies(path);
                for (int i = 0; i < resDependce.Length; i++)
                {
					string tempPath = resDependce[i];
					// 依赖，排除自身和脚本文件
                    if (tempPath == path || path.EndsWith(".cs"))
                    {
						continue;
                    }

					string abName = "";
                    if (resPathDict.TryGetValue(tempPath,out abName))
                    {
						// 判断资源中是否已经包含
                        if (abName == resPathDict[path])
                        {
							continue;
                        }

						// 判断依赖是否已经包含添加过
                        if (abBase.ABDependce.Contains(abName)==false)
                        {
							Debug.Log($" WriteData ：{path} , ABDependce 依赖：{abName}");
							abBase.ABDependce.Add(abName);
                        }
                    }
                }

				// 添加到配置中
				xmlConfig.ABList.Add(abBase);
			}

			// 写入 xml文件
			string xmlPath = Application.dataPath + "/AssetBundleConfig.xml";
			if (File.Exists(xmlPath)) File.Delete(xmlPath);
			FileStream fileStream = new FileStream(xmlPath,FileMode.Create,FileAccess.ReadWrite,FileShare.ReadWrite);
			StreamWriter sw = new StreamWriter(fileStream,System.Text.Encoding.UTF8);
			XmlSerializer xs = new XmlSerializer(xmlConfig.GetType());
			xs.Serialize(sw,xmlConfig);
			sw.Close();
			fileStream.Close();

            // 写入 二进制文件
            //优化文件大小： path 清掉，不必要保存到二进制文件中（因为读取加载的时候不需要，只是为了xml 文件中便于观察而已）
            foreach (ABBase abBase in xmlConfig.ABList)
            {
				abBase.Path = "";
            }
			
			FileStream fs = new FileStream(AB_BYTE_PATH, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
			fs.Seek(0,SeekOrigin.Begin);
			fs.SetLength(0);			
			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(fs,xmlConfig);
			fs.Close();

			AssetDatabase.Refresh();
			SetABNameLabel("assetbundleconfig", AB_BYTE_PATH);
			
		}

		/// <summary>
		/// 删除废弃的AB包
		/// 不全删除，是为了节约打包时间
		/// </summary>
		static void DeleteObsoleteAB() {
			string[] allBundlesName = AssetDatabase.GetAllAssetBundleNames();
			DirectoryInfo directory = new DirectoryInfo(AB_BUILD_OUTPUT_PATH);
			FileInfo[] fileInfos = directory.GetFiles("*",SearchOption.AllDirectories);
            for (int i = 0; i < fileInfos.Length; i++)
            {
				if (IsContainABNameForObsolete(fileInfos[i].Name, allBundlesName)
					|| fileInfos[i].Name.EndsWith(".meta")
					|| fileInfos[i].Name.EndsWith(".manifest") // 不删除这个，可以加速打包
					|| fileInfos[i].Name.EndsWith("assetbundleconfig") // 不删除这个，可以加速打包
					)
				{
					continue;
				}
				else {
					Debug.Log($"此AB包已经被删除或者改名了：{fileInfos[i].Name}");
                    if (File.Exists(fileInfos[i].FullName))
                    {
						File.Delete(fileInfos[i].FullName);
						//Debug.Log($"删除 {fileInfos[i].Name} 废弃的包");
					}
                    if (File.Exists(fileInfos[i].FullName+ ".manifest"))
                    {
						File.Delete(fileInfos[i].FullName + ".manifest");
					}
				}
            }
		}

		/// <summary>
		/// 判断之前打包的AB包名是否存在即将打包 AB的资源名称中
		/// 为了删除废弃的AB包
		/// </summary>
		/// <param name="name">之前打好的AB包名</param>
		/// <param name="strs"></param>
		/// <returns>true:存在/flase：不存在，需要删除</returns>
		static bool IsContainABNameForObsolete(string name,string[] strs) {
            for (int i = 0; i < strs.Length; i++)
            {
                if (name==(strs[i]))
                {
					return true;
                }
            }

			return false;
		}

		/// <summary>
		/// 判断 添加到 XML config 是否有效
		/// 过滤掉属于 prefab 自身的依赖生成到 xml 中
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		static bool IsValidPathForPrefab(string path) {
            for (int i = 0; i < m_XmlConfigFilterList.Count; i++)
            {
                if (path.Contains(m_XmlConfigFilterList[i]))
                {
					return true;
                }
            }

			return false;
		}

        #endregion

        #region 清空所有资源的 AB 标签

        /// <summary>
        /// 清空所有资源的 AB 标签
        /// </summary>
        static void CleallAllABNameLabel() {
			Debug.LogWarning(" CleallAllABNameLabel 清空所有资源的 AB 标签");
			string[] oldABNameLabels = AssetDatabase.GetAllAssetBundleNames();
            for (int i = 0; i < oldABNameLabels.Length; i++)
            {
				AssetDatabase.RemoveAssetBundleName(oldABNameLabels[i],true);
				EditorUtility.DisplayProgressBar("清除 AB 标签", "名字 : " + oldABNameLabels[i], i * 1.0f / oldABNameLabels.Length);
			}

			// 关闭进度条
			EditorUtility.ClearProgressBar();
		}

        #endregion
    }
}
