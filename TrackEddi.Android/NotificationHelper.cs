using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using System.Collections.Generic;

namespace TrackEddi.Droid {

   /// <summary>
   /// zum einfacheren Umgang mit den Notifications
   /// </summary>
   class NotificationHelper {

      private static Context appcontext = global::Android.App.Application.Context;

      const string NOTIFICATION_CHANNELNAME_INFO = "LocationChannel";

      class AppNotificationChannel {

         public readonly List<int> Icon = new List<int>() {
            Resource.Mipmap.location_on,
            Resource.Mipmap.location_off,
         };

         /// <summary>
         /// Name des Kanals (öffentlich sichtbar)
         /// </summary>
         public string ChannelName { get; protected set; }

         /// <summary>
         /// ID des Kanals
         /// </summary>
         public string ChannelID { get; protected set; }

         /// <summary>
         /// ID für die (einzige) Notification dieses Kanals
         /// </summary>
         public int NotificationID { get; protected set; }

         /// <summary>
         /// Builder u.a. zum Erzeugen einer Notification
         /// </summary>
         public NotificationCompat.Builder NotificationBuilder { get; protected set; }


         public AppNotificationChannel(string channelName, string channelID, int notificationID) {
            ChannelName = channelName;
            ChannelID = channelID;
            NotificationID = notificationID;
         }

         /// <summary>
         /// erzeugt einen <see cref="NotificationChannel"/> und registriert ihn im OS
         /// </summary>
         /// <param name="notificationManager"></param>
         /// <returns></returns>
         public NotificationChannel CreateAndRegisterChannel(NotificationManager notificationManager) {
            if (notificationManager != null) {
               notificationManager.DeleteNotificationChannel(ChannelID);

               NotificationChannel notificationChannel = createNotificationChannel(ChannelName, ChannelName);

               notificationManager.CreateNotificationChannel(notificationChannel);  // Kanal wird im System registriert
               return notificationChannel;
            }
            return null;
         }

         public Notification CreateNotification(string text, string title) {
            NotificationBuilder = createNotificationBuilder(ChannelName, text, title);
            return NotificationBuilder.Build();
         }

         NotificationChannel createNotificationChannel(string notificationchannel, string channelname) {
            NotificationChannel notificationChannel = new NotificationChannel(channels[notificationchannel].ChannelID, channelname, NotificationImportance.High);

            /* NotificationImportance:
               IMPORTANCE_MAX          Unused.
               IMPORTANCE_HIGH         Higher notification importance: shows everywhere, makes noise and peeks. May use full screen intents.
               IMPORTANCE_DEFAULT      Default notification importance: shows everywhere, makes noise, but does not visually intrude.
               IMPORTANCE_LOW          Low notification importance: Shows in the shade, and potentially in the status bar (see shouldHideSilentStatusBarIcons()), 
                                       but is not audibly intrusive.
               IMPORTANCE_MIN          Min notification importance: only shows in the shade, below the fold. This should not be used with Service#startForeground(int, Notification) 
                                       since a foreground service is supposed to be something the user cares about so it does not make semantic sense to mark its notification as 
                                       minimum importance. If you do this as of Android version Build.VERSION_CODES.O, the system will show a higher-priority notification about 
                                       your app running in the background.
               IMPORTANCE_NONE         A notification with no importance: does not show in the shade.
               IMPORTANCE_UNSPECIFIED  Value signifying that the user has not expressed an importance. This value is for persisting preferences, and should never be associated with 
                                       an actual notification.
             */

            // NotificationManager#IMPORTANCE_DEFAULT should have a sound. Only modifiable before the channel is submitted to 
            if (notificationchannel == NOTIFICATION_CHANNELNAME_INFO) {

               notificationChannel.SetSound(null, null);
               notificationChannel.EnableLights(false);         // Sets whether notifications posted to this channel should display notification lights, on devices that support that feature. 
               notificationChannel.LockscreenVisibility = NotificationVisibility.Secret;  // Sets whether notifications posted to this channel appear on the lockscreen or not, and if so, whether they appear in a redacted form. 
               notificationChannel.EnableVibration(false);     // Sets whether notification posted to this channel should vibrate.

            }

            notificationChannel.SetShowBadge(true);         // Sets whether notifications posted to this channel can appear as application icon badges in a Launcher. 

            return notificationChannel;
         }

