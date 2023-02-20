//#define WITH_DEBUG_INFO

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.Xaml;
using static System.Net.Mime.MediaTypeNames;

namespace FSofTUtils.Xamarin.Control {

   [XamlCompilation(XamlCompilationOptions.Compile)]

   /*
    * Im Prinzip ist das CarouselView gut geeignet, weil es mit der Loop-Eigenschaft den "Rundumlauf" gut realisiert.
    * Leider benötigt man die PeekAreaInsets-Eigenschaft um die Items "zusammenzurücken".
    * Diese Eigenschaft bzw. deren Veränderung läßt sich nur relativ trickreich berechnen.
    * 
    * Probleme:
    * 
    * Wie erreicht man eine sichere Anpassung/korrekte Darstellung nach Änderung von PeekAreaInsets?
    * 
    * Wie synchronisiert man sicher die Anzeige mit dem intern registrierten Wert?
    * 
    */

   public partial class WheelView : ContentView {

      const int DEFAULT_FONTSIZE = 25;

      #region Events

      public class ValueChangedEventArgs {
         public readonly int Value;

         public ValueChangedEventArgs(int v) {
            Value = v;
         }
      }

      public delegate void ValueChangedEventDelegate(object sender, ValueChangedEventArgs args);

      /// <summary>
      /// der angezeigte Wert hat sich verändert
      /// </summary>
      public event ValueChangedEventDelegate ValueChangedEvent;

      #endregion

      #region Binding-Vars

      #region  Binding-Var BackColor

      public static readonly BindableProperty BackColorProperty = BindableProperty.Create(
         "BackColor",
         typeof(Color),
         typeof(WheelView),
         Color.LightSalmon);

      /// <summary>
      /// Hintergrundfarbe des Controls
      /// </summary>
      public Color BackColor {
         get => (Color)GetValue(BackColorProperty);
         set => SetValue(BackColorProperty, value);
      }

      #endregion

      #region  Binding-Var ItemColor

      public static readonly BindableProperty ItemColorProperty = BindableProperty.Create(
         "ItemColor",
         typeof(Color),
         typeof(WheelView),
         Color.LightSkyBlue);

      /// <summary>
      /// Hintergrundfarbe der Items
      /// </summary>
      public Color ItemColor {
         get => (Color)GetValue(ItemColorProperty);
         set => SetValue(ItemColorProperty, value);
      }

      #endregion

      #region Binding-Var ItemFontSize

      public static readonly BindableProperty ItemFontSizeProperty = BindableProperty.Create(
         "ItemFontSize",
         typeof(int),
         typeof(WheelView),
         DEFAULT_FONTSIZE);

      /// <summary>
      /// Font-Größe für die Items
      /// </summary>
      public int ItemFontSize {
         get => (int)GetValue(ItemFontSizeProperty);
         set => SetValue(ItemFontSizeProperty, value);
      }

      #endregion

      #region  Binding-Var MaxValue

      public static readonly BindableProperty MaxValueProperty = BindableProperty.Create(
                                                                         "MaxValue",            // the name of the bindable property
                                                                         typeof(int),           // the bindable property type
                                                                         typeof(WheelView),     // the parent object type
                                                                         60,                    // the default value for the property
                                                                         propertyChanged: OnMaxValueChanged); // Delegat, der ausgeführt wird, wenn der Wert geändert wurde
      /// <summary>
      /// auswählbarer Maximalwert
      /// </summary>
      public int MaxValue {
         get => (int)GetValue(MaxValueProperty);
         set => SetValue(MaxValueProperty, value);
      }

      static void OnMaxValueChanged(BindableObject bindable, object oldValue, object newValue) {
         if (bindable is WheelView) {
            WheelView wv = bindable as WheelView;
            wv.initItems(0, (int)newValue);
         }
      }

      #endregion

      #region Binding-Var PeekAreaInsets

      public static readonly BindableProperty PeekAreaInsetsProperty = BindableProperty.Create(
         "PeekAreaInsets",
         typeof(Thickness),
         typeof(CarouselView));

      /// <summary>
      /// PeekAreaInsets des CarouselView
      /// </summary>
      public Thickness PeekAreaInsets {
         get => (Thickness)GetValue(PeekAreaInsetsProperty);
         set {
            if (value != null &&
                value != (Thickness)GetValue(PeekAreaInsetsProperty)) {
               SetValue(PeekAreaInsetsProperty, value);
            }
         }
      }

      #endregion

      #endregion

      /// <summary>
      /// Höhe eines Item (kleiner 0, wenn noch nicht bestimmt)
      /// </summary>
      double itemHeight = -1;

