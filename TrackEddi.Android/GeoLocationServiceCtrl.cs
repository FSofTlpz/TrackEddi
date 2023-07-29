using Android.Content;
#if FUSEDLOCATION
using Android.Gms.Common;
#endif
using System;
using Xamarin.Essentials;

[assembly: Xamarin.Forms.Dependency(typeof(TrackEddi.Droid.GeoLocationServiceCtrl))]
namespace TrackEddi.Droid {

   /// <summary>
   /// Android-Interface zur Xamarin-Seite zum Steuern des <see cref="GeoLocationService"/> (Ein, Aus usw.)
   /// </summary>
   internal class GeoLocationServiceCtrl : IGeoLocationServiceCtrl {

      private static Android.Content.Context context = global::Android.App.Application.Context;

      public event EventHandler<LocationChangedArgs> LocationChanged;

      static GeoLocationServiceCtrl serviceCtrl = null;


      /// <summary>
      /// startet den Service
      /// </summary>
      /// <param name="updateintervall"></param>
      /// <param name="updatedistance"></param>
      /// <returns>true, wenn der Service läuft</returns>
      public bool StartService(int updateintervall = 1000, double updatedistance = 1) {
         if (
#if FUSEDLOCATION
             isGooglePlayServicesInstalled() &&
#endif
             !ServiceIsActive()) {

            serviceCtrl = this;
            SetUpdateIntervallMS(updateintervall);
            SetMinDistance((float)updatedistance);

            var myserviceintent = new Android.Content.Intent(context, Java.Lang.Class.FromType(typeof(GeoLocationService)));

            Android.Content.ComponentName componentName;
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
               // If the service is being started or is already running, the ComponentName of the actual service that was started is returned;
               // else if the service does not exist null is returned.
               // ACHTUNG:
               // Unlike the ordinary startService(android.content.Intent), this method can be used at any time, regardless of whether the app hosting the service
               // is in a foreground state.
               componentName = context.StartForegroundService(myserviceintent);  
            else
               componentName = context.StartService(myserviceintent);

            return componentName != null;
         }
         return false;
      }

      /// <summary>
      /// hält den Service an
      /// </summary>
      /// <returns>liefert true wenn kein Service (mehr) läuft</returns>
      public bool StopService() {
         if (ServiceIsActive()) {
            serviceCtrl = null;

            var intent = new Android.Content.Intent(context, typeof(GeoLocationService));

            bool result = context.StopService(intent); 

            NotificationHelper.RemoveLocationIsOnNotification();

            return result;
         }
         return true;
      }

      /// <summary>
      /// hier meldet der <see cref="LocationTracker"/> eine veränderte Position
      /// </summary>
      /// <param name="e"></param>
      public static void TrackerLocationChanged(object sender, LocationChangedArgs e) {
         if (serviceCtrl != null)
            serviceCtrl.LocationChanged?.Invoke(serviceCtrl, e);
      }

      public bool IsValid(double v) =>
         v != LocationTracker.NOTVALID_DOUBLE;

#if FUSEDLOCATION
      /// <summary>
      /// liefert true, wenn die GooglePlayServices installiert sind
      /// </summary>
      /// <returns></returns>
      bool isGooglePlayServicesInstalled() {
         var queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(context);
         if (queryResult == ConnectionResult.Success)
            return true;

         if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult)) {
            var errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
            System.Diagnostics.Debug.WriteLine(errorString);
         }

         return false;
      }
