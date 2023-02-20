using FSofTUtils.Geography.GeoCoding;
using FSofTUtils.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrackEddi {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class OsmSearchPage : ContentPage {

      /// <summary>
      /// Map-Control
      /// </summary>
      SpecialMapCtrl.SpecialMapCtrl map;

      AppData appData;

      /// <summary>
      /// zum Sortieren der <see cref="GeoCodingResultOsm"/>-Ergebnisse (nach "Entfernung" zur Kartenmitte)
      /// </summary>
      public class GeoCodingResultOsmDistanceComparer : Comparer<GeoCodingResultOsm> {

         public double centerlon, centerlat;

         public GeoCodingResultOsmDistanceComparer(double centerlon, double centerlat) {
            this.centerlon = centerlon;
            this.centerlat = centerlat;
         }

         public override int Compare(GeoCodingResultOsm x, GeoCodingResultOsm y) {
            return pseudodistance(centerlon, x.Longitude, centerlat, x.Latitude) <
                   pseudodistance(centerlon, y.Latitude, centerlat, y.Longitude) ? -1 : 1;
         }

         double pseudodistance(double lon1, double lon2, double lat1, double lat2) {
            return (lon1 - lon2) * (lon1 - lon2) + (lat1 - lat2) * (lat1 - lat2);
         }

      }

      ObservableCollection<PlaceItem> osmItems = new ObservableCollection<PlaceItem>();

      ObservableCollection<PlaceItem> osmReverseItems = new ObservableCollection<PlaceItem>();

      /// <summary>
      /// Text für die OSM-Suche
      /// </summary>
      public string OsmPlacePattern {
         get => appData.LastSearchPattern;
         set => appData.LastSearchPattern = value;
      }

      public Command<PlaceItem> OsmPlaceCmd { get; private set; }


      public OsmSearchPage(SpecialMapCtrl.SpecialMapCtrl map,
                           AppData appData) {
         InitializeComponent();
         this.map = map;
         this.appData = appData;

         OsmPlaceCmd = new Command<PlaceItem>(onOsmPlaceCmd);

         ListViewOsm.ItemsSource = PlaceItem.ConvertPlaceList(osmItems, appData.LastSearchResults);

         BindingContext = this;
      }

      #region OSM-Suche

      async private void buttonOsm_Clicked(object sender, EventArgs e) {
         try {
            string pattern = OsmPlacePattern?.Trim();
            if (!string.IsNullOrEmpty(pattern)) {
               osmItems.Clear();
               GeoCodingResultOsm[] geoCodingResultOsm = GeoCodingResultOsm.Get(pattern);
               if (geoCodingResultOsm.Length > 0) {
                  Array.Sort(geoCodingResultOsm, new GeoCodingResultOsmDistanceComparer(map.SpecMapCenterLon, map.SpecMapCenterLat));
                  foreach (GeoCodingResultOsm result in geoCodingResultOsm)
                     osmItems.Add(new PlaceItem(string.Format("{0} ({1:N6}° {2:N6}°)",
                                                            result.Name,
                                                            result.Longitude,
                                                            result.Latitude),
                                                  result.Longitude,
                                                  result.Latitude));
                  appData.LastSearchResults = PlaceItem.ConvertPlaceList(osmItems);
               }
            }
         } catch (Exception ex) {
            await FSofTUtils.Xamarin.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      async void onOsmPlaceCmd(PlaceItem item) {
         try {
            map.SpecMapZoomToRange(new PointD(item.Longitude, item.Latitude),
                                   new PointD(item.Longitude, item.Latitude),
                                   true);
            appData.LastZoom = map.Map_Zoom;
            appData.LastLongitude = map.SpecMapCenterLon;
            appData.LastLatitude = map.SpecMapCenterLat;

            await Navigation.PopAsync();     // diese Seite sofort schließen
         } catch (Exception ex) {
            await FSofTUtils.Xamarin.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      #endregion

      #region OSM-Rückwärtssuche für den Kartenmittelpunkt

      async void buttonOsmBack_Clicked(object sender, EventArgs e) {
         try {
            GeoCodingReverseResultOsm[] geoCodingReverseResultOsms = GeoCodingReverseResultOsm.Get(map.SpecMapCenterLon, map.SpecMapCenterLat);
            if (geoCodingReverseResultOsms.Length > 0) {
               osmReverseItems.Clear();
               foreach (var item in geoCodingReverseResultOsms) {
                  osmReverseItems.Add(new PlaceItem(item.Name, 0, 0));
               }
               ListViewOsmBack.ItemsSource = osmReverseItems;
            }
         } catch (Exception ex) {
            await FSofTUtils.Xamarin.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      #endregion

   }
}