      /// <summary>
      /// Datenarray mit den auswählbaren int-Werten (für ItemsSource)
      /// </summary>
      int[] dat;

      long _idx = 0;

      int idx {
         get {
            return (int)Interlocked.Read(ref _idx);
         }
         set {
            Interlocked.Exchange(ref _idx, value);
         }
      }


      public int Idx {
         get => idx;
         set {
            this.idx = value;
#if WITH_DEBUG_INFO
            showDebug("set Value=" + value);
#endif
            doAsyncAction(WheelAction.SetIdx, new object[] { idx, false });
         }
      }


      public WheelView() {
         InitializeComponent();

         initItems(0, MaxValue);
         //Wheel.BindingContextChanged += Wheel_BindingContextChanged;
         //Wheel.ScrollToRequested += Wheel_ScrollToRequested;
         //Wheel.Scrolled += wheel_Scrolled;
         //Wheel.CurrentItemChanged += wheel_CurrentItemChanged;
         Wheel.PositionChanged += wheel_PositionChanged;

         //Wheel.BindingContextChanged += Wheel_BindingContextChanged;
         //Wheel.MeasureInvalidated += Wheel_MeasureInvalidated;
         //Wheel.PropertyChanged += Wheel_PropertyChanged;
         //Wheel.SizeChanged += Wheel_SizeChanged;
      }

      void initItems(int min, int max) {
         doAsyncAction(WheelAction.SetData, new object[] { min, max });
      }

//      private void Wheel_SizeChanged(object sender, EventArgs e) {
//#if WITH_DEBUG_INFO
//         showDebug("Wheel_SizeChanged()");
//#endif
//      }

//      private void Wheel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
//#if WITH_DEBUG_INFO
//         showDebug("Wheel_PropertyChanged() " + e.PropertyName);
//#endif
//      }

//      private void Wheel_MeasureInvalidated(object sender, EventArgs e) {
//#if WITH_DEBUG_INFO
//         showDebug("Wheel_MeasureInvalidated()");
//#endif
//      }

//      private void Wheel_BindingContextChanged(object sender, EventArgs e) {
//#if WITH_DEBUG_INFO
//         showDebug("Wheel_BindingContextChanged()");
//#endif
//      }

//      private void wheel_Scrolled(object sender, ItemsViewScrolledEventArgs e) {
//#if WITH_DEBUG_INFO
//         showDebug("wheel_Scrolled() A, VerticalDelta=" + e.VerticalDelta);
//#endif
//      }

//      //private void Wheel_ScrollToRequested(object sender, ScrollToRequestEventArgs e) {
//      //   Debug.WriteLine(string.Format(">>> Wheel_ScrollToRequested"));
//      //}

//      //private void Wheel_BindingContextChanged(object sender, EventArgs e) {
//      //   Debug.WriteLine(string.Format(">>> Wheel_BindingContextChanged"));
//      //}

//      // Das ScrollToRequested-event ist nur die Antwort auf ein ScrollTo().

//      // Das Scrolled-event wird vermutlich nach jedem Neuzeichnen geliefert. Der Offset scheint "unendlich" je nach Richtung weiterzuzählen.
//      // Delta gibt die Veränderung zum letzten Offset an.
//      // Es ist aber nicht abzulesen, ob noch ein weiteres scrollen erfolgt.
//      // Der Item-Wechsel ist besser im CurrentItemChanged-event ablesbar.
//      // IsDragging zeigt nur, ob der Nutzer noch "am Rad dreht", NICHT, ob noch ein weiteres Drehen "von alleine" erfolgt.

//      //private void Wheel_Scrolled(object sender, ItemsViewScrolledEventArgs e) {
//      //   Debug.WriteLine(string.Format(">>> Scrolled e.VerticalOffset=" + e.VerticalOffset + ", e.VerticalDelta=" + e.VerticalDelta + ", " + Wheel.IsDragging));
//      //}

//      /// <summary>
//      /// eines der CarouselViews hat einen neuen Wert
//      /// </summary>
//      /// <param name="sender"></param>
//      /// <param name="e"></param>
//      private void wheel_CurrentItemChanged(object sender, CurrentItemChangedEventArgs e) {
//#if WITH_DEBUG_INFO
//         showDebug("wheel_CurrentItemChanged() A: CurrentItem=" + e.CurrentItem + ", PreviousItem=" + e.PreviousItem);
//#endif
//      }

