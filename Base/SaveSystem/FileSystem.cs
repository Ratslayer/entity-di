using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace BB
{
    public sealed class FileSystem : IFileSystem
    {
        [Inject] FileSystemParams _params;

        public string GetFullPath(string localPath)
            => $"{_params.RootPath}/{localPath}";

        public Task<T> Read<T>(ReadFileContext context)
        {
            var path = GetFullPath(context.Path);
            try
            {
                return ReadFromPath(path);
            }
            catch (Exception e)
            {
                Log.Exception(e, $"{path} could not be properly read. Reverting to backup.");

                var backupPath = $"{path}_backup";
                return ReadFromPath(backupPath);
            }
            static async Task<T> ReadFromPath(string path)
            {
                var text = await File.ReadAllTextAsync(path);
                var result = JsonConvert.DeserializeObject<T>(text);
                return result;
            }
        }

        public async Task Write(WriteFileContext context)
        {
            var path = GetFullPath(context.Path);
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(path);
                Directory.Delete(path);
                await WriteToFile();
                return;
            }

            var backupPath = $"{path}_backup";
            File.Copy(path, backupPath, true);
            File.Delete(path);

            await WriteToFile();

            File.Delete(backupPath);

            Task WriteToFile()
            {
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                };

                var json = JsonConvert.SerializeObject(context.Data, settings);
                return File.WriteAllTextAsync(path, json);
            }
        }
    }
}