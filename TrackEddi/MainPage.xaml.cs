using FSofTUtils.Geography.DEM;
using FSofTUtils.Geography.Garmin;
using FSofTUtils.Geography.GeoCoding;
using FSofTUtils.Geometry;
using FSofTUtils.Xamarin.Control;
using FSofTUtils.Xamarin.DependencyTools;
using FSofTUtils.Xamarin.Page;
using FSofTUtils.Xamarin.Touch;
using GMap.NET.CoreExt.MapProviders;
using GMap.NET.Skia;
using SpecialMapCtrl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using TrackEddi.Common;
using TrackEddi.ConfigEdit;
using Xamarin.Forms;
using MapCtrl = SpecialMapCtrl.SpecialMapCtrl;

namespace TrackEddi {
   public partial class MainPage : ContentPage {

      const string TITLE = "TrackEddi, © by FSofT 27.7.2023";

      /// <summary>
      /// Standard-Datenverzeichnis der App
      /// </summary>
      const string DATAPATH = "TrackEddi";

      /// <summary>
      /// Name der Konfigurationsdatei (im <see cref="DATAPATH"/>)
      /// </summary>
      const string CONFIGFILE = "config.xml";

      /// <summary>
      /// Logdatei für Exceptions (im <see cref="DATAPATH"/>)
      /// </summary>
      const string ERRORLOGFILE = "error.txt";

      /// <summary>
      /// normale Logdatei (im <see cref="DATAPATH"/>)
      /// </summary>
      const string LOGFILE = "log.txt";

      /// <summary>
      /// (private) Datei für die Workbench-Daten
      /// </summary>
      const string WORKBENCHGPXFILE = "persistent.gpx";

      readonly Color MainMenuBackcolorStd = Color.LightGreen;

      readonly Color MainMenuBackcolorEdit = Color.FromRgb(255, 128, 128);

      static string oslogfile = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
                                                       "TrackEddiErrorLog.txt");

      /// <summary>
      /// Pfad des 1. (ext.) Volumes
      /// </summary>
      string FirstVolumePath;                   // z.B. "/storage/self/primary";    // Emulator -> /mnt/user/0/primary

      object androidActivity = null;

      StorageHelper storageHelper = null;

      /// <summary>
      /// persistente Programmdate
      /// </summary>
      AppData appData;

      string dataPath;

      string gpxworkbenchfile;

      GpxWorkbench gpxWorkbench;

      /// <summary>
      /// Konfigurationsdaten
      /// </summary>
      Config config;

      /// <summary>
      /// für die Ermittlung der Höhendaten
      /// </summary>
      DemData dem = null;

      /// <summary>
      /// Liste der registrierten Garmin-Symbole
      /// </summary>
      List<GarminSymbol> garminMarkerSymbols;

      /// <summary>
      /// normale Logdatei
      /// </summary>
      string logfile;

      /// <summary>
      /// wenn Tiles geladen werden 1, sonst 0 (threadsichere Abfrage!)
      /// </summary>
      long tileLoadIsRunning = 0;

      bool CenterTargetIsVisible {
         get => map != null ? map.SpecMapShowCenter : false;
         set {
            if (map != null)
               if (map.SpecMapShowCenter != value) {
                  map.SpecMapShowCenter = value;
                  map.SpecMapRefresh(false, false, false);
               }
         }
      }

      /// <summary>
      /// Kartenmitte in Xamarin-Koordinaten
      /// </summary>
      Point XamarinMapCenter {
         get => new Point(MapCtrl.SkiaX2XamarinX(map.Width) / 2,
                          MapCtrl.SkiaY2XamarinY(map.Height) / 2);
      }

      /// <summary>
      /// Kartenmitte in Client-Koordinaten
      /// </summary>
      internal System.Drawing.Point ClientMapCenter {
         get => new System.Drawing.Point(map.Width / 2,
                                         map.Height / 2);
      }

      GeoLocation geoLocation;

      bool firstOnAppearing = true;

      List<int[]> providxpaths = new List<int[]>();

      long _isOnInitMap = 1;

      /// <summary>
      /// App ist in <see cref="initMap(string, bool)"/> bzw. beim Start noch davor
      /// </summary>
      bool isOnInitMap {
         get => Interlocked.Read(ref _isOnInitMap) != 0;
         set => Interlocked.Exchange(ref _isOnInitMap, value ? 1 : 0);
      }

      #region Programmstatus

      /// <summary>
      /// der akt. Status wird noch einmal gesetzt um bestimmte Korrekturen zu erzwingen
      /// </summary>
      void RefreshProgramState() {
         ProgramState = ProgramState;
      }

      internal enum ProgState {
         Unknown,

         /// <summary>
         /// Im Programm können keine Daten verändert werden. 
         /// </summary>
         Viewer,

         /// <summary>
         /// Im Programm können Marker gesetzt/verschoben werden. 
         /// </summary>
         Edit_Marker,

         /// <summary>
         /// Im Programm können Tracks gezeichnet werden. 
         /// </summary>
         Edit_TrackDraw,

         //Edit_ConcatTracks,
         ///// <summary>
         ///// Im Programm können Tracks getrennt werden.
         ///// </summary>
         Edit_TrackSplit,

         ///// <summary>
         ///// Im Programm können Tracks verbunden werden.
         ///// </summary>
         Edit_TrackConcat,

      };

      ProgState _programState = ProgState.Unknown;
      /// <summary>
      /// akt. Programm-Status
      /// </summary>
      internal ProgState ProgramState {
         get => _programState;
         set {
            if (_programState != value) {
               map.SpecMapRefresh(false, false, false);
               _programState = value;
            }
         }
      }

      #endregion


      public MainPage() {
         InitializeComponent();
      }

      public MainPage(object androidactivity) : this() {
#if !DEBUG
         Ctrls4Debug.IsVisible = false;
#else
         Log("Start MainPage()");
#endif
         androidActivity = androidactivity;
#if DEBUG
         Log("End MainPage()");
#endif

         GpxWorkbench.LoadInfoEvent += (sender, e) => appendStartInfo("  " + e.Info);

         initTouchHandling();
      }

      public async void AppEvent(App.AppEvent ev) {
         switch (ev) {
            case App.AppEvent.OnStart:    // nach Konstruktor
               // danach erst OnAppearing()
               break;

            case App.AppEvent.OnResume:
               break;

            case App.AppEvent.OnSleep:    // nach OnDisappearing()
               try {
                  if (gpxWorkbench.Gpx != null) {
                     appData.GpxDataChanged = gpxWorkbench.DataChanged;
                     saveGpxWorkBench();
                  }
               } catch (Exception ex) {
                  await showExceptionMessage("Fehler bei " + nameof(AppEvent), ex);
               }
               break;
         }
      }

      protected override void OnDisappearing() {
         base.OnDisappearing();

         appData.LastZoom = map.SpecMapZoom;
         appData.LastLatitude = map.SpecMapCenterLat;
         appData.LastLongitude = map.SpecMapCenterLon;
         int idx = map.SpecMapActualMapIdx;
         if (idx >= 0)
            appData.LastMapname = map.SpecMapProviderDefinitions[idx].MapName;

         appData.VisibleStatusTrackList = gpxWorkbench.VisibleStatusTrackList;
         appData.VisibleStatusMarkerList = gpxWorkbench.VisibleStatusMarkerList;

      }

      protected override async void OnAppearing() {
         base.OnAppearing();
#if DEBUG
         Log("Start OnAppearing()");
#endif
         if (firstOnAppearing) {
            Title = TITLE + " (v" + Xamarin.Essentials.AppInfo.VersionString + ")";

            await initAll();

            // Buttonstatus rekonstruieren
            ButtonGeoLocationStart.IsVisible = !geoLocation.LocationIsShowing;
            ButtonGeoLocationStop.IsVisible = !ButtonGeoLocationStart.IsVisible;

            ButtonTrackingStart.IsVisible = !geoLocation.LocationTracking;
            ButtonTrackingStop.IsVisible = !ButtonTrackingStart.IsVisible;
         }

         config.LoadData();      // falls aktualisiert

#if DEBUG
         Log("End OnAppearing()");
#endif
         firstOnAppearing = false;
      }

      /// <summary>
      /// Eine URI wurd per AppLink an die App geliefert.
      /// </summary>
      /// <param name="uri"></param>
      /// <returns></returns>
      public async Task ReceiveAppLink(Uri uri) {
         //await UIHelper.ShowInfoMessage(this,
         //   uri.OriginalString +
         //   Environment.NewLine +
         //   Environment.NewLine +
         //   uri.AbsolutePath,

         //    "URI");

         string realfilename = "";
         string filename = HttpUtility.UrlDecode(uri.AbsolutePath);

         if (uri.Scheme == "file") {
            // z.B.
            //		uri.OriginalString = "file:///storage/emulated/0/Download/!notes.abc"
            //		uri.AbsolutePath = "/storage/emulated/0/Download/!notes.abc"
            // oder
            //		uri.AbsolutePath = "/storage/190E-1F12/Documents/!notes.abc"
            realfilename = filename;

         } else if (uri.Scheme == "content") {

            // Idee: vor dem letzten ':' steht das Volume, danach der abs. Pfad.

            // "Explorer" (Files- / Dateien-App)
            //		uri.OriginalString = "content://com.android.externalstorage.documents/document/primary%3ADownload%2F!notes.abc"
            //		uri.AbsolutePath = "/document/primary%3ADownload%2F!notes.abc"
            //
            //	z.B. Total Comander (wenn nicht als file-URI)
            //		uri.OriginalString = content://com.ghisler.files/tree/primary%3A/document/primary%3Astorage%2Femulated%2F0%2FDownload%2F!notes.abc"
            //		uri.AbsolutePath = "/tree/primary%3A/document/primary%3Astorage%2Femulated%2F0%2FDownload%2F!notes.abc"

            int p = filename.LastIndexOf(":");
            if (p >= 0) {
               realfilename = "/" + filename.Substring(p + 1);

               // Volume-Name ermitteln
               string volume = filename.Substring(0, p);
               p = volume.LastIndexOf("/");
               if (p >= 0)
                  volume = volume.Substring(p + 1);

               // Volume-Path zum Volume-Name suchen
               string volumepath = "";
               for (int i = 0; i < storageHelper.Volumes; i++) {
                  if (volume == storageHelper.VolumeNames[i]) {
                     volumepath = storageHelper.VolumePaths[i];
                     break;
                  }
               }

               if (!realfilename.StartsWith(volumepath))
                  realfilename = volumepath + realfilename;

            }
         }

         if (!string.IsNullOrEmpty(realfilename)) {
            for (int i = 0; i < 3; i++)         // wegen ev. Mehrfachencodierung ...
               if (File.Exists(realfilename))
                  break;
               else
                  realfilename = HttpUtility.UrlDecode(realfilename);

            if (await UIHelper.ShowYesNoQuestion_StdIsYes(this,
                                                          "Soll die Datei '" + realfilename + "' hinzugefügt werden?",
                                                          "Achtung"))
               await Loadfile2gpxworkbench(gpxWorkbench, realfilename, true);
         }
      }

