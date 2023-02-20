using GMap.NET.CoreExt.MapProviders;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrackEddi {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class MapChoosingPage : ContentPage {


      public SpecialMapCtrl.SpecialMapCtrl MapControl;

      class ListViewObjectItem {

         MapProviderDefinition mpd;

         public string Text {
            get => mpd.MapName + " (" + mpd.ProviderName + ")";
         }

         public ListViewObjectItem(MapProviderDefinition def) {
            mpd = def;
         }

         public override string ToString() {
            return string.Format("{0}", Text);
         }

      }

      List<ListViewObjectItem> listViewObjectItems;


      public class MapChoosingEventArgs : EventArgs {
         /// <summary>
         /// Index der ausgewählten <see cref="MapProviderDefinition"/>
         /// </summary>
         public int Idx { get; private set; }

         public MapChoosingEventArgs(int idx) {
            Idx = idx;
         }

      }

      /// <summary>
      /// Auswahl ist beendet
      /// </summary>
      public event EventHandler<MapChoosingEventArgs> MapChoosingEvent;



      public MapChoosingPage() {
         InitializeComponent();
      }

      protected override void OnAppearing() {
         base.OnAppearing();

         listViewObjectItems = new List<ListViewObjectItem>();
         int selectedIdx = MapControl.SpecMapActualMapIdx;
         ListViewObjectItem selectedItem = null;
         for (int i = 0; i < MapControl.SpecMapProviderDefinitions.Count; i++) {
            listViewObjectItems.Add(new ListViewObjectItem(MapControl.SpecMapProviderDefinitions[i]));
            if (i == selectedIdx)
               selectedItem = listViewObjectItems[i];
         }
         ListViewMaps.ItemsSource = listViewObjectItems;
         ListViewMaps.SelectedItem = selectedItem;
         if (selectedItem != null)
            ListViewMaps.ScrollTo(selectedItem, ScrollToPosition.Center, false);
      }

      private void ListViewMaps_ItemSelected(object sender, SelectedItemChangedEventArgs e) {
         if (e.SelectedItemIndex != MapControl.SpecMapActualMapIdx) {
            Navigation.PopAsync();     // diese Seite sofort schließen
            MapChoosingEvent?.Invoke(this, new MapChoosingEventArgs(e.SelectedItemIndex));
         }
      }

   }
}