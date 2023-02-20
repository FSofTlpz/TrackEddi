using System;
using System.IO;
using System.Text;
using System.Xml;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrackEddi {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class ConfigfilePage : ContentPage {

      Config config;


      public ConfigfilePage(Config config) {
         InitializeComponent();
         this.config = config;

         editorConfig.Text = getConfigAsFormattedText(config);
      }

      private async void btnSaveConfig_Clicked(object sender, EventArgs e) {
         XmlDocument test = new XmlDocument();
         try {
            // sehr einfacher und grober Test des Textes:
            XmlReaderSettings settings = new XmlReaderSettings() {
               DtdProcessing = DtdProcessing.Ignore,
               XmlResolver = null,

            };
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(editorConfig.Text.Trim()));
            XmlReader reader = XmlReader.Create(ms, settings);
            test.Load(reader);

            //test.Validate(new System.Xml.Schema.ValidationEventHandler((s, a) => {
            //   Debug.WriteLine(a.Message);

            //}));

            config.LoadXml(editorConfig.Text.Trim());
            //config.SaveData(config.XmlFilename, true);
         } catch (Exception ex) {
            await FSofTUtils.Xamarin.Helper.MessageBox(this,
                                                       "Fehler",
                                                       "Die Konfigurationsdatei konnte nicht gespeichert werden:" + Environment.NewLine +
                                                       ex.Message);
         }
      }

      private void editorConfig_TextChanged(object sender, TextChangedEventArgs e) {
         btnSaveConfig.IsEnabled = true;
      }

      string getConfigAsFormattedText(Config config) {
         using (MemoryStream ms = new MemoryStream()) {
            using (XmlWriter writer = XmlWriter.Create(ms, new XmlWriterSettings {
               Encoding = Encoding.UTF8,
               Indent = true,
               IndentChars = "   ",
               NewLineChars = Environment.NewLine,
               NewLineHandling = NewLineHandling.Replace,
               NewLineOnAttributes = true,
               //xmlWriterSettings.OmitXmlDeclaration = true;
               ConformanceLevel = ConformanceLevel.Document,
            })) {
               config.Save(writer);
               writer.Flush();
               writer.Close();
            }
            ms.Position = 0;
            using (StreamReader reader = new StreamReader(ms)) {
               return reader.ReadToEnd();
            }
         }
      }

   }
}