using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Shapes;
using static FSofTUtils.Xamarin.Touch.TouchEffect;

namespace FSofTUtils.Xamarin.Touch {
   /// <summary>
   /// Hilfsklasse, die die "Rohdaten" von <see cref="TouchEffect"/> aufbereitet und als Events liefert
   /// </summary>
   public class TouchPointEvaluator {

      #region Events

      #region Event-Args

      public class BaseGestureEventArgs {

         public readonly Point Point;

         public readonly long ID;

         public BaseGestureEventArgs(long id, Point point) {
            Point = point;
            ID = id;
         }

         public override string ToString() {
            return string.Format("ID={0}, Point={1}", ID, Point);
         }

      }

      public class TappedEventArgs : BaseGestureEventArgs {

         public readonly bool LongTap;

         public readonly double Delay;

         public readonly int TapCount;

         public TappedEventArgs(long id, Point startPoint, bool longtap, double delay, int tapcount) :
            base(id, startPoint) {
            LongTap = longtap;
            Delay = delay;
            TapCount = tapcount;
         }

         public TappedEventArgs(long id, Point startPoint) :
            this(id, startPoint, false, 0, 1) { }

         public TappedEventArgs(long id, Point startPoint, double delay) :
            this(id, startPoint, true, delay, 1) { }

         public TappedEventArgs(long id, Point startPoint, int tapcount) :
            this(id, startPoint, false, 0, tapcount) { }

         public override string ToString() {
            return base.ToString() + string.Format(", LongTap={0}, Delay={1}ms, TapCount={2}",
                                                   LongTap,
                                                   Delay,
                                                   TapCount);
         }

      }


      public class MoveEventArgs : BaseGestureEventArgs {

         public readonly Point Delta2Startpoint;

         public readonly Point Delta2Lastpoint;

         public readonly bool MovingEnded;

         public MoveEventArgs(long id, Point startPoint, Point delta2start, Point delta2last, bool ended) :
            base(id, startPoint) {
            Delta2Startpoint = delta2start;
            Delta2Lastpoint = delta2last;
            MovingEnded = ended;
         }

         public override string ToString() {
            return base.ToString() + string.Format(", MovingEnded={0}, Delta2Startpoint={1}, Delta2Lastpoint={2}",
                                                   MovingEnded,
                                                   Delta2Startpoint,
                                                   Delta2Lastpoint);
         }
      }

      #endregion

      public delegate void TappedEventHandler(object sender, TappedEventArgs e);
      public delegate void MoveEventHandler(object sender, MoveEventArgs e);

      /// <summary>
      /// vermutlich nicht sehr sinnvoll
      /// </summary>
      public event TappedEventHandler TapDownEvent;
      /// <summary>
      /// ein Tap (kurz oder lang) ist erfolgt
      /// </summary>
      public event TappedEventHandler TappedEvent;
      /// <summary>
      /// ein mehrfaches (kurzes) Tap ist erfolgt (folgt nach jedem dazugehörigen <see cref="TappedEvent"/>)
      /// </summary>
      public event TappedEventHandler MultiTappedEvent;
      /// <summary>
      /// eine Bewegung ist erfolgt oder beendet
      /// </summary>
      public event MoveEventHandler MoveEvent;

      #endregion

      public enum TouchAction {
         nothing,
         Pressed,
         Moved,
         Released,
         Cancelled,
         Entered,
         Exited
      }

      /// <summary>
      /// max. Punktdifferenz eines Tap zwischen Pressed und Released (i.A. 0)
      /// </summary>
      public Point Delta4Tapped = new Point(0, 0);
      /// <summary>
      /// max. Punktdifferenz zwischen Taps eines Multi-Tap
      /// </summary>
      public Point Delta4MultiTapped = new Point(40, 40);
      /// <summary>
      /// max. Zeit zwischen Pressed und Released für einen (kurzen) Tap (sonst langer Tap)
      /// </summary>
      public double MaxDelay4ShortTapped = 300;
      /// <summary>
      /// max. Zeit zwischen 2 Released bei aufeinanderfolgenden Taps eines Multi-Tap
      /// </summary>
      public double MaxDelay4MultiTapped = 500;

