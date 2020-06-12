using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace App1.Core
{
    public class TextToTextService
        {
        public static async Task<string> TranslateText(string host, string route, string subscriptionKey, string text)
        {            
            var body = new object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                // Set the method to POST
                request.Method = HttpMethod.Post;
                
                // Construct the full URI
                request.RequestUri = new Uri(host + route);

                // Add the serialized JSON object to your request
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                // Add the authorization header
                request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);               

                // Send request, get response
                var response = await client.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!responseContent.StartsWith("["))
                {
                    responseContent = "[" + responseContent + "]";
                }

                var result = JsonConvert.DeserializeObject<List<TextToTextDto.Result>> (responseContent);

                if (result.First().Error != null)
                {
                    throw new ApplicationException("TextToTextService.TranslateText(): " + result.First().Error.Message);
                }

                // Print the response
                return result.First().Translations.First().Text;
            }
        }
    }
}