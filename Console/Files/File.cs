using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace Console.Files
{
    public class File
    {
        #region Variables
        
        private static object _lock = new object();

        private static BinaryFormatter _binaryFormatter = new BinaryFormatter();
        
        public string FilePath { get; }
        
        public bool IsJson { get; }
        
        public bool IsBinary { get; }
        
        #endregion

        public File(string path)
        {
            var extension = Path.GetExtension(path);
            
            IsJson = extension == ".json";
            IsBinary = extension == ".data";
            
            FilePath = path;
        }
        
        #region Methods

        public bool Exists(bool create)
        {
            lock (_lock)
            {
                if (string.IsNullOrEmpty(FilePath))
                    return false;

                var directory = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    if (create)
                        Directory.CreateDirectory(directory);
                    else
                        return false;
                }

                if (System.IO.File.Exists(FilePath))
                    return true;

                if (create)
                    System.IO.File.Create(FilePath).Close();
                else
                    return false;

                return true;
            }
        }

        public void Write(object obj)
        {
            lock (_lock)
            {
                Exists(true);
                
                if (IsJson)
                    WriteJson(obj);
                
                if (IsBinary)
                    WriteBinary(obj);
            }
        }

        public T Read<T>()
        {
            lock (_lock)
            {
                if (!Exists(false))
                    return GetInstanceOf<T>();
                
                if (IsJson)
                    return ReadJson<T>();
                
                if (IsBinary)
                    return ReadBinary<T>();
            }

            return GetInstanceOf<T>(); // :P
        }

        private void WriteJson(object obj)
        {
            var text = JsonConvert.SerializeObject(obj, Formatting.Indented);
            System.IO.File.WriteAllText(FilePath, text);
        }

        private void WriteBinary(object obj)
        {
            using (var fs = new FileStream(FilePath, FileMode.Truncate))
            {
                using (var writer = new BinaryWriter(fs))
                {
                    writer.Write(GetBytesFromObject(obj));
                }
            }
        }

        private T ReadJson<T>()
        {
            var data = System.IO.File.ReadAllText(FilePath);
            return JsonConvert.DeserializeObject<T>(data);
        }

        private T ReadBinary<T>()
        {
            var bytes = System.IO.File.ReadAllBytes(FilePath);
            return GetObjectFromBytes<T>(bytes);
        }
        
        #endregion
        
        #region Helpers

        private byte[] GetBytesFromObject(object obj)
        {
            using (var ms = new MemoryStream())
            {
                _binaryFormatter.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        private T GetObjectFromBytes<T>(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return (T) _binaryFormatter.Deserialize(ms);
            }
        }

        private T GetInstanceOf<T>() => Activator.CreateInstance<T>();

        #endregion
    }
}