      #region interne Hilfsklassen

      protected class PointData {

         public readonly TouchAction Action;
         public readonly Point Point;
         public readonly DateTime DateTime;

         public PointData(Point pt, TouchAction action) {
            Action = action;
            Point = pt;
            DateTime = DateTime.Now;
         }

      }

      /// <summary>
      /// Hilfsklasse zur Registrierung der jeweils letzten Punktdaten einer ID
      /// </summary>
      protected class PointData4ID {

         Dictionary<long, PointData> pd4id;


         public PointData4ID() {
            pd4id = new Dictionary<long, PointData>();
         }

         public void Clear(long id) {
            if (pd4id.ContainsKey(id))
               pd4id[id] = null;
         }

         public void Clear() {
            pd4id.Clear();
         }

         public void Set(long id, PointData pd) {
            if (pd4id.ContainsKey(id))
               pd4id[id] = pd;
            else
               pd4id.Add(id, pd);
         }

         public bool Contains(long id) {
            return pd4id.TryGetValue(id, out PointData p) && p != null;
         }

         public PointData Get(long id) {
            return pd4id.TryGetValue(id, out PointData pd) ? pd : null;
         }

      }

      protected class PointData4ID2 : PointData4ID {

         Dictionary<long, int> count;


         public PointData4ID2() :
            base() {
            count = new Dictionary<long, int>();
         }

         public new void Clear(long id) {
            base.Clear(id);
            if (count.ContainsKey(id))
               count[id] = 0;
         }

         public new void Clear() {
            base.Clear();
            count.Clear();
         }

         public new void Set(long id, PointData pd) {
            base.Set(id, pd);
            if (!count.ContainsKey(id))
               count.Add(id, 0);
            count[id]++;
         }

         public void SetNew(long id, PointData pd) {
            base.Set(id, pd);
            if (!count.ContainsKey(id))
               count.Add(id, 1);
            count[id] = 1;
         }

         public int Count(long id) {
            return count.ContainsKey(id) ? count[id] : 0;
         }

      }

      /// <summary>
      /// Verwaltung der internen Punktlisten
      /// </summary>
      protected class PtLists {

         Dictionary<long, List<Point>> ptlst4id;

         public PtLists() {
            ptlst4id = new Dictionary<long, List<Point>>();
         }

         public void Add(long id, Point pt) {
            if (ptlst4id.TryGetValue(id, out List<Point> lst))
               lst.Add(pt);
            else {
               List<Point> lst2 = new List<Point>();
               lst2.Add(pt);
               ptlst4id.Add(id, lst2);
            }
         }

         protected void addPoints(long id, IList<Point> ptlst) {
            if (ptlst4id.TryGetValue(id, out List<Point> lst))
               lst.AddRange(ptlst);
            else
               ptlst4id.Add(id, new List<Point>(ptlst));
         }

         public void Clear(long id) {
            if (ptlst4id.TryGetValue(id, out List<Point> lst))
               lst.Clear();
         }

         public int Count(long id) {
            if (ptlst4id.TryGetValue(id, out List<Point> lst))
               return lst.Count;
            return 0;
         }

         public List<Point> Get(long id) {
            if (ptlst4id.TryGetValue(id, out List<Point> lst))
               return lst;
            return null;
         }

         /// <summary>
         /// liefert alle ID's mit min. 1 Punkt
         /// </summary>
         /// <returns></returns>
         public long[] IDs() {
            List<long> ids = new List<long>();
            foreach (var item in ptlst4id) {
               if (item.Value.Count > 0)
                  ids.Add(item.Key);
            }
            return ids.ToArray();
         }


      }

      #endregion

      /// <summary>
      /// letzte <see cref="PointData"/> für Tapped mit Anzahl identischer Taps
      /// </summary>
      protected PointData4ID2 lastTappedWithCount;
      /// <summary>
      /// letzte <see cref="PointData"/> für Pressed
      /// </summary>
      protected PointData4ID lastPressed;
      /// <summary>
      /// letzte <see cref="PointData"/> für Moved
      /// </summary>
      protected PointData4ID lastMoved;
      /// <summary>
      /// interne Punktlisten je ID
      /// </summary>
      protected PtLists ptLists;