#endif

      public bool ServiceIsActive() =>
         GeoLocationService.IsStarted;

      #region Speichern/Lesen spezieller privater (Android-)Vars

      //const string SERVICEISACTIVE = "ServiceIsActive";

      ///// <summary>
      ///// 
      ///// </summary>
      ///// <param name="context">wenn null, dann wird automatisch der App-Context verwendet</param>
      ///// <returns></returns>
      //public static bool GetServiceIsActive(Context context = null) {
      //   if (context != null)
      //      return getPrivateData(context, SERVICEISACTIVE, false);
      //   else
      //      return getPrivateData(SERVICEISACTIVE, false);
      //}

      //public static void SetServiceIsActive(bool on = true, Context context = null) {
      //   if (context != null)
      //      setPrivateData(context, SERVICEISACTIVE, on);
      //   else
      //      setPrivateData(SERVICEISACTIVE, on);
      //}


      const string UPDATEINTERVALLMS = "UpdateIntervallMS";

      /// <summary>
      /// 
      /// </summary>
      /// <param name="context">wenn null, dann wird automatisch der App-Context verwendet</param>
      /// <returns></returns>
      public static int GetUpdateIntervallMS(Context context = null) {
         if (context != null)
            return getPrivateData(context, UPDATEINTERVALLMS, 0);
         else
            return getPrivateData(UPDATEINTERVALLMS, 0);
      }

      public static void SetUpdateIntervallMS(int ms, Context context = null) {
         if (context != null)
            setPrivateData(context, UPDATEINTERVALLMS, ms);
         else
            setPrivateData(UPDATEINTERVALLMS, ms);
      }


      const string MINDISTANCE = "MinDistance";

      /// <summary>
      /// 
      /// </summary>
      /// <param name="context">wenn null, dann wird automatisch der App-Context verwendet</param>
      /// <returns></returns>
      public static float GetMinDistance(Context context = null) {
         if (context != null)
            return getPrivateData(context, MINDISTANCE, 0F);
         else
            return getPrivateData(MINDISTANCE, 0F);
      }

      public static void SetMinDistance(float meter, Context context = null) {
         if (context != null)
            setPrivateData(context, MINDISTANCE, meter);
         else
            setPrivateData(MINDISTANCE, meter);
      }

      #endregion

      #region allg. Funktionen zum Speichern/Lesen privater (Android-)Vars

      const string PREFFILE = "servicevars";

      static void setPrivateData(string varname, bool value) {
         setPrivateData(context, varname, value);
      }

      static void setPrivateData(string varname, int value) {
         setPrivateData(context, varname, value);
      }

      static void setPrivateData(string varname, string value) {
         setPrivateData(context, varname, value);
      }

      static void setPrivateData(string varname, float value) {
         setPrivateData(context, varname, value);
      }

      static bool getPrivateData(string varname, bool defvalue) {
         return getPrivateData(context, varname, defvalue);
      }

      static int getPrivateData(string varname, int defvalue) {
         return getPrivateData(context, varname, defvalue);
      }

      static string getPrivateData(string varname, string defvalue) {
         return getPrivateData(context, varname, defvalue);
      }

      static float getPrivateData(string varname, float defvalue) {
         return getPrivateData(context, varname, defvalue);
      }


      static void setPrivateData(Context context, string varname, bool value) {
         var pref = context.GetSharedPreferences(PREFFILE, FileCreationMode.Private);
         var editor = pref.Edit();
         editor.PutBoolean(varname, value);
         editor.Apply();
      }

      static void setPrivateData(Context context, string varname, int value) {
         var pref = context.GetSharedPreferences(PREFFILE, FileCreationMode.Private);
         var editor = pref.Edit();
         editor.PutInt(varname, value);
         editor.Apply();
      }

      static void setPrivateData(Context context, string varname, string value) {
         var pref = context.GetSharedPreferences(PREFFILE, FileCreationMode.Private);
         var editor = pref.Edit();
         editor.PutString(varname, value);
         editor.Apply();
      }

      static void setPrivateData(Context context, string varname, float value) {
         var pref = context.GetSharedPreferences(PREFFILE, FileCreationMode.Private);
         var editor = pref.Edit();
         editor.PutFloat(varname, value);
         editor.Apply();
      }

      static bool getPrivateData(Context context, string varname, bool defvalue) {
         var pref = context.GetSharedPreferences(PREFFILE, FileCreationMode.Private);
         return pref.GetBoolean(varname, defvalue);
      }

      static int getPrivateData(Context context, string varname, int defvalue) {
         var pref = context.GetSharedPreferences(PREFFILE, FileCreationMode.Private);
         return pref.GetInt(varname, defvalue);
      }

      static string getPrivateData(Context context, string varname, string defvalue) {
         var pref = context.GetSharedPreferences(PREFFILE, FileCreationMode.Private);
         return pref.GetString(varname, defvalue);
      }

      static float getPrivateData(Context context, string varname, float defvalue) {
         var pref = context.GetSharedPreferences(PREFFILE, FileCreationMode.Private);
         return pref.GetFloat(varname, defvalue);
      }

      #endregion

   }

}