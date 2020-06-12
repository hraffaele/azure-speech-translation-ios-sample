using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xamarin.Forms;

namespace App1
{
    public class TextToSpeechService
    {
        public static async Task Speak(string text, string languageCode, string voice)
        {
            // Gets an access token
            string accessToken;

            // Add your subscription key here
            // If your resource isn't in WEST US, change the endpoint
            Authentication auth = new Authentication(Application.Current.Properties["TextToSpeechAuthUrl"].ToString(), Application.Current.Properties["SubscriptionKey"].ToString());
            try
            {
                accessToken = await auth.FetchTokenAsync().ConfigureAwait(false);
                Console.WriteLine("Successfully obtained an access token. \n");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to obtain an access token.");
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.Message);
                return;
            }

            string host = Application.Current.Properties["TextToSpeechHost"].ToString(); 

            // Create SSML document.
            XDocument body = new XDocument(
                    new XElement("speak",
                        new XAttribute("version", "1.0"),
                        new XAttribute(XNamespace.Xml + "lang", languageCode),
                        new XElement("voice",
                            new XAttribute(XNamespace.Xml + "lang", languageCode),                            
                            new XAttribute("name", voice), // Short name for "Microsoft Server Speech Text to Speech Voice (en-US, Jessa24KRUS)"
                            text)));

            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request = new HttpRequestMessage())
                {
                    // Set the HTTP method
                    request.Method = HttpMethod.Post;
                    // Construct the URI
                    request.RequestUri = new Uri(host);
                    // Set the content type header
                    request.Content = new StringContent(body.ToString(), Encoding.UTF8, "application/ssml+xml");
                    // Set additional header, such as Authorization and User-Agent
                    request.Headers.Add("Authorization", "Bearer " + accessToken);
                    request.Headers.Add("Connection", "Keep-Alive");                    
                    // Audio output format. See API reference for full list.
                    request.Headers.Add("X-Microsoft-OutputFormat", "riff-24khz-16bit-mono-pcm");
                    // Create a request
                    Console.WriteLine("Calling the TTS service. Please wait... \n");
                    using (HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                        // Asynchronously read the response
                        using (Stream dataStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        {
                            MemoryStream ms = new MemoryStream();
                            dataStream.CopyTo(ms);
                            var service = DependencyService.Get<IAudioService>();
                            service.PlaySound(ms.ToArray());
                        }
                    }
                }
            }
        }

        // https://github.com/Azure-Samples/Cognitive-Speech-TTS/blob/master/Samples-Http/NETCore/TTSProgram.cs
        public class Authentication
        {
            private string subscriptionKey;
            private string tokenFetchUri;

            public Authentication(string tokenFetchUri, string subscriptionKey)
            {
                if (string.IsNullOrWhiteSpace(tokenFetchUri))
                {
                    throw new ArgumentNullException(nameof(tokenFetchUri));
                }
                if (string.IsNullOrWhiteSpace(subscriptionKey))
                {
                    throw new ArgumentNullException(nameof(subscriptionKey));
                }
                this.tokenFetchUri = tokenFetchUri;
                this.subscriptionKey = subscriptionKey;
            }

            public async Task<string> FetchTokenAsync()
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.subscriptionKey);
                    UriBuilder uriBuilder = new UriBuilder(this.tokenFetchUri);

                    HttpResponseMessage result = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, null).ConfigureAwait(false);
                    return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
