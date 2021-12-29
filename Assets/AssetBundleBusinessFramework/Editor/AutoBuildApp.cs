using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace AssetBundleBusinessFramework { 

	public class AutoBuildApp 
	{
		private static string m_AppName = ReadDataConfig.GetDataConfig().APPName;
		private static string m_AndroidPath = Application.dataPath + "/../BuildTarget/Android/";
		private static string m_iOSPath = Application.dataPath + "/../BuildTarget/iOS/";
		private static string m_WindowsPath = Application.dataPath + "/../BuildTarget/Windows/";
		
		[MenuItem("MyBuild/标准APP包（包含APP、AB 包）")]
		public static void Build() {

			// 打 ab 包
			AssetBundleBuildFlowEditor.Build();

			// 拷贝 AssetBundle
			string abPath = Application.dataPath + "/../AssetBundle/" + EditorUserBuildSettings.activeBuildTarget.ToString();
			Copy(abPath,Application.streamingAssetsPath);

			// 打包应用
			BuildPlatformApp();

			// 删除指定文件夹
			DeleteDir(Application.streamingAssetsPath);
		}

		/// <summary>
		/// 拷贝指定路径下的文件到目标路径
		/// </summary>
		/// <param name="srcPath"></param>
		/// <param name="targetPath"></param>
		private static void Copy(string srcPath,string targetPath) {
            try
            {
                if (Directory.Exists(targetPath)==false)
                {
					Directory.CreateDirectory(targetPath);
                }

				string srcDir = Path.Combine(targetPath,Path.GetFileName(srcPath));
                if (Directory.Exists(srcDir))
                {
					srcDir += Path.DirectorySeparatorChar;
                }
				if (Directory.Exists(srcDir)==false)
				{
					Directory.CreateDirectory(srcDir);
				}

				string[] files = Directory.GetFileSystemEntries(srcPath);
                foreach (string file in files)
                {
					// 递归拷贝文件夹
					if (Directory.Exists(file))
					{
						Copy(file, srcDir);
					}
					else {
						File.Copy(file, srcDir + Path.GetFileName(file),true) ;
					}
                }
			}
            catch (Exception e)
            {

				Debug.LogError($"无法拷贝：{srcPath} 到 {targetPath},Exception = {e}");
            }
		}

		/// <summary>
		/// 打包对应平台APP
		/// </summary>
		private static void BuildPlatformApp() {
			string savePath = "";
			if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
			{
				savePath = m_AndroidPath + m_AppName + "_" + EditorUserBuildSettings.activeBuildTarget + string.Format("_{0:yyyy_MM_dd_HH_mm}",
					DateTime.Now) + ".apk";
			}
			else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
			{
				savePath = m_iOSPath + m_AppName + "_" + EditorUserBuildSettings.activeBuildTarget + string.Format("_{0:yyyy_MM_dd_HH_mm}",
					DateTime.Now);
			}
			else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows
				|| EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
			{
				savePath = m_WindowsPath + m_AppName + "_" + EditorUserBuildSettings.activeBuildTarget + string.Format("_{0:yyyy_MM_dd_HH_mm}/{1}.exe",
					DateTime.Now, m_AppName);
			}

			BuildPipeline.BuildPlayer(FindEnableEditorScenes(), savePath, EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);
		}

		/// <summary>
		/// 获取设置表中 激活的场景
		/// </summary>
		/// <returns></returns>
		private static string[] FindEnableEditorScenes() {
			List<string> editorScenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled==false)
                {
					continue;
                }

				editorScenes.Add(scene.path);
            }

			return editorScenes.ToArray();
		}

		/// <summary>
		/// 删除指定文件夹所有文件
		/// </summary>
		/// <param name="srcPath"></param>
		private static void DeleteDir(string srcPath) {
            try
            {
				DirectoryInfo dir = new DirectoryInfo(srcPath);
				FileSystemInfo[] fileInfo = dir.GetFileSystemInfos();
                foreach (FileSystemInfo info in fileInfo)
                {
					if (info is DirectoryInfo)
					{
						DirectoryInfo subDir = new DirectoryInfo(info.FullName);
						subDir.Delete(true);
					}
					else {
						File.Delete(info.FullName);
					}
                }
            }
            catch (Exception e)
            {

				Debug.LogError($"删除失败 : {e}");
            }
		}
	}
}
