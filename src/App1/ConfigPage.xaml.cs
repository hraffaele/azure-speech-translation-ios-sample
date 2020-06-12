using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App1
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ConfigPage : ContentPage
	{
		private LanguageList LanguageList;		

		public ConfigPage()
		{
			InitializeComponent();

			GetDefaultConfig("SubscriptionKey");
			GetDefaultConfig("Region");
			GetDefaultConfig("LanguageCode1");
			GetDefaultConfig("LanguageCode2");
			GetDefaultConfig("Voice1");
			GetDefaultConfig("Voice2");
			GetDefaultConfig("TextToTextHost");
			GetDefaultConfig("TextToTextRoute");
			GetDefaultConfig("TextToTextSubscriptionKey");
			GetDefaultConfig("Languages");
			GetDefaultConfig("Forms");
			GetDefaultConfig("TextToSpeechHost");
			GetDefaultConfig("TextToSpeechAuthUrl");

			LanguageList = JsonConvert.DeserializeObject<LanguageList>(@"{'Languages':" + Application.Current.Properties["Languages"].ToString() + "}");

			Application.Current.Properties["LanguagesList"] = LanguageList;

			var language1Name = string.Empty;
			var language2Name = string.Empty;

			foreach (var language in LanguageList.Languages)
			{
				ConfigLanguageCode1.Items.Add(language.Name);
				ConfigLanguageCode2.Items.Add(language.Name);

				if (language.ShortCode == Application.Current.Properties["LanguageCode1"].ToString())
				{
					language1Name = language.Name;
				}
				if (language.ShortCode == Application.Current.Properties["LanguageCode2"].ToString())
				{
					language2Name = language.Name;
				}
			}

			Application.Current.Properties["language1Name"] = language1Name;
			Application.Current.Properties["language2Name"] = language2Name;
			ConfigLanguageCode1.SelectedItem = language1Name;
			ConfigLanguageCode2.SelectedItem = language2Name;
		}

		protected override void OnAppearing()
		{
		}

		protected override void OnDisappearing()
		{
			var language1 = LanguageList.Languages.First(x => x.Name == ConfigLanguageCode1.SelectedItem.ToString());
			var language2 = LanguageList.Languages.First(x => x.Name == ConfigLanguageCode2.SelectedItem.ToString());

			Application.Current.Properties["language1Name"] = language1.Name;
			Application.Current.Properties["language2Name"] = language2.Name;
			Application.Current.Properties["LanguageCode1"] = language1.ShortCode;
			Application.Current.Properties["LanguageCode2"] = language2.ShortCode;
			Application.Current.Properties["Voice1"] = language1.Voice;
			Application.Current.Properties["Voice2"] = language2.Voice;
		}

		private static void GetDefaultConfig(string key)
		{			
			Application.Current.Properties[key] = AppSettingsManager.Settings[key];			
		}		
	}
	public class LanguageList
	{		
		public IList<Language> Languages { get; set; }
	}

	public class FormList
	{
		public IList<Form> Forms { get; set; }
	}

	public class Form
	{
		public string Name { get; set; }		
		public List<Item> Items { get; set; }
	}

	public class Item
	{
		public string key { get; set; }
		public string value { get; set; }
	}
}