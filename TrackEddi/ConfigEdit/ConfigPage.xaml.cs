using FSofTUtils.Xamarin.Control;
using FSofTUtils.Xamarin.Page;
using GMap.NET.CoreExt.MapProviders;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrackEddi.Common;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrackEddi.ConfigEdit {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class ConfigPage : ContentPage {

      object androidActivity = null;

      /// <summary>
      /// Map-Control
      /// </summary>
      SpecialMapCtrl.SpecialMapCtrl map;

      /// <summary>
      /// Wurde die Konfiguration für die Karten geändert?
      /// </summary>
      public bool MapsConfigChanged {
         get;
         protected set;
      } = false;

      public List<int[]> ProvIdxPaths;

      /// <summary>
      /// (ev.) veränderte <see cref="Config"/>
      /// </summary>
      public Config ConfigEdit { get; protected set; }

      public bool ConfigChanged { get; protected set; } = false;

      public bool SaveButtonPressed { get; protected set; } = false;

      /// <summary>
      /// gilt für <see cref="OnAppearing"/> und <see cref="OnDisappearing"/>
      /// <para>Beim Schließen der Page und Rückkehr zur aufrufenden Page liegt die akt. Page noch an der Spitze des Stacks.</para>
      /// <para>Beim Aufruf einer Subpage liegt die neue Subpage schon an der Spitze und die akt. Page eine Position darunter.</para>
      /// </summary>
      public bool IsOnOpeningOrClosingSubPage => !Navigation.NavigationStack[Navigation.NavigationStack.Count - 1].Equals(this);


      Config configorg;

      bool changed = false;


      /*    LastUsedMapsCount
       *    DeltaPercent4Search
       *    SymbolZoomfactor
       *    ClickTolerance4Tracks
       *    MinimalTrackpointDistanceX
       *    MinimalTrackpointDistanceY
       *    CacheLocation
       *    
       *    DemPath
       *    MinZoom4DEM
       *    DemHillshadingAzimut
       *    DemHillshadingAltitude
       *    DemHillshadingScale
       *    
       *    LocationSymbolsize
       *    TrackingMinimalPointdistance
       *    TrackingMinimalHeightdistance
       *    
       *    Standard1Color
       *    ...
       *    LiveTrackingColor
       *    MarkedColor
       *    EditableColor
       *    InEditColor
       *    SelectedPartColor
       *    HelperLineColor
       */


      #region Allgemein

      public int LastUsedMapsCount {
         get => ConfigEdit.LastUsedMapsCount;
         set {
            if (ConfigEdit.LastUsedMapsCount != value) {
               ConfigEdit.LastUsedMapsCount = value;
               onPropertyChanged(nameof(LastUsedMapsCount));
            }
         }
      }

      public int DeltaPercent4Search {
         get => ConfigEdit.DeltaPercent4Search;
         set {
            if (ConfigEdit.DeltaPercent4Search != value) {
               ConfigEdit.DeltaPercent4Search = value;
               onPropertyChanged(nameof(DeltaPercent4Search));
            }
         }
      }

      public double SymbolZoomfactor {
         get => ConfigEdit.SymbolZoomfactor;
         set {
            if (ConfigEdit.SymbolZoomfactor != value) {
               ConfigEdit.SymbolZoomfactor = value;
               onPropertyChanged(nameof(SymbolZoomfactor));
            }
         }
      }

      public double ClickTolerance4Tracks {
         get => ConfigEdit.ClickTolerance4Tracks;
         set {
            if (ConfigEdit.ClickTolerance4Tracks != value) {
               ConfigEdit.ClickTolerance4Tracks = value;
               onPropertyChanged(nameof(ClickTolerance4Tracks));
            }
         }
      }

      public double MinimalTrackpointDistanceX {
         get => ConfigEdit.MinimalTrackpointDistanceX;
         set {
            if (ConfigEdit.ClickTolerance4Tracks != value) {
               ConfigEdit.ClickTolerance4Tracks = value;
               onPropertyChanged(nameof(MinimalTrackpointDistanceX));
            }
         }
      }

      public double MinimalTrackpointDistanceY {
         get => ConfigEdit.MinimalTrackpointDistanceY;
         set {
            if (ConfigEdit.ClickTolerance4Tracks != value) {
               ConfigEdit.ClickTolerance4Tracks = value;
               onPropertyChanged(nameof(MinimalTrackpointDistanceY));
            }
         }
      }

      public string CacheLocation {
         get => ConfigEdit.CacheLocation;
         set {
            if (ConfigEdit.CacheLocation != value.Trim()) {
               ConfigEdit.CacheLocation = value.Trim();
               onPropertyChanged(nameof(CacheLocation));
            }
         }
      }

      #endregion

      #region DEM / Hillshading

      public string DemPath {
         get => ConfigEdit.DemPath;
         set {
            string v = value.Trim();
            if (v != ConfigEdit.DemPath) {
               ConfigEdit.DemPath = v;
               onPropertyChanged(nameof(DemPath));
            }
         }
      }

      public int MinZoom4DEM {
         get => ConfigEdit.DemMinZoom;
         set {
            if (value != ConfigEdit.DemMinZoom) {
               ConfigEdit.DemMinZoom = value;
               onPropertyChanged(nameof(MinZoom4DEM));
            }
         }
      }

      public double DemHillshadingAzimut {
         get => ConfigEdit.DemHillshadingAzimut;
         set {
            if (value != ConfigEdit.DemHillshadingAzimut) {
               ConfigEdit.DemHillshadingAzimut = value;
               onPropertyChanged(nameof(DemHillshadingAzimut));
            }
         }
      }

      public double DemHillshadingAltitude {
         get => ConfigEdit.DemHillshadingAltitude;
         set {
            if (value != ConfigEdit.DemHillshadingAltitude) {
               ConfigEdit.DemHillshadingAltitude = value;
               onPropertyChanged(nameof(DemHillshadingAltitude));
            }
         }
      }

      public double DemHillshadingScale {
         get => ConfigEdit.DemHillshadingScale;
         set {
            if (value != ConfigEdit.DemHillshadingScale) {
               ConfigEdit.DemHillshadingScale = value;
               onPropertyChanged(nameof(DemHillshadingScale));
            }
         }
      }

      #endregion

      #region Tracking

      public int LocationSymbolsize {
         get => ConfigEdit.LocationSymbolsize;
         set {
            if (value != ConfigEdit.LocationSymbolsize) {
               ConfigEdit.LocationSymbolsize = value;
               onPropertyChanged(nameof(LocationSymbolsize));
            }
         }
      }

      public double TrackingMinimalPointdistance {
         get => ConfigEdit.TrackingMinimalPointdistance;
         set {
            if (value != ConfigEdit.TrackingMinimalPointdistance) {
               ConfigEdit.TrackingMinimalPointdistance = value;
               onPropertyChanged(nameof(TrackingMinimalPointdistance));
            }
         }
      }

      public double TrackingMinimalHeightdistance {
         get => ConfigEdit.TrackingMinimalHeightdistance;
         set {
            if (value != ConfigEdit.TrackingMinimalHeightdistance) {
               ConfigEdit.TrackingMinimalHeightdistance = value;
               onPropertyChanged(nameof(TrackingMinimalHeightdistance));
            }
         }
      }

      #endregion

      #region Linienfarben

      public Color Standard1Color {
         get => new Color(ConfigEdit.StandardTrackColor.R / 255.0,
                          ConfigEdit.StandardTrackColor.G / 255.0,
                          ConfigEdit.StandardTrackColor.B / 255.0,
                          ConfigEdit.StandardTrackColor.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != ConfigEdit.StandardTrackColor) {
               ConfigEdit.StandardTrackColor = value; // XamarinColor2WinColor(value); Interessanterweise erfolgt die Umwandlung automatisch!
               onPropertyChanged(nameof(Standard1Color));
            }
         }
      }

      public Color Standard2Color {
         get => new Color(ConfigEdit.StandardTrackColor2.R / 255.0,
                          ConfigEdit.StandardTrackColor2.G / 255.0,
                          ConfigEdit.StandardTrackColor2.B / 255.0,
                          ConfigEdit.StandardTrackColor2.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != ConfigEdit.StandardTrackColor2) {
               ConfigEdit.StandardTrackColor2 = value;
               onPropertyChanged(nameof(Standard2Color));
            }
         }
      }

      public Color Standard3Color {
         get => new Color(ConfigEdit.StandardTrackColor3.R / 255.0,
                          ConfigEdit.StandardTrackColor3.G / 255.0,
                          ConfigEdit.StandardTrackColor3.B / 255.0,
                          ConfigEdit.StandardTrackColor3.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != ConfigEdit.StandardTrackColor3) {
               ConfigEdit.StandardTrackColor3 = value;
               onPropertyChanged(nameof(Standard3Color));
            }
         }
      }

      public Color Standard4Color {
         get => new Color(ConfigEdit.StandardTrackColor4.R / 255.0,
                          ConfigEdit.StandardTrackColor4.G / 255.0,
                          ConfigEdit.StandardTrackColor4.B / 255.0,
                          ConfigEdit.StandardTrackColor4.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != ConfigEdit.StandardTrackColor4) {
               ConfigEdit.StandardTrackColor4 = value;
               onPropertyChanged(nameof(Standard4Color));
            }
         }
      }

      public Color Standard5Color {
         get => new Color(ConfigEdit.StandardTrackColor5.R / 255.0,
                          ConfigEdit.StandardTrackColor5.G / 255.0,
                          ConfigEdit.StandardTrackColor5.B / 255.0,
                          ConfigEdit.StandardTrackColor5.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != ConfigEdit.StandardTrackColor5) {
               ConfigEdit.StandardTrackColor5 = value;
               onPropertyChanged(nameof(Standard5Color));
            }
         }
      }

      public Color LiveTrackingColor {
         get => new Color(ConfigEdit.LiveTrackColor.R / 255.0,
                          ConfigEdit.LiveTrackColor.G / 255.0,
                          ConfigEdit.LiveTrackColor.B / 255.0,
                          ConfigEdit.LiveTrackColor.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != ConfigEdit.LiveTrackColor) {
               ConfigEdit.LiveTrackColor = value;
               onPropertyChanged(nameof(LiveTrackingColor));
            }
         }
      }

      public Color MarkedColor {
         get => new Color(ConfigEdit.MarkedTrackColor.R / 255.0,
                          ConfigEdit.MarkedTrackColor.G / 255.0,
                          ConfigEdit.MarkedTrackColor.B / 255.0,
                          ConfigEdit.MarkedTrackColor.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != ConfigEdit.MarkedTrackColor) {
               ConfigEdit.MarkedTrackColor = value;
               onPropertyChanged(nameof(MarkedColor));
            }
         }
      }

      public Color EditableColor {
         get => new Color(ConfigEdit.EditableTrackColor.R / 255.0,
                          ConfigEdit.EditableTrackColor.G / 255.0,
                          ConfigEdit.EditableTrackColor.B / 255.0,
                          ConfigEdit.EditableTrackColor.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != ConfigEdit.EditableTrackColor) {
               ConfigEdit.EditableTrackColor = value;
               onPropertyChanged(nameof(EditableColor));
            }
         }
      }

      public Color InEditColor {
         get => new Color(ConfigEdit.InEditTrackColor.R / 255.0,
                          ConfigEdit.InEditTrackColor.G / 255.0,
                          ConfigEdit.InEditTrackColor.B / 255.0,
                          ConfigEdit.InEditTrackColor.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != ConfigEdit.InEditTrackColor) {
               ConfigEdit.InEditTrackColor = value;
               onPropertyChanged(nameof(InEditColor));
            }
         }
      }

      public Color SelectedPartColor {
         get => new Color(ConfigEdit.SelectedPartTrackColor.R / 255.0,
                          ConfigEdit.SelectedPartTrackColor.G / 255.0,
                          ConfigEdit.SelectedPartTrackColor.B / 255.0,
                          ConfigEdit.SelectedPartTrackColor.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != ConfigEdit.SelectedPartTrackColor) {
               ConfigEdit.SelectedPartTrackColor = value;
               onPropertyChanged(nameof(SelectedPartColor));
            }
         }
      }

      public Color HelperLineColor {
         get => new Color(ConfigEdit.HelperLineColor.R / 255.0,
                          ConfigEdit.HelperLineColor.G / 255.0,
                          ConfigEdit.HelperLineColor.B / 255.0,
                          ConfigEdit.HelperLineColor.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != ConfigEdit.HelperLineColor) {
               ConfigEdit.HelperLineColor = value;
               onPropertyChanged(nameof(HelperLineColor));
            }
         }
      }

      #endregion

      #region Linienbreiten

      public float Standard1Width {
         get => ConfigEdit.StandardTrackWidth;
         set {
            if (value != ConfigEdit.StandardTrackWidth) {
               ConfigEdit.StandardTrackWidth = value;
               onPropertyChanged(nameof(Standard1Width));
            }
         }
      }

      public float Standard2Width {
         get => ConfigEdit.StandardTrackWidth2;
         set {
            if (value != ConfigEdit.StandardTrackWidth2) {
               ConfigEdit.StandardTrackWidth2 = value;
               onPropertyChanged(nameof(Standard2Width));
            }
         }
      }

      public float Standard3Width {
         get => ConfigEdit.StandardTrackWidth3;
         set {
            if (value != ConfigEdit.StandardTrackWidth3) {
               ConfigEdit.StandardTrackWidth3 = value;
               onPropertyChanged(nameof(Standard3Width));
            }
         }
      }

      public float Standard4Width {
         get => ConfigEdit.StandardTrackWidth4;
         set {
            if (value != ConfigEdit.StandardTrackWidth4) {
               ConfigEdit.StandardTrackWidth4 = value;
               onPropertyChanged(nameof(Standard4Width));
            }
         }
      }

      public float Standard5Width {
         get => ConfigEdit.StandardTrackWidth5;
         set {
            if (value != ConfigEdit.StandardTrackWidth5) {
               ConfigEdit.StandardTrackWidth5 = value;
               onPropertyChanged(nameof(Standard5Width));
            }
         }
      }

      public float LiveTrackingWidth {
         get => ConfigEdit.MarkedTrackWidth;
         set {
            if (value != ConfigEdit.LiveTrackWidth) {
               ConfigEdit.LiveTrackWidth = value;
               onPropertyChanged(nameof(LiveTrackingWidth));
            }
         }
      }

      public float MarkedWidth {
         get => ConfigEdit.MarkedTrackWidth;
         set {
            if (value != ConfigEdit.MarkedTrackWidth) {
               ConfigEdit.MarkedTrackWidth = value;
               onPropertyChanged(nameof(MarkedWidth));
            }
         }
      }

      public float EditableWidth {
         get => ConfigEdit.EditableTrackWidth;
         set {
            if (value != ConfigEdit.EditableTrackWidth) {
               ConfigEdit.EditableTrackWidth = value;
               onPropertyChanged(nameof(EditableWidth));
            }
         }
      }

      public float InEditWidth {
         get => ConfigEdit.InEditTrackWidth;
         set {
            if (value != ConfigEdit.InEditTrackWidth) {
               ConfigEdit.InEditTrackWidth = value;
               onPropertyChanged(nameof(InEditWidth));
            }
         }
      }

      public float SelectedPartWidth {
         get => ConfigEdit.SelectedPartTrackWidth;
         set {
            if (value != ConfigEdit.SelectedPartTrackWidth) {
               ConfigEdit.SelectedPartTrackWidth = value;
               onPropertyChanged(nameof(SelectedPartWidth));
            }
         }
      }

      public float HelperLineWidth {
         get => ConfigEdit.HelperLineWidth;
         set {
            if (value != ConfigEdit.HelperLineWidth) {
               ConfigEdit.HelperLineWidth = value;
               onPropertyChanged(nameof(HelperLineWidth));
            }
         }
      }

      #endregion


      enum LineObject {
         Standard1,
         Standard2,
         Standard3,
         Standard4,
         Standard5,
         Marked,
         LiveTracking,
         Editable,
         InEdit,
         SelectedPart,
         HelperLine,
      }


      public ConfigPage(SpecialMapCtrl.SpecialMapCtrl map,
                        Config config,
                        List<int[]> provIdxPaths,
                        object androidactivity) {
         InitializeComponent();

         androidActivity = androidactivity;
         this.map = map;
         configorg = config;
         ConfigEdit = new Config(config.XmlFilename, null);
         BindingContext = this;
         changed = false;
         ProvIdxPaths = provIdxPaths;

         //tvMaps.OnNodeTapped += (s,e) => { TapGestureRecognizerMapProps_Tapped(null, null); };
         tvMaps.OnNodeDoubleTapped += (s, e) => { TapGestureRecognizerMapProps_Tapped(null, null); };
         MapTreeViewHelper.BuildTreeViewContent(configorg, tvMaps, map.SpecMapProviderDefinitions, ProvIdxPaths, map.SpecMapActualMapIdx);
         tvMaps.ExpandOrCollapseAll(true);
      }

      protected override void OnAppearing() {
         base.OnAppearing();

         //if (!IsOnOpeningOrClosingSubPage)
         //   MapTreeViewHelper.BuildTreeViewContent(configorg, tvMaps, map.SpecMapProviderDefinitions, ProvIdxPaths, map.SpecMapActualMapIdx);

      }

      protected override void OnDisappearing() {
         base.OnDisappearing();
         if (!IsOnOpeningOrClosingSubPage)
            if (changed)
               MapTreeViewHelper.RebuildConfig4Maps(tvMaps, ConfigEdit);
      }

      void onPropertyChanged(string propname) {
         changed = true;
         btnSave.IsEnabled = true;
         OnPropertyChanged(propname);
      }

      private async void TapGestureRecognizerStandard1_Tapped(object sender, EventArgs e) => await chooseColor(LineObject.Standard1);

      private async void TapGestureRecognizerStandard2_Tapped(object sender, EventArgs e) => await chooseColor(LineObject.Standard2);

      private async void TapGestureRecognizerStandard3_Tapped(object sender, EventArgs e) => await chooseColor(LineObject.Standard3);

      private async void TapGestureRecognizerStandard4_Tapped(object sender, EventArgs e) => await chooseColor(LineObject.Standard4);

      private async void TapGestureRecognizerStandard5_Tapped(object sender, EventArgs e) => await chooseColor(LineObject.Standard5);

      private async void TapGestureRecognizerMarked_Tapped(object sender, EventArgs e) => await chooseColor(LineObject.Marked);

      private async void TapGestureRecognizerLiveTracking_Tapped(object sender, EventArgs e) => await chooseColor(LineObject.LiveTracking);

      private async void TapGestureRecognizerEditable_Tapped(object sender, EventArgs e) => await chooseColor(LineObject.Editable);

      private async void TapGestureRecognizerInEdit_Tapped(object sender, EventArgs e) => await chooseColor(LineObject.InEdit);

      private async void TapGestureRecognizerSelectedPart_Tapped(object sender, EventArgs e) => await chooseColor(LineObject.SelectedPart);

      private async void TapGestureRecognizerHelperLine_Tapped(object sender, EventArgs e) => await chooseColor(LineObject.HelperLine);

      async Task chooseColor(LineObject lineobj) {
         try {
            Color orgcol = Color.White;
            switch (lineobj) {
               case LineObject.Standard1:
                  orgcol = Standard1Color;
                  break;
               case LineObject.Standard2:
                  orgcol = Standard2Color;
                  break;
               case LineObject.Standard3:
                  orgcol = Standard3Color;
                  break;
               case LineObject.Standard4:
                  orgcol = Standard4Color;
                  break;
               case LineObject.Standard5:
                  orgcol = Standard5Color;
                  break;
               case LineObject.Marked:
                  orgcol = MarkedColor;
                  break;
               case LineObject.LiveTracking:
                  orgcol = LiveTrackingColor;
                  break;
               case LineObject.Editable:
                  orgcol = EditableColor;
                  break;
               case LineObject.InEdit:
                  orgcol = InEditColor;
                  break;
               case LineObject.SelectedPart:
                  orgcol = SelectedPartColor;
                  break;
               case LineObject.HelperLine:
                  orgcol = HelperLineColor;
                  break;
            }
            ColorChoosingPage page = new ColorChoosingPage() {
               ActualColor = orgcol,
            };
            page.EndWithOk += (object sender2, EventArgs e2) => {
               switch (lineobj) {
                  case LineObject.Standard1:
                     Standard1Color = page.ActualColor;
                     break;
                  case LineObject.Standard2:
                     Standard2Color = page.ActualColor;
                     break;
                  case LineObject.Standard3:
                     Standard3Color = page.ActualColor;
                     break;
                  case LineObject.Standard4:
                     Standard4Color = page.ActualColor;
                     break;
                  case LineObject.Standard5:
                     Standard5Color = page.ActualColor;
                     break;
                  case LineObject.Marked:
                     MarkedColor = page.ActualColor;
                     break;
                  case LineObject.LiveTracking:
                     LiveTrackingColor = page.ActualColor;
                     break;
                  case LineObject.Editable:
                     EditableColor = page.ActualColor;
                     break;
                  case LineObject.InEdit:
                     InEditColor = page.ActualColor;
                     break;
                  case LineObject.SelectedPart:
                     SelectedPartColor = page.ActualColor;
                     break;
                  case LineObject.HelperLine:
                     HelperLineColor = page.ActualColor;
                     break;
               }
            };
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await FSofTUtils.Xamarin.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      private async void chooseCacheDir_Clicked(object sender, EventArgs e) {
         try {
            ChooseFilePage chooseFilePage = new ChooseFilePage() {
               AndroidActivity = androidActivity,
               Path = CacheLocation,
               OnlyExistingDirectory = true,
               Title = "Cache-Verzeichnis auswählen",
            };
            chooseFilePage.ChooseFileReadyEvent += (object s, ChooseFile.ChoosePathAndFileEventArgs ea) => {
               if (ea.OK && CacheLocation != ea.Path) {
                  CacheLocation = ea.Path;
                  changed = true;
               }
            };

            await Navigation.PushAsync(chooseFilePage);
         } catch (Exception ex) {
            await UIHelper.ShowExceptionMessage(this,
                                                "Fehler beim Ermitteln des Cache-Verzeichnis",
                                                ex,
                                                null,
                                                false);
         }
      }

      private async void chooseDemDir_Clicked(object sender, EventArgs e) {
         try {
            ChooseFilePage chooseFilePage = new ChooseFilePage() {
               AndroidActivity = androidActivity,
               Path = DemPath,
               OnlyExistingDirectory = true,
               Title = "DEM-Verzeichnis auswählen",
            };
            chooseFilePage.ChooseFileReadyEvent += (object s, ChooseFile.ChoosePathAndFileEventArgs ea) => {
               if (ea.OK && DemPath != ea.Path) {
                  DemPath = ea.Path;
                  changed = true;
               }
            };

            await Navigation.PushAsync(chooseFilePage);
         } catch (Exception ex) {
            await UIHelper.ShowExceptionMessage(this,
                                                "Fehler beim Ermitteln des DEM-Verzeichnis",
                                                ex,
                                                null,
                                                false);
         }
      }

      #region Karten

      void setMapChanged() {
         changed = true;
         btnSave.IsEnabled = true;  // expliziet, da hier kein Propertie geändert wird
      }

      void mapMove(TreeView tv, MapTreeViewHelper.MapMove move) {
         if (tv.SelectedNode != null) {
            TreeViewNode nodeSelected = tv.SelectedNode;
            tv.SelectedNode = null;
            switch (move) {
               case MapTreeViewHelper.MapMove.Left: MapTreeViewHelper.treeNodeCollectionMoveLeft(nodeSelected); break;
               case MapTreeViewHelper.MapMove.Right: MapTreeViewHelper.treeNodeCollectionMoveRight(nodeSelected, null); break;
               case MapTreeViewHelper.MapMove.Up: MapTreeViewHelper.treeNodeCollectionMoveUp(nodeSelected); break;
               case MapTreeViewHelper.MapMove.Down: MapTreeViewHelper.treeNodeCollectionMoveDown(nodeSelected); break;
            }
            tv.SelectedNode = nodeSelected;
            setMapChanged();
         }
      }

      private void TapGestureRecognizerMapMoveUp_Tapped(object sender, EventArgs e) => mapMove(tvMaps, MapTreeViewHelper.MapMove.Up);

      private void TapGestureRecognizerMapMoveDown_Tapped(object sender, EventArgs e) => mapMove(tvMaps, MapTreeViewHelper.MapMove.Down);

      private void TapGestureRecognizerMapMoveLeft_Tapped(object sender, EventArgs e) => mapMove(tvMaps, MapTreeViewHelper.MapMove.Left);

      private void TapGestureRecognizerMapMoveRight_Tapped(object sender, EventArgs e) => mapMove(tvMaps, MapTreeViewHelper.MapMove.Right);

      private async void TapGestureRecognizerMapRemove_Tapped(object sender, EventArgs e) {
         if (tvMaps.SelectedNode != null) {
            TreeViewNode nodeSelected = tvMaps.SelectedNode;
            if (await FSofTUtils.Xamarin.Helper.MessageBox(this,
                                "Löschen",
                                MapTreeViewHelper.IsMapGroupNode(nodeSelected) ?
                                    "Soll die Kartengruppe '" + nodeSelected.Text + "' gelöscht werden?" :
                                    "Soll die Karte '" + nodeSelected.Text + "' gelöscht werden?",
                                "ja", "nein")) {
               tvMaps.SelectedNode = null;
               if (nodeSelected.ParentNode != null)
                  nodeSelected.ParentNode.RemoveChildNode(nodeSelected);
               else
                  nodeSelected.TreeView.RemoveChildNode(nodeSelected);
               setMapChanged();
            }
         }
      }

      private void TapGestureRecognizerMapProps_Tapped(object sender, EventArgs e) {
         if (tvMaps.SelectedNode != null) {
            TreeViewNode node = tvMaps.SelectedNode;
            if (MapTreeViewHelper.IsMapGroupNode(node)) {

               EditGroupnamePage page = new EditGroupnamePage(node.Text);
               page.Disappearing += (s, ea) => {
                  if (page.Ok) {
                     node.Text = page.Groupname;
                     setMapChanged();
                  }
               };
               Navigation.PushAsync(page);

            } else {

               MapProviderDefinition mpd = MapTreeViewHelper.GetMapProviderDefinition(node);
               EditMapDefPage page = new EditMapDefPage(mpd, false, androidActivity);
               page.Disappearing += (s, ea) => {
                  if (page.Ok) {
                     node.Text = page.MapProviderDefinition.MapName;
                     MapTreeViewHelper.SetMapProviderDefinition(node, page.MapProviderDefinition);
                     setMapChanged();
                  }
               };
               Navigation.PushAsync(page);

            }
         }
      }

      private void TapGestureRecognizerMapNew_Tapped(object sender, EventArgs e) {
         MapProviderDefinition mpd = new MapProviderDefinition("neue Karte", GMap.NET.MapProviders.GMapProviders.OpenStreetMap.Name);
         EditMapDefPage page = new EditMapDefPage(mpd, true, androidActivity);
         page.Disappearing += async (s, ea) => {
            if (page.Ok) {
               try {
                  mpd = page.MapProviderDefinition;
                  TreeViewNode node = new TreeViewNode(mpd.MapName);
                  MapTreeViewHelper.SetMapProviderDefinition(node, page.MapProviderDefinition);

                  TreeViewNode nodeSelected = tvMaps.SelectedNode;
                  if (nodeSelected != null) {
                     if (MapTreeViewHelper.IsMapGroupNode(nodeSelected)) {
                        nodeSelected.Expanded = true;
                        nodeSelected.AddChildNode(node);
                     } else {
                        int idx = MapTreeViewHelper.GetNodeIndex(nodeSelected);
                        if (nodeSelected.ParentNode == null)
                           tvMaps.InsertChildNode(idx + 1, node);
                        else
                           nodeSelected.ParentNode.InsertChildNode(idx + 1, node);
                     }
                  } else
                     tvMaps.AddChildNode(node);

                  tvMaps.SelectedNode = node;
                  tvMaps.EnsureVisibleNode(node);
                  setMapChanged();

               } catch (Exception ex) {
                  await UIHelper.ShowExceptionMessage(this, "Fehler", ex, null, false);
               }
            }
         };
         Navigation.PushAsync(page);
      }

      private void TapGestureRecognizerMapGroupNew_Tapped(object sender, EventArgs e) {
         TreeViewNode newnode = tvMaps.InsertChildNode(0, "neue Kartengruppe");       // immer an 1. Pos. in der Hauptgruppe
         newnode.Expanded = true;
         tvMaps.SelectedNode = newnode;
         tvMaps.EnsureVisibleNode(newnode);
         setMapChanged();
      }

      #endregion

      private async void btnSave_Clicked(object sender, EventArgs e) {
         ConfigChanged = changed;
         SaveButtonPressed = true;
         await Navigation.PopAsync();     // diese Seite sofort schließen
      }

   }
}