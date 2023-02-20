using GMap.NET.Skia;
using SpecialMapCtrl;
using SpecialMapCtrl.EditHelper;
using System.Threading.Tasks;
using MapCtrl = SpecialMapCtrl.SpecialMapCtrl;

namespace TrackEddi {
   internal class EditTools {


      /// <summary>
      /// Masterpage
      /// </summary>
      MainPage page;

      /// <summary>
      /// für die Ermittlung der Höhendaten
      /// </summary>
      FSofTUtils.Geography.DEM.DemData dem = null;

      MapCtrl map;

      /// <summary>
      /// alle akt. GPX-Daten
      /// </summary>
      GpxAllExt gpx;

      EditMarkerHelper editMarkerHelper;

      EditTrackHelper editTrackHelper;

      public Track MarkedTrack = null;

      /// <summary>
      /// akt. bearbeiteter Track (oder null)
      /// </summary>
      public Track TrackInEdit => editTrackHelper?.TrackInEdit;

      public bool InWork => editMarkerHelper.InWork || editTrackHelper.InWork;



      public EditTools(MainPage page,
                       MapCtrl map,
                       FSofTUtils.Geography.DEM.DemData dem,
                       GpxAllExt gpx,
                       EditMarkerHelper editMarkerHelper,
                       EditTrackHelper editTrackHelper) {
         this.page = page;
         this.map = map;
         this.dem = dem;
         this.gpx = gpx;
         this.editMarkerHelper = editMarkerHelper;
         this.editTrackHelper = editTrackHelper;
      }

      public void StartMarkerMove(Marker marker) {
         editMarkerHelper.EditStart(marker);
         map.SpecMapRefresh(true, false, false);
      }

      public void MarkerRemove(Marker marker) => editMarkerHelper.Remove(marker);

      public void MarkerNew(System.Drawing.Point clientpoint) {
         editMarkerHelper.EditStart();
         // löst EditMarkerHelper_MarkerShouldInsertEvent() aus:
         editMarkerHelper.EditSetNewPos(clientpoint, dem);
      }

      public async void EndMarker(System.Drawing.Point dest, bool cancel = false) {
         if (!cancel) {
            if (editMarkerHelper.InWork)        // move marked marker
               editMarkerHelper.EditSetNewPos(dest, dem);
            else
               await page.SetNewMarker(dest);        // set new marker
         } else {
            if (editTrackHelper.InWork)
               editMarkerHelper.EditEnd();
         }
      }

      public void StartTrackDraw(Track track = null) {
         if (editTrackHelper.InWork)
            editTrackHelper.EditEndDraw();
         editTrackHelper.EditStart(track);
         MarkedTrack = null;
      }

      public void AddTrackPoint(System.Drawing.Point clientpt) {
         if (!editTrackHelper.InWork)
            editTrackHelper.EditStart();
         editTrackHelper.EditDraw_AppendPoint(clientpt, dem);
      }

      public void RemoveTrackPoint() {
         if (editTrackHelper.InWork)
            editTrackHelper.EditDraw_RemoveLastPoint();
      }

      public void EndTrackDraw() {
         if (editTrackHelper.InWork) {
            Track t = editTrackHelper.TrackInEdit;
            editTrackHelper.EditEndDraw();
            t.UpdateVisualTrack(map); // "echte" Farbe statt Farbe für editierbare Tracks
         }
         MarkedTrack = null;
      }

      public void EndTrackSplit(bool cancel = false) {
         if (editTrackHelper.InWork) {
            Track t = editTrackHelper.TrackInEdit;
            if (!cancel) {
               Track newtrack = editTrackHelper.EndSplit(page.ClientMapCenter);
               t.UpdateVisualTrack(map);           // "echte" Farbe statt Farbe für editierbare Tracks
               if (gpx.TrackList.Count > 0) {      // letzten Track noch sichtbar machen
                  newtrack.IsMarked4Edit = false;
                  page.ShowTrack(newtrack);
                  newtrack.UpdateVisualTrack(map);
               }
            } else {
               editTrackHelper.EndSplit(System.Drawing.Point.Empty);

               t.UpdateVisualTrack(map);           // "echte" Farbe statt Farbe für editierbare Tracks
            }
         }
         MarkedTrack = null;
      }

      public void EndTrackConcat(bool cancel = false) {
         if (editTrackHelper.InWork &&
             MarkedTrack != null) {
            if (!cancel) {
               editTrackHelper.EndConcat(MarkedTrack);
            } else {
               editTrackHelper.EndConcat(null);
            }
         }
         MarkedTrack = null;
      }

      public void TrackRemove(Track track) => editTrackHelper.Remove(track);

      /// <summary>
      /// Wenn eine Editieraktion läuft, wird gefragt ob diese abgebrochen werden soll.
      /// </summary>
      /// <returns>true wenn keine Editieraktion mehr aktiv ist</returns>
      public async Task<bool> Cancel() {
         bool canceled = false;
         bool inwork = false;
         if (editMarkerHelper.InWork) {
            inwork = true;
            if (await FSofTUtils.Xamarin.Helper.MessageBox(page,
                                                           "Marker setzen/bearbeiten",
                                                           "Abbrechen?",
                                                           "ja", "nein")) {
               editMarkerHelper.EditEnd(true);
               MarkedTrack = null;
               canceled = true;
            }
         } else if (editTrackHelper.InWork) {
            inwork = true;
            if (await FSofTUtils.Xamarin.Helper.MessageBox(page,
                                                           "Track bearbeiten",
                                                           "Abbrechen?",
                                                           "ja", "nein")) {
               Track t = editTrackHelper.TrackInEdit;
               switch (page.ProgramState) {
                  //case ProgState.Edit_TrackMark4Edit:
                  //case ProgState.Edit_TrackMark4Split:
                  //case ProgState.Edit_TrackMark4Concat:
                  //   editTrackHelper.EditEndDraw();
                  //   break;

                  case MainPage.ProgState.Edit_TrackDraw:
                     editTrackHelper.EditEndDraw(true);
                     t.UpdateVisualTrack(map); // "echte" Farbe statt Farbe für editierbare Tracks
                     break;

                  case MainPage.ProgState.Edit_TrackSplit:
                     editTrackHelper.EndSplit(System.Drawing.Point.Empty);
                     t.UpdateVisualTrack(map); // "echte" Farbe statt Farbe für editierbare Tracks
                     break;

                  case MainPage.ProgState.Edit_TrackConcat:
                     editTrackHelper.EndConcat(null);
                     t.UpdateVisualTrack(map); // "echte" Farbe statt Farbe für editierbare Tracks
                     break;
               }
               MarkedTrack = null;
               canceled = true;
            }
         }
         return !inwork || canceled;
      }

      public void MapDrawOnTop(GMapControl.DrawExtendedEventArgs e) {
         switch (page.ProgramState) {
            case MainPage.ProgState.Edit_Marker:
               editMarkerHelper.EditDrawDestinationLine(e.Graphics, page.ClientMapCenter);
               break;

            case MainPage.ProgState.Edit_TrackDraw:
               editTrackHelper.DrawDestinationLine(e.Graphics, page.ClientMapCenter);
               break;

            case MainPage.ProgState.Edit_TrackSplit:
               editTrackHelper.DrawSplitPoint(e.Graphics, page.ClientMapCenter);
               break;

            case MainPage.ProgState.Edit_TrackConcat:
               if (MarkedTrack != null)
                  editTrackHelper.DrawConcatLine(e.Graphics, MarkedTrack);
               break;

         }
      }


   }
}