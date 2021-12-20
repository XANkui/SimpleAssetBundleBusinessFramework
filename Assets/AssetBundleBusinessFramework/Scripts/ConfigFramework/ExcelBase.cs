

namespace AssetBundleBusinessFramework { 

	[System.Serializable]
	public class ExcelBase 
	{
		/// <summary>
		/// 可用来编辑器下模拟数据加载
		/// </summary>
		public virtual void Construction() { }

		/// <summary>
		/// 可用来真正运行时的数据加载
		/// </summary>
		public virtual void Init() { }
	}
}