      public TouchPointEvaluator() {
         lastTappedWithCount = new PointData4ID2();
         lastPressed = new PointData4ID();
         lastMoved = new PointData4ID();
         ptLists = new PtLists();

         //Matrix m = GetIsotropicZoomMatrix(new Point(164.556361607143, 93.6875),
         //                                  new Point(244.938616071429, 57.5044642857143),
         //                                  new Point(246.081194196429, 57.5044642857143));

      }

      public void Evaluate(TouchActionEventArgs args) {
         TouchAction touchAction = TouchAction.nothing;
         switch (args.Type) {
            // Android.Views.MotionEventActions.Down           A pressed gesture has started, the motion contains the initial starting location.
            // Android.Views.MotionEventActions.PointerDown    A non-primary pointer has gone down.
            case TouchActionEventArgs.TouchActionType.Pressed:
               touchAction = TouchAction.Pressed;
               break;

            // Android.Views.MotionEventActions.Move:
            case TouchActionEventArgs.TouchActionType.Moved:
               touchAction = TouchAction.Moved;
               break;

            // Android.Views.MotionEventActions.Up:
            // Android.Views.MotionEventActions.Pointer1Up:
            case TouchActionEventArgs.TouchActionType.Released:
               touchAction = TouchAction.Released;
               break;

            // Android.Views.MotionEventActions.Cancel         The current gesture has been aborted. You will not receive any more points in it.
            case TouchActionEventArgs.TouchActionType.Cancelled:
               touchAction = TouchAction.Cancelled;
               break;

            case TouchActionEventArgs.TouchActionType.Entered:
               touchAction = TouchAction.Pressed;
               break;

            case TouchActionEventArgs.TouchActionType.Exited:
               touchAction = TouchAction.Released;
               break;
         }
         evaluateAction(args.Id, new PointData(args.Location, touchAction));
      }

