using System.Diagnostics;
using Xamarin.Forms;

namespace TrackEddi.ConfigEdit {
   public class DoubleMinMaxEntryBehavior : Behavior<Entry> {

      public double MaximumValue { get; set; } = double.MaxValue;

      public double MinimumValue { get; set; } = double.MinValue;

      public double StandardValue { get; set; } = double.MinValue;


      protected override void OnAttachedTo(Entry bindable) {
         base.OnAttachedTo(bindable);
         bindable.TextChanged += Entry_TextChanged;
         bindable.Unfocused += Entry_Unfocused;
      }

      protected override void OnDetachingFrom(Entry bindable) {
         base.OnDetachingFrom(bindable);
      }

      protected virtual void Entry_TextChanged(object sender, TextChangedEventArgs e) {
         if (!string.IsNullOrEmpty(e.NewTextValue)) {
            double value;

            /* MUSS NOCH VERBESSERT WERDEN:
             * 
             * z.B.: Das Min. ist 0,5 und aktuell 0,7 enthalten. Es soll zu 0,6 geändert werden und der Cursor steht am Ende
             *       Beim Backspace entsteht der Text "0," der automatisch auf das Min. "0,5" geändert wird.
             *       Lösung z.Z. z.B. mit Cursor hinter das Komma, Eingabe "6" -> "0,67", Cursor an das Ende, dann Backspace
             *       => Umständlich und nicht intuitiv
             * 
             */

            string newtext = e.NewTextValue;
            if (newtext.Length > 0 &&
                newtext[newtext.Length - 1] == ',')
               newtext += "0";      // Kommastelle simulieren nur für TryParse()

            if (!double.TryParse(newtext, out value)) { 
               ((Entry)sender).Text = e.OldTextValue;
            } else {
               if (value < MinimumValue)
                  ((Entry)sender).Text = MinimumValue.ToString();
               else if (MaximumValue < value)
                  ((Entry)sender).Text = MaximumValue.ToString();
            }
         }
      }

      private void Entry_Unfocused(object sender, FocusEventArgs e) {
         if (!e.IsFocused) {
            if ((sender as Entry).Text.Trim() == "")
               (sender as Entry).Text = StandardValue.ToString();
         }
      }
   }
}
