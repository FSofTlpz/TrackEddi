using FSofTUtils.Geography.Garmin;
using FSofTUtils.Geography.GeoCoding;
using FSofTUtils.Geometry;
using FSofTUtils.Xamarin.DependencyTools;
using FSofTUtils.Xamarin.Page;
using FSofTUtils.Xamarin.Touch;
using GMap.NET.CoreExt.MapProviders;
using GMap.NET.Skia;
using SpecialMapCtrl;
using SpecialMapCtrl.EditHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using MapCtrl = SpecialMapCtrl.SpecialMapCtrl;

namespace TrackEddi {
   public partial class MainPage : ContentPage {

      /// <summary>
      /// Standard-Datenverzeichnis der App
      /// </summary>
      const string DATAPATH = "TrackEddi";

      /// <summary>
      /// Name der Konfigurationsdatei
      /// </summary>
      const string CONFIGFILE = "config.xml";

      const string ERRORFILE = "error.txt";


      /// <summary>
      /// Pfad des 1. (ext.) Volumes
      /// </summary>
      string FirstVolumePath;                   // z.B. "/storage/self/primary";    // Emulator -> /mnt/user/0/primary

      object androidActivity = null;

      StorageHelper storageHelper = null;

      string internalgpxbackfile;

      /// <summary>
      /// persistente Programmdate
      /// </summary>
      AppData appData;

      /// <summary>
      /// alle akt. GPX-Daten
      /// </summary>
      GpxAllExt gpx;

      EditMarkerHelper editMarkerHelper;

      EditTrackHelper editTrackHelper;

      EditTools editTools;

      /// <summary>
      /// Konfigurationsdaten
      /// </summary>
      Config config;

      /// <summary>
      /// für die Ermittlung der Höhendaten
      /// </summary>
      FSofTUtils.Geography.DEM.DemData dem = null;

      /// <summary>
      /// Liste der registrierten Garmin-Symbole
      /// </summary>
      List<GarminSymbol> garminMarkerSymbols;

      bool initIsOk = false;

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

      #region Events der EditHelper

      /// <summary>
      /// ev. Aufname eines neuen <see cref="Marker"/>; wird über <see cref="SetNewMarker"/> ausgelöst
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      async void editMarkerHelper_MarkerShouldInsertEvent(object sender, EditMarkerHelper.MarkerEventArgs e) {
         string[] names = null;
         int providx = map.SpecMapActualMapIdx;
         if (0 <= providx && providx < map.SpecMapProviderDefinitions.Count) {
            if (map.SpecMapProviderDefinitions[providx].Provider is GarminProvider) { // falls Garminkarte, dann Textvorschläge holen
               List<GarminImageCreator.SearchObject> info = map.SpecMapGetGarminObjectInfos(map.SpecMapLonLat2Client(e.Marker.Longitude, e.Marker.Latitude), 10, 10);
               if (info.Count > 0) {
                  names = new string[info.Count];
                  for (int i = 0; i < info.Count; i++)
                     names[i] = !string.IsNullOrEmpty(info[i].Name) ?
                                             info[i].Name :
                                             info[i].TypeName;
               }
            } else {
               GeoCodingReverseResultOsm[] geoCodingReverseResultOsms = GeoCodingReverseResultOsm.Get(e.Marker.Longitude, e.Marker.Latitude);
               if (geoCodingReverseResultOsms.Length > 0) {
                  names = new string[geoCodingReverseResultOsms.Length];
                  for (int i = 0; i < geoCodingReverseResultOsms.Length; i++)
                     names[i] = geoCodingReverseResultOsms[i].Name;
               }
            }
         }

         try {
            if (string.IsNullOrEmpty(e.Marker.Symbolname))
               e.Marker.Symbolname = "Flag, Green";            // <--> passend zum VisualMarker für editierbare Marker

            EditMarkerPage page = new EditMarkerPage(e.Marker, garminMarkerSymbols, names);
            page.EndWithOk += (object sender2, EventArgs e2) => {
               if (string.IsNullOrEmpty(e.Marker.Waypoint.Name))
                  e.Marker.Waypoint.Name = string.Format("M Lon={0:F6}°/Lat={1:F6}°", e.Marker.Waypoint.Lon, e.Marker.Waypoint.Lat);    // autom. Name
               Marker marker = editMarkerHelper.InsertCopy(e.Marker, 0);
               marker.Symbolzoom = config.SymbolZoomfactor;
               ShowMarker(marker);
            };
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await ShowExceptionMessage("Fehler bei " + nameof(SetNewMarker), ex);
         }
      }

      private void editMarkerHelper_RefreshProgramStateEvent(object sender, EventArgs e) {
         RefreshProgramState();
      }

      private void editTrackHelper_RefreshProgramStateEvent(object sender, EventArgs e) {

      }

      private void editTrackHelper_TrackEditShowEvent(object sender, EditTrackHelper.TrackEventArgs e) {

      }

      #endregion

      Point XamarinMapCenter {
         get => new Point(MapCtrl.SkiaX2XamarinX(map.Width) / 2,
                          MapCtrl.SkiaY2XamarinY(map.Height) / 2);
      }

      internal System.Drawing.Point ClientMapCenter {
         get => new System.Drawing.Point(map.Width / 2,
                                         map.Height / 2);
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
               map.Map_Refresh();
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
         internalgpxbackfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "persistent.gpx");

         appData = new AppData();
         MapCtrl.Map_ThreadPoolSize = 2;
         initIsOk = false;

         touchPointEvaluator.MoveEvent += TouchPointEvaluator_MoveEvent;
         touchPointEvaluator.TapDownEvent += TouchPointEvaluator_TapDownEvent;
         touchPointEvaluator.TappedEvent += TouchPointEvaluator_TappedEvent;
         touchPointEvaluator.MultiTappedEvent += TouchPointEvaluator_MultiTappedEvent;

