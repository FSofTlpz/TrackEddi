using FSofTUtils.Geography.Garmin;
using SpecialMapCtrl;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrackEddi {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class EditMarkerPage : ContentPage {

      /// <summary>
      /// Dialog mit Auswahl beendet
      /// </summary>
      public event EventHandler<EventArgs> EndWithOk;


      public Marker Marker { get; protected set; }

      IList<GarminSymbol> garminSymbols;

      public string Name {
         get => Marker.Waypoint.Name;
         set => Marker.Waypoint.Name = value;
      }

      public string Description {
         get => Marker.Waypoint.Description;
         set => Marker.Waypoint.Description = value;
      }

      public string Comment {
         get => Marker.Waypoint.Comment;
         set => Marker.Waypoint.Comment = value;
      }

      public string UTC {
         get => Marker.Waypoint.Time == FSofTUtils.Geography.PoorGpx.BaseElement.NOTUSE_TIME ||
                Marker.Waypoint.Time == FSofTUtils.Geography.PoorGpx.BaseElement.NOTVALID_TIME ?
                     "" :
                     Marker.Waypoint.Time.ToString("G") + " Uhr";
      }

      public string Elevation {
         get => Marker.Waypoint.Elevation == FSofTUtils.Geography.PoorGpx.BaseElement.NOTUSE_DOUBLE||
                Marker.Waypoint.Elevation == FSofTUtils.Geography.PoorGpx.BaseElement.NOTVALID_DOUBLE ?
                     "" :
                     Marker.Waypoint.Elevation.ToString() + " m";
      }

      public string Longitude {
         get => Marker.Waypoint.Lon >= 0 ?
                     Marker.Waypoint.Lon.ToString("f6") + "° E" :
                     (-Marker.Waypoint.Lon).ToString("f6") + "° W";
      }

      public string Latitude {
         get => Marker.Waypoint.Lat >= 0 ?
                     Marker.Waypoint.Lat.ToString("f6") + "° N" :
                     (-Marker.Waypoint.Lat).ToString("f6") + "° S";
      }

      public ImageSource Picture { get; protected set; }

      /// <summary>
      /// akt. Bilddaten für <see cref="Picture"/>
      /// </summary>
      byte[] pictdata;

      string[] proposals = null;


      public EditMarkerPage(Marker marker, IList<GarminSymbol> garminSymbols, IList<string> proposals = null) {
         InitializeComponent();
         this.Marker = marker;
         this.garminSymbols = garminSymbols;

         pictdata = WinHelper.GetImageSource4WindowsBitmap(Marker.Bitmap, out ImageSource picture);
         Picture = picture;

         if (proposals == null || proposals.Count == 0) {
            PickerProposals.IsVisible = false;
         } else {
            this.proposals = new string[proposals.Count];
            proposals.CopyTo(this.proposals, 0);
            PickerProposals.ItemsSource = this.proposals;
            Name = this.proposals[0];
         }

         BindingContext = this;
      }

      async private void ButtonSave_Clicked(object sender, EventArgs e) {
         EndWithOk?.Invoke(this, EventArgs.Empty);
         await Navigation.PopAsync();     // diese Seite sofort schließen
      }

      async private void TapGestureRecognizerSymbol_Tapped(object sender, EventArgs e) {
         try {
            if (garminSymbols != null && garminSymbols.Count > 0) {
               SymbolChoosingPage page = new SymbolChoosingPage(garminSymbols, Marker.Waypoint.Symbol);
               page.EndWithOk += (object sender2, EventArgs e2) => {
                  Marker.Waypoint.Symbol = page.ActualGarminSymbol.Name;
                  pictdata = WinHelper.GetImageSource4WindowsBitmap(Marker.Bitmap, out ImageSource picture);
                  Picture = picture;
                  this.OnPropertyChanged(nameof(Picture));
               };
               await Navigation.PushAsync(page);
            }
         } catch (Exception ex) {
            await FSofTUtils.Xamarin.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      private void PickerProposals_SelectedIndexChanged(object sender, EventArgs e) {
         if (PickerProposals.SelectedItem != null) {
            EntryName.Text = PickerProposals.SelectedItem.ToString();   // das Setzen von "Name" wird NICHT in die Anzeige übernommen
            PickerProposals.SelectedItem = null;
         }
      }
   }
}