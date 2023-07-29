namespace TrackEddi {

   public class LocationChangedArgs {
      public Xamarin.Essentials.Location Location;
      public string Provider;
   }


   /// <summary>
   /// Interface zum Steuern des Service
   /// </summary>
   public interface IGeoLocationServiceCtrl {

      event System.EventHandler<LocationChangedArgs> LocationChanged;

      bool StartService(int updateintervall, double updatedistance);

      bool StopService();

      bool ServiceIsActive();

      bool IsValid(double v);

   }
}
