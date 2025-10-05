using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace nwmCoTo
{
    public class AIHandler
    {


        private string teachPrompt;
        private class OllamaRequest
        {
            public string model {  get; set; }
            public string prompt { get; set; }
            public int max_tokens { get; set; }
        }

        private readonly HttpClient httpClient;

        public AIHandler()
        {
            try
            {
                teachPrompt = File.ReadAllText("teachingPrompt.txt");
                httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("http://127.0.0.1:11434/");
                httpClient.Timeout = TimeSpan.FromMinutes(10);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<string> responseToMsg(string message)
        {
            try
            {
                OllamaRequest requestData = new OllamaRequest
                {
                    model = "llama2-uncensored:7b",
                    prompt = message,
                    max_tokens = 50,
                };

                // zmiana klasy na obiekt JSON
                StringContent messageToBot = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

                // Wyślij POST do endpointa /v1/completions
                var response = await httpClient.PostAsync("v1/completions", messageToBot);


                var responseText = await response.Content.ReadAsStringAsync();

                using JsonDocument doc = JsonDocument.Parse(responseText);
                JsonElement root = doc.RootElement;

                string chatAnswer = root.GetProperty("choices")[0].GetProperty("text").GetString().Trim('"');

                Console.WriteLine("AI odpowiada: " + chatAnswer);

                // Wyrzuci wyjątek jeśli status != 2xx
                if (chatAnswer == null)
                {
                    throw new Exception();
                }
                return chatAnswer;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                return "A weź spierdalaj";
            }
        }

        public async Task teachModel()
        {
            try
            {
                OllamaRequest requestData = new OllamaRequest
                {
                    model = "llama2-uncensored:7b",
                    prompt = teachPrompt,
                    max_tokens = 200,
                };

                StringContent messageToBot = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

                // Wyślij POST do endpointa /v1/completions
                var response = await httpClient.PostAsync("v1/completions", messageToBot);

                // Debug: wypisz status i treść nawet jeśli nie 2xx
                Console.WriteLine("Status code: " + response.StatusCode);
                string responseText = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response: " + responseText);

                // Wyrzuci wyjątek jeśli status != 2xx
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
            
        }
    }
}
