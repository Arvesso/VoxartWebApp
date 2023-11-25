using Microsoft.AspNetCore.Mvc;
using VoxartWebApp.Server.System;
using VoxartWebApp.Shared.Modules.Files;

namespace VoxartWebApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StorageController : ControllerBase
    {
        public StorageController()
        {
            
        }

        [HttpGet("defavatar")]
        public async Task<ActionResult> GetDefaultAvatar()
        {
            var path = LocalStorageController.DefaultAvatarFile;

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, "application/octet-stream", "DefaultAvatar.jpg");
        }

        [HttpGet("{key}/{file}")]
        public async Task<ActionResult> GetFile(string key, string file)
        {
            var path = LocalStorageController.GetFilePath(key, file);

            if (path is null)
                return NotFound();

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, "application/octet-stream", file);
        }

        [HttpGet("{key}/filestype={filestype}")]
        public IEnumerable<string> GetFiles(string key, string filestype)
        {
            if (!Enum.TryParse(filestype, true, out FileType type))
                return Enumerable.Empty<string>();

            var paths = LocalStorageController.GetFileNames(key, type)?.Select(p => p = Path.GetFileName(p));

            if (paths is null)
                return Enumerable.Empty<string>();

            return paths.ToArray();
        }
    }
}
