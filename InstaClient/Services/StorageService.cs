using PubnubMessaging;
using System.IO;

namespace InstaClient.Services
{
    public class StorageService : IStorageService
    {
        #region Fields

        private readonly ISerializer _Serializer;
        private readonly string _RootPath = string.Empty;

        #endregion Fields

        #region Constructors

        public StorageService(IStorageOptions options, ISerializer serializer)
        {
            _RootPath = options.RootPath;
            _Serializer = serializer;

            if (!Path.IsPathRooted(_RootPath)) _RootPath = Path.GetFullPath(_RootPath);
            if (!Directory.Exists(_RootPath)) Directory.CreateDirectory(_RootPath);
        }

        #endregion Constructors

        #region Operations

        public TItem Get<TItem>(string key)
        {
            var file = _RootPath + key;
            if (!File.Exists(file)) return default(TItem);
            //  get the file from storage
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                using (var sr = new StreamReader(stream))
                {
                    return _Serializer.Deserialize<TItem>(sr.ReadToEnd());
                }
            }
        }

        public void Put<TItem>(string key, TItem item)
        {
            var file = _RootPath + key;
            //  overwrite the file
            using (var stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (var sw = new StreamWriter(stream))
                {
                    sw.Write(_Serializer.Serialize(item));
                    sw.Flush();
                }
                stream.Close();
            }
        }

        #endregion Operations
    }
}
