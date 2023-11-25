using Microsoft.AspNetCore.Mvc;
using VoxartWebApp.Server.Services;
using VoxartWebApp.Server.System;
using VoxartWebApp.Shared.Modules.Avatar;
using VoxartWebApp.Shared.Modules.Files;
using VoxartWebApp.Shared.Modules.Synthesize;

namespace VoxartWebApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiController : Controller
    {
        private readonly NeuralAvatarApiService _avatarApiService;
        public ApiController(NeuralAvatarApiService neuralAvatarApi)
        {
            _avatarApiService = neuralAvatarApi;
        }

        [HttpGet("{key}")]
        public ActionResult<bool> Get(string key)
        {
            return LocalStorageController.IsKeyExist(key);
        }

        [HttpGet("register")]
        public ActionResult<string> RegisterNewKey()
        {
            return LocalStorageController.RegisterKey();
        }

        [HttpGet("key={key}/gender={gender}&text={text}")]
        public async Task<ActionResult<string>> Voice(string key, string gender, string text)
        {
            if (!Enum.TryParse(gender, true, out SynthesizeGender type))
                return NotFound();

            var model = await _avatarApiService.GetResultAsync(text, "http://dev.arvesso.fun/storage/defavatar", type);

            if (model is null)
                return NotFound();

            var filename = await LocalStorageController.DownloadAndSaveFile(key, model.AudioUri, FileType.Audio);
            await LocalStorageController.DownloadAndSaveFile(key, model.ResultUri, FileType.Video);

            if (model is null)
                return NotFound();

            return filename;
        }

        [HttpPost("key={key}/gender={gender}")]
        public async Task<ActionResult<string>> Avatar(string key, string gender, [FromBody] RequestModel request)
        {
            if (!Enum.TryParse(gender, true, out SynthesizeGender type))
                return NotFound();

            var avatar = LocalStorageController.SaveFile(key, request.ImageAvatar, FileType.Image);

            var model = await _avatarApiService.GetResultAsync(request.Text, $"https://dev.arvesso.fun/storage/{key}/{avatar}", type);

            if (model is null)
                return NotFound();

            await LocalStorageController.DownloadAndSaveFile(key, model.AudioUri, FileType.Audio);
            var filename = await LocalStorageController.DownloadAndSaveFile(key, model.ResultUri, FileType.Video);

            if (model is null)
                return NotFound();

            return filename;
        }
    }
}
