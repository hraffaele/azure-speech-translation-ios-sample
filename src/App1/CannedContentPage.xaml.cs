using App1.Core;
using App1.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CannedContentPage : ContentPage
    {
        private FormList FormList;
        public CannedContentPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (FormsPicker.Items.Count == 0)
            { 
                FormList = JsonConvert.DeserializeObject<FormList>(@"{'Forms':" + Application.Current.Properties["Forms"].ToString() + "}");
                foreach (var item in FormList.Forms)
                {
                    FormsPicker.Items.Add(item.Name);
                }

                FormsPicker.SelectedIndex = 0;
            }
        }

        async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e == null) return; // has been set to null, do not 'process' tapped event
            
            var itemIndex = e.ItemIndex;
            ((ListView)sender).SelectedItem = null; // de-select the row

            var source = FormList.Forms[FormsPicker.SelectedIndex].Items[itemIndex].value;
            var languageCode1 = Application.Current.Properties["LanguageCode1"].ToString();
            var languageCode2 = Application.Current.Properties["LanguageCode2"].ToString();

            var translationText = await TextToTextService.TranslateText(
                Application.Current.Properties["TextToTextHost"].ToString(),
                Application.Current.Properties["TextToTextRoute"].ToString() + Language.GetIso6391Name(languageCode2),
                Application.Current.Properties["TextToTextSubscriptionKey"].ToString(),
                source);

            var voice = Application.Current.Properties["Voice2"].ToString();
            await App1.TextToSpeechService.Speak(translationText, languageCode2, voice);           

            if (!Application.Current.Properties.ContainsKey("CannedTranslations") || Application.Current.Properties["CannedTranslations"] == null)
            {
                Application.Current.Properties["CannedTranslations"] = new Queue<Translation>();                
            }

            var languageList = (LanguageList) Application.Current.Properties["LanguagesList"];
            var sourceLanguage = languageList.Languages.First(x => x.ShortCode == languageCode1);
            var targetLanguage = languageList.Languages.First(x => x.ShortCode == languageCode2);
            var translation = new Translation(source, translationText, sourceLanguage, targetLanguage);

            // https://forums.xamarin.com/discussion/18244/programmatically-switch-between-tabs-in-a-tabbedpage
            var masterPage = this.Parent as TabbedPage;
            ((DialogPage)masterPage.Children[0]).UpdateUI(LayoutOptions.Start, translation.SourceText, translation.TargetText, translation.TargetLanguage.ShortCode, true);
            masterPage.CurrentPage = masterPage.Children[0];
        }

        private void FormsPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = (string)FormsPicker.SelectedItem;
            var form = FormList.Forms.First(x => x.Name == selectedItem);
            this.BindingContext = form.Items.Select(x => x.key + " - " + x.value).ToArray();
        }
    }    
}