using System.Threading;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrackEddi {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class GeoLocationPage : ContentPage {

      GeoLocation geoLocation;

      Thread t;

      public static BindableProperty GeoProviderNameProperty = BindableProperty.Create(
         nameof(GeoProviderName), typeof(string), typeof(GeoLocationPage), "");

      public string GeoProviderName {
         get => GetValue(GeoProviderNameProperty) as string;
         set => SetValue(GeoProviderNameProperty, value);
      }

      public static BindableProperty GeoLocationTimeProperty = BindableProperty.Create(
         nameof(GeoLocationTime), typeof(string), typeof(GeoLocationPage), "");

      public string GeoLocationTime {
         get => GetValue(GeoLocationTimeProperty) as string;
         set => SetValue(GeoLocationTimeProperty, value);
      }

      public static BindableProperty GeoLongitudeProperty = BindableProperty.Create(
         nameof(GeoLongitude), typeof(string), typeof(GeoLocationPage), "");

      public string GeoLongitude {
         get => GetValue(GeoLongitudeProperty) as string;
         set => SetValue(GeoLongitudeProperty, value);
      }

      public static BindableProperty GeoLatitudeProperty = BindableProperty.Create(
         nameof(GeoLatitude), typeof(string), typeof(GeoLocationPage), "");

      public string GeoLatitude {
         get => GetValue(GeoLatitudeProperty) as string;
         set => SetValue(GeoLatitudeProperty, value);
      }

      public static BindableProperty GeoElevationProperty = BindableProperty.Create(
         nameof(GeoElevation), typeof(string), typeof(GeoLocationPage), "");

      public string GeoElevation {
         get => GetValue(GeoElevationProperty) as string;
         set => SetValue(GeoElevationProperty, value);
      }

      public static BindableProperty GeoCourseProperty = BindableProperty.Create(
         nameof(GeoCourse), typeof(string), typeof(GeoLocationPage), "");

      public string GeoCourse {
         get => GetValue(GeoCourseProperty) as string;
         set => SetValue(GeoCourseProperty, value);
      }

      public static BindableProperty GeoSpeedProperty = BindableProperty.Create(
         nameof(GeoSpeed), typeof(string), typeof(GeoLocationPage), "");

      public string GeoSpeed {
         get => GetValue(GeoSpeedProperty) as string;
         set => SetValue(GeoSpeedProperty, value);
      }

      public static BindableProperty GeoAccuracyProperty = BindableProperty.Create(
         nameof(GeoAccuracy), typeof(string), typeof(GeoLocationPage), "");

      public string GeoAccuracy {
         get => GetValue(GeoAccuracyProperty) as string;
         set => SetValue(GeoAccuracyProperty, value);
      }

      public static BindableProperty GeoAltitudeReferenceSystemProperty = BindableProperty.Create(
         nameof(GeoAltitudeReferenceSystem), typeof(string), typeof(GeoLocationPage), "");

      public string GeoAltitudeReferenceSystem {
         get => GetValue(GeoAltitudeReferenceSystemProperty) as string;
         set => SetValue(GeoAltitudeReferenceSystemProperty, value);
      }


      public GeoLocationPage(GeoLocation geoLocation) {
         InitializeComponent();
         this.geoLocation = geoLocation;
         BindingContext = this;
         refreshData();
      }

      protected override void OnAppearing() {
         base.OnAppearing();
         t = new Thread(timer);
         t.Start();
      }

      protected override void OnDisappearing() {
         base.OnDisappearing();
         t.Abort();
      }

      void timer() {
         do {
            refreshData();
            Thread.Sleep(1000);
         } while (true);
      }

      void refreshData() {
         Location location = geoLocation.GetLastLocation(out string provider);

         GeoProviderName = provider != null ? provider : "";
         if (location != null) {
            GeoLocationTime = location.Timestamp.ToLocalTime().ToString("G");
            GeoLongitude = location.Longitude >= 0 ? location.Longitude.ToString("f6") + "° E" : (-location.Longitude).ToString("f6") + "° W";
            GeoLatitude = location.Latitude >= 0 ? location.Latitude.ToString("f6") + "° N" : (-location.Latitude).ToString("f6") + "° S";
            GeoElevation = location.Altitude != null ? location.Altitude.Value.ToString("f1") + "m" : "";
            GeoCourse = location.Course != null ? location.Course.Value.ToString("f0") + "°" : "";
            GeoSpeed = location.Speed != null ? location.Speed.Value.ToString("f1") + "m/s" : "";
            GeoAccuracy = (location.Accuracy != null ? ("H " + location.Accuracy.Value.ToString("f1") + "m") : "") + " / " +
                          (location.VerticalAccuracy != null ? ("V " + location.VerticalAccuracy.Value.ToString("f1") + "m") : "");
            /*
               Ellipsoid   Das Höhenbezugssystem basiert auf einem Ellipsoid (in der Regel WGS84), das eine mathematische Näherung der Erdform darstellt.
               Geoid       Das Höhenbezugssystem basiert auf der Entfernung über dem Meeresspiegel (parametrisiert durch ein sogenanntes Geoid).
               Surface     Das Höhenbezugssystem basiert auf der Entfernung oberhalb der höchsten Oberflächenstrukturen wie Gebäude, Bäume, Straßen usw., über Gelände- oder Bodenniveau.
               Terrain     Das Höhenreferenzsystem basiert auf der Entfernung über Gelände- oder Bodenniveau.
               Unspecified Das Höhenreferenzsystem wurde nicht angegeben.
             */
            GeoAltitudeReferenceSystem = location.AltitudeReferenceSystem.ToString();
         } else {
            GeoLocationTime = "";
            GeoLongitude = "";
            GeoLatitude = "";
            GeoElevation = "";
            GeoCourse = "";
            GeoSpeed = "";
            GeoAccuracy = "";
            GeoAltitudeReferenceSystem = "";
         }
      }

   }
}