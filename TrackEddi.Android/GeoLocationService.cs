using Android.App;
using Android.Content;
using Android.OS;

namespace TrackEddi.Droid {

   [Service]
   public class GeoLocationService : Service {

      /// <summary>
      /// wird in <see cref="OnCreate"/> erzeugt und registriert und in <see cref="OnDestroy"/> deaktiviert und deregistriert
      /// <para>wird in <see cref="OnStartCommand(Intent, StartCommandFlags, int)"/> aktiviert</para>
      /// </summary>
      LocationTracker locationTracker;

      public static bool IsStarted { get; protected set; } = false;


      #region Service-Interface

      /// <summary>
      /// The system invokes this method by calling bindService() when another component wants to bind with the service (such as to perform RPC). 
      /// In your implementation of this method, you must provide an interface that clients use to communicate with the service by returning an IBinder. 
      /// You must always implement this method; however, if you don't want to allow binding, you should return null.
      /// </summary>
      /// <param name="intent"></param>
      /// <returns></returns>
      public override IBinder OnBind(Intent intent) {
         return null;
      }

      #endregion

      /// <summary>
      /// The system invokes this method by calling startService() when another component (such as an activity) requests that the service be started. 
      /// When this method executes, the service is started and can run in the background indefinitely. If you implement this, it is your responsibility 
      /// to stop the service when its work is complete by calling stopSelf() or stopService(). If you only want to provide binding, you don't need to 
      /// implement this method.
      /// </summary>
      /// <param name="intent"></param>
      /// <param name="flags"></param>
      /// <param name="startId"></param>
      /// <returns></returns>
      public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId) {
         // normale Notification erzeugen ...
         Notification notif = NotificationHelper.CreateInfoNotification("Die Positionsbestimmung ist jetzt eingeschaltet", "TrackEddi");

         // ... und starten
         StartForeground(NotificationHelper.InfoNotificationID, notif);

         IsStarted = true;

         locationTracker.ActivateTracking(GeoLocationServiceCtrl.GetUpdateIntervallMS(), GeoLocationServiceCtrl.GetMinDistance());

         /*
START_NOT_STICKY        If the system kills the service after onStartCommand() returns, do not recreate the service unless there are pending intents to deliver. 
                        This is the safest option to avoid running your service when not necessary and when your application can simply restart any unfinished jobs.
START_STICKY            If the system kills the service after onStartCommand() returns, recreate the service and call onStartCommand(), but do not redeliver the last intent. 
                        Instead, the system calls onStartCommand() with a null intent unless there are pending intents to start the service. 
                        In that case, those intents are delivered. This is suitable for media players (or similar services) that are not executing commands 
                        but are running indefinitely and waiting for a job.
START_REDELIVER_INTENT  If the system kills the service after onStartCommand() returns, recreate the service and call onStartCommand() with the last intent that 
                        was delivered to the service. Any pending intents are delivered in turn. This is suitable for services that are actively performing a job 
                        that should be immediately resumed, such as downloading a file.
         */
         return StartCommandResult.Sticky;
      }

      /// <summary>
      /// The system invokes this method to perform one-time setup procedures when the service is initially created (before it calls either onStartCommand() or onBind()). 
      /// If the service is already running, this method is not called.
      /// </summary>
      public override void OnCreate() {
         locationTracker = new LocationTracker(this);
         locationTracker.LocationChanged += GeoLocationServiceCtrl.TrackerLocationChanged;

         base.OnCreate();
      }

      /// <summary>
      /// The system invokes this method when the service is no longer used and is being destroyed. Your service should implement this to clean up any resources 
      /// such as threads, registered listeners, or receivers. This is the last call that the service receives.
      /// </summary>
      public override void OnDestroy() {
         locationTracker.LocationChanged -= GeoLocationServiceCtrl.TrackerLocationChanged;
         locationTracker.DeactivateTracking();

         NotificationHelper.RemoveLocationIsOnNotification();

         IsStarted = false;

         base.OnDestroy();
      }

   }
}