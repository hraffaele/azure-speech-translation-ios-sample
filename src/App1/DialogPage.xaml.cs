using App1.Core;
using App1.Dto;
using Microsoft.AppCenter.Crashes;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace App1
{
    public partial class DialogPage : ContentPage
    {
        // https://stackoverflow.com/questions/33965857/how-to-implement-a-settings-ui-in-xamarin-forms
        // https://docs.microsoft.com/en-us/dotnet/api/xamarin.forms.tabbedpage?view=xamarin-forms
        // beep? https://forums.xamarin.com/discussion/145050/beep-in-xamarin

        internal static IAudioService service;        
        TranslationRecognizer recognizer;

        public DialogPage()
        {            
            InitializeComponent();

            try
            {
                service = DependencyService.Get<IAudioService>();                
            }
            catch (Exception ex)
            {
                Log("error init " + ex.Message);
                Crashes.TrackError(ex);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Application.Current.Properties.ContainsKey("language1Name"))
            { 
                FromButton.Text = Application.Current.Properties["language1Name"].ToString();
            }
            if (Application.Current.Properties.ContainsKey("language2Name"))
            {
                ToButton.Text = Application.Current.Properties["language2Name"].ToString();
            }            
        }

        private async void OnToButtonPressed(object sender, EventArgs ea)
        {
            ToButton.BackgroundColor = Color.Green;
            ToButton.TextColor = Color.Black;
            await InitializeRecognizer((string)Application.Current.Properties["LanguageCode2"], (string)Application.Current.Properties["LanguageCode1"], (string)Application.Current.Properties["Voice1"]);
        }

        private async void OnFromButtonPressed(object sender, EventArgs ea)
        {
            FromButton.BackgroundColor = Color.Green;
            FromButton.TextColor = Color.Black;
            await InitializeRecognizer((string)Application.Current.Properties["LanguageCode1"], (string)Application.Current.Properties["LanguageCode2"], (string)Application.Current.Properties["Voice2"]);
        }

        Queue<Byte[]> AudioResults = new Queue<byte[]>();
        Queue<KeyValuePair<string, string>> TextResults = new Queue<KeyValuePair<string, string>>();

        private async Task InitializeRecognizer(string languageCode1, string languageCode2, string voice)
        {
            try
            {
                // Creates an instance of a speech translation config with specified subscription key and service region.                
                var config = SpeechTranslationConfig.FromSubscription((string)Application.Current.Properties["SubscriptionKey"], (string)Application.Current.Properties["Region"]);

                // Sets source and target languages.                
                string fromLanguage = languageCode1;
                string toLanguage = languageCode2;
                config.SpeechRecognitionLanguage = fromLanguage;
                config.AddTargetLanguage(toLanguage);

                // Sets the synthesis output voice name.
                // Replace with the languages of your choice, from list found here: https://aka.ms/speech/tts-languages
                config.VoiceName = voice;

                var stopRecognition = new TaskCompletionSource<int>();
                using (recognizer = new TranslationRecognizer(config))
                {
                    // Subscribes to events.
                    recognizer.Recognized += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.TranslatedSpeech)
                        {
                            foreach (var element in e.Result.Translations.Where(x => !string.IsNullOrWhiteSpace(x.Value)))
                            {
                                if (!string.IsNullOrWhiteSpace(element.Value))
                                { 
                                    TextResults.Enqueue(new KeyValuePair<string, string>(e.Result.Text, element.Value));
                                }
                                else
                                {
                                    var x = "WHY?";
                                }
                                var options = new SpeechOptions();
                            }
                        }
                    };

                    recognizer.Synthesizing += async (s, e) =>
                    {
                        var audio = e.Result.GetAudio();

                        if (audio.Length > 0)
                        {
                            try
                            {                             
                                AudioResults.Enqueue(audio);
                            }
                            catch (Exception ex)
                            {
                                Log("error Synthesizing " + ex.Message);
                                Crashes.TrackError(ex);
                            }
                        }
                    };


                    recognizer.Canceled += (s, e) =>
                    {
                        Log($"CANCELED: Reason={e.Reason}");

                        if (e.Reason == CancellationReason.Error)
                        {
                            Log($"CANCELED: ErrorCode={e.ErrorCode}");
                            Log($"CANCELED: ErrorDetails={e.ErrorDetails}");
                            Log($"CANCELED: Did you update the subscription info?");
                        }

                        stopRecognition.TrySetResult(0);
                    };

                    recognizer.SessionStarted += (s, e) =>
                    {
                        Log("Session started event.");
                    };

                    recognizer.SessionStopped += async (s, e) =>
                    {

                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            Log("Session stopped event.");

                            Log("Return results now.");

                            var sourceText = string.Empty;
                            var targetText = string.Empty;
                            KeyValuePair<string, string> result;

                            while (TextResults.Count > 0)
                            {
                                result = TextResults.Dequeue();
                                sourceText = sourceText + " " + result.Key;
                                targetText = targetText + " " + result.Value;
                            }

                            if (!string.IsNullOrWhiteSpace(targetText))
                            { 
                                if (languageCode1 == (string)Application.Current.Properties["LanguageCode1"])
                                {
                                    UpdateUI(LayoutOptions.Start, sourceText, targetText, (string)Application.Current.Properties["LanguageCode2"], true);
                                }
                                else
                                {
                                    UpdateUI(LayoutOptions.End, sourceText, targetText, (string)Application.Current.Properties["LanguageCode1"], false);
                                }

                                service = DependencyService.Get<IAudioService>();

                                service.PlaySound(AudioResults);                                

                                stopRecognition.TrySetResult(0);
                            }
                            else
                            {
                                var x = "WHY?";
                            }
                        });                        
                    };

                    // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    // Waits for completion.
                    // Use Task.WaitAny to keep the task rooted.
                    Task.WaitAny(new[] { stopRecognition.Task });

                    // Stops recognition.
                    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Log("From Pressed Error " + ex.Message);
                Crashes.TrackError(ex);
            }
        }

        private async void OnFromButtonReleased(object sender, EventArgs ea)
        {
            try
            {
                FromButton.BackgroundColor = Color.Gray;
                FromButton.TextColor = Color.White;
                await recognizer.StopContinuousRecognitionAsync();
                Log("Button Released");
            }
            catch (Exception ex)
            {
                Log("From released Error " + ex.Message);
                Crashes.TrackError(ex);
            }
        }

        private async void OnToButtonReleased(object sender, EventArgs ea)
        {
            try
            {
                ToButton.BackgroundColor = Color.Gray;
                ToButton.TextColor = Color.White;
                await recognizer.StopContinuousRecognitionAsync();
                Log("Button Released");
            }
            catch (Exception ex)
            {
                Log("To released Error " + ex.Message);
                Crashes.TrackError(ex);
            }
        }

        public void UpdateUI(LayoutOptions layoutOptions, string message, string message2, string languageCode, bool isLeft)
        {
            var frame = CreateOutputControls(layoutOptions, message, message2, languageCode, isLeft);

            Device.BeginInvokeOnMainThread(() =>
            {
                frame.WidthRequest = diageScrollView.Width * .8;

                contentStack.Children.Add(frame);                
            });
        }

        private static Frame CreateOutputControls(LayoutOptions layoutOptions, string message, string message2, string languageCode, bool isLeft)
        {
            var frame = new Frame();
            frame.CornerRadius = 10;
            frame.Margin = 4;
            frame.Padding = 1;
            frame.HorizontalOptions = layoutOptions;
            frame.IsClippedToBounds = true;
            frame.BorderColor = Color.Black;

            var stackLayout = new StackLayout();
            stackLayout.Orientation = StackOrientation.Vertical;
            stackLayout.Padding = 6;
            stackLayout.HorizontalOptions = LayoutOptions.FillAndExpand;

            stackLayout.IsClippedToBounds = true;

            var sourceRowStackLayout = new StackLayout();
            sourceRowStackLayout.Orientation = StackOrientation.Horizontal;
            sourceRowStackLayout.HorizontalOptions = LayoutOptions.Fill;
            
            var sourceLabel = new Label();
            sourceLabel.FontSize = 16;
            sourceLabel.TextColor = Color.Black;
            sourceLabel.VerticalOptions = LayoutOptions.FillAndExpand;            

            var targetLabel = new Label();
            targetLabel.FontSize = 18;
            targetLabel.TextColor = Color.Black;

            var targetRowStackLayout = new StackLayout();
            targetRowStackLayout.Orientation = StackOrientation.Horizontal;
            targetRowStackLayout.HorizontalOptions = LayoutOptions.FillAndExpand;

            stackLayout.Children.Add(sourceRowStackLayout);
            stackLayout.Children.Add(targetRowStackLayout);
            frame.Content = stackLayout;

            sourceLabel.Text = message;
            targetLabel.Text = message2;

            if (isLeft)
            {
                stackLayout.BackgroundColor = Color.SteelBlue;
                sourceLabel.HorizontalOptions = LayoutOptions.FillAndExpand;
                sourceLabel.BackgroundColor = Color.SteelBlue;
                targetLabel.HorizontalOptions = LayoutOptions.FillAndExpand;
                targetRowStackLayout.Children.Add(targetLabel);
                targetRowStackLayout.Children.Add(CreateCancelButton(message, languageCode));
                targetRowStackLayout.Children.Add(CreateRefreshButton(message2, languageCode));                
                sourceRowStackLayout.Children.Add(sourceLabel);
            }
            else
            {
                stackLayout.BackgroundColor = Color.AliceBlue;
                sourceLabel.BackgroundColor = Color.AliceBlue;
                sourceLabel.IsEnabled = false;
                sourceLabel.HorizontalOptions = LayoutOptions.EndAndExpand;
                targetLabel.HorizontalOptions = LayoutOptions.EndAndExpand;
                targetRowStackLayout.Children.Add(CreateRefreshButton(message2, languageCode));
                targetRowStackLayout.Children.Add(CreateCancelButton(message, languageCode));
                targetRowStackLayout.Children.Add(targetLabel);                
                sourceRowStackLayout.Children.Add(sourceLabel);
            }
            return frame;
        }

        public void Log(String message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DiagnosticText.Text += Environment.NewLine + DateTime.Now.ToString("HH:mm:ss") + " " + message;
                diageScrollView.ScrollToAsync(DiagnosticText, ScrollToPosition.End, true);
            });
        }

        private static ImageButton CreateRefreshButton(string source, string languageCode)
        {
            return CreateButton("refresh", ReplayEvent, source, languageCode);
        }

        private static ImageButton CreateSourceRefreshButton(string source, string languageCode)
        {
            return CreateButton("refresh", Translate, source, languageCode);
        }


        private static ImageButton CreateCancelButton(string source, string languageCode)
        {
            return CreateButton("cancel", CancelEvent, source, languageCode);
        }

        private static ImageButton CreateButton(string source, Action<object, EventArgs, string, string> handler, string message, string languageCode)
        {
            var button = new ImageButton();
            button.Source = source;
            button.Pressed += new EventHandler((sender, e) => handler(sender, e, message, languageCode));
            button.HeightRequest = 30;
            button.WidthRequest = 30;
            button.Margin = new Thickness(0, 0, 0, 0);
            button.Padding = 0;
            button.HorizontalOptions = LayoutOptions.Center;
            button.VerticalOptions = LayoutOptions.Start;
            
            return button;
        }

        private static void CancelEvent(object sender, EventArgs e, string source, string languageCode)
        {
            service.StopSound();
        }

        private static void ReplayEvent(object sender, EventArgs e, string source, string languageCode)
        {
            SpeakUsingFileAsync(source, languageCode).GetAwaiter();
        }

        private static void Translate(object sender, EventArgs e, string source, string languageCode)
        {
            TranslateAsync(sender, e, source, languageCode).GetAwaiter();
        }

        private static async Task TranslateAsync(object sender, EventArgs e, string source, string languageCode)
        {
            try
            {
                var translation = await TextToTextService.TranslateText((string)Application.Current.Properties["TextToTextHost"],
                        (string)Application.Current.Properties["TextToTextRoute"] + Language.GetIso6391Name((string)Application.Current.Properties["LanguageCode2"]),
                        (string)Application.Current.Properties["TextToTextSubscriptionKey"], source);

                await SpeakUsingFileAsync(translation, languageCode);
            }
            catch (Exception ex)
            {
                var x = ex.Message;
            }
        }
        
        public static async Task SpeakUsingFileAsync(string text, string languageCode)
        {
            var config = SpeechConfig.FromSubscription((string)Application.Current.Properties["SubscriptionKey"], (string)Application.Current.Properties["Region"]);
            config.SpeechSynthesisLanguage = languageCode;

            using (var synthesizer = new SpeechSynthesizer(config, null))
            {
                using (var result = await synthesizer.SpeakTextAsync(text).ConfigureAwait(false))
                {
                    if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                    {
                        try
                        {
                            if (service == null)
                            {
                                service = DependencyService.Get<IAudioService>();
                            }
                            service.PlaySound(result.AudioData);
                        }
                        catch (Exception ex)
                        {

                            var e = ex;
                        }
                    }
                }
            }
        }

        private async void contentStack_ChildAdded(object sender, ElementEventArgs e)
        {
            //https://stackoverflow.com/questions/55402646/xamarin-forms-scrollview-scrolltoasync-results-in-blank-content-on-ios
            await Task.Delay(10);
            await contentScrollView.ScrollToAsync(contentStack, ScrollToPosition.End, true);
        }
    }   
}