         pushEditModeButton(ProgState.Viewer);

#if DEBUG
         Log("End MainPage()");
#endif
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
                  if (gpx != null) {
                     appData.GpxDataChanged = gpx.GpxDataChanged;
                     gpx.Save(internalgpxbackfile, "", true);
                  }
               } catch (Exception ex) {
                  await ShowExceptionMessage("Fehler bei " + nameof(AppEvent), ex);
               }
               break;
         }
      }

      protected override void OnDisappearing() {
         base.OnDisappearing();

         appData.LastZoom = map.Map_Zoom;
         appData.LastLatitude = map.SpecMapCenterLat;
         appData.LastLongitude = map.SpecMapCenterLon;
         int idx = map.SpecMapActualMapIdx;
         if (idx >= 0)
            appData.LastMapname = map.SpecMapProviderDefinitions[idx].MapName;

         if (gpx != null) {
            List<bool> tmp = new List<bool>();
            for (int i = 0; i < gpx.TrackList.Count; i++)
               tmp.Add(gpx.TrackList[i].IsVisible);
            appData.VisibleStatusTrackList = tmp;

            tmp.Clear();
            for (int i = 0; i < gpx.MarkerList.Count; i++)
               tmp.Add(gpx.MarkerList[i].IsVisible);
            appData.VisibleStatusMarkerList = tmp;
         }

      }

      protected override async void OnAppearing() {
         base.OnAppearing();
#if DEBUG
         Log("Start OnAppearing()");
#endif
         if (!initIsOk) {
            try {
               // Bis auf Ausnahmen muss die gesamte Init-Prozedur fehlerfrei laufen. Sonst erfolgt ein Prog-Abbruch.
               if (await initDepTools(androidActivity)) {
                  string datapath = Path.Combine(FirstVolumePath, DATAPATH);
                  if (initDataPath(datapath)) {
                     config = initConfig(Path.Combine(datapath, CONFIGFILE));

                     initMapProvider(map, config);

                     dem = initDEM(config);

                     initVisualTrackData(config);

                     initAndStartMap(map, config, dem);

                     try {
                        initGarminMarkerSymbols(datapath, config);
                     } catch (Exception ex) {
                        await ShowExceptionMessage(ex);
                     }

                     initGpx(config); // benötigt die GarminMarkerSymbols
                  }
                  initIsOk = true;
               } else
                  throw new Exception("Kein Zugriff auf das Dateisystem möglich.");
            } catch (Exception ex) {
               await ShowExceptionMessage(ex, true);  // Abbruch
               return;
            }
         }

         if (editMarkerHelper == null) {
            editMarkerHelper = new EditMarkerHelper(map, gpx, config.HelperLineColor, config.HelperLineWidth);
            editMarkerHelper.MarkerShouldInsertEvent += editMarkerHelper_MarkerShouldInsertEvent;
            editMarkerHelper.RefreshProgramStateEvent += editMarkerHelper_RefreshProgramStateEvent;
         }

         if (editTrackHelper == null) {
            editTrackHelper = new EditTrackHelper(map, gpx, config.HelperLineColor, config.HelperLineWidth);
            editTrackHelper.TrackEditShowEvent += editTrackHelper_TrackEditShowEvent;
            editTrackHelper.RefreshProgramStateEvent += editTrackHelper_RefreshProgramStateEvent;
         }

         if (editTools == null)
            editTools = new EditTools(this, map, dem, gpx, editMarkerHelper, editTrackHelper);

         // Provider aktivieren
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

         //map.Map_MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
         map.Map_MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;
         //map.Map_MouseWheelZoomType = GMap.NET.MouseWheelZoomType.ViewCenter;



#if DEBUG
         Log("End OnAppearing()");
#endif
      }

      #region Initialisierung

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

      void initMapProvider(MapCtrl map, Config cfg) {
         // Provider-Definitionen einlesen ...
         string[] providernames = cfg.Provider;

         List<MapProviderDefinition> provdefs = new List<MapProviderDefinition>();
         for (int providx = 0; providx < providernames.Length; providx++) {
            if (providernames[providx] == GarminProvider.Instance.Name)
               provdefs.Add(new GarminProvider.GarminMapDefinitionData(cfg.MapName(providx),
                                                                       cfg.DbIdDelta(providx),
                                                                       cfg.GetZoom4Display(providx),
                                                                       cfg.MinZoom(providx),
                                                                       cfg.MaxZoom(providx),
                                                                       new string[] {
                                                                             getFullPath(cfg.GarminTdb(providx)),
                                                                       },
                                                                       new string[] {
                                                                             getFullPath(cfg.GarminTyp(providx)),
                                                                       },
                                                                       cfg.GarminTextFactor(providx),
                                                                       cfg.GarminLineFactor(providx),
                                                                       cfg.GarminSymbolFactor(providx)));

            else if (providernames[providx] == GarminKmzProvider.Instance.Name)
               provdefs.Add(new GarminKmzProvider.KmzMapDefinition(cfg.MapName(providx),
                                                                   cfg.DbIdDelta(providx),
                                                                   cfg.GetZoom4Display(providx),
                                                                   cfg.MinZoom(providx),
                                                                   cfg.MaxZoom(providx),
                                                                   getFullPath(cfg.GarminKmzFile(providx))));

            else if (providernames[providx] == WMSProvider.Instance.Name)
               provdefs.Add(new WMSProvider.WMSMapDefinition(cfg.MapName(providx),
                                                             cfg.DbIdDelta(providx),
                                                             cfg.GetZoom4Display(providx),
                                                             cfg.MinZoom(providx),
                                                             cfg.MaxZoom(providx),
                                                             cfg.WmsLayers(providx),
                                                             cfg.WmsUrl(providx),
                                                             cfg.WmsSrs(providx),
                                                             cfg.WmsVersion(providx),
                                                             cfg.WmsPictFormat(providx),
                                                             cfg.WmsExtend(providx)));

            else
               provdefs.Add(new MapProviderDefinition(cfg.MapName(providx),
                                                      providernames[providx],
                                                      cfg.GetZoom4Display(providx),
                                                      cfg.MinZoom(providx),
                                                      cfg.MaxZoom(providx)));
         }

         map.SpecMapRegisterProviders(providernames, provdefs);
      }

      GpxAllExt initGpx(Config cfg) {
         gpx = new GpxAllExt();                // Gleich hier global setzen weil ShowTrack() das benötigt!!!
         gpx.TrackColor = cfg.StandardTrackColor;
         gpx.TrackWidth = cfg.StandardTrackWidth;
         gpx.GpxFileEditable = true;
         if (map != null &&
             File.Exists(internalgpxbackfile)) {
            gpx.Load(internalgpxbackfile, true);
            foreach (var track in gpx.TrackList) {
               ShowTrack(track);
            }

            foreach (Marker marker in gpx.MarkerList) {
               marker.Symbolzoom = cfg.SymbolZoomfactor;
               ShowMarker(marker);
            }

            List<bool> tmp = appData.VisibleStatusTrackList;
            for (int i = 0; i < tmp.Count && i < gpx.TrackList.Count; i++)
               gpx.TrackList[i].IsVisible = tmp[i];

            tmp = appData.VisibleStatusMarkerList;
            for (int i = 0; i < tmp.Count && i < gpx.MarkerList.Count; i++)
               gpx.MarkerList[i].IsVisible = tmp[i];
         }
         gpx.GpxDataChanged = appData.GpxDataChanged;

         gpx.TracklistChanged += Gpx_TracklistChanged;
         gpx.MarkerlistlistChanged += Gpx_MarkerlistlistChanged;
         gpx.ChangeIsSet += Gpx_ChangeIsSet;

         return gpx;
      }

      FSofTUtils.Geography.DEM.DemData initDEM(Config cfg) {
         FSofTUtils.Geography.DEM.DemData dem = new FSofTUtils.Geography.DEM.DemData(string.IsNullOrEmpty(config.DemPath) ?
                                                                                          "" :
                                                                                          getFullPath(config.DemPath),
                                                                                     config.DemCachesize) {
            WithHillshade = true
         };
         dem.SetNewHillshadingData(config.DemHillshadingAzimut,
                                   config.DemHillshadingAltitude,
                                   config.DemHillshadingScale);
         dem.GetHeight(config.StartLongitude, config.StartLatitude);  // liest die DEM-Datei ein
         return dem;
      }

      void initVisualTrackData(Config cfg) {
         VisualTrack.StandardColor = cfg.StandardTrackColor;
         VisualTrack.StandardColor2 = cfg.StandardTrackColor2;
         VisualTrack.StandardColor3 = cfg.StandardTrackColor3;
         VisualTrack.StandardColor4 = cfg.StandardTrackColor4;
         VisualTrack.StandardColor5 = cfg.StandardTrackColor5;
         VisualTrack.StandardWidth = cfg.StandardTrackWidth;
         VisualTrack.MarkedColor = cfg.MarkedTrackColor;
         VisualTrack.MarkedWidth = cfg.MarkedTrackWidth;
         VisualTrack.EditableColor = cfg.EditableTrackColor;
         VisualTrack.EditableWidth = cfg.EditableTrackWidth;
         VisualTrack.InEditableColor = cfg.InEditTrackColor;
         VisualTrack.InEditableWidth = cfg.InEditTrackWidth;
         VisualTrack.SelectedPartColor = cfg.SelectedPartTrackColor;
         VisualTrack.SelectedPartWidth = cfg.SelectedPartTrackWidth;
      }

      void initAndStartMap(MapCtrl map, Config cfg, FSofTUtils.Geography.DEM.DemData dem) {
         map.OnMapTileLoadStart += map_OnTileLoadStart;
         map.OnMapTileLoadComplete += map_OnTileLoadComplete;
         map.OnMapPositionChanged += map_OnPositionChanged;
         map.OnMapZoomChanged += map_OnZoomChanged;
         map.OnMapExceptionThrown += map_OnExceptionThrown;

         map.SpecMapMouseEvent += Map_SpecMapMouseEvent;
         map.SpecMapMarkerEvent += Map_SpecMapMarkerEvent;
         map.SpecMapTrackEvent += Map_SpecMapTrackEvent;
         map.SpecMapDrawOnTop += Map_SpecMapDrawOnTop;

         //map.ShowTileGridLines = true;                 // mit EmptyTileBorders gezeichnet
         //map.ShowCenter = true;                        // shows a little red cross on the map to show you exactly where the center is
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

         map.MapServiceStart(appData.LastLongitude,
                             appData.LastLatitude,
                             getFullPath(config.CacheLocation),
                             (int)appData.LastZoom,
                             GMapControl.ScaleModes.Fractional);

         map.Map_ShowTileGridLines = false; // auch bei DEBUG

         if (startprovider >= 0)
            setProviderZoomPosition(startprovider, appData.LastZoom, appData.LastLongitude, appData.LastLatitude);
      }

      void initGarminMarkerSymbols(string datapath, Config cfg) {
         garminMarkerSymbols = new List<GarminSymbol>();
         string[] garmingroups = cfg.GetGarminMarkerSymbolGroupnames();
         if (garmingroups != null)
            for (int g = 0; g < garmingroups.Length; g++) {
               string[] garminnames = cfg.GetGarminMarkerSymbolnames(g);
               if (garminnames != null)
                  for (int i = 0; i < garminnames.Length; i++) {
                     bool withoffset = config.GetGarminMarkerSymboloffset(g, i, out int offsetx, out int offsety);
                     garminMarkerSymbols.Add(new GarminSymbol(garminnames[i],
                                                              garmingroups[g],
                                                              cfg.GetGarminMarkerSymboltext(g, i),
                                                              Path.Combine(datapath, config.GetGarminMarkerSymbolfile(g, i)),
                                                              withoffset? offsetx : int.MinValue,
                                                              withoffset? offsety : int.MinValue));
      }
   }

         // externe Symbole registrieren
         SpecialMapCtrl.VisualMarker.RegisterExternSymbols(garminMarkerSymbols);
      }

      #endregion

      #region Reaktion auf Map-Events

      // ACHTUNG
      // Es muss damit gerechnet werden, dass alle Map-Events NICHT im UI-Thread geliefert werden. Deshalb sicherheitshalber immer: 
      //    Device.BeginInvokeOnMainThread(()=> { ... });

      private void map_OnPositionChanged(object sender, GMapControl.PositionChangedEventArgs e) {
         Device.BeginInvokeOnMainThread(() => {
            labelPos.Text = string.Format("{0:F6}° {1:F6}°", e.Point.Lng, e.Point.Lat);
         });
      }

      private void map_OnZoomChanged(object sender, EventArgs e) {
         Device.BeginInvokeOnMainThread(() => {
            labelInfo.Text = string.Format("Zoom {0:F3}, linear {1:F1}", map.Map_Zoom, map.Map_ZoomLinear);
         });
         dem.IsActiv = map.SpecMapZoom >= dem.MinimalZoom;
      }

      private void map_OnTileLoadComplete(object sender, GMapControl.TileLoadCompleteEventArgs e) {
         Device.BeginInvokeOnMainThread(() => {
            MainMenu.BackgroundColor = Color.LightGreen;
         });
      }

      private void map_OnTileLoadStart(object sender, EventArgs e) {
         Device.BeginInvokeOnMainThread(() => {
            MainMenu.BackgroundColor = Color.LightSalmon;
         });
      }

      private void Map_SpecMapDrawOnTop(object sender, GMapControl.DrawExtendedEventArgs e) {
         editTools?.MapDrawOnTop(e);
      }

      private async void Map_SpecMapTrackEvent(object sender, MapCtrl.TrackEventArgs e) {
         if (e.Eventtype == MapCtrl.MapMouseEventArgs.EventType.Click)                 // Click => Tapped
            await userTapAction(e.Button != System.Windows.Forms.MouseButtons.Left,    // long-Tap -> Right
                                null,
                                e.Track,
                                Client2XamarinPoint(map.SpecMapLonLat2Client(e.Lon, e.Lat)));
      }

      private async void Map_SpecMapMarkerEvent(object sender, MapCtrl.MarkerEventArgs e) {
         if (e.Eventtype == MapCtrl.MapMouseEventArgs.EventType.Click)                 // Click => Tapped
            await userTapAction(e.Button != System.Windows.Forms.MouseButtons.Left,    // long-Tap -> Right
                                e.Marker,
                                null,
                                Client2XamarinPoint(map.SpecMapLonLat2Client(e.Lon, e.Lat)));
      }

      private void Map_SpecMapMouseEvent(object sender, MapCtrl.MapMouseEventArgs e) {

         // fkt. NICHT, weil danach noch zusätzlich Map_SpecMapMarkerEvent() oder Map_SpecMapTrackEvent() ausgelöst werden kann

         //if (e.Eventtype == MapCtrl.MapMouseEventArgs.EventType.Click) // Click => Tapped
         //   await userTapAction(e.Button != System.Windows.Forms.MouseButtons.Left,
         //                       null,
         //                       null,
         //                       Client2XamarinPoint(e.Location));
      }

      #endregion

      #region Touch-Reaktionen

      double startzoomlinear;
      readonly Dictionary<long, List<Point>> move4ID = new Dictionary<long, List<Point>>();
      readonly TouchPointEvaluator touchPointEvaluator = new TouchPointEvaluator();

      private void mapTouchAction(object sender, TouchEffect.TouchActionEventArgs args) {
         touchPointEvaluator.Evaluate(args);
      }

      private void TouchPointEvaluator_TapDownEvent(object sender, TouchPointEvaluator.TappedEventArgs e) {
         Debug.WriteLine(">>> TapDownEvent: e=" + e);

         move4ID.Add(e.ID, new List<Point>() { e.Point, Point.Zero });
         startzoomlinear = map.Map_ZoomLinear;
      }

      private void TouchPointEvaluator_TappedEvent(object sender, TouchPointEvaluator.TappedEventArgs e) {
         Debug.WriteLine(">>> TappedEvent: e=" + e);

         if (e.TapCount == 1) {
            if (e.LongTap)
               gestureLongTap(e.Point);
            else {


               // Verzögerung nötig, wenn noch ein 2. Tap kommt


               gestureStdTap(e.Point);
            }
         }
         gestureEnd();
      }

      private void TouchPointEvaluator_MultiTappedEvent(object sender, TouchPointEvaluator.TappedEventArgs e) {
         Debug.WriteLine(">>> MultiTappedEvent: e=" + e);

         if (e.TapCount == 2)
            gestureDoubleTap(e.Point);
         gestureEnd();
      }

      private void TouchPointEvaluator_MoveEvent(object sender, TouchPointEvaluator.MoveEventArgs e) {
         Debug.WriteLine(">>> MoveEvent: e=" + e);

         if (move4ID.Count > 1) {    // mehrere Finger
            move4ID[e.ID][1] = e.Point;
            long[] id = new long[move4ID.Keys.Count];
            if (id.Length >= 1) {
               move4ID.Keys.CopyTo(id, 0);
               gestureZoom(move4ID[id[0]][0], move4ID[id[1]][0], move4ID[id[0]][1], move4ID[id[1]][1]);
            }
         } else { // nur 1 Finger
            gestureMove(e.Point.Offset(-e.Delta2Lastpoint.X, -e.Delta2Lastpoint.Y),
                        e.Point,
                        e.MovingEnded);
         }
         if (e.MovingEnded)
            gestureEnd();
      }

      void gestureEnd() {
         foreach (var id in move4ID.Keys)
            move4ID[id].Clear();
         move4ID.Clear();
      }

      async void gestureStdTap(Point point) {
         await mapTapped(point, false);
      }

      async void gestureLongTap(Point point) {
         await mapTapped(point, true);
      }

      /// <summary>
      /// z.Z. NICHT NUTZBAR
      /// </summary>
      /// <param name="point"></param>
      void gestureDoubleTap(Point point) {
         //PointD platlon = XamarinPoint2LatLon(point);
      }

      async void gestureMove(Point from, Point to, bool last) {
         if (!map.Map_IsDragging)
            await map.MapDragStart(from);
         if (last)
            await map.MapDragEnd(to);
         else
            await map.MapDrag(to);
      }

      void gestureZoom(Point p0start, Point p1start, Point p0end, Point p1end) {
         double dist0 = p0start.Distance(p1start);
         double dist1 = p0end.Distance(p1end);
         map.Map_ZoomLinear = startzoomlinear * dist1 / dist0;
      }

      #endregion

      #region Toolbaritems

      async private void ToolbarItem_ChooseMap_Clicked(object sender, EventArgs e) {
         await MapChoosing();
      }

      private void ToolbarItem_ReloadMap_Clicked(object sender, EventArgs e) {
         map.Map_Reload();
      }

      #region Load and Save

      private void ToolbarItem_GPXOpen_Clicked(object sender, EventArgs e) {
         toolbarItem_GPXOpen(false);
      }

      private void ToolbarItem_GPXAppendClicked(object sender, EventArgs e) {
         toolbarItem_GPXOpen(true);
      }

      async private void toolbarItem_GPXOpen(bool append) {
         try {
            ChooseFilePage chooseFilePage = new ChooseFilePage() {
               AndroidActivity = androidActivity,
               Path = appData.LastLoadSavePath,
               Filename = "",
               OnlyExistingFile = true,   // ohne Eingabefeld für Namen
               Match4Filenames = new System.Text.RegularExpressions.Regex(@"\.(gpx|kml|kmz)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase),
               Title = "GPX-Datei auswählen",
            };
            chooseFilePage.ChooseFileReadyEvent += (object sender, FSofTUtils.Xamarin.Control.ChooseFile.ChoosePathAndFileEventArgs e) => {
               if (e.OK) {
                  appData.LastLoadSavePath = e.Path;
                  loadgpxfile(Path.Combine(e.Path, e.Filename), append);
               }
            };

            await Navigation.PushAsync(chooseFilePage);
         } catch (Exception ex) {
            await ShowExceptionMessage("Fehler beim Lesen einer GPX-Datei", ex);
         }
      }

      async void loadgpxfile(string file, bool append) {
         try {
            if (!append &&
                (gpx.TrackList.Count > 0 ||
                 gpx.MarkerList.Count > 0)) {
               string txt1 = gpx.TrackList.Count == 0 ?
                                 "" :
                                 gpx.TrackList.Count == 1 ?
                                    "1 Track" :
                                    gpx.TrackList.Count.ToString() + " Tracks";
               string txt2 = gpx.MarkerList.Count == 0 ?
                                 "" :
                                 gpx.MarkerList.Count == 1 ?
                                    "1 Marker" :
                                    gpx.MarkerList.Count.ToString() + " Marker";

               if (txt1.Length > 0 && txt2.Length > 0) {
                  txt1 += " und " + txt2;
               } else {
                  if (txt2.Length > 0)
                     txt1 = txt2;
               }
               txt1 += (gpx.TrackList.Count + gpx.MarkerList.Count > 1) ? " ist" : " sind";
               txt1 += " breits vorhandenen. Sollen diese Daten überschrieben werden?";

               bool overwrite = await FSofTUtils.Xamarin.Helper.MessageBox(this,
                                                                           "Achtung",
                                                                           txt1,
                                                                           "Ja", "Nein");
               if (!overwrite)
                  return;
            }

            GpxAllExt gpxnew = new GpxAllExt();
            gpxnew.Load(file, true);

            if (!append) {
               gpx.TrackRemoveAll();
               gpx.MarkerRemoveAll();
            }

            for (int i = 0; i < gpxnew.TrackList.Count; i++) {
               Track track = gpx.TrackInsertCopy(gpxnew.TrackList[i]);
               track.LineWidth = config.StandardTrackWidth;
               ShowTrack(track);
            }
            for (int i = 0; i < gpxnew.MarkerList.Count; i++) {
               Marker marker = gpx.MarkerInsertCopy(gpxnew.MarkerList[i]);
               marker.Symbolzoom = config.SymbolZoomfactor;
               ShowMarker(marker);
            }
            gpxnew.VisualRefresh();

            await FSofTUtils.Xamarin.Helper.MessageBox(this, "Info", "Die Datei '" + file + "' wurde eingelesen.");

         } catch (Exception ex) {
            await ShowExceptionMessage("Fehler beim Lesen der GPX-Daten", ex);
         }
      }

      //async private void ToolbarItem_SaveClicked(object sender, EventArgs e) {
      //   if (string.IsNullOrEmpty(appData.LastFullSaveFilename))
      //      ToolbarItem_SaveAsClicked(sender, e);
      //   else {
      //      if (await FSofTUtils.Xamarin.Helper.MessageBox(this,
      //                                                     "Achtung",
      //                                                     "Als Datei '" + appData.LastFullSaveFilename + "' speichern?",
      //                                                     "Ja", "Nein"))
      //         savegpxfile(appData.LastFullSaveFilename, true);
      //   }
      //}

      async private void ToolbarItem_SaveAsClicked(object sender, EventArgs e) {
         await _toolbarItem_SaveAsClicked(false);
      }

      async private void ToolbarItem_SaveAsMultiClicked(object sender, EventArgs e) {
         await _toolbarItem_SaveAsClicked(true);
      }

      async private Task _toolbarItem_SaveAsClicked(bool multi) {
         try {
            ChooseFilePage chooseFilePage = new ChooseFilePage() {
               AndroidActivity = androidActivity,
               Path = string.IsNullOrEmpty(appData.LastFullSaveFilename) ?
                                                   appData.LastLoadSavePath :
                                                   Path.GetDirectoryName(appData.LastFullSaveFilename),
               Filename = string.IsNullOrEmpty(appData.LastFullSaveFilename) ?
                                                   "" :
                                                   Path.GetFileName(appData.LastFullSaveFilename),
               Match4Filenames = new System.Text.RegularExpressions.Regex(@"\.(gpx|kml|kmz)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase),
               OnlyExistingFile = false,   // mit Eingabefeld für Namen
               Title = "Zieldatei auswählen",
            };
            if (multi)
               chooseFilePage.ChooseFileReadyEvent += chooseFilePageSaveMulti_ChooseFileReadyEvent;
            else
               chooseFilePage.ChooseFileReadyEvent += chooseFilePageSave_ChooseFileReadyEvent;

            await Navigation.PushAsync(chooseFilePage);

            //if (multi)
            //   chooseFilePage.ChooseFileReadyEvent -= chooseFilePageSave_ChooseFileReadyEvent;
            //else
            //   chooseFilePage.ChooseFileReadyEvent -= chooseFilePageSaveMulti_ChooseFileReadyEvent;
         } catch (Exception ex) {
            await ShowExceptionMessage("Fehler beim Speichern", ex);
         }

      }

      async void chooseFilePageSave_ChooseFileReadyEvent(object sender, FSofTUtils.Xamarin.Control.ChooseFile.ChoosePathAndFileEventArgs e) {
         await _chooseFilePageSave_ChooseFileReadyEvent(e, false);
      }

      async void chooseFilePageSaveMulti_ChooseFileReadyEvent(object sender, FSofTUtils.Xamarin.Control.ChooseFile.ChoosePathAndFileEventArgs e) {
         await _chooseFilePageSave_ChooseFileReadyEvent(e, true);
      }

      async Task _chooseFilePageSave_ChooseFileReadyEvent(FSofTUtils.Xamarin.Control.ChooseFile.ChoosePathAndFileEventArgs e, bool multi) {
         if (e.OK) {
            string filename = Path.Combine(e.Path, e.Filename);
            string extension = Path.GetExtension(filename).ToLower();
            if (extension == "")     // dummy-Extension
               filename += ".gpx";
            else {
               if (!(extension == ".gpx" ||
                    extension == ".kml" ||
                    extension == ".kmz")) {
                  await FSofTUtils.Xamarin.Helper.MessageBox(this, "Achtung", "Der Dateiname darf nicht mit '" + extension + "' enden (nur .gpx, .kml und .kmz erlaubt).");
                  return;
               }
            }

            if (!File.Exists(filename)) {
               if (multi)
                  savegpxfiles(filename, false);
               else
                  savegpxfile(filename, false);
            } else {
               bool overwrite = await FSofTUtils.Xamarin.Helper.MessageBox(this, "Achtung", "Die Datei '" + filename + "' existiert schon. Soll sie überschrieben werden?", "Ja", "Nein");
               if (overwrite)
                  if (multi)
                     savegpxfiles(filename, true);
                  else
                     savegpxfile(filename, true);
            }
         }
      }

      /// <summary>
      /// alle angezeigten <see cref="Track"/> und <see cref="Marker"/> werden als Datei gespeichert
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="overwrite"></param>
      void savegpxfile(string filename, bool overwrite) {
         if ((File.Exists(filename) && overwrite) ||
             !File.Exists(filename)) {
            GpxAllExt tmp = new GpxAllExt(gpx.AsXml(int.MaxValue));

            // akt. "unsichtbare" Tracks und Marker entfernen
            for (int i = gpx.TrackList.Count - 1; i >= 0; i--)
               if (!gpx.TrackList[i].IsVisible)
                  tmp.RemoveTrack(i);
            for (int i = gpx.MarkerList.Count - 1; i >= 0; i--)
               if (!gpx.MarkerList[i].IsVisible)
                  tmp.RemoveWaypoint(i);

            tmp.Save(filename, Xamarin.Essentials.AppInfo.Name, true);
            appData.LastFullSaveFilename = filename;
         }
      }

      /// <summary>
      /// ein einzelner <see cref="Track"/> wird als Datei gespeichert
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="track"></param>
      /// <param name="overwrite"></param>
      void savegpxfile(string filename, Track track, bool overwrite) {
         if ((File.Exists(filename) && overwrite) ||
             !File.Exists(filename)) {
            GpxAllExt tmp = new GpxAllExt();
            tmp.TrackInsertCopy(track);
            tmp.Save(filename, Xamarin.Essentials.AppInfo.Name, true);
         }
      }

      /// <summary>
      /// alle <see cref="Marker"/> der Liste werden als Datei gespeichert
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="markerlst"></param>
      /// <param name="overwrite"></param>
      void savegpxfile(string filename, IList<Marker> markerlst, bool overwrite) {
         if ((File.Exists(filename) && overwrite) ||
             !File.Exists(filename)) {
            GpxAllExt tmp = new GpxAllExt();
            foreach (var item in markerlst)
               tmp.MarkerInsertCopy(item);
            tmp.Save(filename, Xamarin.Essentials.AppInfo.Name, true);
         }
      }

      /// <summary>
      /// alle angezeigten <see cref="Track"/> werden jeweils in 1 Datei gespeichert und alle angezeigten <see cref="Marker"/> werden gemeinsam in einer Datei gespeichert
      /// </summary>
      /// <param name="basefilename"></param>
      /// <param name="overwrite"></param>
      void savegpxfiles(string basefilename, bool overwrite) {
         string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(basefilename);
         string fileNameExtension = Path.GetExtension(basefilename);
         string path = Path.GetDirectoryName(basefilename);

         int count = 0;
         foreach (var track in gpx.TrackList)
            if (track.IsVisible)
               savegpxfile(Path.Combine(path, fileNameWithoutExtension + "_track" + (++count).ToString() + fileNameExtension), track, overwrite);

         GpxAllExt tmp = new GpxAllExt();
         foreach (var marker in gpx.MarkerList)
            if (marker.IsVisible)
               tmp.MarkerInsertCopy(marker);
         savegpxfile(Path.Combine(path, fileNameWithoutExtension + "_marker" + fileNameExtension), tmp.MarkerList, overwrite);

         appData.LastLoadSavePath = path;
      }

      #endregion

      async private void ToolbarItem_OsmSearchClicked(object sender, EventArgs e) {
         await OsmSearch();
      }

      async private void ToolbarItem_GoToClicked(object sender, EventArgs e) {
         await GoTo();
      }

      async private void ToolbarItem_GpxContent_Clicked(object sender, EventArgs e) {
         await TracklistAndMarkerlist();
      }

      async private void ToolbarItem_Config_Clicked(object sender, EventArgs e) {
         try {
            ConfigPage page = new ConfigPage(map, config);
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await ShowExceptionMessage("Fehler bei " + nameof(ConfigPage), ex);
         }

      }

      #endregion

      #region MainMenu

      private void ZoomIn_Clicked(object sender, EventArgs e) {
         if (map.Map_Zoom % 1.0 == 0)
            map.Map_Zoom++;
         else
            map.Map_Zoom = Math.Ceiling(map.Map_Zoom);     // auf "ganzzahlig einrasten"
      }

      private void ZoomOut_Clicked(object sender, EventArgs e) {
         if (map.Map_Zoom % 1.0 == 0)
            map.Map_Zoom--;
         else
            map.Map_Zoom = Math.Floor(map.Map_Zoom);       // auf "ganzzahlig einrasten"
      }

      private void ChooseMap_Clicked(object sender, EventArgs e) {
         ToolbarItem_ChooseMap_Clicked(sender, e);
      }

      private void TrackMarkerList_Clicked(object sender, EventArgs e) {
         ToolbarItem_GpxContent_Clicked(sender, e);
      }

      #region "Radiobutton" Editiermodus

      async void EditModeNothing_Clicked(object sender, EventArgs e) => await pushEditModeButton(ProgState.Viewer);

      async void EditModeMarkerSet_Clicked(object sender, EventArgs e) => await pushEditModeButton(ProgState.Edit_Marker);

      async void EditModeTrackDraw_Clicked(object sender, EventArgs e) => await pushEditModeButton(ProgState.Edit_TrackDraw);

      async void EditModeTrackSplit_Clicked(object sender, EventArgs e) => await pushEditModeButton(ProgState.Edit_TrackSplit);

      async void EditModeTrackConcat_Clicked(object sender, EventArgs e) => await pushEditModeButton(ProgState.Edit_TrackConcat);

      async Task pushEditModeButton(ProgState newProgState) {
         if (ProgramState != newProgState) {
            if (!initIsOk ||                       // ohne Rückfrage
                await endEditCancelAsync()) {
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
                     editTools?.StartTrackDraw(editTools.MarkedTrack);
                     break;

                  case ProgState.Edit_TrackSplit:
                  case ProgState.Edit_TrackConcat:
                     if (editTools?.MarkedTrack != null)
                        editTools?.StartTrackDraw(editTools.MarkedTrack);
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
      /// <returns>false wenn Edit läuft und NICHT beendet werden soll, sonst true</returns>
      async Task<bool> endEditCancelAsync() {
         return await editTools?.Cancel();   // Edit läuft und soll NICHT beendet werden.
      }

      /// <summary>
      /// akt. Editieraktion beenden mit OK
      /// </summary>
      /// <returns></returns>
      async Task endEditOKAsync() {
         switch (ProgramState) {
            case ProgState.Viewer: break;
            case ProgState.Edit_Marker: editTools?.EndMarker(ClientMapCenter); break;
            case ProgState.Edit_TrackDraw: editTools?.EndTrackDraw(); break;
            case ProgState.Edit_TrackSplit: editTools?.EndTrackSplit(); break;
            case ProgState.Edit_TrackConcat: editTools?.EndTrackConcat(); break;
         }
         await pushEditModeButton(ProgState.Viewer);
      }

      #endregion

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
      private async void ButtonEditTarget_Clicked(object sender, EventArgs e) {
         await mapTapped(XamarinMapCenter, false);
      }

      /// <summary>
      /// letzten Trackpunkt entfernen
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void ButtonEditMinus_Clicked(object sender, EventArgs e) {
         if (ProgramState == ProgState.Edit_TrackDraw) {
            editTools?.RemoveTrackPoint();
            showEditInfoText(editTools?.TrackInEdit);
         }
      }

      /// <summary>
      /// Ende einer Editierfunktion
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private async void ButtonEditEnd_Clicked(object sender, EventArgs e) {
         await endEditOKAsync();
      }

      /// <summary>
      /// Abbruch einer Editierfunktion
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private async void ButtonEditCancel_Clicked(object sender, EventArgs e) {
         await pushEditModeButton(ProgState.Viewer);
      }

      #endregion

      /// <summary>
      /// ein langer oder kurzer Tap auf die Karte ist erfolgt
      /// </summary>
      /// <param name="point"></param>
      /// <param name="longtap"></param>
      /// <returns></returns>
      async Task mapTapped(Point point, bool longtap) {
         PointD platlon = XamarinPoint2LatLon(point);
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
                     await ShowShortMarkerProps(marker);
                  else
                     await EditMarkerProps(marker);
               } else if (track != null) {         // track tapped
                  if (!longtap)
                     await ShowShortTrackProps(track);
                  else
                     await EditTrackProps(track);
               } else {                            // marker nor track tapped
                  if (!longtap) {
                     ;
                  } else
                     await Info4LonLatAsync(point);
               }
               break;

            case ProgState.Edit_Marker:
               if (marker != null) {               // marker tapped
                  showEditInfoText(marker.Text);
                  if (!longtap)
                     await MoveMarker(marker);     // start move marker
                  else
                     await RemoveMarker(marker);   // remove marker
                  showEditInfoText();
               } else if (track != null) {         // track tapped

               } else {                            // marker nor track tapped
                  if (!longtap) {
                     editTools?.EndMarker(Xamarin2ClientPoint(point));
                     showEditInfoText();
                  } else
                     await Info4LonLatAsync(point);
               }
               break;

            case ProgState.Edit_TrackDraw:
               if (marker != null) {                        // marker tapped
                  if (!longtap) {
                     if (editTools != null) {
                        if (editTools.InWork)
                           editTools.AddTrackPoint(Xamarin2ClientPoint(point));
                        else
                           editTools.StartTrackDraw();      // neuer Track
                        showEditInfoText(editTools.TrackInEdit);
                     }
                  } else {
                     await Info4LonLatAsync(point);
                  }
               } else if (track != null) {                  // track tapped
                  if (!longtap) {
                     if (editTools != null) {
                        if (editTools.InWork)
                           editTools.AddTrackPoint(Xamarin2ClientPoint(point));
                        else
                           editTools.StartTrackDraw(track); // Track auf den getippt wurde
                        showEditInfoText(editTools.TrackInEdit);
                     }
                  } else {
                     await RemoveTrack(track);              // Track auf den getippt wurde
                  }
               } else {                                     // marker nor track tapped
                  if (!longtap) {
                     if (editTools != null &&
                         !editTools.InWork)
                        editTools.StartTrackDraw();         // neuer Track    
                     editTools?.AddTrackPoint(Xamarin2ClientPoint(point));
                     showEditInfoText(editTools.TrackInEdit);
                  } else {
                     await Info4LonLatAsync(point);
                  }
               }
               break;

            case ProgState.Edit_TrackSplit:
               if (marker != null) {               // marker tapped
                  ;
               } else if (track != null) {         // track tapped
                  if (editTools != null &&
                      !editTools.InWork) {
                     editTools.StartTrackDraw(track);
                     showEditInfoText(track);
                  }
               } else {                            // marker nor track tapped
                  ;
               }
               break;

            case ProgState.Edit_TrackConcat:
               if (marker != null) {               // marker tapped
                  ;
               } else if (track != null) {         // track tapped
                  if (editTools != null) {
                     if (!editTools.InWork) {
                        editTools.StartTrackDraw(track);
                        showEditInfoText(track, "1. Track: ");
                     } else {
                        editTools.MarkedTrack = track;
                        showEditInfoText(track, "2. Track: ");
                        map.Map_Refresh();
                     }
                  }
               } else {                            // marker nor track tapped
                  ;
               }
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
         double length = track.Length();
         showEditInfoText((pretxt != null ? pretxt : "") +
                          track.VisualName + " (" +
                          (length < 1000 ? string.Format("{0:F0}m", length) : string.Format("{0:F1}km", length / 1000)) + ")");
      }

      #region Reaktion auf Änderungen der GPX-Datei

      private void Gpx_ChangeIsSet(object sender, EventArgs e) { }

      private void Gpx_MarkerlistlistChanged(object sender, GpxAllExt.MarkerlistChangedEventArgs e) { }

      private void Gpx_TracklistChanged(object sender, GpxAllExt.TracklistChangedEventArgs e) { }

      #endregion

      /// <summary>
      /// liefert den abs. Pfad und ersetzt gegebenenfalls Umgebungsvariablen
      /// </summary>
      /// <param name="path"></param>
      /// <returns></returns>
      string getFullPath(string path) {
         if (!string.IsNullOrEmpty(path)) {
            if (!Path.IsPathRooted(path))
               path = Path.Combine(FirstVolumePath, DATAPATH, path);
            if (path.Contains("%"))
               foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
                  path = path.Replace("%" + de.Key + "%", de.Value.ToString());
         }
         return path;
      }

      /// <summary>
      /// setzt die gewünschte Karte, den Zoom und die Position
      /// </summary>
      /// <param name="mapidx">Index für die <see cref="SpecialMapCtrl.SpecialMapCtrl.SpecMapProviderDefinitions"/></param>
      /// <param name="zoom"></param>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      async void setProviderZoomPosition(int mapidx, double zoom, double lon, double lat) {
         try {
            // Zoom und Pos. einstellen
            if (zoom != map.Map_Zoom ||
                lon != map.SpecMapCenterLon ||
                lat != map.SpecMapCenterLat)
               map.SpecMapSetLocationAndZoom(zoom, lon, lat);

            if (0 <= mapidx &&
                mapidx != map.SpecMapActualMapIdx)
               map.SpecMapSetActivProvider(mapidx,
                                           config.HillshadingAlpha(mapidx),
                                           dem);
         } catch (Exception ex) {
            await ShowExceptionMessage("Fehler bei " + nameof(setProviderZoomPosition), ex);
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
                                 visible ? gpx.NextVisibleTrack(track) : null);
      }

      /// <summary>
      /// <see cref="Marker"/> anzeigen oder verbergen
      /// </summary>
      /// <param name="idx"></param>
      /// <param name="visible"></param>
      void ShowMarker(int idx, bool visible = true) {
         if (0 <= idx && idx < gpx.MarkerList.Count)
            ShowMarker(gpx.MarkerList[idx], visible);
      }

      /// <summary>
      /// <see cref="Marker"/> anzeigen oder verbergen
      /// </summary>
      /// <param name="marker"></param>
      /// <param name="visible"></param>
      /// <returns></returns>
      public int ShowMarker(Marker marker, bool visible = true) {
         if (marker.IsVisible == visible)
            return -1;
         map.SpecMapShowMarker(marker,
                               visible,
                               visible ? gpx.NextVisibleMarker(marker) : null);

         //editableTracklistControl1.ShowMarker(marker, visible);

         return gpx.MarkerIndex(marker);
      }

      #region Subpages ("Dialoge"), Infos und Aktionen

      /// <summary>
      /// Auswahl der Karte
      /// </summary>
      /// <returns></returns>
      async Task MapChoosing() {
         MapChoosingPage page;
         try {
            page = new MapChoosingPage() {
               MapControl = map,
            };
            page.MapChoosingEvent += MapChoosingPage_MapChoosingEvent;
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await ShowExceptionMessage("Fehler bei " + nameof(MapChoosing), ex);
         }
      }

      void MapChoosingPage_MapChoosingEvent(object sender, MapChoosingPage.MapChoosingEventArgs e) {
         if (e.Idx >= 0) {
            appData.LastMapname = map.SpecMapProviderDefinitions[e.Idx].MapName;
            setProviderZoomPosition(e.Idx, appData.LastZoom, appData.LastLongitude, appData.LastLatitude);
         }
      }

      /// <summary>
      /// (bearbeitbare) Track- und Markerliste anzeigen
      /// </summary>
      /// <returns></returns>
      async Task TracklistAndMarkerlist() {
         try {
            GpxContentPage page = new GpxContentPage(gpx,
                                                     map,
                                                     editMarkerHelper,
                                                     garminMarkerSymbols,
                                                     appData);
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await ShowExceptionMessage("Fehler bei " + nameof(TracklistAndMarkerlist), ex);
         }
      }

      /// <summary>
      /// geografische Suche per OSM (nur online)
      /// </summary>
      /// <returns></returns>
      async Task OsmSearch() {
         try {
            OsmSearchPage page = new OsmSearchPage(map,
                                                   appData);
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await ShowExceptionMessage("Fehler bei OSM-Suche", ex);
         }
      }

      async Task ShowGarminInfo4LonLat(IList<GarminImageCreator.SearchObject> infos) {
         try {
            ShowGarminInfo4LonLat page = new ShowGarminInfo4LonLat(infos);
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await ShowExceptionMessage("Fehler bei GarminInfo-Anzeige", ex);
         }
      }

      /// <summary>
      /// zu einem "benannten" Ort oder zu geografischen Koordinaten gehen
      /// </summary>
      /// <returns></returns>
      async Task GoTo() {
         try {
            GoToPage page = new GoToPage(map,
                                         appData);
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await ShowExceptionMessage("Fehler bei " + nameof(GoTo), ex);
         }
      }

      /// <summary>
      /// zeigt die (editierbare) Seite der Marker-Eigenschaften an
      /// </summary>
      /// <param name="marker"></param>
      /// <returns></returns>
      async Task EditMarkerProps(Marker marker) {
         try {
            EditMarkerPage page = new EditMarkerPage(marker, garminMarkerSymbols);
            page.EndWithOk += (object sender, EventArgs e) => {
               int idx = gpx.MarkerIndex(page.Marker);
               if (idx >= 0) {
                  gpx.GpxDataChanged = true;
                  gpx.Waypoints[idx] = page.Marker.Waypoint;
                  editMarkerHelper.RefreshOnMap(page.Marker);
               }
            };
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await ShowExceptionMessage("Fehler bei " + nameof(EditMarkerProps), ex);
         }
      }

      /// <summary>
      /// einfache Anzeige der Marker-Eigenschaften
      /// </summary>
      /// <param name="marker"></param>
      /// <returns></returns>
      async Task ShowShortMarkerProps(Marker marker) {
         await ShowMessage("Marker", marker.Waypoint.Name);
      }

      /// <summary>
      /// zeigt die (editierbare) Seite der Track-Eigenschaften an
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      async Task EditTrackProps(Track track) {
         try {
            Track trackcopy = Track.CreateCopy(track);
            EditTrackPage page = new EditTrackPage(trackcopy);
            page.EndWithOk += (object sender2, EventArgs e2) => {
               track.LineColor = trackcopy.LineColor;
               track.GpxTrack.Name = trackcopy.GpxTrack.Name;
               track.GpxTrack.Description = trackcopy.GpxTrack.Description;
               track.GpxTrack.Comment = trackcopy.GpxTrack.Comment;
               track.GpxTrack.Source = trackcopy.GpxTrack.Source;
            };
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await ShowExceptionMessage("Fehler bei " + nameof(EditTrackProps), ex);
         }
      }

      /// <summary>
      /// einfache Anzeige der Track-Eigenschaften
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      async Task ShowShortTrackProps(Track track) {
         await ShowMessage("Track",
                           track.Trackname + System.Environment.NewLine +
                              (track.StatLength < 1000 ? string.Format("{0:F0}m", track.StatLength) : string.Format("{0:F1}km", track.StatLength / 1000)));
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
            editTools?.MarkerNew(clientpoint);
      }

      /// <summary>
      /// ev. Marker löschen
      /// </summary>
      /// <param name="marker"></param>
      /// <returns></returns>
      async Task RemoveMarker(Marker marker) {
         if (await FSofTUtils.Xamarin.Helper.MessageBox(this,
                                                        "Marker löschen",
                                                        "'" + marker.Waypoint.Name + "'" + Environment.NewLine + Environment.NewLine +
                                                        "löschen?",
                                                        "ja", "nein"))
            editTools?.MarkerRemove(marker);
      }

      /// <summary>
      /// ev. Marker verschieben (starten)
      /// </summary>
      /// <param name="marker"></param>
      /// <returns></returns>
      async Task MoveMarker(Marker marker) {
         if (await FSofTUtils.Xamarin.Helper.MessageBox(this,
                                                        "Marker verschieben",
                                                        "Neue Position für" + Environment.NewLine + Environment.NewLine +
                                                        "'" + marker.Waypoint.Name + "'" + Environment.NewLine + Environment.NewLine +
                                                        "setzen?",
                                                        "ja", "nein"))
            editTools?.StartMarkerMove(marker);
      }

      /// <summary>
      /// ev. Track löschen
      /// </summary>
      /// <param name="track"></param>
      /// <returns></returns>
      async Task RemoveTrack(Track track) {
         if (await FSofTUtils.Xamarin.Helper.MessageBox(this,
                                                        "Track löschen",
                                                        "'" + track.VisualName + "'" + Environment.NewLine + Environment.NewLine +
                                                        "löschen?",
                                                        "ja", "nein"))
            editTools?.TrackRemove(track);
      }

      async Task Info4LonLatAsync(Point xamarinpoint, bool onlyosm = false) {
         if (await FSofTUtils.Xamarin.Helper.MessageBox(this,
                                            "Info suchen",
                                            "Soll nach Informationen zu diesem Punkt gesucht werden?",
                                            "ja", "nein")) {
            PointD geopt = map.SpecMapClient2LonLat(Xamarin2ClientPoint(xamarinpoint));
            await ShowInfo4LonLatAsync(geopt.X, geopt.Y);
         }
      }

      /// <summary>
      /// suchen und anzeigen von Infos zum Punkt (Garmin- oder OSM-Suche)
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="onlyosm"></param>
      /// <returns></returns>
      async Task ShowInfo4LonLatAsync(double lon, double lat, bool onlyosm = false) {
         int providx = map.SpecMapActualMapIdx;
         if (!onlyosm &&
             0 <= providx && providx < map.SpecMapProviderDefinitions.Count) {
            if (map.SpecMapProviderDefinitions[providx].Provider is GarminProvider) { // falls Garminkarte ...
               int delta = (Math.Min(map.ClientSizeHeight, map.ClientSizeWidth) * config.DeltaPercent4Search) / 100;
               List<GarminImageCreator.SearchObject> info = map.SpecMapGetGarminObjectInfos(map.SpecMapLonLat2Client(lon, lat), delta, delta);
               if (info.Count > 0)
                  await ShowGarminInfo4LonLat(info);
               else
                  await ShowMessage("Garmin-Info", "Keine Infos für diesen Punkt vorhanden.");
            } else {
               GeoCodingReverseResultOsm[] geoCodingReverseResultOsms = GeoCodingReverseResultOsm.Get(lon, lat);
               if (geoCodingReverseResultOsms.Length > 0) {
                  string[] names = new string[geoCodingReverseResultOsms.Length];
                  for (int i = 0; i < geoCodingReverseResultOsms.Length; i++)
                     names[i] = geoCodingReverseResultOsms[i].Name;
                  string txt = names.Length > 0 ?
                                          string.Join(Environment.NewLine, names) :
                                          "Keine Info für diesen Punkt vorhanden.";
                  await ShowMessage("OSM-Info", txt);
               }
            }
         }
      }

      #endregion

      #region Anzeige von Infos und Exceptions

      async Task ShowMessage(string title, string msg) {
         await FSofTUtils.Xamarin.Helper.MessageBox(this, title, msg);
      }

      /// <summary>
      /// Exception anzeigen
      /// </summary>
      /// <param name="ex"></param>
      /// <param name="exit">wenn true, dann Prog sofort abbrechen</param>
      /// <returns></returns>
      async Task ShowExceptionMessage(Exception ex, bool exit = false) {
         await ShowExceptionMessage("Fehler", ex, exit);
      }

      /// <summary>
      /// Exception anzeigen
      /// </summary>
      /// <param name="caption"></param>
      /// <param name="ex"></param>
      /// <param name="exit"></param>
      /// <returns></returns>
      async Task ShowExceptionMessage(string caption, Exception ex, bool exit = false) {
         string message = "";
         do {
            if (message.Length > 0)
               message += Environment.NewLine + Environment.NewLine;
            message += ex.Message;
            message += Environment.NewLine + Environment.NewLine + ex.StackTrace;
            ex = ex.InnerException;
         } while (ex != null);

         try {
            string file = Path.Combine(FirstVolumePath, DATAPATH, ERRORFILE);
            File.AppendAllText(file, DateTime.Now.ToString("G") + " " + caption + ": " + message);
         } catch { }

         await ShowMessage(caption, message);
         if (exit) {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            Environment.Exit(0);
         }
      }

      async private void map_OnExceptionThrown(Exception ex) {
         await ShowExceptionMessage("Fehler bei " + nameof(map.OnMapExceptionThrown), ex);
      }

      #endregion

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

      #region Punktumrechnungen

      Point Client2XamarinPoint(System.Drawing.Point client) {
         return new Point(MapCtrl.SkiaX2XamarinX(client.X),
                          MapCtrl.SkiaY2XamarinY(client.Y));
      }

      System.Drawing.Point Xamarin2ClientPoint(Point xamarin) {
         return new System.Drawing.Point((int)MapCtrl.XamarinX2SkiaX(xamarin.X),
                                         (int)MapCtrl.XamarinY2SkiaY(xamarin.Y));
      }

      PointD XamarinPoint2LatLon(Point point) {
         return map.SpecMapClient2LonLat((int)GMap.NET.Skia.GMapControl.XamarinX2SkiaX(point.X),
                                         (int)GMap.NET.Skia.GMapControl.XamarinY2SkiaY(point.Y));
      }

      #endregion

      void clearAndReloadMap() {
         map.SpecMapClearCache();
         map.SpecMapClearMemoryCache();
         map.Map_Reload();
      }


      private void buttonTestA_Clicked(object sender, EventArgs e) {
         //map_OnExceptionThrown(new Exception("Test"));

         //startMapSettingsPage();

         map.Map_Zoom -= .5;
      }

      private void buttonTestB_Clicked(object sender, EventArgs e) {
         map.Map_Zoom += .5;
      }

      async private void buttonTestC_Clicked(object sender, EventArgs e) {

         try {
            ConfigfilePage page = new ConfigfilePage(config);
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await ShowExceptionMessage("Fehler bei " + nameof(buttonTestC_Clicked), ex);
         }


      }

   }
}