      protected void evaluateAction(long id, PointData pdActual) {
         // Es kann nur Pressed-, Released-, Moved- und Cancelled-Aktionen geben.
         switch (pdActual.Action) {
            case TouchAction.Pressed:
               lastMoved.Clear(id);
               lastPressed.Set(id, pdActual);

               ptLists.Clear(id);
               ptLists.Add(id, pdActual.Point);

               TapDownEvent?.Invoke(this, new TappedEventArgs(id, pdActual.Point));
               break;

            case TouchAction.Moved: {
                  ptLists.Add(id, pdActual.Point);

                  PointData pdLastPressed = lastPressed.Get(id);
                  if (pdLastPressed != null) {
                     bool needevent = false;
                     PointData pdLastMove = lastMoved.Get(id);
                     if (pdLastMove != null) {
                        if (!isSamePosition(pdLastMove.Point, pdActual.Point))
                           needevent = true;
                     } else {
                        if (!isSamePosition(pdLastPressed.Point, pdActual.Point))
                           needevent = true;
                     }
                     lastMoved.Set(id, pdActual);
                     if (needevent)
                        MoveEvent?.Invoke(this, new MoveEventArgs(id,
                                                                  pdActual.Point,
                                                                  getDelta(pdLastPressed.Point, pdActual.Point),
                                                                  getDelta(pdLastMove != null ? pdLastMove.Point : pdLastPressed.Point, pdActual.Point),
                                                                  false));    // noch in Bewegung
                  }
               }
               break;

            case TouchAction.Released: {
                  ptLists.Add(id, pdActual.Point);

                  PointData pdLastPressed = lastPressed.Get(id);
                  if (pdLastPressed != null) {
                     Point delta = getDelta(pdLastPressed.Point, pdActual.Point);
                     double ms = getDelay(pdLastPressed.DateTime, pdActual.DateTime);
                     if (isDelta4Tap(delta)) {
                        if (isDelay4ShortTap(ms)) {
                           TappedEvent?.Invoke(this, new TappedEventArgs(id, pdActual.Point));  // ein short Tap
                                                                                                // Multi-Tap?
                           if (lastTappedWithCount.Count(id) == 0)
                              lastTappedWithCount.Set(id, pdActual);
                           else {
                              PointData pdLastTapRelease = lastTappedWithCount.Get(id);
                              if (isDelta4MultiTap(getDelta(pdLastTapRelease.Point, pdActual.Point)) &&
                                  isDelay4MultiTap(getDelay(pdLastTapRelease.DateTime, pdActual.DateTime))) {
                                 lastTappedWithCount.Set(id, pdActual);
                                 MultiTappedEvent?.Invoke(this, new TappedEventArgs(id, pdActual.Point, lastTappedWithCount.Count(id)));   // mehrere short Tap
                              } else {
                                 //Debug.WriteLine("NO Multitap: " + getDelta(pdLastTapRelease.Point, actpd.Point) + ", " +
                                 //                        ", " + pdLastTapRelease.Point +
                                 //                        ", " + actpd.Point +
                                 //                        getDelay(pdLastTapRelease.DateTime, actpd.DateTime) + "ms");
                                 lastTappedWithCount.SetNew(id, pdActual);
                              }
                           }
                        } else {
                           lastTappedWithCount.Clear(id);
                           TappedEvent?.Invoke(this, new TappedEventArgs(id, pdActual.Point, ms));   // ein long Tap (in TappedEventArgs gesetzt)
                                                                                                     //Clear(id);
                        }
                     } else {
                        lastTappedWithCount.Clear(id);
                        PointData pdLastMove = lastMoved.Get(id);
                        MoveEvent?.Invoke(this, new MoveEventArgs(id,
                                                                  pdActual.Point,
                                                                  delta,
                                                                  getDelta(pdLastMove != null ? pdLastMove.Point : pdLastPressed.Point, pdActual.Point),
                                                                  true));   // Bewegung beendet
                                                                            //Clear(id);
                     }
                     lastPressed.Clear(id);
                     lastMoved.Clear(id);
                  }
                  ptLists.Clear(id);
               }
               break;

            case TouchAction.Cancelled:
               ptLists.Clear(id);

               lastPressed.Clear(id);
               lastMoved.Clear(id);
               lastTappedWithCount.Clear(id);
               break;
         }
      }

      //public enum Movement {
      //   nothing,

      //   Stretch,
      //   Pinch,

      //   SwipeLeft,
      //   SwipeRight,
      //   SwipeUp,
      //   SwipeDown,

      //   // ...
      //}

      ///// <summary>
      ///// versucht, die aktuell intern gespeicherte Punktliste zu interpretieren
      ///// </summary>
      ///// <param name="id"></param>
      ///// <returns></returns>
      //public Movement InterpretPointList(long id) {
      //   // Swipe muss "gerade genug", "lang genug" und "achsenparallel genug" sein


      //   return Movement.nothing;
      //}

      //public Movement InterpretPointList(IList<long> id) {
      //   if (id.Count == 2) {
      //      if (CountPoints(id[0]) > 0 &&
      //          CountPoints(id[1]) > 0) {    // -> 2-Finger-Bewegung
      //         List<Point> ptList1 = getPoints(id[0]);
      //         List<Point> ptList2 = getPoints(id[1]);
      //         Point ptStart1 = ptList1[0];
      //         Point ptStart2 = ptList2[0];
      //         Point ptEnd1 = ptList1[ptList1.Count - 1];
      //         Point ptEnd2 = ptList2[ptList2.Count - 1];

      //      }
      //   }
      //   return Movement.nothing;
      //}

      #region public Matrix-Funktionen

      /// <summary>
      /// Bewegungsmatrix anfügen
      /// </summary>
      /// <param name="matrix"></param>
      /// <param name="newmove"></param>
      public void AppendMatrix(Matrix matrix, Matrix newmove) {
         matrix.Append(newmove);
      }

      /// <summary>
      /// liefert die Bewegung der letzten beiden Punkte als Verschiebung
      /// </summary>
      /// <param name="moveID">ID des "beweglichen" Fingers"</param>
      /// <returns></returns>
      public Matrix GetTranslateMatrix(long moveID) {
         List<Point> ptlist = ptLists.Get(moveID);
         if (ptlist != null && ptlist.Count > 1)
            return getTranslateMatrix(ptlist[ptlist.Count - 2], ptlist[ptlist.Count - 1]);
         return new Matrix();
      }

