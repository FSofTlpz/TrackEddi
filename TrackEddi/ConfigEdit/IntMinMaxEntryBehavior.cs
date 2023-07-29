using System;
using Xamarin.Forms;

namespace TrackEddi.ConfigEdit {
   public class IntMinMaxEntryBehavior : Behavior<Entry> {
      /// <summary>
      /// Check about value entered in the entry and convert it
      /// </summary>
      protected Action<Entry, string> AdditionalCheck;

      public int MaximumValue { get; set; } = int.MaxValue;

      public int MinimumValue { get; set; } = int.MinValue;

      public int StandardValue { get; set; } = int.MinValue;


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
            int value;
            if (!int.TryParse(e.NewTextValue, out value) ||
                value.ToString() != e.NewTextValue) {
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