      #region Initialisierung

      void appendStartInfo(string txt) {
         Device.BeginInvokeOnMainThread(() => {
            StartInfoArea.Text += System.Environment.NewLine + txt;
         });
      }

      /// <summary>
      /// Karte oder Infobereich anzeigen
      /// </summary>
      /// <param name="visible"></param>
      void showMap(bool visible = true) {
         Device.BeginInvokeOnMainThread(async () => {
            await pushEditModeButton(ProgState.Viewer);
            StartInfoArea.IsVisible = !visible;
            map.IsVisible = visible;
            MainMenu.IsVisible = visible;
            InvalidateMeasure();
         });
      }

      async Task initAll() {
         showMap(false);
         await Task.Run(async () => {
            appendStartInfo("Init ...");

            gpxworkbenchfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), WORKBENCHGPXFILE);

            appData = new Common.AppData();
            GMapControl.Map_ThreadPoolSize = 2;

            try {
               appendStartInfo("initDepTools() ...");
               // Bis auf Ausnahmen muss die gesamte Init-Prozedur fehlerfrei laufen. Sonst erfolgt ein Prog-Abbruch.
               if (await initDepTools(androidActivity)) {
                  dataPath = Path.Combine(FirstVolumePath, DATAPATH);
                  UIHelper.ExceptionLogfile = Path.Combine(dataPath, ERRORLOGFILE);

                  // Wenn im Android-ErrorLog etwas steht, wird es übernommen und das Android-ErrorLog wird gelöscht.
                  if (File.Exists(oslogfile) &&
                      new System.IO.FileInfo(oslogfile).Length > 0) {
                     File.AppendAllLines(UIHelper.ExceptionLogfile, File.ReadAllLines(oslogfile));
                     File.Delete(oslogfile);
                  }

                  logfile = Path.Combine(dataPath, LOGFILE);

                  if (initDataPath(dataPath)) {
                     string currentpath = Directory.GetCurrentDirectory();
                     Directory.SetCurrentDirectory(dataPath); // Directory.GetCurrentDirectory() liefert z.B.: /storage/emulated/0/TrackEddi

                     appendStartInfo(nameof(initConfig) + "(" + Path.Combine(dataPath, CONFIGFILE) + ") ...");
                     config = initConfig(Path.Combine(dataPath, CONFIGFILE));

                     initMap(dataPath, true);

                     appendStartInfo(nameof(initVisualTrackData) + "() ...");
                     initVisualTrackData(config);

                     try {
                        appendStartInfo(nameof(initGarminMarkerSymbols) + "() ...");
                        garminMarkerSymbols = initGarminMarkerSymbols(dataPath, config);
                        SpecialMapCtrl.VisualMarker.RegisterExternSymbols(garminMarkerSymbols);
                     } catch (Exception ex) {
                        await showExceptionMessage("Fehler beim Lesen der Garmin-Symbole", ex);
                     }

                     appendStartInfo(nameof(initWorkbench) + "() ...");
                     gpxWorkbench = initWorkbench(config, appData, gpxworkbenchfile, map, dem);
                  }
               } else
                  throw new Exception("Kein Zugriff auf das Dateisystem möglich.");
            } catch (Exception ex) {
               await showExceptionMessage("Fehler mit App-Abbruch", ex, true);  // Abbruch
               return;
            }

            //map.Map_MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            map.Map_MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;
            //map.Map_MouseWheelZoomType = GMap.NET.MouseWheelZoomType.ViewCenter;

            if (geoLocation == null)
               geoLocation = new GeoLocation(map) {
                  logfile = Path.Combine(FirstVolumePath, DATAPATH, "location.txt")
               };

            showMap();

         });

      }

      async Task<bool> initDepTools(object androidactivity) {
         storageHelper = DepToolsWrapper.GetStorageHelper(androidactivity);
         List<string> volumenpaths = storageHelper.VolumePaths;

         if (volumenpaths.Count < 1) {
            throw new Exception("Kein external Storage vorhanden.");
         } else {
            FirstVolumePath = volumenpaths[0];
         }

         bool[] results = new bool[volumenpaths.Count];
         for (int v = 0; v < storageHelper.Volumes; v++)
            if (!storageHelper.SetAndroidPersistentPermissions(v))
               results[v] = await storageHelper.Ask4AndroidPersistentPermissonAndWait(v);
            else
               results[v] = true;

         return results[0];
      }

      bool initDataPath(string datapath) {
         if (!storageHelper.DirectoryExists(datapath))
            if (!storageHelper.CreateDirectory(datapath))
               return false;
         return true;
      }

      Config initConfig(string configfile) {
         if (!storageHelper.FileExists(configfile))      // wenn noch keine Config-Datei ex. wird die Dummy-Datei in das Verzeichnis geschrieben
            using (StreamWriter sw = new StreamWriter(storageHelper.OpenFile(configfile, "rw"))) {
               sw.Write(Properties.Resources.configdummy);
            }
         return new Config(configfile, null);
      }

      /// <summary>
      /// entweder aus <see cref="initAll()"/> aufgerufen oder nach der Änderung der Konfiguration
      /// </summary>
      /// <param name="datapath"></param>
      /// <param name="firstcall"></param>
      void initMap(string datapath, bool firstcall) {
         isOnInitMap = true;
         showMap(false);
         //map.MapServiceEnd();

         appendStartInfo(nameof(initDEM) + "() ...");
         dem = initDEM(config);
         appendStartInfo("   DemPath " + config.DemPath);
         appendStartInfo("   DemCachesize " + config.DemCachesize);
         appendStartInfo("   DemCachePath " + config.DemCachePath);

         map.SpecMapCacheLocation = datapath;
         appendStartInfo(nameof(initMapProvider) + "() ...");
         initMapProvider(map, config);

         appendStartInfo(nameof(initAndStartMap) + "() ...");
         initAndStartMap(map, config, firstcall);

         appendStartInfo(nameof(setProviderZoomPositionExt) + "() ...");
         int idx = config.StartProvider;
         for (int i = 0; i < map.SpecMapProviderDefinitions.Count; i++) {
            if (map.SpecMapProviderDefinitions[i].MapName == appData.LastMapname) {
               idx = i;
               break;
            }
         }
         setProviderZoomPosition(idx,                 // entweder config.StartProvider oder entsprechend appData.LastMapname
                                 appData.LastZoom,
                                 appData.LastLongitude,
                                 appData.LastLatitude);
         isOnInitMap = false;
      }

      DemData initDEM(Config cfg) => ConfigHelper.ReadDEMDefinition(cfg);

      void initMapProvider(MapCtrl map, Config cfg) {
         List<MapProviderDefinition> provdefs = ConfigHelper.ReadProviderDefinitions(cfg, out providxpaths, out List<string> providernames);
         for (int i = 0; i < provdefs.Count; i++)
            appendStartInfo("   " + provdefs[i].MapName + " (" + provdefs[i].ProviderName + ")");
         map.SpecMapRegisterProviders(providernames, provdefs);
      }

      void initAndStartMap(MapCtrl map, Config cfg, bool firstcall) {
         if (firstcall) {
            map.OnMapTileLoadStart += map_OnTileLoadStart;
            map.OnMapTileLoadComplete += map_OnTileLoadComplete;
            map.OnMapPositionChanged += map_OnPositionChanged;
            map.OnMapZoomChanged += map_OnZoomChanged;
            map.OnMapExceptionThrown += async (Exception ex) =>
               await showExceptionMessage("Fehler bei " + nameof(map.OnMapExceptionThrown), ex);

            map.SpecMapMouseEvent += map_SpecMapMouseEvent;
            map.SpecMapMarkerEvent += map_SpecMapMarkerEvent;
            map.SpecMapTrackEvent += map_SpecMapTrackEvent;
            map.SpecMapDrawOnTop += map_SpecMapDrawOnTopEvent;
         }

         map.Map_ShowTileGridLines =
#if DBEUG
            true;                 // mit EmptyTileBorders gezeichnet
#else
            false;
#endif
         map.SpecMapShowCenter = false;                        // shows a little red cross on the map to show you exactly where the center is

         map.Map_EmptyMapBackgroundColor = Color.LightYellow;   // Tile (noch) ohne Daten
         map.Map_EmptyTileText = "keine Daten";            // Hinweistext für "Tile ohne Daten"
         map.Map_EmptyTileColor = Color.LightGray;        // Tile (endgültig) ohne Daten

         MapCtrl.SpecMapCacheIsActiv = !cfg.ServerOnly;
         MapCtrl.SpecMapSetProxy(cfg.WebProxyName,
                                 cfg.WebProxyPort,
                                 cfg.WebProxyUser,
                                 cfg.WebProxyPassword);

         CenterTargetIsVisible = false;
         map.Map_ClickTolerance4Tracks = 6;

         List<MapProviderDefinition> provdefs = map.SpecMapProviderDefinitions;
         int startprovider = config.StartProvider;       // EmptyProvider.Instance, GoogleMapProvider.Instance
         if (!appData.IsCreated) {     // wurde noch nie verwendet
            appData.LastLatitude = config.StartLatitude;
            appData.LastLongitude = config.StartLongitude;
            appData.LastZoom = config.StartZoom;
            appData.IsCreated = true;
         } else {
            string mapname = appData.LastMapname;
            for (int i = 0; i < provdefs.Count; i++) {
               if (provdefs[i].MapName == mapname) {
                  startprovider = i;
                  break;
               }
            }
         }
         if (startprovider >= provdefs.Count)
            startprovider = -1;

         if (firstcall)
            map.MapServiceStart(appData.LastLongitude,
                                appData.LastLatitude,
                                IOHelper.GetFullPath(config.CacheLocation),
                                (int)appData.LastZoom,
                                GMapControl.ScaleModes.Fractional);
         else
            map.SpecMapCacheLocation = config.CacheLocation;

         //map.Map_ShowTileGridLines = false; // auch bei DEBUG

         if (startprovider >= 0)
            setProviderZoomPosition(startprovider, appData.LastZoom, appData.LastLongitude, appData.LastLatitude);
      }

      void initVisualTrackData(Config cfg) => ConfigHelper.ReadVisualTrackDefinitions(cfg);

      GpxWorkbench initWorkbench(Config config, AppData appData, string gpxworkbenchfile, MapCtrl map, DemData dem) {
         GpxWorkbench wb = new GpxWorkbench(this,
                                            map,
                                            dem,
                                            gpxworkbenchfile,
                                            config.HelperLineColor,
                                            config.HelperLineWidth,
                                            config.StandardTrackColor,
                                            config.StandardTrackWidth,
                                            config.SymbolZoomfactor,
                                            appData.GpxDataChanged);
         if (map != null) {
            // Nach dem Einlesen sind alle Tracks "unsichtbar".
            List<bool> tmp = appData.VisibleStatusTrackList;
            for (int i = 0; i < tmp.Count && i < wb.TrackCount; i++)
               if (tmp[i])
                  ShowTrack(wb.GetTrack(i));

            tmp = appData.VisibleStatusMarkerList;
            for (int i = 0; i < tmp.Count && i < wb.MarkerCount; i++)
               if (tmp[i])
                  ShowMarker(wb.GetMarker(i));
         }

         //wb.Gpx.TracklistChanged += gpxWorkbench_TracklistChanged;
         //wb.Gpx.MarkerlistlistChanged += gpxWorkbench_MarkerlistlistChanged;
         //wb.Gpx.ChangeIsSet += gpxWorkbench_ChangeIsSet;
         wb.MarkerShouldInsertEvent += gpxWorkbench_MarkerShouldInsertEvent;
         wb.RefreshProgramStateEvent += gpxWorkbench_RefreshProgramStateEvent;
         //wb.TrackEditShowEvent += gpxWorkbench_TrackEditShowEvent;

         return wb;
      }

      List<GarminSymbol> initGarminMarkerSymbols(string datapath, Config cfg) => ConfigHelper.ReadGarminMarkerSymbols(cfg, datapath);

      #endregion

      #region Reaktion auf Map-Events

      // ACHTUNG
      // Es muss damit gerechnet werden, dass alle Map-Events NICHT im UI-Thread geliefert werden. Deshalb sicherheitshalber immer: 
      //    Device.BeginInvokeOnMainThread(()=> { ... });

      private void map_OnPositionChanged(object sender, GMapControl.PositionChangedEventArgs e) {
#if DEBUG
         Device.BeginInvokeOnMainThread(() => {
            labelPos.Text = string.Format("{0:F6}° {1:F6}°", e.Point.Lng, e.Point.Lat);
         });
#endif
      }

      private void map_OnZoomChanged(object sender, EventArgs e) {
#if DEBUG
         Device.BeginInvokeOnMainThread(() => {
            labelInfo.Text = string.Format("Zoom {0:F3}, linear {1:F1}", map.SpecMapZoom, map.Map_ZoomLinear);
         });
         dem.IsActiv = map.SpecMapZoom >= dem.MinimalZoom;
#endif
      }

      private void map_OnTileLoadComplete(object sender, GMapControl.TileLoadCompleteEventArgs e) {
         Device.BeginInvokeOnMainThread(() => {
            MainMenu.BackgroundColor = MainMenuBackcolorStd;
            showTilesInWork(0);
            Interlocked.Exchange(ref tileLoadIsRunning, 0);
         });
      }

      private void map_OnTileLoadStart(object sender, EventArgs e) {
         Device.BeginInvokeOnMainThread(() => {
            MainMenu.BackgroundColor = MainMenuBackcolorEdit;
            Interlocked.Exchange(ref tileLoadIsRunning, 1);
         });
      }

      /// <summary>
      /// nach der Karte, den Tracks usw. ev. noch etwas zusätzlich zeichnen
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void map_SpecMapDrawOnTopEvent(object sender, GMapControl.DrawExtendedEventArgs e) {
         //gpxWorkbench?.MapDrawOnTop(e);
         switch (ProgramState) {
            case ProgState.Edit_Marker:
               gpxWorkbench.TrackDrawDestinationLine(e.Graphics, ClientMapCenter);
               break;

            case ProgState.Edit_TrackDraw:
               gpxWorkbench.TrackDrawDestinationLine(e.Graphics, ClientMapCenter);
               break;

            case ProgState.Edit_TrackSplit:
               gpxWorkbench.TrackDrawSplitPoint(e.Graphics, ClientMapCenter);
               break;

            case ProgState.Edit_TrackConcat:
               if (gpxWorkbench.MarkedTrack != null)
                  gpxWorkbench.TrackDrawConcatLine(e.Graphics, gpxWorkbench.MarkedTrack);
               break;
         }

         geoLocation?.ShowPosition(e.Graphics, config.LocationSymbolsize);

         showTilesInWork(map.SpecMapWaitingTasks());
      }

      private void Compass_ReadingChanged(object sender, Xamarin.Essentials.CompassChangedEventArgs e) {
         //e.Reading.HeadingMagneticNorth
         throw new NotImplementedException();
      }

      private async void map_SpecMapTrackEvent(object sender, MapCtrl.TrackEventArgs e) {
         if (e.Eventtype == MapCtrl.MapMouseEventArgs.EventType.Click)                 // Click => Tapped
            await userTapAction(e.Button != System.Windows.Forms.MouseButtons.Left,    // long-Tap -> Right
                                null,
                                e.Track,
                                client2XamarinPoint(map.SpecMapLonLat2Client(e.Lon, e.Lat)));
      }

      private async void map_SpecMapMarkerEvent(object sender, MapCtrl.MarkerEventArgs e) {
         if (e.Eventtype == MapCtrl.MapMouseEventArgs.EventType.Click)                 // Click => Tapped
            await userTapAction(e.Button != System.Windows.Forms.MouseButtons.Left,    // long-Tap -> Right
                                e.Marker,
                                null,
                                client2XamarinPoint(map.SpecMapLonLat2Client(e.Lon, e.Lat)));
      }

      private void map_SpecMapMouseEvent(object sender, MapCtrl.MapMouseEventArgs e) {

         // fkt. NICHT, weil danach noch zusätzlich Map_SpecMapMarkerEvent() oder Map_SpecMapTrackEvent() ausgelöst werden kann

         //if (e.Eventtype == MapCtrl.MapMouseEventArgs.EventType.Click) // Click => Tapped
         //   await userTapAction(e.Button != System.Windows.Forms.MouseButtons.Left,
         //                       null,
         //                       null,
         //                       Client2XamarinPoint(e.Location));
      }

      #endregion

      #region Touch-Reaktionen

      private void mapTouchAction(object sender, TouchEffect.TouchActionEventArgs args) =>
        touchHandling.MapTouchAction(sender, args);

      TouchHandling touchHandling;

      double startzoomlinear = 0;

      void initTouchHandling() {
         touchHandling = new TouchHandling();
         //touchHandling.mainPage = this;

         touchHandling.TapDown += (sender, e) => {
            if (e.Fingers == 2)
               startzoomlinear = map.Map_ZoomLinear;
            //Log("touchHandling.TapDown: startzoomlinear=" + startzoomlinear);
         };

         touchHandling.StdTap += async (sender, e) => {
            startzoomlinear = 0;
            await mapTapped(e.Point, false);
         };

         touchHandling.LongTap += async (sender, e) => {
            startzoomlinear = 0;
            await mapTapped(e.Point, true);
         };

         //touchHandling.DoubleTap += async (sender, e) => await x(e.Point, true);

         touchHandling.Move += async (sender, e) => {
            startzoomlinear = 0;
            // Log("touchHandling.Move");
            if (!map.Map_IsDragging)
               await map.MapDragStart(e.From);
            if (e.Last)
               await map.MapDragEnd(e.To);
            else
               await map.MapDrag(e.To);
         };

         touchHandling.Zoom += (sender, e) => {
            if (e.Center.X >= 0 && e.Center.Y >= 0) {
               PointD latlon = xamarin2LatLon(e.Center);
               map.Map_Position = new GMap.NET.PointLatLng(latlon.Y, latlon.X);
            }
            //Log("touchHandling.Zoom: e.Zoom=" + e.Zoom);
            if (startzoomlinear > 0 &&
                map.Map_ZoomLinear != startzoomlinear * e.Zoom)
               map.Map_ZoomLinear = startzoomlinear * e.Zoom;
            //Log("touchHandling.Zoom: map.Map_ZoomLinear=" + map.Map_ZoomLinear);

         };
      }

      #endregion

      #region Toolbaritems

      async private void ToolbarItem_ChooseMap_Clicked(object sender, EventArgs e) => await mapChoosing();

      private void ToolbarItem_ReloadMap_Clicked(object sender, EventArgs e) {
         if (!isOnInitMap)
            map.SpecMapRefresh(true, false, false);
      }

      #region Load and Save

      private void ToolbarItem_GPXOpen_Clicked(object sender, EventArgs e) => toolbarItem_GPXOpen(false);

      private void ToolbarItem_GPXAppendClicked(object sender, EventArgs e) => toolbarItem_GPXOpen(true);

      async private void toolbarItem_GPXOpen(bool append) {
         if (!isOnInitMap)
            try {
               ChooseFilePage chooseFilePage = new ChooseFilePage() {
                  AndroidActivity = androidActivity,
                  Path = appData.LastLoadSavePath,
                  Filename = "",
                  OnlyExistingFile = true,   // ohne Eingabefeld für Namen
                  Match4Filenames = new System.Text.RegularExpressions.Regex(@"\.(gpx|kml|kmz)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase),
                  Title = "GPX-Datei auswählen",
               };
               chooseFilePage.ChooseFileReadyEvent += async (object sender, FSofTUtils.Xamarin.Control.ChooseFile.ChoosePathAndFileEventArgs e) => {
                  if (e.OK) {
                     appData.LastLoadSavePath = e.Path;
                     await Loadfile2gpxworkbench(gpxWorkbench, Path.Combine(e.Path, e.Filename), append);
                  }
               };

               await Navigation.PushAsync(chooseFilePage);
            } catch (Exception ex) {
               await showExceptionMessage("Fehler beim Lesen einer GPX-Datei", ex);
            }
      }

      internal async Task Loadfile2gpxworkbench(GpxWorkbench gpxWorkbench, string file, bool append) {
         int tracksold = gpxWorkbench.TrackCount;
         int markersold = gpxWorkbench.MarkerCount;

         try {
            IsBusy = true;

            await gpxWorkbench.Load(this, file, append, config.StandardTrackWidth, config.SymbolZoomfactor);

            for (int i = tracksold; i < gpxWorkbench.TrackCount; i++)
               ShowTrack(gpxWorkbench.GetTrack(i));
            for (int i = markersold; i < gpxWorkbench.MarkerCount; i++)
               ShowMarker(gpxWorkbench.GetMarker(i));

            gpxWorkbench.VisualRefresh();
         } catch (Exception ex) {
            throw new Exception(ex.Message);
         } finally {
            IsBusy = false;
         }
      }

      async private void ToolbarItem_SaveAsClicked(object sender, EventArgs e) => await _toolbarItem_SaveAsClicked(false);

      async private void ToolbarItem_SaveAsMultiClicked(object sender, EventArgs e) => await _toolbarItem_SaveAsClicked(true);

      async private Task _toolbarItem_SaveAsClicked(bool multi) {
         if (!isOnInitMap)
            try {
               ChooseFilePage chooseFilePage = new ChooseFilePage() {
                  AndroidActivity = androidActivity,
                  Path = string.IsNullOrEmpty(appData.LastFullSaveFilename) ?
                                                      appData.LastLoadSavePath :
                                                      Path.GetDirectoryName(appData.LastFullSaveFilename),
                  Filename = string.IsNullOrEmpty(appData.LastFullSaveFilename) ?
                                                      "" :
                                                      Path.GetFileName(appData.LastFullSaveFilename),
                  Match4Filenames = new Regex(@"\.(gpx|kml|kmz)$", RegexOptions.IgnoreCase),
                  OnlyExistingFile = false,   // mit Eingabefeld für Namen
                  Title = "Zieldatei auswählen",
               };
               if (multi)
                  chooseFilePage.ChooseFileReadyEvent += async (object sender, FSofTUtils.Xamarin.Control.ChooseFile.ChoosePathAndFileEventArgs e) => {
                     if (e.OK)
                        await saveGpxWorkbench(Path.Combine(e.Path, e.Filename), true);
                  };
               else
                  chooseFilePage.ChooseFileReadyEvent += async (object sender, FSofTUtils.Xamarin.Control.ChooseFile.ChoosePathAndFileEventArgs e) => {
                     if (e.OK)
                        await saveGpxWorkbench(Path.Combine(e.Path, e.Filename), false);
                  };

               await Navigation.PushAsync(chooseFilePage);
            } catch (Exception ex) {
               await showExceptionMessage("Fehler beim Speichern", ex);
            }
      }

      async Task saveGpxWorkbench(string filename, bool multi) {
         try {
            IsBusy = true;
            saveGpxWorkBench();
            if (await IOHelper.SaveGpx(this,
                                       gpxWorkbench.Gpx,
                                       filename,
                                       multi,
                                       Xamarin.Essentials.AppInfo.Name,
                                       true)) {
               if (multi)
                  appData.LastLoadSavePath = Path.GetDirectoryName(filename);
               else
                  appData.LastFullSaveFilename = filename;
            }
         } catch (Exception ex) {
            throw new Exception(ex.Message);
         } finally {
            IsBusy = false;
         }
      }

      #endregion

      async private void ToolbarItem_OsmSearchClicked(object sender, EventArgs e) => await osmSearch();

      async private void ToolbarItem_GoToClicked(object sender, EventArgs e) => await goTo();

      async private void ToolbarItem_GpxContent_Clicked(object sender, EventArgs e) => await showTracklistAndMarkerlist();

      bool isOnGPXSearch = false;

      async private void ToolbarItem_GPXSearch(object sender, EventArgs e) {
         if (!isOnGPXSearch) {
            isOnGPXSearch = true;

            PointD topleftLatLon = map.SpecMapClient2LonLat(0, 0);
            PointD bottomrightLatLon = map.SpecMapClient2LonLat(map.Width, map.Height);

            try {
               ChooseFilePage chooseFilePage = new ChooseFilePage() {
                  AndroidActivity = androidActivity,
                  Path = appData.LastGpxSearchPath,
                  OnlyExistingDirectory = true,
                  Title = "GPX-Verzeichnis auswählen",
               };
               chooseFilePage.ChooseFileReadyEvent += async (object s, ChooseFile.ChoosePathAndFileEventArgs ea) => {
                  if (ea.OK) {
                     if (appData.LastGpxSearchPath != ea.Path)
                        appData.LastGpxSearchPath = ea.Path;

                     showGpxSearchInfo(0, 0);
                     List<string> gpxfiles = new List<string>();
                     CheckRouteCrossing checkRouteCrossing = new CheckRouteCrossing();
                     await checkRouteCrossing.TestpathsAsync(new string[] {
                                                                  appData.LastGpxSearchPath
                                                             },
                                                             gpxfiles,
                                                             topleftLatLon.X,
                                                             bottomrightLatLon.X,
                                                             bottomrightLatLon.Y,
                                                             topleftLatLon.Y,
                                                             showGpxSearchInfo);
                     if (gpxfiles.Count > 0) {
                        try {
                           GPXSearchPage page = new GPXSearchPage(this, gpxWorkbench, gpxfiles);
                           await Navigation.PushAsync(page);
                        } catch (Exception ex) {
                           await showExceptionMessage("Fehler bei der Gpx-Dateianzeige", ex);
                        }
                     }
                     isOnGPXSearch = false;
                     showGpxSearchInfo(-1, 0);
                  }
               };

               await Navigation.PushAsync(chooseFilePage);
            } catch (Exception ex) {
               showGpxSearchInfo(-1, 0);
               await UIHelper.ShowExceptionMessage(this,
                                                   "Fehler beim Lesen des GPX-Verzeichnis",
                                                   ex,
                                                   null,
                                                   false);
            }
         }
      }

      async private void ToolbarItem_LastLocation(object sender, EventArgs e) {
         if (geoLocation != null && !isOnInitMap)
            await showGeoLocation();
      }

      async private void ToolbarItem_Config_Clicked(object sender, EventArgs e) {
         if (!isOnInitMap)
            try {
               ConfigPage page = new ConfigPage(map, config, providxpaths, androidActivity);
               page.Disappearing += async (s, ea) => {
                  if (!page.IsOnOpeningOrClosingSubPage) {  // Page beendet oder Taskwechsel steht bevor
                     if (page.ConfigChanged && page.SaveButtonPressed) { // die Konfigurationsdatei wurde geändert
                        IsBusy = true;
                        page.ConfigEdit.SaveData();
                        config.Load(config.XmlFilename);

                        showMap(false);
                        StartInfoArea.Text = string.Empty;
                        initMap(dataPath, false);
                        showMap(true);
                        IsBusy = false;

                        await UIHelper.ShowInfoMessage(this, @"Je nach veränderten Daten der Konfiguration muss ev. der Kartencache für eine oder alle Karten gelöscht werden!
                                   
Sonst werden die Änderungen ev. nicht wirksam.", "Achtung");
                     }
                  }
               };
               await Navigation.PushAsync(page);
            } catch (Exception ex) {
               await showExceptionMessage("Fehler bei " + nameof(ConfigPage), ex);
            }
      }

      async private void ToolbarItem_DeleteCache_Clicked(object sender, EventArgs e) => await deleteCache(map.SpecMapActualMapIdx);

      async private void ToolbarItem_DeleteCacheAll_Clicked(object sender, EventArgs e) => await deleteCache(-1);

      async private void ToolbarItem_Help_Clicked(object sender, EventArgs e) {
         if (!isOnInitMap)
            try {
               await Navigation.PushAsync(new HelpPage());
            } catch (Exception ex) {
               await showExceptionMessage("Fehler bei " + nameof(HelpPage), ex);
            }
      }

      #endregion

      #region MainMenu

      private void ZoomIn_Clicked(object sender, EventArgs e) {
         if (map.SpecMapZoom % 1.0 == 0)
            map.SpecMapZoom++;
         else
            map.SpecMapZoom = Math.Ceiling(map.SpecMapZoom);     // auf "ganzzahlig einrasten"
      }

      private void ZoomOut_Clicked(object sender, EventArgs e) {
         if (map.SpecMapZoom % 1.0 == 0)
            map.SpecMapZoom--;
         else
            map.SpecMapZoom = Math.Floor(map.SpecMapZoom);       // auf "ganzzahlig einrasten"
      }

      private void ChooseMap_Clicked(object sender, EventArgs e) {
         ToolbarItem_ChooseMap_Clicked(sender, e);
      }

      private void TrackMarkerList_Clicked(object sender, EventArgs e) {
         ToolbarItem_GpxContent_Clicked(sender, e);
      }

      private void GeoLocationStart_Clicked(object sender, EventArgs e) {
         if (geoLocation != null &&
             !geoLocation.LocationIsShowing &&
             geoLocation.StartGeoLocationService()) {
            ButtonGeoLocationStart.IsVisible = false;
            ButtonGeoLocationStop.IsVisible = !ButtonGeoLocationStart.IsVisible;
            geoLocation.LocationIsShowing = true;
            geoLocation.LocationSelfCentering = true;
         }
      }

      private void GeoLocationStop_Clicked(object sender, EventArgs e) {
         if (geoLocation != null &&
             geoLocation.LocationIsShowing) {
            ButtonGeoLocationStart.IsVisible = true;
            ButtonGeoLocationStop.IsVisible = !ButtonGeoLocationStart.IsVisible;
            geoLocation.LocationIsShowing = false;
            geoLocation.LocationSelfCentering = false;
            if (!geoLocation.LocationTracking)
               geoLocation.StopGeoLocationService();
         }
      }

      private void TrackingStart_Clicked(object sender, EventArgs e) {
         if (geoLocation != null &&
             !geoLocation.LocationTracking &&
             geoLocation.StartGeoLocationService()) {
            ButtonTrackingStart.IsVisible = false;
            ButtonTrackingStop.IsVisible = !ButtonTrackingStart.IsVisible;
            geoLocation.StartTracking(gpxWorkbench.Gpx, config.TrackingMinimalPointdistance, config.TrackingMinimalHeightdistance);
         }
      }

      private void TrackingStop_Clicked(object sender, EventArgs e) {
         if (geoLocation != null &&
             geoLocation.LocationTracking) {
            ButtonTrackingStart.IsVisible = true;
            ButtonTrackingStop.IsVisible = !ButtonTrackingStart.IsVisible;
            geoLocation.EndTracking();
            if (!geoLocation.LocationIsShowing)
               geoLocation.StopGeoLocationService();
         }
      }

      #region "Radiobutton" Editiermodus

      async void EditModeNothing_Clicked(object sender, EventArgs e) => await pushEditModeButton(ProgState.Viewer);

      async void EditModeMarkerSet_Clicked(object sender, EventArgs e) => await pushEditModeButton(ProgState.Edit_Marker);

      async void EditModeTrackDraw_Clicked(object sender, EventArgs e) => await pushEditModeButton(ProgState.Edit_TrackDraw);

      async void EditModeTrackSplit_Clicked(object sender, EventArgs e) => await pushEditModeButton(ProgState.Edit_TrackSplit);

      async void EditModeTrackConcat_Clicked(object sender, EventArgs e) => await pushEditModeButton(ProgState.Edit_TrackConcat);

      async Task pushEditModeButton(ProgState newProgState) {
         if (ProgramState != newProgState) {
            //if (!firstOnAppearing ||                       // ohne Rückfrage
            //    await endEditCancelAsync()) {

            bool stopcancel = false;
            if (!firstOnAppearing)
               stopcancel = await endEditCancelAsync();

            if (!stopcancel) {
               showEditInfoText();
               Frame frame = null;
               switch (ProgramState) {
                  case ProgState.Viewer: frame = EditModeNothing; break;
                  case ProgState.Edit_Marker: frame = EditModeMarkerSet; break;
                  case ProgState.Edit_TrackDraw: frame = EditModeTrackDraw; break;
                  case ProgState.Edit_TrackSplit: frame = EditModeTrackSplit; break;
                  case ProgState.Edit_TrackConcat: frame = EditModeTrackConcat; break;
               }
               if (frame != null)
                  frame.BackgroundColor = Color.LightGray;

               frame = null;
               switch (newProgState) {
                  case ProgState.Viewer: frame = EditModeNothing; EditModeText.Text = "Standardansicht"; break;
                  case ProgState.Edit_Marker: frame = EditModeMarkerSet; EditModeText.Text = "Marker setzen/verschieben"; break;
                  case ProgState.Edit_TrackDraw: frame = EditModeTrackDraw; EditModeText.Text = "Track zeichnen"; break;
                  case ProgState.Edit_TrackSplit: frame = EditModeTrackSplit; EditModeText.Text = "Track trennen"; break;
                  case ProgState.Edit_TrackConcat: frame = EditModeTrackConcat; EditModeText.Text = "Tracks verbinden"; break;
               }
               if (frame != null)
                  frame.BackgroundColor = Color.DarkGray;

               ProgramState = newProgState;

               switch (ProgramState) {
                  case ProgState.Edit_TrackDraw:
                     gpxWorkbench?.StartTrackDraw(gpxWorkbench.MarkedTrack);
                     break;

                  case ProgState.Edit_TrackSplit:
                  case ProgState.Edit_TrackConcat:
                     if (gpxWorkbench?.MarkedTrack != null)
                        gpxWorkbench?.StartTrackDraw(gpxWorkbench.MarkedTrack);
                     break;
               }

               fitMainMenu(ProgramState);
            }
         }
      }

      /// <summary>
      /// akt. Editieraktion beenden mit Abbruch
      /// </summary>
      /// <param name="cancel"></param>
      /// <returns>false wenn kein Edit läuft oder wenn ein Edit läuft und NICHT beendet werden soll, sonst true</returns>
      async Task<bool> endEditCancelAsync() {
         if (gpxWorkbench != null && gpxWorkbench.InWork)
            return await gpxWorkbench.Cancel();   // Edit läuft und soll NICHT beendet werden.
         return false;
      }

      /// <summary>
      /// akt. Editieraktion beenden mit OK
      /// </summary>
      /// <returns></returns>
      async Task endEditOKAsync() {
         switch (ProgramState) {
            case ProgState.Viewer: break;
            case ProgState.Edit_Marker: gpxWorkbench?.EndMarker(ClientMapCenter); break;
            case ProgState.Edit_TrackDraw: gpxWorkbench?.TrackEndDraw(); break;
            case ProgState.Edit_TrackSplit: gpxWorkbench?.EndTrackSplit(); break;
            case ProgState.Edit_TrackConcat: gpxWorkbench?.EndTrackConcat(); break;
         }
         await pushEditModeButton(ProgState.Viewer);
      }

      #endregion

      void saveGpxWorkBench() {
         if (gpxWorkbench != null && gpxWorkbench.DataChanged)
            gpxWorkbench.Save();
      }

      void fitMainMenu(ProgState progState) {
         if (progState == ProgState.Viewer) {
            EditButtonsGroup.BackgroundColor = Color.FromRgba(0, 0, 0, 0);
            ExtEditButtons.IsVisible = false;
         } else {
            EditButtonsGroup.BackgroundColor = Color.FromRgba(1, .8, .8, 1);
            ExtEditButtons.IsVisible = true;
         }

         switch (progState) {
            case ProgState.Viewer:
            default:
               CenterTargetIsVisible = false;
               ButtonEditTarget.IsVisible = false;
               ButtonEditMinus.IsVisible = false;
               ButtonEditEnd.IsVisible = false;
               ButtonEditCancel.IsVisible = false;
               break;

            case ProgState.Edit_Marker:
               CenterTargetIsVisible = true;
               ButtonEditTarget.IsVisible = true;
               ButtonEditMinus.IsVisible = false;
               ButtonEditEnd.IsVisible = false;
               ButtonEditCancel.IsVisible = true;
               break;

            case ProgState.Edit_TrackDraw:
               ExtEditButtons.IsVisible = true;
               CenterTargetIsVisible = true;
               ButtonEditTarget.IsVisible = true;
               ButtonEditMinus.IsVisible = true;
               ButtonEditEnd.IsVisible = true;
               ButtonEditCancel.IsVisible = true;
               break;

            case ProgState.Edit_TrackSplit:
               ExtEditButtons.IsVisible = true;
               CenterTargetIsVisible = true;
               ButtonEditTarget.IsVisible = true;
               ButtonEditMinus.IsVisible = false;
               ButtonEditEnd.IsVisible = true;
               ButtonEditCancel.IsVisible = true;
               break;

            case ProgState.Edit_TrackConcat:
               ExtEditButtons.IsVisible = true;
               CenterTargetIsVisible = true;
               ButtonEditTarget.IsVisible = true;
               ButtonEditMinus.IsVisible = false;
               ButtonEditEnd.IsVisible = true;
               ButtonEditCancel.IsVisible = true;
               break;
         }
      }

      /// <summary>
      /// "Ziel"-Button für Track: unterschiedliche Funktion je nach Editierfunktion
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private async void ButtonEditTarget_Clicked(object sender, EventArgs e) => await mapTapped(XamarinMapCenter, false);


      /// <summary>
      /// letzten Trackpunkt entfernen
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void ButtonEditMinus_Clicked(object sender, EventArgs e) {
         if (ProgramState == ProgState.Edit_TrackDraw) {
            gpxWorkbench?.TrackRemovePoint();
            showEditInfoText(gpxWorkbench?.TrackInEdit);
         }
      }

      /// <summary>
      /// Ende einer Editierfunktion
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private async void ButtonEditEnd_Clicked(object sender, EventArgs e) => await endEditOKAsync();

      /// <summary>
      /// Abbruch einer Editierfunktion
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private async void ButtonEditCancel_Clicked(object sender, EventArgs e) => await pushEditModeButton(ProgState.Viewer);

      #endregion

      #region Events der GpxWorkbench

      //private void gpxWorkbench_ChangeIsSet(object sender, EventArgs e) { }

      //private void gpxWorkbench_MarkerlistlistChanged(object sender, GpxAllExt.MarkerlistChangedEventArgs e) { }

      //private void gpxWorkbench_TracklistChanged(object sender, GpxAllExt.TracklistChangedEventArgs e) { }

      //private void gpxWorkbench_TrackEditShowEvent(object sender, EditTrackHelper.TrackEventArgs e) { }

      /// <summary>
      /// ev. Aufname eines neuen <see cref="Marker"/>; wird über <see cref="SetNewMarker"/> ausgelöst
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      async void gpxWorkbench_MarkerShouldInsertEvent(object sender, EditHelper.MarkerEventArgs e) {
         try {
            if (string.IsNullOrEmpty(e.Marker.Symbolname))
               e.Marker.Symbolname = "Flag, Green";            // <--> passend zum VisualMarker für editierbare Marker

            string[] names = await gpxWorkbench.GetNamesForGeoPointAsync(e.Marker.Longitude, e.Marker.Latitude);
            EditMarkerPage page = new EditMarkerPage(e.Marker, garminMarkerSymbols, names);
            page.EndWithOk += (object sender2, EventArgs e2) => {
               if (string.IsNullOrEmpty(e.Marker.Waypoint.Name))
                  e.Marker.Waypoint.Name = string.Format("M Lon={0:F6}°/Lat={1:F6}°", e.Marker.Waypoint.Lon, e.Marker.Waypoint.Lat);    // autom. Name
               Marker marker = gpxWorkbench.MarkerInsertCopy(e.Marker);
               marker.Symbolzoom = config.SymbolZoomfactor;
               ShowMarker(marker);
            };
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await showExceptionMessage("Fehler bei " + nameof(SetNewMarker), ex);
         }
      }

      private void gpxWorkbench_RefreshProgramStateEvent(object sender, EventArgs e) => RefreshProgramState();

      #endregion

      /// <summary>
      /// setzt die gewünschte Karte, den Zoom und die Position oder liefert eine Exception
      /// </summary>
      /// <param name="mapidx">Index für die <see cref="SpecialMapCtrl.SpecialMapCtrl.SpecMapProviderDefinitions"/></param>
      /// <param name="zoom"></param>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      void setProviderZoomPosition(int mapidx, double zoom, double lon, double lat) {
         // Zoom und Pos. einstellen
         if (zoom != map.SpecMapZoom ||
             lon != map.SpecMapCenterLon ||
             lat != map.SpecMapCenterLat)
            map.SpecMapSetLocationAndZoom(zoom, lon, lat);

         if (mapidx != map.SpecMapActualMapIdx) {                 // andere Karte anzeigen
            if (0 <= mapidx &&
                mapidx < providxpaths.Count &&
                mapidx < map.SpecMapProviderDefinitions.Count) {  // erlaubter Index
               bool hillshade = false;
               byte hillshadealpha = 0;
               bool hillshadeisactiv = map.SpecMapZoom >= dem.MinimalZoom;

               if (0 <= mapidx) {
                  map.SpecMapClearWaitingTaskList();

                  MapProviderDefinition mapProviderDefinition = map.SpecMapProviderDefinitions[mapidx];
                  if (mapProviderDefinition.ProviderName == "Garmin") {
                     map.SpecMapCancelTileBuilds();
                     hillshadealpha = (mapProviderDefinition as GarminProvider.GarminMapDefinitionData).HillShadingAlpha;
                     hillshade = (mapProviderDefinition as GarminProvider.GarminMapDefinitionData).HillShading;
                  } else if (mapProviderDefinition.ProviderName == "GarminKMZ") {
                     map.SpecMapCancelTileBuilds();
                     hillshadealpha = (mapProviderDefinition as GarminKmzProvider.KmzMapDefinition).HillShadingAlpha;
                     hillshade = (mapProviderDefinition as GarminKmzProvider.KmzMapDefinition).HillShading;
                  }
               }
               dem.WithHillshade = hillshade;
               dem.IsActiv = hillshadeisactiv;
               map.SpecMapSetActivProvider(mapidx, hillshadealpha, dem);
            } else
               throw new Exception("Der Kartenindex " + mapidx + " ist nicht vorhanden. (max. " + (map.SpecMapProviderDefinitions.Count - 1) + ")");
         }
      }

      /// <summary>
      /// setzt die gewünschte Karte, den Zoom und die Position (async nur wegen Anzeige bei einer Exception)
      /// </summary>
      /// <param name="mapidx">Index für die <see cref="SpecialMapCtrl.SpecialMapCtrl.SpecMapProviderDefinitions"/></param>
      /// <param name="zoom"></param>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      async Task setProviderZoomPositionExt(int mapidx, double zoom, double lon, double lat) {
         try {
            setProviderZoomPosition(mapidx, zoom, lon, lat);
         } catch (Exception ex) {
            await showExceptionMessage("Fehler bei " + nameof(setProviderZoomPositionExt), ex);
         }
      }

      /// <summary>
      /// ein langer oder kurzer Tap auf die Karte ist erfolgt
      /// </summary>
      /// <param name="point"></param>
      /// <param name="longtap"></param>
      /// <returns></returns>
      async Task mapTapped(Point point, bool longtap) {
         PointD platlon = xamarin2LatLon(point);
         labelPos.Text = string.Format("Tap {0:F6}° {1:F6}°", platlon.X, platlon.Y);

         // weiterleiten auf Marker- und Track-Events:
         //    Map_SpecMapMouseEvent(), Map_SpecMapMarkerEvent() und Map_SpecMapTrackEvent()
         // und von dort auf userTapAction()
         // Da in Map_SpecMapMouseEvent() aber noch nicht klar ist, ob danach noch Map_SpecMapMarkerEvent() und/oder Map_SpecMapTrackEvent() folgt
         // muss diese Auswertung von Map_SpecMapMouseEvent() entfallen und hier erfolgen!

         map.SpecMapDoMouseClick((int)GMapControl.XamarinX2SkiaX(point.X),
                                 (int)GMapControl.XamarinY2SkiaY(point.Y),
                                 true,
                                 !longtap ?
                                    System.Windows.Forms.MouseButtons.Left :
                                    System.Windows.Forms.MouseButtons.Right,
                                 out List<Marker> markerlst,
                                 out List<Track> tracklst);

         if (markerlst.Count == 0 &&
             tracklst.Count == 0)
            await userTapAction(longtap, null, null, point);   // Tap in den "freien Raum"
      }

      /// <summary>
      /// Zusammenführung aller Tap-Aktionen (Tap auf <see cref="Marker"/>, <see cref="Track"/> oder in den "freien Raum"
      /// <para><see cref="ProgState.Viewer"/>; kurz - auf Marker: (Kurz)Anzeige der Markereigenschaften</para>
      /// <para><see cref="ProgState.Viewer"/>; lang - auf Marker: Bearbeiten der Markereigenschaften</para>
      /// <para><see cref="ProgState.Viewer"/>; kurz - auf Track: (Kurz)Anzeige der Trackeigenschaften</para>
      /// <para><see cref="ProgState.Viewer"/>; lang - auf Track: Bearbeiten der Trackeigenschaften</para>
      /// <para><see cref="ProgState.Viewer"/>; lang: Info für diesen geografischen Punkt</para>
      /// <para><see cref="ProgState.Edit_Marker"/>; kurz - auf Marker: Start Markerverschiebung</para>
      /// <para><see cref="ProgState.Edit_Marker"/>; lang - auf Marker: (ev.) Marker löschen</para>
      /// <para><see cref="ProgState.Edit_Marker"/>; kurz: neuen Marker setzen</para>
      /// <para><see cref="ProgState.Edit_Marker"/>; lang: Info für diesen geografischen Punkt</para> 
      /// <para><see cref="ProgState.Edit_TrackDraw"/>; kurz - auf Track: Track für Veränderung aktivieren/markieren</para>
      /// <para><see cref="ProgState.Edit_TrackDraw"/>; lang - auf Track: (ev.) diesen Track löschen</para>
      /// <para><see cref="ProgState.Edit_TrackDraw"/>; kurz: Start eines neuen Tracks oder an aktivierten/markierten Track Punkt anhängen</para>
      /// <para><see cref="ProgState.Edit_TrackDraw"/>; lang: Info für diesen geografischen Punkt</para> 
      /// <para><see cref="ProgState.Edit_TrackConcat"/>; kurz - auf 1. Track: Start der Aktion für diesen Track</para>
      /// <para><see cref="ProgState.Edit_TrackConcat"/>; kurz - auf 2. Track: 2. Track an 1. Track anhängen</para>
      /// <para><see cref="ProgState.Edit_TrackSplit"/>; kurz - auf Track: Start der Aktion für diesen Track</para>
      /// </summary>
      /// <param name="longtap">Standard- oder langer Tap</param>
      /// <param name="marker">Tap auf <see cref="Marker"/></param>
      /// <param name="track">Tap auf <see cref="Track"/></param>
      /// <param name="point">Tap weder auf <see cref="Marker"/> oder <see cref="Track"/></param>
      async Task userTapAction(bool longtap, Marker marker, Track track, Point point) {
         // mögliche Varianten:  [5xProgState] *
         //                      [2xTap (long/short)] *
         //                      [3xObject(Marker, Track, -)] = 30

         switch (ProgramState) {
            case ProgState.Viewer:
               showEditInfoText();
               if (marker != null) {               // marker tapped
                  if (!longtap)
                     await showShortMarkerProps(marker);
                  else
                     await editMarkerProps(marker);
               } else if (track != null) {         // track tapped
                  if (!longtap)
                     await showShortTrackProps(track);
                  else
                     await editTrackProps(track);
               } else {                            // marker nor track tapped
                  if (!longtap) {
                     ;
                  } else
                     await info4LonLatAsync(point);
               }
               break;

            case ProgState.Edit_Marker:
               if (gpxWorkbench.InWork) { // 1 Marker wurde schon (für das Verschieben) ausgewählt
                  if (!longtap) {                                 // short tap
                     gpxWorkbench?.EndMarker(xamarin2ClientPoint(point));     // falls noch kein Marker "InWork" neuer Marker, sonst neue Position für den Maker "InWork"
                     showEditInfoText();
                  }
               } else {
                  if (marker != null) {                           // marker tapped
                     showEditInfoText(marker.Text);
                     if (!longtap)
                        await startMoveMarker(marker);               // start move this marker
                     else
                        await removeMarker(marker);                  // remove this marker
                     showEditInfoText();
                  } else {
                     if (!longtap)
                        await SetNewMarker(xamarin2ClientPoint(point)); // set new marker
                     else
                        await info4LonLatAsync(point);
                  }
               }
               break;

            case ProgState.Edit_TrackDraw:
               if (!longtap) {                                       // short tap
                  if (gpxWorkbench != null) {
                     if (!gpxWorkbench.InWork) {
                        if (track != null)
                           gpxWorkbench.StartTrackDraw(track);          // track extend
                        else {
                           gpxWorkbench.StartTrackDraw();               // start new track
                           gpxWorkbench.TrackAddPoint(xamarin2ClientPoint(point));  // start with this point
                        }
                     } else
                        gpxWorkbench.TrackAddPoint(xamarin2ClientPoint(point));
                     showEditInfoText(gpxWorkbench.TrackInEdit);
                  }
               } else {                                              // long tap
                  if (track != null &&
                      (!gpxWorkbench.InWork ||
                       gpxWorkbench?.TrackInEdit != track))             // another track tapped long
                     await removeTrack(track);                       // remove this track
                  else
                     await info4LonLatAsync(point);
               }
               break;

            case ProgState.Edit_TrackSplit:
               if (!longtap) {
                  if (track != null) {         // track tapped
                     if (gpxWorkbench != null &&
                         !gpxWorkbench.InWork) {
                        gpxWorkbench.StartTrackDraw(track);
                        showEditInfoText(track);
                     }
                  }
               } else
                  await info4LonLatAsync(point);
               break;

            case ProgState.Edit_TrackConcat:
               if (!longtap) {
                  if (track != null) {         // track tapped
                     if (gpxWorkbench != null) {
                        if (!gpxWorkbench.InWork) {
                           gpxWorkbench.StartTrackDraw(track);
                           showEditInfoText(track, "1. Track: ");
                        } else {
                           gpxWorkbench.MarkedTrack = track;
                           showEditInfoText(track, "2. Track: ");
                           map.SpecMapRefresh(false, false, false);
                        }
                     }
                  }
               } else
                  await info4LonLatAsync(point);
               break;
         }
      }

      /// <summary>
      /// zeigt einen Infotext beim Editieren an
      /// </summary>
      /// <param name="txt"></param>
      void showEditInfoText(string txt = null) {
         if (string.IsNullOrEmpty(txt))
            EditInfoText.IsVisible = false;
         else {
            EditInfoText.Text = txt;
            EditInfoText.IsVisible = true;
            MainMenu.ScrollToAsync(EditInfoText, ScrollToPosition.End, true);
         }
      }

      /// <summary>
      /// zeigt einen Infotext beim Editieren eines Tracks an
      /// </summary>
      /// <param name="track"></param>
      /// <param name="pretxt"></param>
      void showEditInfoText(Track track, string pretxt = null) {
         if (track != null) {
            double length = track.Length();
            showEditInfoText((pretxt != null ? pretxt : "") +
                             track.VisualName + " (" +
                             (length < 1000 ? string.Format("{0:F0}m", length) : string.Format("{0:F1}km", length / 1000)) + ")");
         }
      }

      /// <summary>
      /// <see cref="Track"/> anzeigen oder verbergen
      /// </summary>
      /// <param name="track"></param>
      /// <param name="visible"></param>
      internal void ShowTrack(Track track, bool visible = true) {
         if (track.IsVisible != visible)
            map.SpecMapShowTrack(track,
                                 visible,
                                 visible ? track.GpxDataContainer?.NextVisibleTrack(track) : null);
      }

      /// <summary>
      /// <see cref="Marker"/> anzeigen oder verbergen
      /// </summary>
      /// <param name="marker"></param>
      /// <param name="visible"></param>
      /// <returns></returns>
      internal int ShowMarker(Marker marker, bool visible = true) {
         if (marker.IsVisible == visible)
            return -1;
         map.SpecMapShowMarker(marker,
                               visible,
                               visible ? marker.GpxDataContainer?.NextVisibleMarker(marker) : null);

         //editableTracklistControl1.ShowMarker(marker, visible);

         return marker.GpxDataContainer.MarkerIndex(marker);
      }

      #region Subpages ("Dialoge"), Infos und Aktionen

      /// <summary>
      /// Auswahl der Karte
      /// </summary>
      /// <returns></returns>
      async Task mapChoosing() {
         if (!isOnInitMap)
            try {
               MapChoosingPage page = new MapChoosingPage() {
                  MapControl = map,
                  ProvIdxPaths = providxpaths,
                  Config = config,
                  AppData = appData,
               };
               page.MapChoosingEvent += mapChoosingPage_MapChoosingEvent;
               await Navigation.PushAsync(page);
            } catch (Exception ex) {
               await showExceptionMessage("Fehler bei " + nameof(mapChoosing), ex);
            }
      }

      async void mapChoosingPage_MapChoosingEvent(object sender, MapChoosingPage.MapChoosingEventArgs e) {
         if (e.Idx >= 0) {
            appData.LastMapname = map.SpecMapProviderDefinitions[e.Idx].MapName;
            await setProviderZoomPositionExt(e.Idx, appData.LastZoom, appData.LastLongitude, appData.LastLatitude);
         }
      }

      /// <summary>
      /// (bearbeitbare) Track- und Markerliste anzeigen
      /// </summary>
      /// <returns></returns>
      async Task showTracklistAndMarkerlist() {
         if (!isOnInitMap)
            try {
               GpxContentPage page = new GpxContentPage(map,
                                                        gpxWorkbench,
                                                        garminMarkerSymbols,
                                                        appData);
               await Navigation.PushAsync(page);
            } catch (Exception ex) {
               await showExceptionMessage("Fehler bei " + nameof(showTracklistAndMarkerlist), ex);
            }
      }

      /// <summary>
      /// geografische Suche per OSM (nur online)
      /// </summary>
      /// <returns></returns>
      async Task osmSearch() {
         if (!isOnInitMap)
            try {
               OsmSearchPage page = new OsmSearchPage(map,
                                                      appData);
               await Navigation.PushAsync(page);
            } catch (Exception ex) {
               await showExceptionMessage("Fehler bei OSM-Suche", ex);
            }
      }

      async Task showGarminInfo4LonLat(IList<GarminImageCreator.SearchObject> infos, string pretext) {
         try {
            ShowGarminInfo4LonLat page = new ShowGarminInfo4LonLat(infos, pretext);
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await showExceptionMessage("Fehler bei GarminInfo-Anzeige", ex);
         }
      }

      /// <summary>
      /// zu einem "benannten" Ort oder zu geografischen Koordinaten gehen
      /// </summary>
      /// <returns></returns>
      async Task goTo() {
         if (!isOnInitMap)
            try {
               GoToPage page = new GoToPage(map,
                                            appData);
               await Navigation.PushAsync(page);
            } catch (Exception ex) {
               await showExceptionMessage("Fehler bei " + nameof(goTo), ex);
            }
      }

      /// <summary>
      /// zeigt die (editierbare) Seite der Marker-Eigenschaften an
      /// </summary>
      /// <param name="marker"></param>
      /// <returns></returns>
      async Task editMarkerProps(Marker marker) {
         try {
            EditMarkerPage page = new EditMarkerPage(marker, garminMarkerSymbols);
            page.EndWithOk += (object sender, EventArgs e) => gpxWorkbench.RefreshMarkerWaypoint(page.Marker);
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await showExceptionMessage("Fehler bei " + nameof(editMarkerProps), ex);
         }
      }

      /// <summary>
      /// einfache Anzeige der Marker-Eigenschaften
      /// </summary>
      /// <param name="marker"></param>
      /// <returns></returns>
      async Task showShortMarkerProps(Marker marker) {
         await UIHelper.ShowInfoMessage(this, marker.Waypoint.Name, "Marker");
      }

      /// <summary>
      /// zeigt die (editierbare) Seite der Track-Eigenschaften an
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      async Task editTrackProps(Track track) {
         try {
            Track trackcopy = Track.CreateCopy(track);
            EditTrackPage page = new EditTrackPage(trackcopy);
            page.EndWithOk += (object sender2, EventArgs e2) => gpxWorkbench.RefreshTrackProps(track, trackcopy);
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await showExceptionMessage("Fehler bei " + nameof(editTrackProps), ex);
         }
      }

      /// <summary>
      /// einfache Anzeige der Track-Eigenschaften
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      async Task showShortTrackProps(Track track) {
         await UIHelper.ShowInfoMessage(this,
                                        track.Trackname + System.Environment.NewLine +
                                             (track.StatLength < 1000 ? string.Format("{0:F0}m", track.StatLength) : string.Format("{0:F1}km", track.StatLength / 1000)),
                                        "Track");
      }

      async Task showGeoLocation() {
         try {
            GeoLocationPage page = new GeoLocationPage(geoLocation);
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await showExceptionMessage("Fehler bei " + nameof(showGeoLocation), ex);
         }
      }

      /// <summary>
      /// ev. einen Marker an dieser Bildschirmpos. setzen
      /// </summary>
      /// <param name="xamarinpoint"></param>
      /// <returns></returns>
      internal async Task SetNewMarker(System.Drawing.Point clientpoint) {
         if (await FSofTUtils.Xamarin.Helper.MessageBox(this,
                                                        "Neuer Marker",
                                                        "Einen neuen Marker an dieser Position setzen?",
                                                        "ja", "nein"))
            gpxWorkbench?.MarkerNew(clientpoint);
      }

      /// <summary>
      /// ev. Marker löschen
      /// </summary>
      /// <param name="marker"></param>
      /// <returns></returns>
      async Task removeMarker(Marker marker) {
         if (await FSofTUtils.Xamarin.Helper.MessageBox(this,
                                                        "Marker löschen",
                                                        "'" + marker.Waypoint.Name + "'" + Environment.NewLine + Environment.NewLine +
                                                        "löschen?",
                                                        "ja", "nein"))
            gpxWorkbench?.MarkerRemove(marker);
      }

      /// <summary>
      /// ev. Marker verschieben (starten)
      /// </summary>
      /// <param name="marker"></param>
      /// <returns></returns>
      async Task startMoveMarker(Marker marker) {
         if (await FSofTUtils.Xamarin.Helper.MessageBox(this,
                                                        "Marker verschieben",
                                                        "Neue Position für" + Environment.NewLine + Environment.NewLine +
                                                        "'" + marker.Waypoint.Name + "'" + Environment.NewLine + Environment.NewLine +
                                                        "setzen?",
                                                        "ja", "nein"))
            gpxWorkbench?.StartMarkerMove(marker);
      }

      /// <summary>
      /// ev. Track löschen
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      async Task removeTrack(Track track) {
         if (await FSofTUtils.Xamarin.Helper.MessageBox(this,
                                                        "Track löschen",
                                                        "'" + track.VisualName + "'" + Environment.NewLine + Environment.NewLine +
                                                        "löschen?",
                                                        "ja", "nein"))
            gpxWorkbench?.TrackRemove(track);
      }

      async Task info4LonLatAsync(Point xamarinpoint, bool onlyosm = false) {
         if (await FSofTUtils.Xamarin.Helper.MessageBox(this,
                                            "Info suchen",
                                            "Soll nach Informationen zu diesem Punkt gesucht werden?",
                                            "ja", "nein")) {
            PointD geopt = map.SpecMapClient2LonLat(xamarin2ClientPoint(xamarinpoint));
            await showInfo4LonLatAsync(geopt.X, geopt.Y);
         }
      }

      /// <summary>
      /// suchen und anzeigen von Infos zum Punkt (Garmin- oder OSM-Suche)
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="onlyosm"></param>
      /// <returns></returns>
      async Task showInfo4LonLatAsync(double lon, double lat, bool onlyosm = false) {
         int providx = map.SpecMapActualMapIdx;

         string distanceText = getDistanceText(lon, lat);

         int height = dem != null ? dem.GetHeight(lon, lat) : DEM1x1.DEMNOVALUE;
         string heightText = height != DEM1x1.DEMNOVALUE ? height.ToString() + "m" : "";

         string preText = "";
         if (distanceText != "")
            preText = preText + "Entfernung " + distanceText + System.Environment.NewLine;
         if (heightText != "")
            preText = preText + "Höhe " + heightText + System.Environment.NewLine;

         preText = preText + (lat >= 0 ? lat.ToString("f6") + "° N" : (-lat).ToString("f6") + "° S") + System.Environment.NewLine;
         preText = preText + (lon >= 0 ? lon.ToString("f6") + "° E" : (-lon).ToString("f6") + "° W") + System.Environment.NewLine;

         if (!onlyosm &&
             0 <= providx && providx < map.SpecMapProviderDefinitions.Count) {
            if (map.SpecMapProviderDefinitions[providx].Provider is GarminProvider) { // falls Garminkarte ...
               int delta = (Math.Min(map.Height, map.Width) * config.DeltaPercent4Search) / 100;
               List<GarminImageCreator.SearchObject> info = map.SpecMapGetGarminObjectInfos(map.SpecMapLonLat2Client(lon, lat), delta, delta);

               if (info.Count > 0)
                  await showGarminInfo4LonLat(info, preText);
               else
                  await UIHelper.ShowInfoMessage(this, preText + "Keine Infos für diesen Punkt vorhanden.", "Garmin-Info");
            } else {
               GeoCodingReverseResultOsm[] geoCodingReverseResultOsms = GeoCodingReverseResultOsm.Get(lon, lat);
               if (geoCodingReverseResultOsms.Length > 0) {
                  string[] names = new string[geoCodingReverseResultOsms.Length];
                  for (int i = 0; i < geoCodingReverseResultOsms.Length; i++)
                     names[i] = geoCodingReverseResultOsms[i].Name;
                  string txt = names.Length > 0 ?
                                          string.Join(Environment.NewLine, names) :
                                          "Keine Info für diesen Punkt vorhanden.";
                  await UIHelper.ShowInfoMessage(this, preText + txt, "OSM-Info");
               }
            }
         }
      }

      string getDistanceText(double lon, double lat) {
         double mylon = 0, mylat = 0;
         bool mypos = geoLocation != null && geoLocation.GetLastPosition(out mylon, out mylat, out double myheight, out DateTime mydatetime);
         if (mypos) {
            double distance = GeoLocation.Distance(mylon, mylat, lon, lat);
            if (distance < 10000)
               return distance.ToString("f0") + "m";
            return (distance / 1000).ToString("f1") + "km";
         }
         return "";
      }

      #endregion

      /// <summary>
      /// Exception anzeigen
      /// </summary>
      /// <param name="caption"></param>
      /// <param name="ex"></param>
      /// <param name="exit"></param>
      /// <returns></returns>
      async Task showExceptionMessage(string caption, Exception ex, bool exit = false) =>
         await UIHelper.ShowExceptionMessage(this, caption, ex, null, exit);

      /// <summary>
      /// zeigt die Anzahl der noch zu ladenden Tiles an
      /// </summary>
      /// <param name="count"></param>
      void showTilesInWork(int count) {
         if (count <= 0 || Interlocked.Read(ref tileLoadIsRunning) == 0)
            TilesInWork.IsVisible = false;
         else {
            TilesInWork.IsVisible = true;
            TilesInWork.Text = count.ToString();
         }
      }

      /// <summary>
      /// "Callback"
      /// </summary>
      /// <param name="filecount"></param>
      /// <param name="filefound"></param>
      /// <param name="found"></param>
      /// <param name="filename"></param>
      void showGpxSearchInfo(int filecount, int filefound, bool found, string filename) {
         Device.BeginInvokeOnMainThread(() => {
            showGpxSearchInfo(filefound, filecount);
         });
      }

      void showGpxSearchInfo(int count, int all) {
         if (count < 0)
            GpxSearch.IsVisible = false;
         else {
            GpxSearch.IsVisible = true;
            GpxSearch.Text = "Gpx-Suche: " + count + "/" + all;
         }
      }

      #region Ausgabe Text in Logdatei

      static object loglocker = new object();

      static string logtext = "";

      /// <summary>
      /// hängt einen Text an die Logdatei an
      /// </summary>
      /// <param name="txt"></param>
      public void Log(string txt) {
         try {
            lock (loglocker) {
               string logfile = string.IsNullOrEmpty(FirstVolumePath) ?
                                             "" :
                                             Path.Combine(FirstVolumePath, DATAPATH, "log.txt");
               txt = DateTime.Now.ToString("G") + " " + txt + Environment.NewLine;
               if (logfile.Length == 0)
                  logtext += txt;
               else {
                  if (logtext.Length > 0) {
                     File.AppendAllText(logfile, logtext);
                     logtext = "";
                  }
                  File.AppendAllText(logfile, txt);
               }
            }
         } catch { }
      }

      #endregion

      #region Punktumrechnungen Xamarin - Client - LatLon

      Point client2XamarinPoint(System.Drawing.Point client) =>
         new Point(GMapControl.SkiaX2XamarinX(client.X),
                   GMapControl.SkiaY2XamarinY(client.Y));

      System.Drawing.Point xamarin2ClientPoint(Point xamarin) =>
         new System.Drawing.Point((int)GMapControl.XamarinX2SkiaX(xamarin.X),
                                  (int)GMapControl.XamarinY2SkiaY(xamarin.Y));

      PointD xamarin2LatLon(Point point) =>
         map.SpecMapClient2LonLat((int)GMapControl.XamarinX2SkiaX(point.X),
                                  (int)GMapControl.XamarinY2SkiaY(point.Y));

      #endregion

      void clearAndReloadMap() => map.SpecMapRefresh(true, true, true);

      private async Task deleteCache(int mapidx = -1) {
         if (isOnInitMap)
            return;
         bool delete = false;
         if (mapidx >= 0) {
            if (!await UIHelper.ShowYesNoQuestion_StdIsNo(this,
                                                          "Soll der Cache für '" +
                                                             map.SpecMapProviderDefinitions[mapidx].MapName +
                                                             "' wirklich gelöscht werden?" +
                                                             Environment.NewLine +
                                                             Environment.NewLine +
                                                             "Das Löschen kann einige Zeit in Anspruch nehmen.",
                                                          "Achtung")) {
               delete = true;
            }
         } else {
            if (!await UIHelper.ShowYesNoQuestion_StdIsNo(this,
                                                          "Soll der Cache wirklich für ALLE Karten gelöscht werden?" +
                                                             Environment.NewLine +
                                                             Environment.NewLine +
                                                             "Das Löschen kann einige Zeit in Anspruch nehmen.",
                                                          "Achtung")) {
               delete = true;
            }
         }
         if (delete) {
            IsBusy = true;
            map.SpecMapClearMemoryCache();
            int tiles = map.SpecMapClearCache(mapidx);
            IsBusy = false;
            if (0 <= tiles) {
               await FSofTUtils.Xamarin.Helper.MessageBox(this,
                                                          "Cache gelöscht",
                                                          tiles + " Kartenteile gelöscht");
               map.SpecMapRefresh(true, false, false);
            }
         }
      }



      private void buttonTestA_Clicked(object sender, EventArgs e) {
         //map_OnExceptionThrown(new Exception("Test"));

         //startMapSettingsPage();

         //map.Map_Zoom -= .5;

         //geoLocation?.StartGeoLocationService();

         ToolbarItem_Config_Clicked(null, null);
      }

      private void buttonTestB_Clicked(object sender, EventArgs e) {
         //map.Map_Zoom += .5;

         //geoLocation?.StopGeoLocationService();
      }

      async private void buttonTestC_Clicked(object sender, EventArgs e) {

         try {
            ConfigfilePage page = new ConfigfilePage(config);
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await showExceptionMessage("Fehler bei " + nameof(buttonTestC_Clicked), ex);
         }


      }

   }
}