      /// <summary>
      /// liefert die Zoommatrix
      /// </summary>
      /// <param name="pivotID">ID des "festen" Fingers</param>
      /// <param name="moveID">ID des "beweglichen" Fingers"</param>
      /// <returns></returns>
      public Matrix GetAnisotropicZoomMatrix(long pivotID, long moveID) {
         List<Point> ptPivotList = ptLists.Get(pivotID);
         List<Point> ptMoveList = ptLists.Get(moveID);
         if (ptPivotList != null && ptPivotList.Count > 0 &&
             ptMoveList != null && ptMoveList.Count > 1) {
            return getAnisotropicZoomMatrix(ptPivotList[ptPivotList.Count - 1],
                                            ptMoveList[ptMoveList.Count - 2],
                                            ptMoveList[ptMoveList.Count - 1]);
         }
         return new Matrix();
      }

      /// <summary>
      /// liefert die Zoommatrix
      /// </summary>
      /// <param name="pivotID">ID des "festen" Fingers</param>
      /// <param name="moveID">ID des "beweglichen" Fingers"</param>
      /// <returns></returns>
      public Matrix GetIsotropicZoomMatrix(long pivotID, long moveID) {
         List<Point> ptPivotList = ptLists.Get(pivotID);
         List<Point> ptMoveList = ptLists.Get(moveID);
         if (ptPivotList != null && ptPivotList.Count > 0 &&
             ptMoveList != null && ptMoveList.Count > 1) {
            return getIsotropicZoomMatrix(ptPivotList[ptPivotList.Count - 1],
                                          ptMoveList[ptMoveList.Count - 2],
                                          ptMoveList[ptMoveList.Count - 1]);
         }
         return new Matrix();
      }

      /// <summary>
      /// liefert die Rotationsmatrix
      /// </summary>
      /// <param name="pivotID">ID des "festen" Fingers</param>
      /// <param name="moveID">ID des "Drehfingers"</param>
      /// <param name="angle">Drehwinkel</param>
      /// <returns></returns>
      public Matrix GetRotateMatrix(long pivotID, long moveID, out double angle) {
         List<Point> ptPivotList = ptLists.Get(pivotID);
         List<Point> ptMoveList = ptLists.Get(moveID);
         if (ptPivotList != null && ptPivotList.Count > 0 &&
             ptMoveList != null && ptMoveList.Count > 1)
            return getRotateMatrix(ptPivotList[ptPivotList.Count - 1],
                                   ptMoveList[ptMoveList.Count - 2],
                                   ptMoveList[ptMoveList.Count - 1],
                                   out angle);
         angle = 0;
         return new Matrix();
      }

      /// <summary>
      /// Es dürfen intern nur für 2 Finger Punkte registriert sein. Der "feste" Punkt wird vom gerade nicht "bewegten" Finger geliefert.
      /// </summary>
      /// <param name="moveID">ID des "bewegten" Fingers</param>
      /// <returns></returns>
      public Matrix Get2FingerAnisotropicZoomMatrix(long moveID) {
         long[] id = ptLists.IDs();
         if (id.Length == 2)
            return GetAnisotropicZoomMatrix(id[0] == moveID ? id[1] : id[0], moveID);
         return new Matrix();
      }

      /// <summary>
      /// Es dürfen intern nur für 2 Finger Punkte registriert sein. Der "feste" Punkt wird vom gerade nicht "bewegten" Finger geliefert.
      /// </summary>
      /// <param name="moveID">ID des "bewegten" Fingers</param>
      /// <returns></returns>
      public Matrix Get2FingerIsotropicZoomMatrix(long moveID) {
         long[] id = ptLists.IDs();
         if (id.Length == 2)
            return GetIsotropicZoomMatrix(id[0] == moveID ? id[1] : id[0], moveID);
         return new Matrix();
      }

