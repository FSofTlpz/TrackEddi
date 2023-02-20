using GMap.NET.CoreExt.MapProviders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrackEddi {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class ConfigPage : ContentPage {

      /// <summary>
      /// Map-Control
      /// </summary>
      SpecialMapCtrl.SpecialMapCtrl map;

      Config configedit;

      Config configorg;

      bool changed = false;

      public double Zoom4Display {
         get => configedit.GetZoom4Display(0);
         set {
            double v = Math.Max(0.1F, value);
            if (v != configedit.GetZoom4Display(0)) {
               for (int p = 0; p < configedit.Provider.Length; p++)
                  configedit.SetZoom4Display(p, v);
               changed = true;
               onPropertyChanged(nameof(Zoom4Display));
            }
         }
      }

      public double SymbolZoomfactor {
         get => configedit.SymbolZoomfactor;
         set {
            double v = Math.Max(0.1F, value);
            if (v != configedit.SymbolZoomfactor) {
               configedit.SymbolZoomfactor = v;
               changed = true;
               onPropertyChanged(nameof(SymbolZoomfactor));
            }
         }
      }

      #region DEM

      public int MinZoom4DEM {
         get => configedit.DemMinZoom;
         set {
            int v = Math.Min(Math.Max(1, value), 24);
            if (v != configedit.DemMinZoom) {
               configedit.DemMinZoom = v;
               changed = true;
               onPropertyChanged(nameof(MinZoom4DEM));
            }
         }
      }

      public string Path4DEM {
         get => configedit.DemPath;
         set {
            string v = value.Trim();
            if (v != configedit.DemPath) {
               configedit.DemPath = v;
               OnPropertyChanged(nameof(Path4DEM));
            }
         }
      }

      #endregion


      #region Linienfarben

      public Color Standard1Color {
         get => new Color(configedit.StandardTrackColor.R / 255.0,
                          configedit.StandardTrackColor.G / 255.0,
                          configedit.StandardTrackColor.B / 255.0,
                          configedit.StandardTrackColor.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != configedit.StandardTrackColor) {
               configedit.StandardTrackColor = value; // XamarinColor2WinColor(value); Interessanterweise erfolgt die Umwandlung automatisch!
               onPropertyChanged(nameof(Standard1Color));
            }
         }
      }

      public Color Standard2Color {
         get => new Color(configedit.StandardTrackColor2.R / 255.0,
                          configedit.StandardTrackColor2.G / 255.0,
                          configedit.StandardTrackColor2.B / 255.0,
                          configedit.StandardTrackColor2.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != configedit.StandardTrackColor2) {
               configedit.StandardTrackColor2 = value;
               onPropertyChanged(nameof(Standard2Color));
            }
         }
      }

      public Color Standard3Color {
         get => new Color(configedit.StandardTrackColor3.R / 255.0,
                          configedit.StandardTrackColor3.G / 255.0,
                          configedit.StandardTrackColor3.B / 255.0,
                          configedit.StandardTrackColor3.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != configedit.StandardTrackColor3) {
               configedit.StandardTrackColor3 = value;
               onPropertyChanged(nameof(Standard3Color));
            }
         }
      }

      public Color Standard4Color {
         get => new Color(configedit.StandardTrackColor4.R / 255.0,
                          configedit.StandardTrackColor4.G / 255.0,
                          configedit.StandardTrackColor4.B / 255.0,
                          configedit.StandardTrackColor4.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != configedit.StandardTrackColor4) {
               configedit.StandardTrackColor4 = value;
               onPropertyChanged(nameof(Standard4Color));
            }
         }
      }

      public Color Standard5Color {
         get => new Color(configedit.StandardTrackColor5.R / 255.0,
                          configedit.StandardTrackColor5.G / 255.0,
                          configedit.StandardTrackColor5.B / 255.0,
                          configedit.StandardTrackColor5.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != configedit.StandardTrackColor5) {
               configedit.StandardTrackColor5 = value;
               onPropertyChanged(nameof(Standard5Color));
            }
         }
      }

      public Color MarkedColor {
         get => new Color(configedit.MarkedTrackColor.R / 255.0,
                          configedit.MarkedTrackColor.G / 255.0,
                          configedit.MarkedTrackColor.B / 255.0,
                          configedit.MarkedTrackColor.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != configedit.MarkedTrackColor) {
               configedit.MarkedTrackColor = value;
               onPropertyChanged(nameof(MarkedColor));
            }
         }
      }

      public Color EditableColor {
         get => new Color(configedit.EditableTrackColor.R / 255.0,
                          configedit.EditableTrackColor.G / 255.0,
                          configedit.EditableTrackColor.B / 255.0,
                          configedit.EditableTrackColor.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != configedit.EditableTrackColor) {
               configedit.EditableTrackColor = value;
               onPropertyChanged(nameof(EditableColor));
            }
         }
      }

      public Color InEditColor {
         get => new Color(configedit.InEditTrackColor.R / 255.0,
                          configedit.InEditTrackColor.G / 255.0,
                          configedit.InEditTrackColor.B / 255.0,
                          configedit.InEditTrackColor.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != configedit.InEditTrackColor) {
               configedit.InEditTrackColor = value;
               onPropertyChanged(nameof(InEditColor));
            }
         }
      }

      public Color SelectedPartColor {
         get => new Color(configedit.SelectedPartTrackColor.R / 255.0,
                          configedit.SelectedPartTrackColor.G / 255.0,
                          configedit.SelectedPartTrackColor.B / 255.0,
                          configedit.SelectedPartTrackColor.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != configedit.SelectedPartTrackColor) {
               configedit.SelectedPartTrackColor = value;
               onPropertyChanged(nameof(SelectedPartColor));
            }
         }
      }

      public Color HelperLineColor {
         get => new Color(configedit.HelperLineColor.R / 255.0,
                          configedit.HelperLineColor.G / 255.0,
                          configedit.HelperLineColor.B / 255.0,
                          configedit.HelperLineColor.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != configedit.HelperLineColor) {
               configedit.HelperLineColor = value;
               onPropertyChanged(nameof(HelperLineColor));
            }
         }
      }

      #endregion

      #region Linienbreiten

      public float Standard1Width {
         get => configedit.StandardTrackWidth;
         set {
            float v = Math.Max(0.1F, value);
            if (v != configedit.StandardTrackWidth) {
               configedit.StandardTrackWidth = v;
               onPropertyChanged(nameof(Standard1Width));
            }
         }
      }

      public float Standard2Width {
         get => configedit.StandardTrackWidth2;
         set {
            float v = Math.Max(0.1F, value);
            if (v != configedit.StandardTrackWidth2) {
               configedit.StandardTrackWidth2 = v;
               onPropertyChanged(nameof(Standard2Width));
            }
         }
      }

      public float Standard3Width {
         get => configedit.StandardTrackWidth3;
         set {
            float v = Math.Max(0.1F, value);
            if (v != configedit.StandardTrackWidth3) {
               configedit.StandardTrackWidth3 = v;
               onPropertyChanged(nameof(Standard3Width));
            }
         }
      }

      public float Standard4Width {
         get => configedit.StandardTrackWidth4;
         set {
            float v = Math.Max(0.1F, value);
            if (v != configedit.StandardTrackWidth4) {
               configedit.StandardTrackWidth4 = v;
               onPropertyChanged(nameof(Standard4Width));
            }
         }
      }

      public float Standard5Width {
         get => configedit.StandardTrackWidth5;
         set {
            float v = Math.Max(0.1F, value);
            if (v != configedit.StandardTrackWidth5) {
               configedit.StandardTrackWidth5 = v;
               onPropertyChanged(nameof(Standard5Width));
            }
         }
      }

      public float MarkedWidth {
         get => configedit.MarkedTrackWidth;
         set {
            float v = Math.Max(0.1F, value);
            if (v != configedit.MarkedTrackWidth) {
               configedit.MarkedTrackWidth = v;
               onPropertyChanged(nameof(MarkedWidth));
            }
         }
      }

      public float EditableWidth {
         get => configedit.EditableTrackWidth;
         set {
            float v = Math.Max(0.1F, value);
            if (v != configedit.EditableTrackWidth) {
               configedit.EditableTrackWidth = v;
               onPropertyChanged(nameof(EditableWidth));
            }
         }
      }

      public float InEditWidth {
         get => configedit.InEditTrackWidth;
         set {
            float v = Math.Max(0.1F, value);
            if (v != configedit.InEditTrackWidth) {
               configedit.InEditTrackWidth = v;
               onPropertyChanged(nameof(InEditWidth));
            }
         }
      }

      public float SelectedPartWidth {
         get => configedit.SelectedPartTrackWidth;
         set {
            float v = Math.Max(0.1F, value);
            if (v != configedit.SelectedPartTrackWidth) {
               configedit.SelectedPartTrackWidth = v;
               onPropertyChanged(nameof(SelectedPartWidth));
            }
         }
      }

      public float HelperLineWidth {
         get => configedit.HelperLineWidth;
         set {
            float v = Math.Max(0.1F, value);
            if (v != configedit.HelperLineWidth) {
               configedit.HelperLineWidth = v;
               onPropertyChanged(nameof(HelperLineWidth));
            }
         }
      }

      #endregion

      public class ListViewObjectItem : INotifyPropertyChanged {

         public event PropertyChangedEventHandler PropertyChanged;

         MapProviderDefinition mpd;

         public string Name {
            get => mpd.MapName + " (" + mpd.ProviderName + ")";
            set {

            }
         }


         public ListViewObjectItem(MapProviderDefinition def) {
            mpd = def;
         }

         /// <summary>
         /// zum Auslösen eines <see cref="PropertyChanged"/>-Events (auch "extern")
         /// </summary>
         /// <param name="propname"></param>
         public void Notify4PropChanged(string propname) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
         }

      }


      public Command<ListViewObjectItem> MapDelete { get; private set; }

      public Command<ListViewObjectItem> MapMoveDown { get; private set; }

      public Command<ListViewObjectItem> MapMoveUp { get; private set; }


      readonly ObservableCollection<ListViewObjectItem> maplst;




      enum LineObject {
         Standard1,
         Standard2,
         Standard3,
         Standard4,
         Standard5,
         Marked,
         Editable,
         InEdit,
         SelectedPart,
         HelperLine,
      }


      public ConfigPage(SpecialMapCtrl.SpecialMapCtrl map, Config config) {
         InitializeComponent();
         this.map = map;
         configorg = config;
         configedit = new Config(config.XmlFilename, null);
         BindingContext = this;

         //MapDelete = new Command<ListViewObjectItem>(onMapDelete);
         //MapMoveUp = new Command<ListViewObjectItem>(onMapMoveUp);
         //MapMoveDown = new Command<ListViewObjectItem>(onMapMoveDown);

         maplst = new ObservableCollection<ListViewObjectItem>();
         //buildMapList();
      }

      protected async override void OnDisappearing() {
         base.OnDisappearing();

         if (changed)
            if (await FSofTUtils.Xamarin.Helper.MessageBox(this,
                                            "speichern",
                                            "Änderungen der Konfiguration speichern?",
                                            "ja", "nein"))
               btnSave_Clicked(null, null);
      }

      private async void btnCacheDelete_Clicked(object sender, EventArgs e) {
         if (map.SpecMapActualMapIdx >= 0) {
            if (await FSofTUtils.Xamarin.Helper.MessageBox(this,
                                               "Cache löschen",
                                               "Cache für die Karte '" + map.SpecMapProviderDefinitions[map.SpecMapActualMapIdx].MapName + "' löschen?",
                                               "ja", "nein")) {
               map.SpecMapClearMemoryCache();
               await FSofTUtils.Xamarin.Helper.MessageBox(this,
                                                          "Cache gelöscht",
                                                          map.SpecMapClearCache(map.SpecMapActualMapIdx) + " Kartenteile gelöscht");
               map.SpecMapRefresh(true, false, false);
            }
         }
      }

      private async void btnCacheDeleteAll_Clicked(object sender, EventArgs e) {
         if (map.SpecMapActualMapIdx >= 0) {
            if (await FSofTUtils.Xamarin.Helper.MessageBox(this,
                                               "Cache löschen",
                                               "Cache für ALLE Karten löschen?",
                                               "ja", "nein")) {
               map.SpecMapClearMemoryCache();
               await FSofTUtils.Xamarin.Helper.MessageBox(this,
                                                          "Cache gelöscht",
                                                          map.SpecMapClearCache() + " Kartenteile gelöscht");
               map.SpecMapRefresh(true, true, false);
            }
         }
      }

      private async void TapGestureRecognizerStandard1_Tapped(object sender, EventArgs e) {
         await chooseColor(LineObject.Standard1);
      }

      private async void TapGestureRecognizerStandard2_Tapped(object sender, EventArgs e) {
         await chooseColor(LineObject.Standard2);
      }

      private async void TapGestureRecognizerStandard3_Tapped(object sender, EventArgs e) {
         await chooseColor(LineObject.Standard3);
      }

      private async void TapGestureRecognizerStandard4_Tapped(object sender, EventArgs e) {
         await chooseColor(LineObject.Standard4);
      }

      private async void TapGestureRecognizerStandard5_Tapped(object sender, EventArgs e) {
         await chooseColor(LineObject.Standard5);
      }

      private async void TapGestureRecognizerMarked(object sender, EventArgs e) {
         await chooseColor(LineObject.Marked);
      }

      private async void TapGestureRecognizerEditable_Tapped(object sender, EventArgs e) {
         await chooseColor(LineObject.Editable);
      }

      private async void TapGestureRecognizerInEdit_Tapped(object sender, EventArgs e) {
         await chooseColor(LineObject.InEdit);
      }

      private async void TapGestureRecognizerSelectedPart_Tapped(object sender, EventArgs e) {
         await chooseColor(LineObject.SelectedPart);
      }

      private async void TapGestureRecognizerHelperLine_Tapped(object sender, EventArgs e) {
         await chooseColor(LineObject.HelperLine);
      }

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
      private void btnSave_Clicked(object sender, EventArgs e) {
         configedit.SaveData();
         configedit.Load(configedit.XmlFilename);
         configorg.Load(configedit.XmlFilename);
      }
      void onPropertyChanged(string propname) {
         changed = true;
         btnSave.IsEnabled = true;
         OnPropertyChanged(propname);
      }


      //void buildMapList() {
      //   foreach (var item in map.SpecMapProviderDefinitions) {
      //      maplst.Add(new ListViewObjectItem(item));
      //   }
      //   ListViewMaps.ItemsSource = maplst;
      //}

      //async void onMapDelete(ListViewObjectItem td) {
      //   if (await FSofTUtils.Xamarin.Helper.MessageBox(this,
      //                                                  "Achtung",
      //                                                  "Soll die die Karte '" + td.Name + "' wirklich entfernt werden?",
      //                                                  "ja", "nein")) {
      //      List<MapProviderDefinition> maps = map.SpecMapProviderDefinitions;
      //      int idx = -1;
      //      for (int i = 0; i < maps.Count; i++) {
      //         if (maps[i].MapName == td.Name) {
      //            idx = i;
      //            break;
      //         }
      //      }
      //      if (idx >= 0) {
      //         //configedit.Provider

      //      }
      //   }
      //}

      //void onMapMoveUp(ListViewObjectItem td) {
      //   int idx = maplst.IndexOf(td);
      //   if (0 < idx) {
      //      maplst.RemoveAt(idx);
      //      maplst.Insert(idx - 1, td);
      //      ListViewMaps.ScrollTo(td, ScrollToPosition.MakeVisible, true);
      //   }
      //}

      //void onMapMoveDown(ListViewObjectItem td) {
      //   int idx = maplst.IndexOf(td);
      //   if (0 <= idx && idx < maplst.Count - 1) {
      //      maplst.RemoveAt(idx);
      //      maplst.Insert(idx + 1, td);
      //      ListViewMaps.ScrollTo(td, ScrollToPosition.MakeVisible, true);
      //   }
      //}



   }
}