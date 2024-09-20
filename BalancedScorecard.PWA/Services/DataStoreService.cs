using Microsoft.JSInterop;
using static System.Net.WebRequestMethods;

namespace BalancedScorecard.PWA.Services
{
    public class DataStoreService
    {
        private HttpClient _httpClient;
        private IJSRuntime _jsRuntime;
        public DataStoreService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task UpdateDataStore()
        {
            try
            {
                var file_path = @"data.csv";
                var file_content = await _httpClient.GetStringAsync("/test/");
                await _jsRuntime.InvokeVoidAsync("saveTextFile", file_path, file_content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public async Task LoadData()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
