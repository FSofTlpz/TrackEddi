using FSofTUtils.Geography.Garmin;
using FSofTUtils.Geometry;
using SpecialMapCtrl;
using SpecialMapCtrl.EditHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrackEddi {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class GpxContentPage : ContentPage {

      /// <summary>
      /// alle akt. GPX-Daten
      /// </summary>
      GpxAllExt gpx;

      /// <summary>
      /// Map-Control
      /// </summary>
      SpecialMapCtrl.SpecialMapCtrl map;

      EditMarkerHelper editMarkerHelper;

      /// <summary>
      /// Garmin-Symbole
      /// </summary>
      List<GarminSymbol> garminMarkerSymbols;

      AppData appData;


      public class ListViewObjectItem : INotifyPropertyChanged {

         public event PropertyChangedEventHandler PropertyChanged;


         public Track Track { get; protected set; }

         public Marker Marker { get; protected set; }


         /// <summary>
         /// Ist <see cref="Track"/> oder <see cref="Marker"/>.
         /// </summary>
         public bool IsTrack {
            get => Track != null;
         }

         public bool IsVisible {
            get => IsTrack ?
                        Track.IsVisible :
                        Marker.IsVisible;
            set {
               if (IsTrack) {
                  if (Track.IsVisible != value) {
                     Track.IsVisible = value;
                     Notify4PropChanged(nameof(IsVisible));
                  }
               } else {
                  if (Marker.IsVisible != value) {
                     Marker.IsVisible = value;
                     Notify4PropChanged(nameof(IsVisible));
                  }
               }
            }
         }

         public string Text1 {
            get => IsTrack ?
                        Track.Trackname :
                        Marker.Text;
         }

         public string Text2 {
            get {
               return IsTrack ?
                        (Track.StatLength < 1000 ?
                           (Track.StatLength.ToString("f0") + "m") :
                           ((Track.StatLength / 1000).ToString("f1") + "km"))
                        + (minDateTimeString != "" || maxDateTimeString != "" ? ", " : "")
                        + minDateTimeString
                        + (minDateTimeString != "" || maxDateTimeString != "" ? " .. " : "")
                        + maxDateTimeString :
                        Marker.Symbolname;
            }
         }

         string minDateTimeString => Track.StatMinDateTime == DateTime.MaxValue ? "" : Track.StatMinDateTime.ToString("g");

         string maxDateTimeString => Track.StatMaxDateTime == DateTime.MinValue ? "" : Track.StatMaxDateTime.ToString("g");

         /// <summary>
         /// nur für <see cref="Track"/>
         /// </summary>
         public Xamarin.Forms.Color TrackColor {
            get => IsTrack ?
                        new Xamarin.Forms.Color(Track.LineColor.R / 255.0,
                                                Track.LineColor.G / 255.0,
                                                Track.LineColor.B / 255.0,
                                                Track.LineColor.A / 255.0) :
                        Xamarin.Forms.Color.DarkGray;      // Dummy
            set {
               if (IsTrack) {
                  System.Drawing.Color col = value;
                  if (col != Track.LineColor) {
                     Track.LineColor = value; // XamarinColor2WinColor(value); Interessanterweise erfolgt die Umwandlung automatisch!
                     Notify4PropChanged(nameof(TrackColor));
                  }
               }
            }
         }

         /// <summary>
         /// nur für <see cref="Marker"/>
         /// </summary>
         public ImageSource Picture { get; protected set; }

         /// <summary>
         /// akt. Bilddaten für <see cref="Picture"/>
         /// </summary>
         byte[] pictdata;



         public ListViewObjectItem(Track t) {
            Track = t;
         }

         public ListViewObjectItem(Marker m) {
            Marker = m;
            pictdata = WinHelper.GetImageSource4WindowsBitmap(Marker.Bitmap, out ImageSource picture);
            Picture = picture;
         }

         /// <summary>
         /// zum Auslösen eines <see cref="PropertyChanged"/>-Events (auch "extern")
         /// </summary>
         /// <param name="propname"></param>
         public void Notify4PropChanged(string propname) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
         }

         public void SetMarkerPicture(string newsymbolname) {
            Marker.Symbolname = newsymbolname;
            pictdata = WinHelper.GetImageSource4WindowsBitmap(Marker.Bitmap, out ImageSource picture);
            Picture = picture;
            Notify4PropChanged(nameof(Picture));
         }

         //static System.Drawing.Color XamarinColor2WinColor(Color xamcol) {
         //   return System.Drawing.Color.FromArgb((int)Math.Round(xamcol.A * 255), 
         //                                        (int)Math.Round(xamcol.R * 255), 
         //                                        (int)Math.Round(xamcol.G * 255),
         //                                        (int)Math.Round(xamcol.B * 255));
         //}

         public override string ToString() {
            return string.Format("{0}: Text1={1}, Text2={2}",
                                 IsTrack ? "Track" : "Waypoint",
                                 Text1,
                                 Text2);
         }

      }

      readonly ObservableCollection<ListViewObjectItem> tracklst;
      readonly ObservableCollection<ListViewObjectItem> waypointlst;


      public Command<ListViewObjectItem> TrackProps { get; private set; }
      public Command<ListViewObjectItem> TrackColor { get; private set; }
      public Command<ListViewObjectItem> TrackZoom { get; private set; }
      public Command<ListViewObjectItem> TrackTurnBack { get; private set; }
      public Command<ListViewObjectItem> TrackSimpl { get; private set; }
      public Command<ListViewObjectItem> TrackDelete { get; private set; }
      public Command<ListViewObjectItem> TrackMoveDown { get; private set; }
      public Command<ListViewObjectItem> TrackMoveUp { get; private set; }

      public Command<ListViewObjectItem> MarkerProps { get; private set; }
      public Command<ListViewObjectItem> MarkerSymbol { get; private set; }
      public Command<ListViewObjectItem> MarkerZoom { get; private set; }
      public Command<ListViewObjectItem> MarkerDelete { get; private set; }
      public Command<ListViewObjectItem> MarkerMoveDown { get; private set; }
      public Command<ListViewObjectItem> MarkerMoveUp { get; private set; }


      public GpxContentPage(GpxAllExt gpx,
                            SpecialMapCtrl.SpecialMapCtrl map,
                            EditMarkerHelper editMarkerHelper,
                            List<GarminSymbol> garminMarkerSymbols,
                            AppData appdata) {
         InitializeComponent();

         BindingContext = this;

         this.gpx = gpx;
         this.map = map;
         this.editMarkerHelper = editMarkerHelper;
         this.garminMarkerSymbols = garminMarkerSymbols;
         appData = appdata;

         tracklst = new ObservableCollection<ListViewObjectItem>();
         waypointlst = new ObservableCollection<ListViewObjectItem>();
         init();

         TrackProps = new Command<ListViewObjectItem>(onTrackProps);
         TrackColor = new Command<ListViewObjectItem>(onTrackColor);
         TrackZoom = new Command<ListViewObjectItem>(onTrackZoom);
         TrackTurnBack = new Command<ListViewObjectItem>(onTrackTurnBack);
         TrackDelete = new Command<ListViewObjectItem>(onTrackDelete);
         TrackSimpl = new Command<ListViewObjectItem>(onTrackSimpl);
         TrackMoveDown = new Command<ListViewObjectItem>(onTrackMoveDown);
         TrackMoveUp = new Command<ListViewObjectItem>(onTrackMoveUp);

         MarkerProps = new Command<ListViewObjectItem>(onMarkerProps);
         MarkerSymbol = new Command<ListViewObjectItem>(onMarkerSymbol);
         MarkerZoom = new Command<ListViewObjectItem>(onMarkerZoom);
         MarkerDelete = new Command<ListViewObjectItem>(onMarkerDelete);
         MarkerMoveDown = new Command<ListViewObjectItem>(onMarkerMoveDown);
         MarkerMoveUp = new Command<ListViewObjectItem>(onMarkerMoveUp);

      }

      void init() {
         for (int i = 0; i < gpx.TrackList.Count; i++)
            tracklst.Add(new ListViewObjectItem(gpx.TrackList[i]));

         for (int i = 0; i < gpx.MarkerList.Count; i++)
            waypointlst.Add(new ListViewObjectItem(gpx.MarkerList[i]));

         ListViewTracks.ItemsSource = tracklst;
         ListViewMarker.ItemsSource = waypointlst;
      }

      #region Toolbaritems

      private void ToolbarItem_ShowAllTracks_Clicked(object sender, EventArgs e) {
         foreach (var t in tracklst)
            if (!t.IsVisible)
               t.IsVisible = true;
      }

      private void ToolbarItem_HideAllTracks_Clicked(object sender, EventArgs e) {
         foreach (var t in tracklst)
            if (t.IsVisible)
               t.IsVisible = false;
      }

      async private void ToolbarItem_DeleteAllVisibleTracks_Clicked(object sender, EventArgs e) {
         try {
            int count = 0;
            foreach (var t in tracklst)
               if (t.IsVisible)
                  count++;
            if (count > 0)
               if (await FSofTUtils.Xamarin.Helper.MessageBox(this, "Achtung", "Sollen wirklich ALLE angezeigten Tracks (" + count + ") gelöscht werden?", "ja", "nein")) {
                  for (int i = tracklst.Count - 1; i >= 0; i--)
                     if (tracklst[i].IsVisible)
                        trackDelete(tracklst[i]);
               }
         } catch (Exception ex) {
            await FSofTUtils.Xamarin.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      private void ToolbarItem_ShowAllMarker_Clicked(object sender, EventArgs e) {
         foreach (var wp in waypointlst)
            if (!wp.IsVisible)
               wp.IsVisible = true;
      }

      private void ToolbarItem_HideAllMarker_Clicked(object sender, EventArgs e) {
         foreach (var wp in waypointlst)
            if (wp.IsVisible)
               wp.IsVisible = false;
      }

      async private void ToolbarItem_DeleteAllVisibleMarker_Clicked(object sender, EventArgs e) {
         try {
            int count = 0;
            foreach (var td in waypointlst)
               if (td.IsVisible)
                  count++;
            if (count > 0)
               if (await FSofTUtils.Xamarin.Helper.MessageBox(this, "Achtung", "Sollen wirklich ALLE angezeigten Marker (" + count + ") gelöscht werden?", "ja", "nein")) {
                  for (int i = waypointlst.Count - 1; i >= 0; i--)
                     if (waypointlst[i].IsVisible)
                        markerDelete(waypointlst[i]);
               }
         } catch (Exception ex) {
            await FSofTUtils.Xamarin.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      #endregion

      #region Track-Bearbeitung

      async void onTrackProps(ListViewObjectItem td) {
         try {
            Track trackcopy = Track.CreateCopy(td.Track);
            EditTrackPage page = new EditTrackPage(trackcopy);
            page.EndWithOk += (object sender2, EventArgs e2) => {
               td.Track.LineColor = trackcopy.LineColor;
               td.Track.GpxTrack.Name = trackcopy.GpxTrack.Name;
               td.Track.GpxTrack.Description = trackcopy.GpxTrack.Description;
               td.Track.GpxTrack.Comment = trackcopy.GpxTrack.Comment;
               td.Track.GpxTrack.Source = trackcopy.GpxTrack.Source;
               td.Notify4PropChanged(nameof(ListViewObjectItem.Text1));
            };
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await FSofTUtils.Xamarin.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      async void onTrackColor(ListViewObjectItem td) {
         try {
            ColorChoosingPage page = new ColorChoosingPage() {
               ActualColor = td.TrackColor,
            };
            page.EndWithOk += (object sender, EventArgs e) => {
               td.TrackColor = page.ActualColor;
            };
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await FSofTUtils.Xamarin.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      async void onTrackZoom(ListViewObjectItem td) {
         map.SpecMapZoomToRange(new PointD(td.Track.Bounds.MinLon, td.Track.Bounds.MaxLat),
                                new PointD(td.Track.Bounds.MaxLon, td.Track.Bounds.MinLat),
                                true);
         appData.LastZoom = map.Map_Zoom;
         appData.LastLongitude = map.SpecMapCenterLon;
         appData.LastLatitude = map.SpecMapCenterLat;

         await Navigation.PopAsync();     // diese Seite sofort schließen
      }

      async void onTrackTurnBack(ListViewObjectItem td) {
         if (await FSofTUtils.Xamarin.Helper.MessageBox(this, "Achtung", "Soll die Richtung des Tracks '" + td.Text1 + "' umgekehrt werden?", "ja", "nein")) {
            map.SpecMapShowTrack(td.Track, false, null);
            td.Track.ChangeDirection();
            map.SpecMapShowTrack(td.Track, true, null);
         }
      }

      async void onTrackSimpl(ListViewObjectItem td) {
         try {
            SimplifyTrackPage page = new SimplifyTrackPage(td.Track, appData);
            page.EndWithOk += (object sender2, EventArgs e2) => {
               if (page.NewTrack != null
                   && page.NewTrack.GpxTrack.Segments[0].Points.Count > 1) {
                  int pos = tracklst.IndexOf(td);
                  Track newtrack = gpx.TrackInsertCopy(page.NewTrack, pos);      // "über" dem Originaltrack
                  newtrack.Trackname = gpx.GetUniqueTrackname(newtrack.Trackname);
                  newtrack.LineColor = td.Track.LineColor;
                  newtrack.LineWidth = td.Track.LineWidth;

                  map.SpecMapShowTrack(newtrack, true, pos == 0 ? null : gpx.TrackList[pos - 1]);

                  tracklst.Insert(pos, new ListViewObjectItem(newtrack) { IsVisible = true });
               }
            };
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await FSofTUtils.Xamarin.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      async void onTrackDelete(ListViewObjectItem td) {
         if (await FSofTUtils.Xamarin.Helper.MessageBox(this, "Achtung", "Soll der Track '" + td.Text1 + "' wirklich gelöscht werden?", "ja", "nein"))
            trackDelete(td);
      }

      void onTrackMoveUp(ListViewObjectItem td) {
         int idx = tracklst.IndexOf(td);
         if (0 < idx) {
            gpx.TrackOrderChange(idx, idx - 1);

            tracklst.RemoveAt(idx);
            tracklst.Insert(idx - 1, td);
            ListViewTracks.ScrollTo(td, ScrollToPosition.MakeVisible, true);
         }
      }

      void onTrackMoveDown(ListViewObjectItem td) {
         int idx = tracklst.IndexOf(td);
         if (0 <= idx && idx < tracklst.Count - 1) {
            gpx.TrackOrderChange(idx, idx + 1);

            tracklst.RemoveAt(idx);
            tracklst.Insert(idx + 1, td);
            ListViewTracks.ScrollTo(td, ScrollToPosition.MakeVisible, true);
         }
      }

      #endregion

      #region Marker-Bearbeitung

      async void onMarkerProps(ListViewObjectItem td) {
         try {
            EditMarkerPage page = new EditMarkerPage(td.Marker, garminMarkerSymbols);
            page.EndWithOk += (object sender, EventArgs e) => {
               td.SetMarkerPicture(page.Marker.Symbolname);      // ev. geändert
               int idx = gpx.MarkerIndex(page.Marker);
               if (idx >= 0) {
                  gpx.GpxDataChanged = true;
                  gpx.Waypoints[idx] = page.Marker.Waypoint;
                  editMarkerHelper.RefreshOnMap(page.Marker);
                  td.Notify4PropChanged(nameof(ListViewObjectItem.Text1));
                  td.Notify4PropChanged(nameof(ListViewObjectItem.Picture));
               }
            };
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await FSofTUtils.Xamarin.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      async void onMarkerSymbol(ListViewObjectItem td) {
         try {
            SymbolChoosingPage page = new SymbolChoosingPage(garminMarkerSymbols, td.Marker.Waypoint.Symbol);
            page.EndWithOk += (object sender, EventArgs e) => {
               td.Marker.Waypoint.Symbol = page.ActualGarminSymbol.Name;
               td.SetMarkerPicture(page.ActualGarminSymbol.Name);

               int idx = gpx.MarkerIndex(td.Marker);
               if (idx >= 0) {
                  gpx.GpxDataChanged = true;
                  gpx.Waypoints[idx] = td.Marker.Waypoint;
                  editMarkerHelper.RefreshOnMap(td.Marker);
               }
            };
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await FSofTUtils.Xamarin.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      async void onMarkerZoom(ListViewObjectItem td) {
         map.SpecMapSetLocationAndZoom(map.SpecMapZoom, td.Marker.Longitude, td.Marker.Latitude);

         appData.LastLongitude = map.SpecMapCenterLon;
         appData.LastLatitude = map.SpecMapCenterLat;

         await Navigation.PopAsync();     // diese Seite sofort schließen
      }

      async void onMarkerDelete(ListViewObjectItem td) {
         if (await FSofTUtils.Xamarin.Helper.MessageBox(this, "Achtung", "Soll der Marker '" + td.Text1 + "' wirklich gelöscht werden?", "ja", "nein"))
            markerDelete(td);
      }

      void onMarkerMoveUp(ListViewObjectItem td) {
         int idx = waypointlst.IndexOf(td);
         if (0 < idx) {
            gpx.MarkerOrderChange(idx, idx - 1);

            waypointlst.RemoveAt(idx);
            waypointlst.Insert(idx - 1, td);
            ListViewMarker.ScrollTo(td, ScrollToPosition.MakeVisible, true);
         }
      }

      void onMarkerMoveDown(ListViewObjectItem td) {
         int idx = waypointlst.IndexOf(td);
         if (0 <= idx && idx < waypointlst.Count - 1) {
            gpx.MarkerOrderChange(idx, idx + 1);

            waypointlst.RemoveAt(idx);
            waypointlst.Insert(idx + 1, td);
            ListViewMarker.ScrollTo(td, ScrollToPosition.MakeVisible, true);
         }
      }

      #endregion

      void trackDelete(ListViewObjectItem td) {
         map.SpecMapShowTrack(td.Track, false, null);
         gpx.TrackRemove(td.Track);
         tracklst.Remove(td);
      }

      void markerDelete(ListViewObjectItem td) {
         map.SpecMapShowMarker(td.Marker, false);
         gpx.MarkerRemove(td.Marker);
         waypointlst.Remove(td);
      }

   }
}