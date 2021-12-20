using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;

namespace AssetBundleBusinessFramework { 

	public class BinarySerializeOpt
	{
        /// <summary>
        /// 类序列化成 xml 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
		public static bool XmlSerialize(string path,System.Object obj) {
            try
            {
                using (FileStream fs = new FileStream(path,FileMode.Create,FileAccess.ReadWrite,FileShare.ReadWrite))
                {
                    using (StreamWriter sw = new StreamWriter(fs,System.Text.Encoding.UTF8))
                    {
                        // 修改或者清空xml 头 （根据需要改写）
                        //XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                        //namespaces.Add(string.Empty,string.Empty);

                        XmlSerializer xs = new XmlSerializer(obj.GetType());
                        xs.Serialize(sw, obj);
                    }
                    
                }

                return true;
            }
            catch (System.Exception e)
            {

                Debug.LogError($"此类无法转换成xml {obj.GetType()} : {e}");
            }

            return false;
        }

        /// <summary>
        /// xml 文件读取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public T XmlDeserialize<T>(string path)where T:class
        {
            T t = default(T);
            try
            {
                using (FileStream fs = new FileStream(path,FileMode.Open,FileAccess.ReadWrite,FileShare.ReadWrite))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(T));
                    t = (T)xs.Deserialize(fs);
                }
            }
            catch (System.Exception e)
            {

                Debug.LogError($"此 xml 无法转为 二进制 {path} ：{e}");
            }

            return t;
        }

        /// <summary>
        /// 运行时读取 xml 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public T XmlDeserializeRuntime<T>(string path) where T : class{
            T t = default(T);
            TextAsset textAsset = ResourceManager.Instance.LoadResource<TextAsset>(path);
            if (textAsset == null)
            {
                UnityEngine.Debug.LogError($"cant load TextAsset:{path}");
                return null;
            }

            try
            {
                using (MemoryStream stream = new MemoryStream(textAsset.bytes))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(T));
                    t = (T)xs.Deserialize(stream);
                }
                ResourceManager.Instance.ReleaseResource(path,true);
            }
            catch (System.Exception e)
            {

                Debug.LogError($"load TextAsset exception {path} ：{e}");
            }

            return t;
        }

        /// <summary>
        /// 二进制序列化
        /// </summary>
        /// <param name="path"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool BinarySeralize(string path, System.Object obj) {
            try
            {
                using (FileStream fs = new FileStream(path,FileMode.Create,FileAccess.ReadWrite,FileShare.ReadWrite))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs,obj);
                }

                return true;
            }
            catch (System.Exception e)
            {

                Debug.LogError($"此类无法转换成 Binary {obj.GetType()} : {e}");
            }
            return false;
        }

        /// <summary>
        /// 读取 Binary 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public T BinaryDeserialize<T>(string path) where T : class
        {
            T t = default(T);
            TextAsset textAsset = ResourceManager.Instance.LoadResource<TextAsset>(path);
            if (textAsset == null)
            {
                UnityEngine.Debug.LogError($"cant load TextAsset:{path}");
                return null;
            }

            try
            {
                using (MemoryStream stream = new MemoryStream(textAsset.bytes))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    t = (T)bf.Deserialize(stream);
                }
                ResourceManager.Instance.ReleaseResource(path, true);
            }
            catch (System.Exception e)
            {

                Debug.LogError($"load TextAsset exception {path} ：{e}");
            }

            return t;
        }
    }
}
