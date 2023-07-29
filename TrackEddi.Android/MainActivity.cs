using Android;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using System;
using System.Threading.Tasks;

namespace TrackEddi.Droid {
   // Groß-/Kleinschreibung für Buttons: siehe .\*.Android\Resources\values\styles.xml

   // zusätzlich: LaunchMode = LaunchMode.SingleTask,

   [Activity(Label = "TrackEddi",
             Icon = "@mipmap/icon",
             Theme = "@style/MainTheme",
             MainLauncher = true,
             LaunchMode = LaunchMode.SingleTask,
             ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
   [IntentFilter(new[] { Android.Content.Intent.ActionView },
      Categories = new[] {
         Android.Content.Intent.CategoryDefault,
         Android.Content.Intent.CategoryBrowsable
      },
      DataSchemes = new[] { "file", "content" },
      DataHost = "*",
      DataMimeType = "*/*",
      DataPathPattern = @"/.*\\.gpx")]
   [IntentFilter(new[] { Android.Content.Intent.ActionView },
      Categories = new[] {
         Android.Content.Intent.CategoryDefault,
         Android.Content.Intent.CategoryBrowsable
      },
      DataSchemes = new[] { "file", "content" },
      DataHost = "*",
      DataMimeType = "*/*",
      DataPathPattern = @"/.*\\.kml")]
   [IntentFilter(new[] { Android.Content.Intent.ActionView },
      Categories = new[] {
         Android.Content.Intent.CategoryDefault,
         Android.Content.Intent.CategoryBrowsable
      },
      DataSchemes = new[] { "file", "content" },
      DataHost = "*",
      DataMimeType = "*/*",
      DataPathPattern = @"/.*\\.kmz")]
   public partial class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {

      static object loglocker = new object();

      static string logfile = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
                                                     "TrackEddiErrorLog.txt");


      protected override void OnCreate(Bundle savedInstanceState) {
         //TabLayoutResource = Resource.Layout.Tabbar;
         //ToolbarResource = Resource.Layout.Toolbar;
         base.OnCreate(savedInstanceState);

         /* 
          * "https://nominatim.openstreetmap.org/search?q={0}&format=xml";
          * führt zu einer Exception:
          *       Error: TrustFailure (Authentication failed, see inner exception.)
          *       Ssl error:1000007d:SSL routines:OPENSSL_internal:CERTIFICATE_VERIFY_FAILED
          *       
          * deshalb:
         */
         System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

         Xamarin.Essentials.Platform.Init(this, savedInstanceState);
         global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
         //LoadApplication(new App());

         AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
         TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
         AndroidEnvironment.UnhandledExceptionRaiser += OnAndroidEnvironmentUnhandledExceptionRaiser;

         // zusätzlich
         try {
            onCreateExtend(savedInstanceState,
                           Build.VERSION.SdkInt < BuildVersionCodes.R ?  // < Android 10
                              new string[] {
                                 Manifest.Permission.AccessNetworkState,
                                 Manifest.Permission.WriteExternalStorage,    // u.a. für Cache
                                 Manifest.Permission.ReadExternalStorage,     // u.a. für Karten und Konfig.
                                 Manifest.Permission.AccessFineLocation,      // GPS-Standort            ACHTUNG: Dieses Recht muss ZUSÄTZLICH im Manifest festgelegt sein, sonst wird es NICHT angefordert!
                              } :
                              new string[] {
                                 Manifest.Permission.AccessNetworkState,
                                 //Manifest.Permission.WriteExternalStorage,    // u.a. für Cache                 UNNÖTIG!!!
                                 //Manifest.Permission.ReadExternalStorage,     // u.a. für Karten und Konfig.    UNNÖTIG!!!
                                 Manifest.Permission.AccessFineLocation,      // GPS-Standort            ACHTUNG: Dieses Recht muss ZUSÄTZLICH im Manifest festgelegt sein, sonst wird es NICHT angefordert!
                                 //Manifest.Permission.ManageExternalStorage,   // erst ab R vorhanden      HIER UNNÖTIG -> AndroidManifest.xml !!!
                                 Manifest.Permission.AccessBackgroundLocation,
                              }
                           );

         } catch (Exception ex) {
            ErrorLog(ex.Message);
         }
      }

      public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults) {
         Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

         // zusätzlich
         onRequestPermissionsResult(requestCode, permissions, grantResults);

         base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
      }

      private void OnAndroidEnvironmentUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs unhandledExceptionEventArgs) {
         var newExc = new Exception("OnAndroidEnvironmentUnhandledExceptionRaiser", unhandledExceptionEventArgs.Exception);
         LogUnhandledException(newExc);
      }

      private static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs) {
         var newExc = new Exception("TaskSchedulerOnUnobservedTaskException", unobservedTaskExceptionEventArgs.Exception);
         LogUnhandledException(newExc);
      }

      private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs) {
         var newExc = new Exception("CurrentDomainOnUnhandledException", unhandledExceptionEventArgs.ExceptionObject as Exception);
         LogUnhandledException(newExc);
      }

      static void LogUnhandledException(Exception exception) {
         ErrorLog(exception.ToString());
      }

      static void ErrorLog(string txt) {
         try {
            lock (loglocker) {
               System.IO.File.AppendAllText(logfile, DateTime.Now.ToString("G") + " " + txt + System.Environment.NewLine);
            }
         } catch { }
      }


   }
}