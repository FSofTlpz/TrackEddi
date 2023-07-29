using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrackEddi {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class ShowGarminInfo4LonLat : ContentPage {

      public class ListViewObjectItem {

         public string Name { get; protected set; }

         public string TypeName { get; protected set; }

         public bool NameIsSet => !string.IsNullOrEmpty(Name);

         public ImageSource Picture { get; protected set; }

         /// <summary>
         /// akt. Bilddaten für <see cref="Picture"/>
         /// </summary>
         readonly byte[] pictdata;


         public ListViewObjectItem(GarminImageCreator.SearchObject info) {
            pictdata = WinHelper.GetImageSource4WindowsBitmap(info.Bitmap, out ImageSource picture);
            Picture = picture;
            Name = info.Name;
            TypeName = info.TypeName;
         }

      }


      private List<ListViewObjectItem> _listOfResultsLists;

      public List<ListViewObjectItem> ListOfResultsLists {
         get => _listOfResultsLists;
         set {
            _listOfResultsLists = value;
            base.OnPropertyChanged();
         }
      }


      public ShowGarminInfo4LonLat(IList<GarminImageCreator.SearchObject> infos, string pretext) {
         InitializeComponent();

         ListOfResultsLists = new List<ListViewObjectItem>();
         foreach (GarminImageCreator.SearchObject so in infos)
            ListOfResultsLists.Add(new ListViewObjectItem(so));
         ListViewResults.ItemsSource = ListOfResultsLists;

         LabelPreText.Text = pretext.Trim();
      }
   }
}