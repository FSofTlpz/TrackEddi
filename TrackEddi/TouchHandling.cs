using FSofTUtils.Xamarin.Touch;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace TrackEddi {
   /// <summary>
   /// fasst das Touch-Handling weitgehend zusammen und stellt die Ergebnisse als Events zur Verfügung
   /// </summary>
   internal class TouchHandling {

      public class TapEventArgs : EventArgs {

         public readonly Point Point;

         /// <summary>
         /// Anahl der akt. registrierten Finger (nur bei <see cref="TapDown"/>)
         /// </summary>
         public readonly int Fingers;

         public readonly object Sender;

         public TapEventArgs(object sender, Point point, int fingers = 0) {
            Point = point;
            Fingers = fingers;
            Sender = sender;
         }
      }

      /// <summary>
      /// Finger aufgesetzt
      /// </summary>
      public event EventHandler<TapEventArgs> TapDown;

      /// <summary>
      /// normaler Fingertap
      /// </summary>
      public event EventHandler<TapEventArgs> StdTap;

      /// <summary>
      /// langer Fingertap
      /// </summary>
      public event EventHandler<TapEventArgs> LongTap;

      /// <summary>
      /// doppelter Fingertap (Z.Z. NICHT NUTZBAR!)
      /// </summary>
      public event EventHandler<TapEventArgs> DoubleTap;

      public class MoveEventArgs : EventArgs {

         public readonly Point From;

         public readonly Point To;

         public bool Last;

         public readonly object Sender;

         public MoveEventArgs(object sender, Point ptfrom, Point ptto, bool last) {
            From = ptfrom;
            To = ptto;
            Last = last;
            Sender = sender;
         }
      }

      /// <summary>
      /// Bewegung eines (!) Fingers
      /// </summary>
      public event EventHandler<MoveEventArgs> Move;

      public class ZoomEventArgs : EventArgs {

         public readonly double Zoom;

         public readonly Point Center;

         public readonly object Sender;

         public ZoomEventArgs(object sender, double zoom, Point center) {
            Zoom = zoom;
            Center = center;
            Sender = sender;
         }
      }

      public event EventHandler<ZoomEventArgs> Zoom;


      /// <summary>
      /// Punktliste je ID (Finger)
      /// </summary>
      readonly Dictionary<long, Point[]> move4ID;

      readonly TouchPointEvaluator touchPointEvaluator;


      //public MainPage mainPage;



      public TouchHandling() {
         touchPointEvaluator = new TouchPointEvaluator();
         touchPointEvaluator.MoveEvent += moveOrZoomEvent;
         touchPointEvaluator.TapDownEvent += tapDownEvent;
         touchPointEvaluator.TappedEvent += tappedEvent;
         touchPointEvaluator.MultiTappedEvent += multiTappedEvent;

         move4ID = new Dictionary<long, Point[]>();
      }

      /// <summary>
      /// registriert ein neues Touch-Event
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="args"></param>
      public void MapTouchAction(object sender, TouchEffect.TouchActionEventArgs args) =>
         touchPointEvaluator.Evaluate(args);

      void tapDownEvent(object sender, TouchPointEvaluator.TappedEventArgs e) {
         //mainPage?.Log("TouchHandling.tapDownEvent: " + e.ToString());
         move4ID.Add(e.ID, new Point[] {     // Punktliste für diesen Finger aufnehmen
            e.Point,       // Startpunkt Fingers
            e.Point        // = Endpunkt des Fingers
         });
         TapDown?.Invoke(this, new TapEventArgs(sender, e.Point, move4ID.Count));
      }

      void tappedEvent(object sender, TouchPointEvaluator.TappedEventArgs e) {
         //mainPage?.Log("TouchHandling.tappedEvent: " + e.ToString());
         if (e.TapCount == 1) {
            if (e.LongTap)
               LongTap?.Invoke(this, new TapEventArgs(sender, e.Point));
            else {


               // Verzögerung nötig, wenn noch ein 2. Tap kommt


               StdTap?.Invoke(this, new TapEventArgs(sender, e.Point));
            }
         }
         gestureEnd();
      }

      void multiTappedEvent(object sender, TouchPointEvaluator.TappedEventArgs e) {
         //mainPage?.Log("TouchHandling.multiTappedEvent: " + e.ToString());
         if (e.TapCount == 2)
            DoubleTap?.Invoke(this, new TapEventArgs(sender, e.Point));
         gestureEnd();
      }

      void moveOrZoomEvent(object sender, TouchPointEvaluator.MoveEventArgs e) {
         //mainPage?.Log("TouchHandling.moveOrZoomEvent: " + e.ToString());
         if (move4ID.Count > 1) {    // mehrere Finger
            move4ID[e.ID][1] = e.Point;

            long[] id = new long[move4ID.Keys.Count];    // ID's (Finger)
            move4ID.Keys.CopyTo(id, 0);

            gestureZoom(sender,
                        move4ID[id[0]][0],      // Startpunkt Finger 1
                        move4ID[id[1]][0],      // Startpunkt Finger 2
                        move4ID[id[0]][1],      // Endpunkt Finger 1
                        move4ID[id[1]][1]);     // Endpunkt Finger 2
         } else { // nur 1 Finger
            //mainPage?.Log("TouchHandling.moveOrZoomEvent: Move " + e.ToString());
            Move?.Invoke(this,
                         new MoveEventArgs(sender,
                                           e.Point.Offset(-e.Delta2Lastpoint.X, -e.Delta2Lastpoint.Y),
                                           e.Point,
                                           e.MovingEnded));
         }
         if (e.MovingEnded)
            gestureEnd();
      }

      void gestureEnd() {
         //foreach (var id in move4ID.Keys)
         //   move4ID[id].Clear();
         move4ID.Clear();
      }

      void gestureZoom(object sender, Point p0start, Point p1start, Point p0end, Point p1end) {
         double startdist = p0start.Distance(p1start);
         double enddist = p0end.Distance(p1end);
         //mainPage?.Log("TouchHandling.gestureZoom: " + enddist / startdist + ", " + new Point(p1end.X - p0end.X, p1end.Y - p0end.Y));
         Zoom?.Invoke(this,
                      new ZoomEventArgs(sender,
                                        enddist / startdist,
                                        new Point(-1, -1)));
         //new Point(p0start.X + (p1start.X - p0start.X) / 2,
         //          p0start.Y + (p1start.Y - p0start.Y) / 2)));
      }


   }
}
