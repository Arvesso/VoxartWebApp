using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace VoxartWebApp.Client.Shared
{
    public class BaseComponent : ComponentBase
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected HttpClient Http { get; set; }


        private protected const string CACHE_KEY = "UserKey";

        private protected string? Key = null;

        protected override async Task OnInitializedAsync()
        {
            var cachedData = await JSRuntime.InvokeAsync<string>("localStorageFunctions.getItem", CACHE_KEY);

            if (string.IsNullOrEmpty(cachedData))
            {
                var dataFromApi = await Http.GetStringAsync("Api/register");
                await JSRuntime.InvokeVoidAsync("localStorageFunctions.setItem", CACHE_KEY, dataFromApi);
                cachedData = dataFromApi;
            }

            Key = cachedData;
        }
    }

}
