using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AboloLib
{
    public class PersistantStorage : AboloSingleton<PersistantStorage>
    {
        [SerializeField]public string _mSaveName = "Abolo";

        private  string savePath;

        public  void Setup()
        {
            Init();
#if UNITY_EDITOR
            savePath = Path.Combine(Application.streamingAssetsPath, "saveFile" + _mSaveName);

            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }

            //if (!File.Exists(savePath))
            //{
            //    Debug.Log($"没有路径需要新建{Application.streamingAssetsPath}");
            //    File.Create(Application.streamingAssetsPath);
            //}
#else
            savePath = Path.Combine(Application.persistentDataPath, "saveFile" + _mSaveName);
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(Application.persistentDataPath);
            }
#endif
        }

        public void ClearSave()
        {
            if (File.Exists(savePath)) File.Delete(savePath);
        }

        public void Save(IPersistable o , int version)
        {

            using (var writer = new BinaryWriter(File.Open(savePath, FileMode.Create)))
            {
                writer.Write(-version);
                o.Save(new GameDataWriter(writer));
            }
            Debug.Log(savePath);
        }

        public void Load(IPersistable o )
        {
            //using (var reader = new BinaryReader(File.Open(savePath , FileMode.Open)))
            //{
            //    o.Load(new GameDataReader(reader , -reader.ReadInt32()));
            //}

            ///解决协程延迟后读取file已经被关闭的问题，缓存一份存档数据来读取
            byte[] data = File.ReadAllBytes(savePath);
            var reader = new BinaryReader(new MemoryStream(data));
            o.Load(new GameDataReader(reader , -reader.ReadInt32()));
        }

        public  void DeleteSavedFile()
        {
            //检测是否存在文件用File.Exists-----Directory.Exists返回值一直为false
            if (File.Exists(savePath))
            {
                Debug.Log("删除路径 ：" + savePath);
                File.Delete(savePath);
                
            }
        }
    }

}