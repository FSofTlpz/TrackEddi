using SpecialMapCtrl;
using System;
using System.Collections.Generic;
//using System.Drawing;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrackEddi {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class EditTrackPage : ContentPage {

      /// <summary>
      /// Dialog mit Auswahl beendet
      /// </summary>
      public event EventHandler<EventArgs> EndWithOk;

      public Track Track { get; protected set; }

      public Color TrackColor {
         get => new Color(Track.LineColor.R / 255.0,
                          Track.LineColor.G / 255.0,
                          Track.LineColor.B / 255.0,
                          Track.LineColor.A / 255.0);
         set {
            System.Drawing.Color col = value;
            if (col != Track.LineColor) {
               Track.LineColor = value; // XamarinColor2WinColor(value); Interessanterweise erfolgt die Umwandlung automatisch!
               OnPropertyChanged(nameof(TrackColor));
            }
         }
      }

      public string Name {
         get => Track.GpxTrack.Name;
         set => Track.GpxTrack.Name = value;
      }

      public string Description {
         get => Track.GpxTrack.Description;
         set => Track.GpxTrack.Description = value;
      }

      public string Comment {
         get => Track.GpxTrack.Comment;
         set => Track.GpxTrack.Comment = value;
      }

      public string Source {
         get => Track.GpxTrack.Source;
         set => Track.GpxTrack.Source = value;
      }

      public string Length {
         get => Track.Length() < 1000 ?
                     Track.Length().ToString() + " m" :
                     (Track.Length() / 1000).ToString("f1") + " km";
      }

      public ImageSource ElevationProfile {
         get {
            System.Drawing.Bitmap bm = WinHelper.BuildImage4Track(1200, 900, Track, new List<int>());
            pictdata = WinHelper.GetImageSource4WindowsBitmap(bm, out ImageSource ims);
            return ims;
         }
      }

      byte[] pictdata;

      public string StatisticalInfo {
         get => Track.GetSimpleStatsText().Trim();
      }


      public EditTrackPage(Track track) {
         InitializeComponent();

         Track = track;
         BindingContext = this;
      }

      async private void Button_Clicked(object sender, EventArgs e) {
         EndWithOk?.Invoke(this, EventArgs.Empty);
         await Navigation.PopAsync();     // diese Seite sofort schließen
      }

      async private void TapGestureRecognizerColor_Tapped(object sender, EventArgs e) {
         try {
            ColorChoosingPage page = new ColorChoosingPage() {
               ActualColor = Track.LineColor,
            };
            page.EndWithOk += (object sender2, EventArgs e2) => {
               TrackColor = page.ActualColor;
            };
            await Navigation.PushAsync(page);
         } catch (Exception ex) {
            await FSofTUtils.Xamarin.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

   }
}