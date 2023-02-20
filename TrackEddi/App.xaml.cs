using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrackEddi {
   public partial class App : Application {

      public enum AppEvent {
         OnStart,
         OnSleep,
         OnResume,
      }

      MainPage mainPage;


      public App() {
         InitializeComponent();
      }

      public App(object androidactivity) : this() {
#if DEBUG
         string starttime= DateTime.Now.ToString("G");
#endif
         mainPage = new MainPage(androidactivity);
#if DEBUG
         mainPage.Log(starttime + " Start App()");
#endif
         MainPage = new NavigationPage(mainPage) {
            BarBackgroundColor = Color.FromRgb(0, 0.5, 0),
            BarTextColor = Color.White,
         };
#if DEBUG
         mainPage.Log("End App()");
#endif
      }

      protected override void OnStart() {
         // wird u.U. NICHT ausgelöst (z.B. wenn vom OS Rückfragen über Userrechte kommt)

         mainPage.AppEvent(AppEvent.OnStart);
      }

      protected override void OnSleep() {
         mainPage.AppEvent(AppEvent.OnSleep);
      }

      protected override void OnResume() {
         mainPage.AppEvent(AppEvent.OnResume);
      }
   }
}