         NotificationCompat.Builder createNotificationBuilder(string notificationchannel, string text, string title) {
#if WITHOUT_INTENT
         // Mit dieser einfachen Variante kann beim Tippen auf die Notification die App NICHT wieder gestartet werden!

         builder = new NotificationCompat.Builder(context, channelid)
                            //.SetSubText("ein längerer Subtext")         // This provides some additional information that is displayed in the notification. (hinter dem Progname)
                            .SetContentTitle(title)                     // Set the first line of text in the platform notification template. 
                            .SetContentText(text)                       // Set the second line of text in the platform notification template. 
                            .SetSmallIcon(Resource.Drawable.ic_mtrl_chip_close_circle)
                            .SetOngoing(true)                           // Set whether this is an "ongoing" notification. ???
                            .SetProgress(100, 35, false)                // Set the progress this notification represents.
                                                                        //.SetUsesChronometer(true)                   // Show the Notification When field as a stopwatch. 
                                                                        //.SetShowWhen(true)                          // Control whether the timestamp set with setWhen is shown in the content view. 
                                                                        //.SetWhen(?)                                 // Add a timestamp pertaining to the notification(usually the time the event occurred). 
                            .SetContentText(text)
                            .SetContentTitle(title);
#else
            var intent = new Intent(appcontext, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.SingleTop);
            intent.PutExtra("Title", "Message");

            var pendingIntent = PendingIntent.GetActivity(appcontext, 0, intent, PendingIntentFlags.UpdateCurrent);

            NotificationCompat.Builder builder = null;
            if (notificationchannel == NOTIFICATION_CHANNELNAME_INFO) {

               builder = new NotificationCompat.Builder(appcontext, notificationchannel)
                  .SetAutoCancel(true)
                                  .SetOngoing(false);                           // true -> kann vom Anwender nicht beseitigt werden
               builder.SetSmallIcon(Icon[0]);

            }
#endif
            // Building channel if API verion is 26 or above
            if (global::Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.O)
               builder.SetChannelId(ChannelID);


            //builder.SetUsesChronometer(true)                    // Show the Notification When field as a stopwatch. 
            //builder.SetShowWhen(true)                           // Control whether the timestamp set with setWhen is shown in the content view. 
            //builder.SetWhen(?)                                  // Add a timestamp pertaining to the notification(usually the time the event occurred). 
            //builder.SetSmallIcon(Resource.Drawable.ic_mtrl_chip_close_circle);
            builder.SetContentTitle(title);                     // Set the first line of text in the platform notification template. 
            builder.SetContentText(text);                       // Set the second line of text in the platform notification template. 
            builder.SetContentIntent(pendingIntent);
            //builder.SetGroup("mygroupkey");                    // fkt. wahrscheinlich nur bei gleichem Kanal

            return builder;
         }

         public override string ToString() {
            return string.Format("Name '{0}', ID '{1}', NotificationID '{2}'", ChannelName, ChannelID, NotificationID);
         }
      }

      private static NotificationManager notificationManager = null;

      private static Dictionary<string, AppNotificationChannel> channels = new Dictionary<string, AppNotificationChannel>() {
         {  NOTIFICATION_CHANNELNAME_INFO, new AppNotificationChannel(NOTIFICATION_CHANNELNAME_INFO, "LOCATION", 1000) },
      };


      /// <summary>
      /// Notification-ID für die normale Info-Notification
      /// </summary>
      public static int InfoNotificationID => 
         channels[NOTIFICATION_CHANNELNAME_INFO].NotificationID;

      /// <summary>
      /// Erzeugung der Info-Notification
      /// </summary>
      /// <param name="text"></param>
      /// <param name="title"></param>
      /// <returns></returns>
      public static Notification CreateInfoNotification(string text, string title) =>
         createNotification(NOTIFICATION_CHANNELNAME_INFO, text, title);

      /// <summary>
      /// entfernen einer ev. vorhandenen Location-Notification
      /// </summary>
      public static void RemoveLocationIsOnNotification() {
         removeAlarmNotification(NOTIFICATION_CHANNELNAME_INFO);
      }

      static void createAndRegisterChannels() {
         if (notificationManager == null) {
            notificationManager = appcontext.GetSystemService(Context.NotificationService) as NotificationManager;

            // Building channel if API verion is 26 or above
            if (global::Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.O)
               foreach (var item in channels)
                  item.Value.CreateAndRegisterChannel(notificationManager);
         }
      }

      static Notification createNotification(string channelname, string text, string title) {
         createAndRegisterChannels();
         return channels[channelname].CreateNotification(text, title);
      }

      static void removeAlarmNotification(string channelname) {
         createAndRegisterChannels();
         notificationManager.Cancel(channels[channelname].NotificationID);
      }

   }

}
