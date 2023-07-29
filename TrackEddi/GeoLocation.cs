using SkiaSharp;
using SpecialMapCtrl;
using System;
using System.Threading;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TrackEddi {
   public class GeoLocation : IDisposable {

      /// <summary>
      /// ungültiger Wert
      /// </summary>
      const double NOTVALID_DOUBLE = FSofTUtils.Geography.PoorGpx.BaseElement.NOTVALID_DOUBLE;

      public string logfile;


      SpecialMapCtrl.SpecialMapCtrl mapControl;

      Location lastLocation = null;

      string lastLocationProvider = null;

      object locationLocker = new object();

      public bool LocationSelfCentering { get; set; }

      /// <summary>
      /// der Service wurde gestartet, ist aber ev. noch nicht aktiv (siehe <see cref="LocationServiceIsActiv"/>)
      /// </summary>
      public bool LocationServiceIsStarted { get; protected set; }

      /// <summary>
      /// läuft der Service (kann noch eine kurze Zeit false nach <see cref="StartGeoLocationService"/> liefern!)
      /// </summary>
      public bool LocationServiceIsActiv =>
         serviceCtrl.ServiceIsActive();

      bool _locationIsShowing = false;

      /// <summary>
      /// Wird die akt. Position angezeigt?
      /// </summary>
      public bool LocationIsShowing {
         get => LocationServiceIsActiv && _locationIsShowing;
         set {
            if (LocationIsShowing != value) {
               _locationIsShowing = value;
               if (_locationIsShowing)
                  lastLocation = null;
               mapControl.SpecMapRefresh(false, false, false);
            }
         }
      }

      /// <summary>
      /// Wird die akt. Bewegung als Track aufgezeichnet?
      /// <para>Start mit <see cref="StartTracking"/></para>
      /// <para>Stop mit <see cref="EndTracking"/></para>
      /// </summary>
      public bool LocationTracking =>
         LocationServiceIsActiv && LiveTrack != null;

      Track LiveTrack { get; set; }

      double mindistance = 0, mindeltaheight = 0;

      /// <summary>
      /// die Richtung (gemessen in Grad) relativ zum magnetischen Norden
      /// </summary>
      double headingMagneticNorth;

      IGeoLocationServiceCtrl serviceCtrl { get; set; }


      public GeoLocation(SpecialMapCtrl.SpecialMapCtrl map) {
         mapControl = map;
         serviceCtrl = DependencyService.Get<IGeoLocationServiceCtrl>();
         serviceCtrl.LocationChanged += locationChanged;
         Compass.ReadingChanged += Compass_ReadingChanged;
      }

      private void Compass_ReadingChanged(object sender, CompassChangedEventArgs e) =>
         Interlocked.Exchange(ref headingMagneticNorth, e.Reading.HeadingMagneticNorth);

      /// <summary>
      /// startet den Geo-Service
      /// </summary>
      /// <param name="updateintervall">Zeit in ms für Update</param>
      /// <param name="updatedistance">Positionsänderung in m für Update</param>
      /// <returns>true, wenn der Service arbeitet</returns>
      public bool StartGeoLocationService(int updateintervall = 1000, double updatedistance = 1) {
         if (!Compass.IsMonitoring)
            Compass.Start(SensorSpeed.Default, true);
         if (!LocationServiceIsActiv)
            // ACHTUNG: Der Start des Service wurde initiiert, aber er läuft ev. erst etwas verzögert an!
            return LocationServiceIsStarted = serviceCtrl.StartService(updateintervall, updatedistance);
         return LocationServiceIsActiv;
      }

      /// <summary>
      /// stopt den Geo-Service
      /// </summary>
      /// <returns>true, wenn der Service nicht (mehr) arbeitet</returns>
      public bool StopGeoLocationService() {
         if (Compass.IsMonitoring)
            Compass.Stop();
         if (LocationServiceIsActiv) {
            EndTracking();
            LocationServiceIsStarted = false;
            return serviceCtrl.StopService();
         }
         return !LocationServiceIsActiv;
      }

      /// <summary>
      /// falls <see cref="LocationIsShowing"/>==true wird die akt. Position gezeichnet
      /// </summary>
      /// <param name="g"></param>
      /// <param name="symbolradius">Symbolradius in Clientkoordinaten</param>
      public void ShowPosition(System.Drawing.Graphics g, float symbolradius = 50F) {
         if (LocationIsShowing) {
            Location location = GetLastLocation(out _);
            if (location != null)
               if (drawLocationSymbol(g, symbolradius, location, (int)symbolradius) &&
                   LocationSelfCentering)
                  mapControl.SpecMapSetLocationAndZoom(mapControl.SpecMapZoom, location.Longitude, location.Latitude);
         }
      }

      /// <summary>
      /// startet einen neuen Track der aufgezeichnet wird
      /// </summary>
      /// <param name="gpx"></param>
      /// <param name="mindistance"></param>
      /// <param name="mindeltaheight"></param>
      /// <param name="track"></param>
      public void StartTracking(GpxAllExt gpx, double mindistance, double mindeltaheight, Track track = null) {
         EndTracking();
         this.mindistance = mindistance;
         this.mindeltaheight = mindeltaheight;
         if (LocationServiceIsStarted) {
            if (track != null)
               LiveTrack = track;
            else {
               LiveTrack = gpx.TrackInsertCopy(new Track(new FSofTUtils.Geography.PoorGpx.GpxTrackPoint[0],
                                                         "Livetrack " + DateTime.Now.ToString("G")) {
                  LineColor = VisualTrack.LiveDrawColor,
                  LineWidth = VisualTrack.LiveDrawWidth,
               },
                                               0,
                                               true);
               LiveTrack.IsOnLiveDraw = true;
            }
         }
      }

      /// <summary>
      /// Wenn <see cref="LocationServiceIsActiv"/>==true kann die letzte bekannte Position geliefert werden. Kann keine
      /// Position ermittelt werden, wird false geliefert
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="height"></param>
      /// <param name="datetime"></param>
      /// <returns></returns>
      public bool GetLastPosition(out double lon, out double lat, out double height, out DateTime datetime) {
         if (LocationServiceIsActiv)
            return getLastPosition(out lon, out lat, out height, out datetime);
         else {
            lon = lat = height = 0;
            datetime = DateTime.Now;
         }
         return false;
      }

      /// <summary>
      /// fügt einen neuen Punkt zum akt. Track hinzu, wenn sein Abstand zum letzten Punkt oder die Höhenänderung groß genug ist
      /// </summary>
      /// <param name="mindistance"></param>
      /// <param name="mindeltaheight"></param>
      void appendTrackPoint(double mindistance, double mindeltaheight) {
         if (LiveTrack != null &&
             GetLastPosition(out double lon, out double lat, out double height, out DateTime datetime)) {

            double distance = 0;
            double deltaheigth = 0;
            if (0 < LiveTrack.GpxSegment.Points.Count) {
               FSofTUtils.Geography.PoorGpx.GpxTrackPoint lastTP = LiveTrack.GetGpxPoint(LiveTrack.GpxSegment.Points.Count - 1);
               distance = Distance(lon, lat, lastTP.Lon, lastTP.Lat);
               deltaheigth = lastTP.Elevation != NOTVALID_DOUBLE &&
                             height != NOTVALID_DOUBLE ?
                                    height - lastTP.Elevation :
                                    0;
            }

            if (LiveTrack.GpxSegment.Points.Count == 0 ||
                distance >= mindistance ||
                deltaheigth >= mindeltaheight) {
               showTrack(LiveTrack, false);
               LiveTrack.GpxSegment.Points.Add(new FSofTUtils.Geography.PoorGpx.GpxTrackPoint(lon, lat, height, datetime));
               LiveTrack.GpxDataContainer.GpxDataChanged = true;
               LiveTrack.UpdateVisualTrack(mapControl);
               showTrack(LiveTrack, true);
            }
         }
      }

      /// <summary>
      /// beendet die Trackaufzeichnung
      /// </summary>
      public void EndTracking() {
         if (LocationTracking)
            if (LiveTrack != null) {
               if (LiveTrack.GpxSegment.Points.Count > 1) {       // Trackaufzeichnung beenden und Track im Container speichern
                  LiveTrack.IsOnLiveDraw = false;
                  LiveTrack.RefreshBoundingbox();
                  LiveTrack.CalculateStats();
               } else {       // zu wenig Punkte
                  showTrack(LiveTrack, false);                    // Sichtbarkeit ausschalten
                  LiveTrack.GpxDataContainer.TrackRemove(LiveTrack);
               }
               LiveTrack = null;
            }
      }

      /// <summary>
      /// liefert threadsicher eine Kopie der letzten Position (oder null)
      /// </summary>
      /// <param name="provider"></param>
      /// <returns></returns>
      public Location GetLastLocation(out string provider) {
         lock (locationLocker) {
            provider = lastLocationProvider;
            return lastLocation != null ? new Location(lastLocation) : null;
         }
      }

      /// <summary>
      /// berechnet die Entfernung in Meter
      /// </summary>
      /// <param name="fromlon"></param>
      /// <param name="fromlat"></param>
      /// <param name="tolon"></param>
      /// <param name="tolat"></param>
      /// <returns></returns>
      public static double Distance(double fromlon, double fromlat, double tolon, double tolat) =>
         new Location(fromlat, fromlon).CalculateDistance(tolat, tolon, DistanceUnits.Kilometers) * 1000;


      bool getLastPosition(out double lon, out double lat, out double heigth, out DateTime datetime) {
         lon = lat = heigth = double.MinValue;
         datetime = DateTime.MinValue;
         Location location = GetLastLocation(out _);
         if (location != null) {
            lon = location.Longitude;
            lat = location.Latitude;
            if (location.Altitude != null)
               heigth = (double)location.Altitude;
            else
               heigth = NOTVALID_DOUBLE;
            datetime = new DateTime(location.Timestamp.Ticks);
            return true;
         }
         return false;
      }

      /// <summary>
      /// die Geo-Position wurde verändert
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void locationChanged(object sender, LocationChangedArgs e) {
         lock (locationLocker) {
            if (e.Provider == "network" &&
                e.Location.Accuracy > 15)
               return;

            lastLocation = new Location(e.Location);
            lastLocationProvider = e.Provider;

            if (e.Location.Altitude != null && !serviceCtrl.IsValid((double)e.Location.Altitude))
               lastLocation.Altitude = null;
            if (e.Location.Course != null && !serviceCtrl.IsValid((double)e.Location.Course))
               lastLocation.Course = null;
            if (e.Location.Speed != null && !serviceCtrl.IsValid((double)e.Location.Speed))
               lastLocation.Speed = null;
            if (e.Location.Accuracy != null && !serviceCtrl.IsValid((double)e.Location.Accuracy))
               lastLocation.Accuracy = null;
            if (e.Location.VerticalAccuracy != null && !serviceCtrl.IsValid((double)e.Location.VerticalAccuracy))
               lastLocation.VerticalAccuracy = null;
         }


         System.IO.File.AppendAllText(logfile,
                                      lastLocation.Timestamp.ToString("G") + " " + lastLocationProvider + ": "
                                      + lastLocation.Longitude.ToString("f6") + "° "
                                      + lastLocation.Latitude.ToString("f6") + "° "
                                      + (lastLocation.Altitude != null ? lastLocation.Altitude.Value.ToString("f1") + "m " : " ")
                                      + (lastLocation.Course != null ? lastLocation.Course.Value.ToString("f1") + "° " : " ")
                                      + (lastLocation.Speed != null ? lastLocation.Speed.Value.ToString("f1") + "m/s " : " ")
                                      + (lastLocation.Accuracy != null ? lastLocation.Accuracy.Value.ToString("f1") + "m " : " ")
                                      + (lastLocation.VerticalAccuracy != null ? lastLocation.VerticalAccuracy.Value.ToString("f1") + "m " : " ")
                                      + System.Environment.NewLine);



         if (LocationIsShowing || LocationTracking)
            mapControl.SpecMapRefresh(false, false, false);

         if (LocationTracking)
            appendTrackPoint(mindistance, mindeltaheight);
      }

      /// <summary>
      /// zeichnet ein Symbol für die akt. Position
      /// </summary>
      /// <param name="g"></param>
      /// <param name="symbolradius">Größe des Symbols in Clientkoordinaten</param>
      /// <param name="geplocation">Geo-Position</param>
      /// <param name="selfcenteringdistance">Distanz in Clientkoordinaten ab der eine neue Zentrierung der Karte erfolgen sollte</param>
      /// <returns>liefert true, wenn eine Zentrierung erfolgen sollte</returns>
      bool drawLocationSymbol(System.Drawing.Graphics g,
                              float symbolradius = 50,
                              Location geplocation = null,
                              int selfcenteringdistance = 50) {
         if (geplocation != null) {
            System.Drawing.Point ptClient = mapControl.SpecMapLonLat2Client(geplocation.Longitude, geplocation.Latitude);

            double? course = geplocation.Course;
            if (course == null)
               course = headingMagneticNorth;

            float radians = course != null ?
                                 (float)((course - 90) / 180 * Math.PI) :
                                 float.MinValue;

            System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(180, System.Drawing.Color.Red));
            System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Red, 3);

            float radius = symbolradius;
            g.DrawEllipse(pen,
                          ptClient.X - radius,
                          ptClient.Y - radius,
                          2 * radius,
                          2 * radius);

            SKMatrix orgMatrix = g.SKCanvas.TotalMatrix;
            if (radians != float.MinValue) {
               SKMatrix rotMatrix = SKMatrix.CreateRotation(radians, ptClient.X, ptClient.Y);
               g.SKCanvas.SetMatrix(g.SKCanvas.TotalMatrix.PreConcat(rotMatrix));    // jetzt neues KS: Linie kommt "von links" und endet in (xend, yend)
            }

            if (radians != float.MinValue) {
               g.DrawLine(pen,
                          ptClient.X - symbolradius,
                          ptClient.Y,
                          ptClient.X,
                          ptClient.Y);
               drawArrow(g,
                         brush,
                         ptClient.X,
                         ptClient.Y,
                         ptClient.X - 0.7F * symbolradius,
                         symbolradius / 3);
            } else {
               radius = symbolradius / 10;
               g.FillEllipse(brush,
                             ptClient.X - radius,
                             ptClient.Y - radius,
                             2 * radius,
                             2 * radius);
            }

            g.SKCanvas.SetMatrix(orgMatrix);

            brush.Dispose();
            pen.Dispose();

            if (ptClient.Y < mapControl.Height / 2 - selfcenteringdistance || mapControl.Height / 2 + selfcenteringdistance < ptClient.Y ||
                ptClient.X < mapControl.Width / 2 - selfcenteringdistance || mapControl.Width / 2 + selfcenteringdistance < ptClient.X)
               return true;
         }
         return false;
      }

      /// <summary>
      /// einfache Pfeilspitze von links nach rechts
      /// </summary>
      /// <param name="g"></param>
      /// <param name="brush"></param>
      /// <param name="peakx"></param>
      /// <param name="peaky"></param>
      /// <param name="basex"></param>
      /// <param name="basey"></param>
      void drawArrow(System.Drawing.Graphics g, System.Drawing.SolidBrush brush, float peakx, float peaky, float basex, float basey) =>
         g.FillPolygon(brush, new System.Drawing.PointF[] { new System.Drawing.PointF(peakx, peaky),
                                                            new System.Drawing.PointF(basex, peaky + basey),
                                                            new System.Drawing.PointF(basex + (peakx - basex) / 3, peaky),
                                                            new System.Drawing.PointF(basex, peaky - basey),
                                                            new System.Drawing.PointF(peakx, peaky)});

      void showTrack(Track track, bool visible) => mapControl.SpecMapShowTrack(track,
                                                                               visible,
                                                                               visible ?
                                                                                    track.GpxDataContainer.NextVisibleTrack(track) :
                                                                                    null);

      #region Implementierung der IDisposable-Schnittstelle

      /// <summary>
      /// true, wenn schon ein Dispose() erfolgte
      /// </summary>
      private bool _isdisposed = false;

      /// <summary>
      /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
      /// </summary>
      public void Dispose() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      /// <summary>
      /// überschreibt die Standard-Methode
      /// <para></para>
      /// </summary>
      /// <param name="notfromfinalizer">falls, wenn intern vom Finalizer aufgerufen</param>
      protected virtual void Dispose(bool notfromfinalizer) {
         if (!this._isdisposed) {            // bisher noch kein Dispose erfolgt
            if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben

            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)
            LocationIsShowing = false;
            StopGeoLocationService();
            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion
   }
}
