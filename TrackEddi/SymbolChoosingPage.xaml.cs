using FSofTUtils.Geography.Garmin;
using SpecialMapCtrl;
using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrackEddi {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class SymbolChoosingPage : ContentPage {

      /// <summary>
      /// Dialog mit Auswahl beendet
      /// </summary>
      public event EventHandler<EventArgs> EndWithOk;


      public class ListViewObjectItem {

         public string Name { get; protected set; }

         public string Group { get; protected set; }

         public ImageSource Picture { get; protected set; }

         /// <summary>
         /// akt. Bilddaten für <see cref="Picture"/>
         /// </summary>
         byte[] pictdata;

         public GarminSymbol GarminSymbol { get; protected set; }


         public ListViewObjectItem(GarminSymbol symbol) {
            pictdata = WinHelper.GetImageSource4WindowsBitmap(symbol.Bitmap, out ImageSource picture);
            Picture = picture;
            Name = symbol.Name;
            Group = symbol.Group;
            this.GarminSymbol = symbol;
         }

      }

      public class SubList : List<ListViewObjectItem> {
         public string Heading { get; set; }

         public List<ListViewObjectItem> Symbols => this;

         public override string ToString() {
            return Heading + " (" + Count + ")";
         }

      }


      public Command<ListViewObjectItem> ChooseSymbol { get; private set; }

      private List<SubList> _listOfSymbolLists;

      public List<SubList> ListOfSymbolLists {
         get => _listOfSymbolLists;
         set {
            _listOfSymbolLists = value;
            base.OnPropertyChanged();
         }
      }

      public GarminSymbol ActualGarminSymbol { get; protected set; }

      string oldsymbolname = null;


      public SymbolChoosingPage(IList<GarminSymbol> garminmarkersymbols, string oldsymbolname) {
         InitializeComponent();

         this.oldsymbolname = oldsymbolname;
         if (string.IsNullOrEmpty(this.oldsymbolname))
            oldsymbolname = "Flag, Green";            // <--> passend zum VisualMarker für editierbare Marker

         ChooseSymbol = new Command<ListViewObjectItem>(onChoosing);

         ListOfSymbolLists = new List<SubList>();
         SubList lst = null;
         string groupname = "";
         ListViewObjectItem oldsymbol = null;
         foreach (var item in garminmarkersymbols) {
            if (groupname != item.Group) {
               groupname = item.Group;
               lst = new SubList() {
                  Heading = groupname,
               };
               ListOfSymbolLists.Add(lst);
            }
            lst.Add(new ListViewObjectItem(item));
            if (!string.IsNullOrEmpty(this.oldsymbolname) &&
                this.oldsymbolname == item.Name) {
               oldsymbol = lst[lst.Count - 1];
            }
         }
         ListViewSymbols.ItemsSource = ListOfSymbolLists;

         if (oldsymbol != null) {
            ListViewSymbols.ScrollTo(oldsymbol, ScrollToPosition.MakeVisible, true);
            ListViewSymbols.SelectedItem = oldsymbol;
         }
      }

      async void onChoosing(ListViewObjectItem td) {
         ActualGarminSymbol = td.GarminSymbol;
         EndWithOk?.Invoke(this, EventArgs.Empty);
         await Navigation.PopAsync();     // diese Seite sofort schließen
      }

   }
}