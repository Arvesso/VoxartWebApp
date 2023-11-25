using RestSharp;
using VoxartWebApp.Shared.Modules.Files;

namespace VoxartWebApp.Server.System
{
    public class LocalStorageController // Providing methods to processing local storage (derectories)
    {
        public const string STORAGE = "Storage";
        static LocalStorageController()
        {
            Directory.CreateDirectory(STORAGE);
        }

        private static readonly Dictionary<FileType, string> TypefileDirectories = new()
        {
            { FileType.Image, "Images" },
            { FileType.Video, "Videos" },
            { FileType.Audio, "Audios" }
        };

        private static readonly Dictionary<FileType, string> TypefileExtensions = new()
        {
            { FileType.Image, ".jpg" },
            { FileType.Video, ".mp4" },
            { FileType.Audio, ".mp3" }
        };

        public static string DefaultAvatarFile => Path.Combine(STORAGE, "Avatar.jpg");

        public static bool IsKeyExist(string key) => Directory.Exists(Path.Combine(STORAGE, key));
        public static string RegisterKey()
        {
            while (true)
            {
                var key = GetKey();
                var path = Path.Combine(STORAGE, key);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    return key;
                }
            }
        }
        public static string SaveFile(string key, string base64, FileType fileType)
        {
            var fileName = Guid.NewGuid().ToString() + TypefileExtensions[fileType];
            var directory = Path.Combine(STORAGE, key);
            var fileDirectory = Path.Combine(directory, TypefileDirectories[fileType]);
            var filePath = Path.Combine(fileDirectory, fileName);

            Directory.CreateDirectory(directory);
            Directory.CreateDirectory(fileDirectory);

            var imageBytes = Convert.FromBase64String(base64);
            File.WriteAllBytes(filePath, imageBytes);
            
            return fileName;
        }
        public static async Task<string> DownloadAndSaveFile(string key, string uri, FileType fileType)
        {
            var fileName = Guid.NewGuid().ToString() + TypefileExtensions[fileType];
            var directory = Path.Combine(STORAGE, key);
            var fileDirectory = Path.Combine(directory, TypefileDirectories[fileType]);
            var filePath = Path.Combine(fileDirectory, fileName);

            Directory.CreateDirectory(directory);
            Directory.CreateDirectory(fileDirectory);

            var options = new RestClientOptions(uri);
            var client = new RestClient(options);

            var response = await client.GetAsync(new());

            if (response.IsSuccessful)
            {
                File.WriteAllBytes(filePath, response.RawBytes!);
                return fileName;
            }

            return string.Empty;
        }
        public static IEnumerable<string>? GetFileNames(string key, FileType fileType)
        {
            var directory = Path.Combine(STORAGE, key);

            if (!Directory.Exists(directory))
                return default;

            var fileDirectory = Path.Combine(directory, TypefileDirectories[fileType]);

            if (!Directory.Exists(fileDirectory))
                return default;

            return Directory.GetFiles(fileDirectory).Select(p => p = Path.GetFileName(p));
        }
        public static string? GetFilePath(string key, string filename)
        {
            var directory = Path.Combine(STORAGE, key);

            if (!Directory.Exists(directory))
                return default;

            return Directory.GetFiles(directory, "*", SearchOption.AllDirectories).First(p => Path.GetFileName(p) == filename);
        }
        private static string GetKey()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
