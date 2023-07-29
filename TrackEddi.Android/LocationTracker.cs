//#define FUSEDLOCATION

using Android.Content;
#if FUSEDLOCATION
using Android.Gms.Location;
#else
using Android.OS;
#endif
using Android.Locations;
using System;

namespace TrackEddi.Droid {

   /// <summary>
   /// zur Ermittlung der akt. Position
   /// <para>Sowohl der LocationManager von Android als auch Xamarin.Essentials.Location liefern z.B. (auch in einem Service) keine Daten mehr, 
   /// wenn die App nicht mehr auf dem Bilschirm sichtbar ist. Aktuell scheint der FusedLocationProviderClient aus den GooglePlayServices
   /// die einzige Möglichkeit zu bieten, ein Tracking auch im Hintergund zu ermöglichen.</para>
   /// </summary>
   internal class LocationTracker
#if !FUSEDLOCATION
                                  : Java.Lang.Object, ILocationListener
#endif
      {

      /// <summary>
      /// ungültiger Wert
      /// </summary>
      public const double NOTVALID_DOUBLE = double.MinValue;

      public event EventHandler<LocationChangedArgs> LocationChanged;

#if FUSEDLOCATION
      FusedLocationProviderClient fusedLocationProviderClient;

      LocationCallback locationCallback = null;
#else
      LocationManager locationManager = null;
#endif

      bool isActiv = false;


      public LocationTracker(Context context) {
#if FUSEDLOCATION
         fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(context);
#else
         locationManager = (LocationManager)context.GetSystemService(Context.LocationService);
         //IList<string> acceptableLocationProviders = locationManager.GetProviders(false);
#endif
      }

      /// <summary>
      /// akt. das Tracking
      /// <para>Requests location updates with the given request and results delivered to the given callback on the specified Looper. 
      /// A previous request for location updates for the same callback will be replaced by this request. 
      /// If the location request has a priority higher than Priority.PRIORITY_PASSIVE, a wakelock may be held on the client's behalf while delivering locations. 
      /// A wakelock will not be held while delivering availability updates.</para>
      /// </summary>
      /// <param name="priority"><para>PRIORITY_BALANCED_POWER_ACCURACY - Requests a tradeoff that is balanced between location accuracy and power usage.</para>
      /// <para>PRIORITY_HIGH_ACCURACY - Requests a tradeoff that favors highly accurate locations at the possible expense of additional power usage.</para>
      /// <para>PRIORITY_LOW_POWER - Requests a tradeoff that favors low power usage at the possible expense of location accuracy.</para>
      /// <para>PRIORITY_PASSIVE - Ensures that no extra power will be used to derive locations. This enforces that the request will act as a passive listener that will only receive "free" locations calculated on behalf of other clients, and no locations will be calculated on behalf of only this request.</para>
      /// </param>
      /// <param name="intervalmillies"></param>
      public void ActivateTracking(long intervalmillies = 1000, float mindistance = 1) {

#if FUSEDLOCATION
         if (fusedLocationProviderClient != null) {
            if (isActiv)
               DeactivateTracking();

            Android.Gms.Location.LocationRequest req = Android.Gms.Location.LocationRequest.Create();
            req.SetPriority(Priority.PriorityHighAccuracy);
            //req.SetPriority(Priority.PriorityBalancedPowerAccuracy);
            req.SetInterval(intervalmillies);
            req.SetSmallestDisplacement(mindistance);

            locationCallback = new LocationCallback();
            locationCallback.LocationResult += locationCallback_LocationResult;
            fusedLocationProviderClient.RequestLocationUpdates(req, locationCallback, null);

            isActiv = true;
         }
#else
         if (locationManager != null) {
            if (isActiv)
               DeactivateTracking();

            /*
               LocationManager.FusedProvider
                     Standard name of the fused location provider.
                     If present, this provider may combine inputs from several other location providers to provide the best possible location fix. 
                     It is implicitly used for all requestLocationUpdates APIs that involve a Criteria.
                     Constant Value: "fused"                
            
               LocationManager.GpsProvider
                     Standard name of the GNSS location provider.
                     If present, this provider determines location using GNSS satellites. The responsiveness and accuracy of location fixes may depend 
                     on GNSS signal conditions.
                     Locations returned from this provider are with respect to the primary GNSS antenna position within the device. getGnssAntennaInfos() may be used 
                     to determine the GNSS antenna position with respect to the Android Coordinate System, and convert between them if necessary. This is generally 
                     only necessary for high accuracy applications.
                     The extras Bundle for locations derived by this location provider may contain the following key/value pairs:
                         satellites - the number of satellites used to derive the fix 
                     Constant Value: "gps" 

               LocationManager.NetworkProvider
                     Standard name of the network location provider.
                     If present, this provider determines location based on nearby of cell tower and WiFi access points. 
                     Operation of this provider may require a data connection.
                     Constant Value: "network"                
            
               LocationManager.PassiveProvider      A special location provider for receiving locations without actively initiating a location fix.
                     A special location provider for receiving locations without actively initiating a location fix. This location provider is always present.
                     This provider can be used to passively receive location updates when other applications or services request them without actually requesting 
                     the locations yourself. This provider will only return locations generated by other providers.
                     Constant Value: "passive" 
             */
            locationManager.RequestLocationUpdates(
               LocationManager.NetworkProvider,
               intervalmillies,
               mindistance,
               this);

            locationManager.RequestLocationUpdates(
               LocationManager.GpsProvider,
               intervalmillies,
               mindistance,
               this);

            isActiv = true;
         }
#endif
      }

      /// <summary>
      /// stopt das Tracking
      /// </summary>
      public void DeactivateTracking() {
         if (isActiv) {
#if FUSEDLOCATION
            fusedLocationProviderClient.RemoveLocationUpdates(locationCallback);
            locationCallback.Dispose();
            locationCallback = null;
#else
            locationManager?.RemoveUpdates(this);
#endif
            isActiv = false;
         }
      }

#if FUSEDLOCATION
      void locationCallback_LocationResult(object sender, LocationCallbackResultEventArgs e) {
         if (e.Result != null)
            foreach (var item in e.Result.Locations)
               LocationChanged?.Invoke(this, Convert(item));
      }
#endif

      static Xamarin.Essentials.Location Convert(Location location) =>
         new Xamarin.Essentials.Location(location.Latitude, location.Longitude) {
            Altitude = location.HasAltitude && location.Altitude != double.MaxValue ? location.Altitude : NOTVALID_DOUBLE,
            Course = location.HasBearing ? location.Bearing : NOTVALID_DOUBLE,
            Speed = location.HasSpeed && location.Speed != double.MaxValue ? location.Speed : NOTVALID_DOUBLE,
            Accuracy = location.HasAccuracy ? location.Accuracy : NOTVALID_DOUBLE,
            VerticalAccuracy = location.HasVerticalAccuracy ? location.VerticalAccuracyMeters : NOTVALID_DOUBLE,
            Timestamp = System.DateTimeOffset.FromUnixTimeMilliseconds(location.Time),
         };

#if !FUSEDLOCATION
      #region Interface Android.Locations.ILocationListener 

      Location lastLoction = null;

      public void OnLocationChanged(Location location) {
         if (lastLoction != null)
            if (location.Time == lastLoction.Time &&
                location.Longitude == lastLoction.Longitude &&
                location.Longitude == lastLoction.Longitude &&
                location.Provider == lastLoction.Provider)
               return;
         lastLoction = location;

         LocationChanged?.Invoke(this,
                                 new LocationChangedArgs() {
                                    Location = Convert(location),
                                    Provider = location.Provider,
                                 });
      }

      public void OnProviderDisabled(string provider) { }

      public void OnProviderEnabled(string provider) { }

      void ILocationListener.OnStatusChanged(string provider, Availability status, Bundle extras) { }

      #endregion
#endif

   }
}