      /// <summary>
      /// transformiert einen einzelnen Punkt mit der Matrix
      /// </summary>
      /// <param name="m"></param>
      /// <param name="org"></param>
      /// <returns>neuer Punkt</returns>
      public static Point Transform(Matrix m, Point org) {
         return m.Transform(org);
      }

      /// <summary>
      /// transformiert ein Punkt-Array mit der Matrix
      /// </summary>
      /// <param name="m"></param>
      /// <param name="org"></param>
      public static void Transform(Matrix m, Point[] org) {
         m.Transform(org);
      }

      /// <summary>
      /// liefert eine Zeichenkette für die ersten 2 Zeilen der Matrix (für debuggen)
      /// </summary>
      /// <param name="m"></param>
      /// <returns></returns>
      public static string MatrixToString(Matrix m) {
         StringBuilder sb = new StringBuilder();
         sb.AppendFormat(CultureInfo.InvariantCulture, "[{0,15:F3}", m.M11);
         sb.AppendFormat(CultureInfo.InvariantCulture, ", {0,15:F3}", m.M21);
         sb.AppendFormat(CultureInfo.InvariantCulture, ", {0,15:F3}", m.OffsetX);
         sb.AppendFormat(CultureInfo.InvariantCulture, " / {0,15:F3}", m.M12);
         sb.AppendFormat(CultureInfo.InvariantCulture, ", {0,15:F3}", m.M22);
         sb.AppendFormat(CultureInfo.InvariantCulture, ", {0,15:F3}]", m.OffsetY);
         return sb.ToString();
      }

      #endregion

      #region protected-Funktionen

      #region Matrix-Funktionen

      protected static Matrix getTranslateMatrix(Point ptFrom, Point ptTo) {
         return new Matrix(0, 0, 0, 0, ptTo.X - ptFrom.X, ptTo.Y - ptFrom.Y);
      }

      /// <summary>
      /// liefert die Zoommatrix
      /// <para>ACHTUNG: Der Zoom horizontal und vertikal muss NICHT identisch sein.</para>
      /// </summary>
      /// <param name="ptPivot">"fester" Finger</param>
      /// <param name="ptFrom">Startpunkt "beweglicher" Finger</param>
      /// <param name="ptTo">Endpunkt "beweglicher" Finger</param>
      /// <returns></returns>
      protected static Matrix getAnisotropicZoomMatrix(Point ptPivot, Point ptFrom, Point ptTo) {
         double scx = (ptTo.X - ptPivot.X) / (ptFrom.X - ptPivot.X);
         double scy = (ptTo.Y - ptPivot.Y) / (ptFrom.Y - ptPivot.Y);
         if (double.IsNaN(scx) || double.IsInfinity(scx) ||
             double.IsNaN(scy) || double.IsInfinity(scy))
            return new Matrix();

         Matrix m = new Matrix(1, 0, 0, 1, -ptPivot.X, -ptPivot.Y);  // Verschiebung, so dass Pivot im Koordinatneursprung liegt
         m.Append(new Matrix(scx, 0, 0, scy, 0, 0));                 // Streckung
         m.Append(new Matrix(1, 0, 0, 1, ptPivot.X, ptPivot.Y));     // Zurück-Verschiebung

         //Point pve = Transform(m1, ptPivot);
         //Point p1e = Transform(m1, ptFrom);
         //Debug.WriteLine("::: " + ptPivot + " <-> " + pve);
         //Debug.WriteLine("::: " + ptTo + " <-> " + p1e);

         return m;
      }

      /// <summary>
      /// liefert die Zoommatrix (aus der Relation der Entfernungen zum festen Punkt)
      /// </summary>
      /// <param name="ptPivot">"fester" Finger</param>
      /// <param name="ptFrom">Startpunkt "beweglicher" Finger</param>
      /// <param name="ptTo">Endpunkt "beweglicher" Finger</param>
      /// <returns></returns>
      protected static Matrix getIsotropicZoomMatrix(Point ptPivot, Point ptFrom, Point ptTo) {
         double scale = length(ptPivot, ptTo) / length(ptPivot, ptFrom);
         if (double.IsNaN(scale) || double.IsInfinity(scale))
            return new Matrix();
         Matrix m = new Matrix(1, 0, 0, 1, -ptPivot.X, -ptPivot.Y);  // Verschiebung, so dass Pivot im Koordinatneursprung liegt
         m.Append(new Matrix(scale, 0, 0, scale, 0, 0));             // Streckung
         m.Append(new Matrix(1, 0, 0, 1, ptPivot.X, ptPivot.Y));     // Zurück-Verschiebung
         return m;
      }