      private void wheel_PositionChanged(object sender, PositionChangedEventArgs e) {
#if WITH_DEBUG_INFO
         showDebug("wheel_PositionChanged(): CurrentPosition=" + e.CurrentPosition);
#endif
         idx = e.CurrentPosition;
         OnValueChanged();
      }

      /// <summary>
      /// die Größe für die Items hat sich geändert (wird nur aus dem CarouselView.ItemTemplate gesendet)
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void ItemFrame_SizeChanged(object sender, EventArgs e) {
         Frame frame = sender as Frame;
         if (frame != null &&
             itemHeight != frame.Height) {
            itemHeight = frame.Height;
            doAsyncAction(WheelAction.SetPeekAreaInsets, new object[] { itemHeight });
         }
      }

      /// <summary>
      /// löst das <see cref="ValueChangedEvent"/> mit dem akt. <see cref="Idx"/> aus
      /// </summary>
      public virtual void OnValueChanged() {
         ValueChangedEvent?.Invoke(this, new ValueChangedEventArgs(Idx));
      }


      enum WheelAction {
         SetData,
         SetPeekAreaInsets,
         SetIdx,
      }

      object objActionDoor = new object();

      /// <summary>
      /// führt Aktionen asynchron aus und sichert dabei, dass eine Aktion immer abgeschlossen sein muss, bevor die nächste startet
      /// </summary>
      /// <param name="action"></param>
      /// <param name="paras"></param>
      async void doAsyncAction(WheelAction action, object[] paras) {
#if WITH_DEBUG_INFO
         showDebug("doAsyncAction() A: action=" + action);
#endif
         await Task.Run(() => {
            lock (objActionDoor) {  // Task wartet, bis kein anderer Task mehr arbeitet
               switch (action) {
                  case WheelAction.SetData:
                     int min = (int)paras[0];
                     int max = (int)paras[1];
#if WITH_DEBUG_INFO
                     showDebug("doAsyncAction() B: action=" + action + ", min=" + min + ", max=" + max);
#endif
                     Wheel.IsScrollAnimated = false;
                     Wheel.ItemsSource = null;
                     dat = new int[max - min];
                     for (int i = 0; i < dat.Length; i++)
                        dat[i] = i;
                     Wheel.ItemsSource = dat;
                     Wheel.IsScrollAnimated = true;
#if WITH_DEBUG_INFO
                     showDebug("doAsyncAction() C: action=" + action + ", min=" + min + ", max=" + max);
#endif
                     break;

                  case WheelAction.SetPeekAreaInsets:
                     int orgidx = Idx;
                     double itemHeight = (double)paras[0];
#if WITH_DEBUG_INFO
                     showDebug("doAsyncAction() B: action=" + action + ", itemHeight=" + itemHeight);
#endif
                     MainThread.BeginInvokeOnMainThread(() => {
                        Wheel.IsScrollAnimated = false;
                        Wheel.CancelAnimations();
                        PeekAreaInsets = new Thickness(0,
                                                       Wheel.Height - itemHeight,
                                                       0,
                                                       0);
                        Wheel.Position = orgidx;
                        Wheel.IsScrollAnimated = true;
                     });
#if WITH_DEBUG_INFO
                     showDebug("doAsyncAction() C: action=" + action + ", itemHeight=" + itemHeight);
#endif
                     break;

                  case WheelAction.SetIdx:
                     int idx = (int)paras[0];
                     bool animation = (bool)paras[1];
#if WITH_DEBUG_INFO
                     showDebug("doAsyncAction() B: action=" + action + ", idx=" + idx + ", animation=" + animation);
#endif
                     MainThread.BeginInvokeOnMainThread(() => {
                        this.idx = idx;
                        if (!animation) {
                           Wheel.IsScrollAnimated = false;
                           Wheel.CancelAnimations();
                           Wheel.Position = Idx;
                           Wheel.IsScrollAnimated = true;
                        } else {
                           Wheel.ScrollTo(Idx, -1, ScrollToPosition.Center, true);
                        }
                     });
#if WITH_DEBUG_INFO
                     showDebug("doAsyncAction() C: action=" + action + ", idx=" + idx + ", animation=" + animation);
#endif
                     break;
               }
            }
         });
#if WITH_DEBUG_INFO
         showDebug("doAsyncAction() E: action=" + action);
#endif
      }


#if WITH_DEBUG_INFO
      void showDebug(string txt = "") {
         Debug.WriteLine(string.Format(">>> WheelView (Pos {0}): Value={1}, VerticalThickness={2}, {3}",
                                       Wheel.Position,
                                       Idx,
                                       Wheel.PeekAreaInsets.VerticalThickness,
                                       txt
                                       ));
      }
#endif

   }
}
