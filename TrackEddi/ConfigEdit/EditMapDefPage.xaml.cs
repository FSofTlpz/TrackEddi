using FSofTUtils.Xamarin.Page;
using GMap.NET.CoreExt.MapProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TrackEddi.Common;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrackEddi.ConfigEdit {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class EditMapDefPage : ContentPage {

      public MapProviderDefinition MapProviderDefinition { get; protected set; }

      public bool Ok { get; protected set; } = false;

      bool isNewMapProviderDefinition = false;

      List<string> providernames = new List<string>();

      object androidActivity = null;

      enum FileTypeRegister {
         KMZ,
         TDB,
         TYP,
      }


      public EditMapDefPage(MapProviderDefinition definition, bool newmap, object androidactivity) {
         InitializeComponent();

         androidActivity = androidactivity;
         isNewMapProviderDefinition = newmap;
         init(definition);
      }

      void init(MapProviderDefinition definition) {
         if (definition is GarminProvider.GarminMapDefinitionData)
            MapProviderDefinition = new GarminProvider.GarminMapDefinitionData(definition as GarminProvider.GarminMapDefinitionData);
         else if (definition is GarminKmzProvider.KmzMapDefinition)
            MapProviderDefinition = new GarminKmzProvider.KmzMapDefinition(definition as GarminKmzProvider.KmzMapDefinition);
         else if (definition is WMSProvider.WMSMapDefinition)
            MapProviderDefinition = new WMSProvider.WMSMapDefinition(definition as WMSProvider.WMSMapDefinition);
         else
            MapProviderDefinition = new MapProviderDefinition(definition);

         providernames.Add(GarminProvider.Instance.Name);
         providernames.Add(GarminKmzProvider.Instance.Name);
         providernames.Add(WMSProvider.Instance.Name);
         foreach (var item in GMap.NET.MapProviders.GMapProviders.List) {
            providernames.Add(item.Name);
         }

         pickerProvider.ItemsSource = providernames;
         pickerProvider.SelectedItem = MapProviderDefinition.ProviderName;
         entryMapname.Text = MapProviderDefinition.MapName;
         entryZoomFrom.Text = MapProviderDefinition.MinZoom.ToString();
         entryZoomTo.Text = MapProviderDefinition.MaxZoom.ToString();
         entryDisplayzoom.Text = MapProviderDefinition.Zoom4Display.ToString();

         pickerProvider.IsEnabled = isNewMapProviderDefinition;
         entryMapname.IsReadOnly = false; // !IsNewMapProviderDefinition;

         cbHillshading.IsEnabled = true;
         entryHillshadingalpha.IsEnabled = true;

         if (MapProviderDefinition is GarminProvider.GarminMapDefinitionData) {

            GarminProvider.GarminMapDefinitionData specmpd = MapProviderDefinition as GarminProvider.GarminMapDefinitionData;
            lblTdbFile.Text = specmpd.TDBfile[0];
            lblTypFile.Text = specmpd.TYPfile[0];
            entryTextFactor.Text = isNewMapProviderDefinition ? "1" : specmpd.TextFactor.ToString();
            entrySymbolFactor.Text = isNewMapProviderDefinition ? "1" : specmpd.SymbolFactor.ToString();
            entryLineFactor.Text = isNewMapProviderDefinition ? "1" : specmpd.LineFactor.ToString();
            cbHillshading.IsChecked = isNewMapProviderDefinition ? false : specmpd.HillShading;
            entryHillshadingalpha.Text = isNewMapProviderDefinition ? "100" : specmpd.HillShadingAlpha.ToString();
            entryTextFactor.IsEnabled =
            entrySymbolFactor.IsEnabled =
            entryLineFactor.IsEnabled =
            entryHillshadingalpha.IsEnabled = true;
            cbHillshading.IsEnabled = true;

         } else if (MapProviderDefinition is GarminKmzProvider.KmzMapDefinition) {

            GarminKmzProvider.KmzMapDefinition specmpd = MapProviderDefinition as GarminKmzProvider.KmzMapDefinition;
            lblKmzFile.Text = specmpd.KmzFile;
            cbHillshading.IsChecked = isNewMapProviderDefinition ? false : specmpd.HillShading;
            entryHillshadingalpha.Text = isNewMapProviderDefinition ? "100" : specmpd.HillShadingAlpha.ToString();
            cbHillshading.IsEnabled = true;
            entryHillshadingalpha.IsEnabled = true;

         } else if (MapProviderDefinition is WMSProvider.WMSMapDefinition) {

            WMSProvider.WMSMapDefinition specmpd = MapProviderDefinition as WMSProvider.WMSMapDefinition;
            entryUrl.Text = specmpd.URL;
            entryVersion.Text = specmpd.Version;
            entrySrs.Text = specmpd.SRS;
            pickerWmsPictFormat.SelectedItem = specmpd.PictureFormat.ToUpper();
            entryLayer.Text = specmpd.Layer;
            entryExtParams.Text = specmpd.ExtendedParameters;

         }
      }

      private async void ChooseKmzFile_Clicked(object sender, EventArgs e) {
         await registerFile(FileTypeRegister.KMZ);
      }

      private async void ChooseTdbFile_Clicked(object sender, EventArgs e) {
         await registerFile(FileTypeRegister.TDB);
      }

      private async void ChooseTypFile_Clicked(object sender, EventArgs e) {
         await registerFile(FileTypeRegister.TYP);
      }

      private void pickerProvider_SelectedIndexChanged(object sender, EventArgs e) {
         frameGarmin.IsVisible = false;
         frameGarminKmz.IsVisible = false;
         frameWms.IsVisible = false;
         frameHillshading.IsVisible = false;

         string providername = (sender as Picker).SelectedItem.ToString();
         if (providername == GarminProvider.Instance.Name) {
            frameGarmin.IsVisible = true;
            frameHillshading.IsVisible = true;

            entryTextFactor.Text = "1";
            entrySymbolFactor.Text = "1";
            entryLineFactor.Text = "1";
            cbHillshading.IsChecked = false;
            entryHillshadingalpha.Text = "100";

         } else if (providername == GarminKmzProvider.Instance.Name) {
            frameGarminKmz.IsVisible = true;
            frameHillshading.IsVisible = true;

            cbHillshading.IsChecked = false;
            entryHillshadingalpha.Text = "100";

         } else if (providername == WMSProvider.Instance.Name)
            frameWms.IsVisible = true;
      }

      async private Task registerFile(FileTypeRegister type) {
         string typ = type == FileTypeRegister.TDB ? "TDB" :
                      type == FileTypeRegister.TYP ? "TYP" :
                                                     "KMZ";
         try {
            string path = type == FileTypeRegister.TDB ? lblTdbFile.Text :
                          type == FileTypeRegister.TYP ? lblTypFile.Text :
                                                         lblKmzFile.Text;

            if (string.IsNullOrEmpty(path)) {
               if (type == FileTypeRegister.TDB)
                  path = lblTypFile.Text;
               else if (type == FileTypeRegister.TYP)
                  path = lblTdbFile.Text;
            }
            path = path != "" ? Path.GetDirectoryName(path) : "";
            // allerletzter Versuch:
            if (string.IsNullOrEmpty(path))
               path = ChooseFilePage.LastChoosedPath;

            ChooseFilePage chooseFilePage = new ChooseFilePage() {
               AndroidActivity = androidActivity,
               Path = path,
               Filename = "",
               OnlyExistingFile = true,   // ohne Eingabefeld für Namen
               Match4Filenames = new System.Text.RegularExpressions.Regex(@"\.(" + typ.ToLower() + ")$", System.Text.RegularExpressions.RegexOptions.IgnoreCase),
               Title = typ + "-Datei auswählen",
            };

            chooseFilePage.ChooseFileReadyEvent += async (object sender, FSofTUtils.Xamarin.Control.ChooseFile.ChoosePathAndFileEventArgs e) => {
               if (e.OK) {
                  string fullfilename = Path.Combine(e.Path, e.Filename);
                  switch (type) {
                     case FileTypeRegister.KMZ: lblKmzFile.Text = fullfilename; break;
                     case FileTypeRegister.TDB: lblTdbFile.Text = fullfilename; break;
                     case FileTypeRegister.TYP: lblTypFile.Text = fullfilename; break;
                  }
               }
            };

            await Navigation.PushAsync(chooseFilePage);
         } catch (Exception ex) {
            await UIHelper.ShowExceptionMessage(this,
                                                "Fehler beim Ermitteln der " + typ + "-Datei",
                                                ex,
                                                null,
                                                false);
         }
      }

      async Task showError(string message) {
         await FSofTUtils.Xamarin.Helper.MessageBox(this, "Fehler", message);
      }

      bool hasText(Entry ctrl) => hasText(ctrl.Text);

      bool hasText(Label ctrl) => hasText(ctrl.Text);

      bool hasText(string txt) => !string.IsNullOrEmpty(txt) && txt.Trim().Length > 0;

      double getDouble(Entry ctrl) => string.IsNullOrEmpty(ctrl.Text) ? 0 : Convert.ToDouble(ctrl.Text);

      int getInt(Entry ctrl) => string.IsNullOrEmpty(ctrl.Text) ? 0 : Convert.ToInt32(ctrl.Text);

      string getText(Entry ctrl) => string.IsNullOrEmpty(ctrl.Text) ? "" : ctrl.Text.Trim();

      string getText(Label ctrl) => string.IsNullOrEmpty(ctrl.Text) ? "" : ctrl.Text.Trim();

      string getText(Picker ctrl) => ctrl.SelectedItem == null ? "" : ctrl.SelectedItem.ToString().Trim();

      private async void btnSave_Clicked(object sender, EventArgs e) {
         string providername = getText(pickerProvider);

         if (!hasText(providername)) {
            await showError("Ein Kartenprovider muss ausgewählt sein.");
            return;
         }
         if (!hasText(entryMapname)) {
            await showError("Ein Kartenname muss angegeben sein.");
            return;
         }

         if (providername == GarminProvider.Instance.Name) {
            if (!hasText(lblTdbFile)) {
               await showError("Eine TDB-Datei muss angegeben sein.");
               return;
            }
            if (!hasText(lblTypFile)) {
               await showError("Eine TYP-Datei muss angegeben sein.");
               return;
            }
         } else if (providername == GarminKmzProvider.Instance.Name) {
            if (!hasText(lblKmzFile)) {
               await showError("Eine KMZ-Datei muss angegeben sein.");
               return;
            }
         } else if (providername == WMSProvider.Instance.Name) {
            if (!hasText(entryUrl)) {
               await showError("Eine URL muss angegeben sein.");
               return;
            }
            if (!hasText(entrySrs)) {
               await showError("Eine SRS (Koordinatensystem) muss angegeben sein.");
               return;
            }
            if (!hasText(entryVersion)) {
               await showError("Eine WMS-Version muss angegeben sein.");
               return;
            }
         }

         Ok = true;

         // ACHTUNG!  Wenn bestimmte Daten geändert werden muss DbIdDelta neu ermittelt werden, d.h. eine neue Def. ist nötig!
         if (!isNewMapProviderDefinition) {
            if (providername == GarminProvider.Instance.Name) {
               if (getText(entryMapname) != MapProviderDefinition.MapName ||
                   getText(lblTdbFile) != (MapProviderDefinition as GarminProvider.GarminMapDefinitionData).TDBfile[0] ||
                   getText(lblTypFile) != (MapProviderDefinition as GarminProvider.GarminMapDefinitionData).TYPfile[0]) {
                  isNewMapProviderDefinition = true;
               }
            } else if (providername == GarminKmzProvider.Instance.Name) {
               if (getText(entryMapname) != MapProviderDefinition.MapName ||
                   getText(lblKmzFile) != (MapProviderDefinition as GarminKmzProvider.KmzMapDefinition).KmzFile) {
                  isNewMapProviderDefinition = true;
               }
            } else if (providername == WMSProvider.Instance.Name) {
               if (getText(entryMapname) != MapProviderDefinition.MapName ||
                   getText(entryLayer) != (MapProviderDefinition as WMSProvider.WMSMapDefinition).Layer ||
                   getText(entryUrl) != (MapProviderDefinition as WMSProvider.WMSMapDefinition).URL ||
                   getText(entrySrs) != (MapProviderDefinition as WMSProvider.WMSMapDefinition).SRS ||
                   getText(entryVersion) != (MapProviderDefinition as WMSProvider.WMSMapDefinition).Version ||
                   getText(pickerWmsPictFormat) != (MapProviderDefinition as WMSProvider.WMSMapDefinition).PictureFormat ||
                   getText(entryExtParams) != (MapProviderDefinition as WMSProvider.WMSMapDefinition).ExtendedParameters) {
                  isNewMapProviderDefinition = true;
               }
            }
         }

         // Übernahme der Werte nach MapProviderDefinition bzw. Erzeugung einer neuen MapProviderDefinition

         if (isNewMapProviderDefinition) {

            if (providername == GarminProvider.Instance.Name) {
               MapProviderDefinition = new GarminProvider.GarminMapDefinitionData(
                                                getText(entryMapname),
                                                getDouble(entryDisplayzoom),
                                                getInt(entryZoomFrom),
                                                getInt(entryZoomTo),
                                                new string[] {
                                                   getText(lblTdbFile),
                                                },
                                                new string[] {
                                                   getText(lblTypFile),
                                                },
                                                getDouble(entryTextFactor),
                                                getDouble(entryLineFactor),
                                                getDouble(entrySymbolFactor),
                                                cbHillshading.IsChecked,
                                                (byte)getInt(entryHillshadingalpha));
            } else if (providername == GarminKmzProvider.Instance.Name) {
               MapProviderDefinition = new GarminKmzProvider.KmzMapDefinition(
                                                getText(entryMapname),
                                                getDouble(entryDisplayzoom),
                                                getInt(entryZoomFrom),
                                                getInt(entryZoomTo),
                                                getText(lblKmzFile),
                                                cbHillshading.IsChecked,
                                                (byte)getInt(entryHillshadingalpha));
            } else if (providername == WMSProvider.Instance.Name) {
               MapProviderDefinition = new WMSProvider.WMSMapDefinition(
                                                getText(entryMapname),
                                                getDouble(entryDisplayzoom),
                                                getInt(entryZoomFrom),
                                                getInt(entryZoomTo),
                                                getText(entryLayer),
                                                getText(entryUrl),
                                                getText(entrySrs),
                                                getText(entryVersion),
                                                getText(pickerWmsPictFormat),
                                                getText(entryExtParams));
            } else {
               MapProviderDefinition = new MapProviderDefinition();
               MapProviderDefinition.ProviderName = providername;
               MapProviderDefinition.MapName = getText(entryMapname);
               MapProviderDefinition.MinZoom = getInt(entryZoomFrom);
               MapProviderDefinition.MaxZoom = getInt(entryZoomTo);
               MapProviderDefinition.Zoom4Display = getDouble(entryDisplayzoom);
            }

         } else {       // nur Daten verändert

            MapProviderDefinition.ProviderName = providername;
            MapProviderDefinition.MapName = getText(entryMapname);
            MapProviderDefinition.MinZoom = getInt(entryZoomFrom);
            MapProviderDefinition.MaxZoom = getInt(entryZoomTo);
            MapProviderDefinition.Zoom4Display = getDouble(entryDisplayzoom);

            if (MapProviderDefinition is GarminProvider.GarminMapDefinitionData) {

               GarminProvider.GarminMapDefinitionData specmpd = MapProviderDefinition as GarminProvider.GarminMapDefinitionData;
               specmpd.TDBfile[0] = getText(lblTdbFile);
               specmpd.TYPfile[0] = getText(lblTypFile);
               specmpd.TextFactor = getDouble(entryTextFactor);
               specmpd.SymbolFactor = getDouble(entrySymbolFactor);
               specmpd.LineFactor = getDouble(entryLineFactor);
               specmpd.HillShading = cbHillshading.IsChecked;
               specmpd.HillShadingAlpha = (byte)getInt(entryHillshadingalpha);

            } else if (MapProviderDefinition is GarminKmzProvider.KmzMapDefinition) {

               GarminKmzProvider.KmzMapDefinition specmpd = MapProviderDefinition as GarminKmzProvider.KmzMapDefinition;
               specmpd.KmzFile = getText(lblKmzFile);
               specmpd.HillShading = cbHillshading.IsChecked;
               specmpd.HillShadingAlpha = (byte)getInt(entryHillshadingalpha);

            } else if (MapProviderDefinition is WMSProvider.WMSMapDefinition) {

               WMSProvider.WMSMapDefinition specmpd = MapProviderDefinition as WMSProvider.WMSMapDefinition;
               specmpd.URL = getText(entryUrl);
               specmpd.Version = getText(entryVersion);
               specmpd.SRS = getText(entrySrs);
               specmpd.PictureFormat = getText(pickerWmsPictFormat);
               specmpd.Layer = getText(entryLayer);
               specmpd.ExtendedParameters = getText(entryExtParams);

            }

         }

         await Navigation.PopAsync();     // diese Seite sofort schließen
      }

   }
}