      /// <summary>
      /// liefert die Rotationsmatrix
      /// </summary>
      /// <param name="ptPivot">"fester" Finger</param>
      /// <param name="ptFrom">Startpunkt "beweglicher" Finger</param>
      /// <param name="ptTo">Endpunkt "beweglicher" Finger</param>
      /// <param name="angle">Drehwinkel</param>
      /// <returns></returns>
      protected static Matrix getRotateMatrix(Point ptPivot, Point ptFrom, Point ptTo, out double angle) {
         // Calculate two vectors
         Point oldVector = subtract(ptFrom, ptPivot);
         Point newVector = subtract(ptTo, ptPivot);

         // Find angles from pivot point to touch points
         double oldAngle = Math.Atan2(oldVector.Y, oldVector.X);
         double newAngle = Math.Atan2(newVector.Y, newVector.X);

         // Calculate rotation matrix
         angle = newAngle - oldAngle;
         Matrix touchMatrix = new Matrix();
         touchMatrix.RotateAt(angle, ptPivot.X, ptPivot.Y);

         // Effectively rotate the old vector
         double magnitudeRatio = length(oldVector) / length(newVector);
         oldVector.X = magnitudeRatio * newVector.X;
         oldVector.Y = magnitudeRatio * newVector.Y;

         // Isotropic scaling!
         double scale = 1 / magnitudeRatio;

         if (!double.IsNaN(scale) && !double.IsInfinity(scale))
            touchMatrix.Append(new Matrix(scale, 0, 0, scale, ptPivot.X, ptPivot.Y));

         return touchMatrix;
      }

      #endregion

      /// <summary>
      /// Entfernung zum Koordinatenursprung (Länge des Vektors)
      /// </summary>
      /// <param name="pt"></param>
      /// <returns></returns>
      protected static double length(Point pt) {
         return pt.Distance(Point.Zero);
      }

      /// <summary>
      /// Entfernung zwischen den beiden Punkten (Länge des Vektors)
      /// </summary>
      /// <param name="pt1"></param>
      /// <param name="pt2"></param>
      /// <returns></returns>
      protected static double length(Point pt1, Point pt2) {
         return new Point(pt2.X - pt1.X, pt2.Y - pt1.Y).Distance(Point.Zero);
      }

      /// <summary>
      /// pt1 - pt2
      /// </summary>
      /// <param name="pt1"></param>
      /// <param name="pt2"></param>
      /// <returns></returns>
      protected static Point subtract(Point pt1, Point pt2) {
         return pt1.Offset(-pt2.X, -pt2.Y);
      }

      protected static Point getDelta(Point pstart, Point pend) {
         return new Point(pend.X - pstart.X,
                          pend.Y - pstart.Y);
      }

      protected static double getDelay(DateTime start, DateTime end) {
         return end.Subtract(start).TotalMilliseconds;
      }

      protected bool isDelta4Tap(Point delta) {
         return Math.Abs(delta.X) <= Delta4Tapped.X &&
                Math.Abs(delta.Y) <= Delta4Tapped.Y;
      }

      protected bool isDelta4MultiTap(Point delta) {
         return Math.Abs(delta.X) <= Delta4MultiTapped.X &&
                Math.Abs(delta.Y) <= Delta4MultiTapped.Y;
      }

      protected bool isDelay4ShortTap(double ms) {
         return Math.Abs(ms) <= MaxDelay4ShortTapped;
      }

      protected bool isDelay4MultiTap(double ms) {
         return Math.Abs(ms) <= MaxDelay4MultiTapped;
      }

      protected bool isSamePosition(Point pt1, Point pt2) {
         return pt1.X == pt2.X && pt1.Y == pt2.Y;
      }

      #endregion

   }
}
