using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrackEddi.Common;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrackEddi {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class GPXSearchPage : ContentPage {

      MainPage mainPage;
      GpxWorkbench gpxWorkbench;
      List<string> gpxFilenames;
      List<Label> lblList;


      public GPXSearchPage(MainPage mainpage,
                           GpxWorkbench gpxworkbench,
                           List<string> gpxfilenames) {
         InitializeComponent();
         mainPage = mainpage;
         gpxWorkbench = gpxworkbench;
         gpxFilenames = gpxfilenames;
         lblList = new List<Label>();
      }

      protected override void OnAppearing() {
         base.OnAppearing();
         foreach (string filename in gpxFilenames)
            master.Children.Add(createControl4Filename(filename));
      }

      Label createControl4Filename(string filename) {
         Label label = new Label();
         label.Text = filename;
         label.TextColor = Color.DarkBlue;
         label.FontSize = Device.GetNamedSize(NamedSize.Medium, label);

         var tapGestureRecognizer = new TapGestureRecognizer();
         tapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped1;
         label.GestureRecognizers.Add(tapGestureRecognizer);

         lblList.Add(label);

         return label;
      }

      private async void TapGestureRecognizer_Tapped1(object sender, EventArgs e) {
         Label label = sender as Label;
         int idx = lblList.IndexOf(label);
         if (idx >= 0) {
            string filename = gpxFilenames[idx];
            master.Children.RemoveAt(idx);
            gpxFilenames.RemoveAt(idx);
            lblList.RemoveAt(idx);
            await load(filename);
         }
      }

      async Task load(string filename) => await mainPage.Loadfile2gpxworkbench(gpxWorkbench, filename, true);

   }
}