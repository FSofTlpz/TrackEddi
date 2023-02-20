using FSofTUtils.Geography;
using FSofTUtils.Geography.PoorGpx;
using SpecialMapCtrl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrackEddi {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class SimplifyTrackPage : ContentPage {

      /// <summary>
      /// Dialog mit Auswahl beendet
      /// </summary>
      public event EventHandler<EventArgs> EndWithOk;

      public Track NewTrack { get; protected set; }

      Track orgTrack;

      AppData appData;

      public class SimplificationData {

         public string Name;

         public GpxSimplification.HSimplification HSimplification = GpxSimplification.HSimplification.Nothing;
         public double HSimplificationWidth = 0;

         public GpxSimplification.VSimplification VSimplification = GpxSimplification.VSimplification.Nothing;
         public double VSimplificationWidth = 0;

         public double SpeedOutlier = 0;

         public double AscendOutlier = 0;
         public int AscendOutlierLength = 0;

         public bool RemoveTimestamps = false;
         public bool RemoveHeights = false;

         public bool MinimalHeightIsActiv = false;
         public double MinimalHeight = 0;

         public bool MaximalHeightIsActiv = false;
         public double MaximalHeight = 0;

         public bool HSimplificationIsActiv => HSimplification != GpxSimplification.HSimplification.Nothing && 0 < HSimplificationWidth;
         public bool VSimplificationIsActiv => VSimplification != GpxSimplification.VSimplification.Nothing && 0 < VSimplificationWidth;
         public bool SpeedOutlierIsActiv => 0 < SpeedOutlier;
         public bool AscendOutlierIsActiv => 0 < AscendOutlier && 0 < AscendOutlierLength;

         public bool PointRangeIsActiv = false;
         public double PointRangeHeight = 0;
         public int PointRangeStart = 0;
         public int PointRangeCount = 0;

         public bool GapFill = false;


         public SimplificationData() { }

         public SimplificationData(string name) {
            Name = name;
         }

         public string AsString() => AsString(this);

         public static string AsString(SimplificationData sd) {
            StringBuilder sb = new StringBuilder(sd.Name);
            sb.Append("\t");
            sb.Append((int)sd.HSimplification);
            sb.Append("\t");
            sb.Append(sd.HSimplificationWidth);
            sb.Append("\t");
            sb.Append((int)sd.VSimplification);
            sb.Append("\t");
            sb.Append(sd.VSimplificationWidth);
            sb.Append("\t");
            sb.Append(sd.SpeedOutlier);
            sb.Append("\t");
            sb.Append(sd.AscendOutlier);
            sb.Append("\t");
            sb.Append(sd.AscendOutlierLength);
            sb.Append("\t");
            sb.Append(sd.RemoveTimestamps);
            sb.Append("\t");
            sb.Append(sd.RemoveHeights);
            sb.Append("\t");
            sb.Append(sd.MinimalHeightIsActiv);
            sb.Append("\t");
            sb.Append(sd.MinimalHeight);
            sb.Append("\t");
            sb.Append(sd.MaximalHeightIsActiv);
            sb.Append("\t");
            sb.Append(sd.MaximalHeight);
            sb.Append("\t");
            sb.Append(sd.PointRangeIsActiv);
            sb.Append("\t");
            sb.Append(sd.PointRangeHeight);
            sb.Append("\t");
            sb.Append(sd.PointRangeStart);
            sb.Append("\t");
            sb.Append(sd.PointRangeCount);
            sb.Append("\t");
            sb.Append(sd.GapFill);

            return sb.ToString();
         }

         public static SimplificationData FromString(string txt) {
            SimplificationData sd = new SimplificationData();
            string[] tmp = txt.Split('\t');
            int i = 0;
            if (i < tmp.Length)
               sd.Name = tmp[i++];
            if (i < tmp.Length)
               sd.HSimplification = (GpxSimplification.HSimplification)Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.HSimplificationWidth = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplification = (GpxSimplification.VSimplification)Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplificationWidth = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.SpeedOutlier = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.AscendOutlier = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.AscendOutlierLength = Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.RemoveTimestamps = Convert.ToBoolean(tmp[i++]);
            if (i < tmp.Length)
               sd.RemoveHeights = Convert.ToBoolean(tmp[i++]);
            if (i < tmp.Length)
               sd.MinimalHeightIsActiv = Convert.ToBoolean(tmp[i++]);
            if (i < tmp.Length)
               sd.MinimalHeight = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.MaximalHeightIsActiv = Convert.ToBoolean(tmp[i++]);
            if (i < tmp.Length)
               sd.MaximalHeight = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.PointRangeIsActiv = Convert.ToBoolean(tmp[i++]);
            if (i < tmp.Length)
               sd.PointRangeHeight = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.PointRangeStart = Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.PointRangeCount = Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.GapFill = Convert.ToBoolean(tmp[i++]);

            return sd;
         }

      }


      public class ListViewObjectItem : INotifyPropertyChanged {

         public event PropertyChangedEventHandler PropertyChanged;

         public string Name { get; private set; }

         public SimplificationData SimplificationData { get; private set; }


         public ListViewObjectItem(string name, SimplificationData sd) {
            Name = name;
            SimplificationData = sd;
         }

         /// <summary>
         /// zum Auslösen eines <see cref="PropertyChanged"/>-Events (auch "extern")
         /// </summary>
         /// <param name="propname"></param>
         public void Notify4PropChanged(string propname) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
         }

         public override string ToString() {
            return Name;
         }

      }

      readonly ObservableCollection<ListViewObjectItem> simplificationdatalst;


      public Command<ListViewObjectItem> SimplificationDataUse { get; private set; }
      public Command<ListViewObjectItem> SimplificationDataDelete { get; private set; }
      public Command<ListViewObjectItem> SimplificationDataMoveDown { get; private set; }
      public Command<ListViewObjectItem> SimplificationDataMoveUp { get; private set; }


      public SimplifyTrackPage() {
         InitializeComponent();
      }

      public SimplifyTrackPage(Track track, AppData appdata) : this() {
         BindingContext = this;

         orgTrack = track;
         NewTrack = null;

         appData = appdata;

         simplificationdatalst = new ObservableCollection<ListViewObjectItem>();

         SimplificationDataUse = new Command<ListViewObjectItem>(onSimplificationDataUse);
         SimplificationDataDelete = new Command<ListViewObjectItem>(onSimplificationDataDelete);
         SimplificationDataMoveDown = new Command<ListViewObjectItem>(onSimplificationDataMoveDown);
         SimplificationDataMoveUp = new Command<ListViewObjectItem>(onSimplificationDataMoveUp);

         loadSimplificationData();
         ListViewSimplificationData.ItemsSource = simplificationdatalst;
         if (simplificationdatalst.Count > 0)
            setActualData(simplificationdatalst[0].SimplificationData);
         else
            setActualData(null);

         Title = "Track vereinfachen: " + track.VisualName;
      }

      void saveSimplificationData() {
         List<string> tmp = new List<string>();
         foreach (var item in simplificationdatalst)
            tmp.Add(item.SimplificationData.AsString());
         appData.SimplifyDatasetList = tmp;
      }

      void loadSimplificationData() {
         List<string> tmp = new List<string>(appData.SimplifyDatasetList);
         foreach (string item in tmp) {
            SimplificationData sd = SimplificationData.FromString(item);
            simplificationdatalst.Add(new ListViewObjectItem(sd.Name, sd));
         }
      }

      /// <summary>
      /// liefert ein <see cref="SimplificationData"/>-Objekt entsprechend der akt. Daten im Form
      /// </summary>
      /// <returns></returns>
      SimplificationData getActualData() {
         return new SimplificationData() {
            Name = entryDatasetName.Text == null ? "" : entryDatasetName.Text.Trim(),

            RemoveTimestamps = cbDeleteTimestamps.IsChecked,
            RemoveHeights = cbDeleteHeights.IsChecked,

            MinimalHeightIsActiv = cbMinHeight.IsChecked,
            MinimalHeight = getEntryValue(entryMinHeight, -15000, 15000, 0),

            MaximalHeightIsActiv = cbMaxHeight.IsChecked,
            MaximalHeight = getEntryValue(entryMaxHeight, -15000, 15000, 5000),

            PointRangeIsActiv = cbPointRangeHeight.IsChecked,
            PointRangeHeight = getEntryValue(entryPointRangeHeight, -15000, 15000, 0),
            PointRangeStart = getEntryValue(entryPointRangeStart, 1, 999999, 1),
            PointRangeCount = getEntryValue(entryPointRangeCount, 1, 999999, 10),

            SpeedOutlier = cbSpeedOulier.IsChecked ? getEntryValue(entrySpeedOulier, 1, 500, 10) : -1,

            AscendOutlier = cbAscentOulier.IsChecked ? getEntryValue(entryAscentOulier, 1, 100, 25) : -1,
            AscendOutlierLength = cbAscentOulier.IsChecked ? getEntryValue(entryAscentOulierLength, 1, 1000, 50) : -1,

            GapFill = cbRemoveGaps.IsChecked,

            HSimplification = HSimpDP.IsChecked ? GpxSimplification.HSimplification.Douglas_Peucker :
                              HSimpRW.IsChecked ? GpxSimplification.HSimplification.Reumann_Witkam :
                                                  GpxSimplification.HSimplification.Nothing,
            HSimplificationWidth = getEntryValue(entryHSimplWidth, 0.01, 50, 0.2),

            VSimplification = VSimpSI.IsChecked ? GpxSimplification.VSimplification.SlidingIntegral :
                              VSimpSM.IsChecked ? GpxSimplification.VSimplification.SlidingMean :
                                                  GpxSimplification.VSimplification.Nothing,
            VSimplificationWidth = getEntryValue(entryVSimplWidth, 1, 1000, 100),
         };
      }

      /// <summary>
      /// setzt die Daten entsprechend des <see cref="SimplificationData"/>
      /// </summary>
      /// <param name="sd"></param>
      void setActualData(SimplificationData sd) {
         if (sd != null) {
            entryDatasetName.Text = sd.Name;

            cbDeleteTimestamps.IsChecked = sd.RemoveTimestamps;
            cbDeleteHeights.IsChecked = sd.RemoveHeights;

            cbMinHeight.IsChecked = sd.MinimalHeightIsActiv;
            entryMinHeight.Text = sd.MinimalHeight.ToString();

            cbMaxHeight.IsChecked = sd.MaximalHeightIsActiv;
            entryMaxHeight.Text = sd.MaximalHeight.ToString();

            cbPointRangeHeight.IsChecked = sd.PointRangeIsActiv;
            entryPointRangeHeight.Text = sd.PointRangeHeight.ToString();
            entryPointRangeStart.Text = sd.PointRangeStart.ToString();
            entryPointRangeCount.Text = sd.PointRangeCount.ToString();

            cbSpeedOulier.IsChecked = sd.SpeedOutlierIsActiv;
            entrySpeedOulier.Text = sd.SpeedOutlier.ToString();

            cbAscentOulier.IsChecked = sd.AscendOutlierIsActiv;
            entryAscentOulier.Text = sd.AscendOutlier.ToString();
            entryAscentOulierLength.Text = sd.AscendOutlierLength.ToString();

            cbRemoveGaps.IsChecked = sd.GapFill;

            switch (sd.HSimplification) {
               case GpxSimplification.HSimplification.Douglas_Peucker: HSimpDP.IsChecked = true; break;
               case GpxSimplification.HSimplification.Reumann_Witkam: HSimpRW.IsChecked = true; break;
               default: HSimpNo.IsChecked = true; break;
            }
            entryHSimplWidth.Text = sd.HSimplificationWidth.ToString();

            switch (sd.VSimplification) {
               case GpxSimplification.VSimplification.SlidingMean: VSimpSM.IsChecked = true; break;
               case GpxSimplification.VSimplification.SlidingIntegral: VSimpSI.IsChecked = true; break;
               default: VSimpNo.IsChecked = true; break;
            }
            entryVSimplWidth.Text = sd.VSimplificationWidth.ToString();

         } else {

            entryDatasetName.Text = "";

            cbDeleteTimestamps.IsChecked = false;
            cbDeleteHeights.IsChecked = false;

            cbMinHeight.IsChecked = false;
            entryMinHeight.Text = "0";

            cbMaxHeight.IsChecked = false;
            entryMaxHeight.Text = "5000";

            cbPointRangeHeight.IsChecked = false;
            entryPointRangeHeight.Text = "0";
            entryPointRangeStart.Text = "1";
            entryPointRangeCount.Text = "10";

            cbSpeedOulier.IsChecked = false;
            entrySpeedOulier.Text = "10";

            cbAscentOulier.IsChecked = false;
            entryAscentOulier.Text = "25";
            entryAscentOulierLength.Text = "50";

            cbRemoveGaps.IsChecked = false;

            HSimpNo.IsChecked = true;
            entryHSimplWidth.Text = (0.2).ToString();

            VSimpNo.IsChecked = true;
            entryVSimplWidth.Text = "100";
         }
      }

      double getEntryValue(Entry entry, double min, double max, double def) {
         double v = def;
         try {
            v = Math.Max(min, Math.Min(Convert.ToDouble(entry.Text == null ? "" : entry.Text), max));
         } catch { }
         return v;
      }

      double getEntryValue(Entry entry, double min, double max) {
         return getEntryValue(entry, min, max, min);
      }

      int getEntryValue(Entry entry, int min, int max, int def) {
         int v = def;
         try {
            v = Math.Max(min, Math.Min(Convert.ToInt32(entry.Text == null ? "" : entry.Text), max));
         } catch { }
         return v;
      }

      async void onSimplificationDataUse(ListViewObjectItem item) {
         try {
            int idx = simplificationdatalst.IndexOf(item);
            if (0 <= idx)
               setActualData(simplificationdatalst[idx].SimplificationData);
         } catch (Exception ex) {
            await FSofTUtils.Xamarin.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      async void onSimplificationDataDelete(ListViewObjectItem item) {
         if (await FSofTUtils.Xamarin.Helper.MessageBox(this, "Achtung", "Soll der Track '" + item.Name + "' wirklich gelöscht werden?", "ja", "nein")) {
            int idx = simplificationdatalst.IndexOf(item);
            simplificationdatalst.RemoveAt(idx);
            //if (idx > 0)
            //   ListViewSimplificationData.SelectedItem = simplificationdatalst[idx];
            //else if (simplificationdatalst.Count > 0)
            //   ListViewSimplificationData.SelectedItem = 0;
            saveSimplificationData();
         }
      }

      void onSimplificationDataMoveUp(ListViewObjectItem item) {
         int idx = simplificationdatalst.IndexOf(item);
         if (0 < idx) {
            simplificationdatalst.RemoveAt(idx);
            simplificationdatalst.Insert(idx - 1, item);
            ListViewSimplificationData.ScrollTo(item, ScrollToPosition.MakeVisible, true);
            saveSimplificationData();
         }
      }

      void onSimplificationDataMoveDown(ListViewObjectItem item) {
         int idx = simplificationdatalst.IndexOf(item);
         if (0 <= idx && idx < simplificationdatalst.Count - 1) {
            simplificationdatalst.RemoveAt(idx);
            simplificationdatalst.Insert(idx + 1, item);
            ListViewSimplificationData.ScrollTo(item, ScrollToPosition.MakeVisible, true);
            saveSimplificationData();
         }
      }

      private void btnSave_Clicked(object sender, EventArgs e) {
         string name = entryDatasetName.Text.Trim();
         if (!string.IsNullOrEmpty(name)) {
            SimplificationData sd = getActualData();
            sd.Name = name;
            simplificationdatalst.Add(new ListViewObjectItem(sd.Name, sd));
            //ListViewSimplificationData.SelectedItem = simplificationdatalst[simplificationdatalst.Count - 1];
            saveSimplificationData();
         }
      }

      private void entryDatasetName_TextChanged(object sender, TextChangedEventArgs e) {
         btnSave.IsEnabled = (sender as Entry).Text.Trim().Length > 0;
      }

      private void cbMinHeight_CheckedChanged(object sender, CheckedChangedEventArgs e) {
         entryMinHeight.IsEnabled = (sender as CheckBox).IsChecked;
      }

      private void cbMaxHeight_CheckedChanged(object sender, CheckedChangedEventArgs e) {
         entryMaxHeight.IsEnabled = (sender as CheckBox).IsChecked;
      }

      private void cbPointRangeHeight_CheckedChanged(object sender, CheckedChangedEventArgs e) {
         entryPointRangeHeight.IsEnabled =
         entryPointRangeStart.IsEnabled =
         entryPointRangeCount.IsEnabled = (sender as CheckBox).IsChecked;
      }

      private void cbSpeedOulier_CheckedChanged(object sender, CheckedChangedEventArgs e) {
         entrySpeedOulier.IsEnabled = (sender as CheckBox).IsChecked;
      }

      private void cbAscentOulier_CheckedChanged(object sender, CheckedChangedEventArgs e) {
         entryAscentOulier.IsEnabled =
         entryAscentOulierLength.IsEnabled = (sender as CheckBox).IsChecked;
      }

      private void HSimpNo_CheckedChanged(object sender, CheckedChangedEventArgs e) {
         entryHSimplWidth.IsEnabled = !(sender as RadioButton).IsChecked;
      }

      private void VSimpNo_CheckedChanged(object sender, CheckedChangedEventArgs e) {
         entryVSimplWidth.IsEnabled = !(sender as RadioButton).IsChecked;
      }

      async private void Button_Clicked(object sender, EventArgs e) {
         int removedtimestamps = 0;
         int removedheights = 0;
         int setminheights = 0;
         int setmaxheights = 0;
         int setheights = 0;
         int speedoutliers = 0;
         int heightoutliers = 0;
         int gapfilledheights = 0;
         int gapfilledtimestamps = 0;
         int removedhsimpl = 0;
         int removedvsimpl = 0;

         try {

            SimplificationData sd = getActualData();

            List<GpxTrackPoint> gpxTrackPoints = new List<GpxTrackPoint>();
            for (int i = 0; i < orgTrack.GpxSegment.Points.Count; i++)
               gpxTrackPoints.Add(new GpxTrackPoint(orgTrack.GpxSegment.Points[i]));

            if (sd.RemoveTimestamps)
               removedtimestamps = GpxSimplification.RemoveTimestamp(gpxTrackPoints);

            if (sd.RemoveHeights)
               removedheights = GpxSimplification.RemoveHeight(gpxTrackPoints);

            if (sd.MinimalHeightIsActiv)
               GpxSimplification.SetHeight(gpxTrackPoints, out setminheights, out _, sd.MinimalHeight);

            if (sd.MaximalHeightIsActiv)
               GpxSimplification.SetHeight(gpxTrackPoints, out _, out setmaxheights, double.MinValue, sd.MaximalHeight);

            if (sd.PointRangeIsActiv)
               setheights = GpxSimplification.SetHeight(gpxTrackPoints, sd.PointRangeHeight, sd.PointRangeStart, sd.PointRangeCount);

            if (sd.SpeedOutlierIsActiv)
               speedoutliers = GpxSimplification.RemoveSpeedOutlier(gpxTrackPoints, sd.SpeedOutlier / 3.6);

            if (sd.AscendOutlierIsActiv)
               heightoutliers = GpxSimplification.RemoveHeigthOutlier(gpxTrackPoints, sd.AscendOutlier, sd.AscendOutlierLength);

            if (sd.GapFill)
               GpxSimplification.GapFill(gpxTrackPoints, out gapfilledheights, out gapfilledtimestamps);

            if (sd.HSimplificationIsActiv)
               removedhsimpl = GpxSimplification.HorizontalSimplification(gpxTrackPoints, sd.HSimplification, sd.HSimplificationWidth);

            if (sd.VSimplificationIsActiv)
               removedvsimpl = GpxSimplification.VerticalSimplification(gpxTrackPoints, sd.VSimplification, sd.VSimplificationWidth);

            if (removedtimestamps > 0 ||
                removedheights > 0 ||
                setminheights > 0 ||
                setmaxheights > 0 ||
                setheights > 0 ||
                speedoutliers > 0 ||
                heightoutliers > 0 ||
                gapfilledheights > 0 ||
                gapfilledtimestamps > 0 ||
                removedhsimpl > 0 ||
                removedvsimpl > 0) {
               NewTrack = new Track(gpxTrackPoints, orgTrack.VisualName + " (vereinfacht)");
               StringBuilder sb = new StringBuilder();

               if (removedtimestamps > 0)
                  sb.AppendLine("* " + removedtimestamps + " Zeitstempel entfernt");
               if (removedheights > 0)
                  sb.AppendLine("* " + removedheights + " Höhen entfernt");
               if (setminheights > 0)
                  sb.AppendLine("* " + setminheights + " Höhen auf Minimum " + sd.MinimalHeight + "m gesetzt");
               if (setmaxheights > 0)
                  sb.AppendLine("* " + setmaxheights + " Höhen auf Maximum " + sd.MaximalHeight + "m gesetzt");
               if (setheights > 0)
                  sb.AppendLine("* " + setmaxheights + " Höhen auf " + sd.PointRangeHeight + "m gesetzt");
               if (speedoutliers > 0)
                  sb.AppendLine("* " + speedoutliers + " Punkte wegen Überschreitung der Maximalgeschwindigkeit " + sd.PointRangeHeight + "km/h entfernt");
               if (heightoutliers > 0)
                  sb.AppendLine("* " + heightoutliers + " Höhen wegen Überschreitung der max. Anstiegs " + sd.AscendOutlier + "% angepasst");
               if (gapfilledheights > 0)
                  sb.AppendLine("* " + gapfilledheights + " Punkte ohne Höhe mit interpolierter Höhe gesetzt");
               if (gapfilledtimestamps > 0)
                  sb.AppendLine("* " + gapfilledtimestamps + " Punkte ohne Zeitstempel mit interpolierten Zeitstempel gesetzt");
               if (removedhsimpl > 0)
                  sb.AppendLine("* " + removedhsimpl + " Punkte bei horizontaler Vereinfachung entfernt");
               if (removedvsimpl > 0)
                  sb.AppendLine("* " + removedvsimpl + " Punkte bei vertikaler Vereinfachung entfernt");

               await FSofTUtils.Xamarin.Helper.MessageBox(this, "Ergebnis", sb.ToString());
            } else
               await FSofTUtils.Xamarin.Helper.MessageBox(this, "Ergebnis", "Es gab keine Veränderungen am Track.");
         } catch (Exception ex) {
            await FSofTUtils.Xamarin.Helper.MessageBox(this, "Fehler", ex.Message);
         }
         
         EndWithOk?.Invoke(this, EventArgs.Empty);
         await Navigation.PopAsync();     // diese Seite sofort schließen
      }
   }
}