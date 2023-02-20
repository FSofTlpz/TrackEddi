using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TrackEddi {
   public class PlaceItem {
      public string Name { get; protected set; }

      public double Longitude { get; protected set; }

      public double Latitude { get; protected set; }

      public double Zoom { get; protected set; }

      public PlaceItem(string name, double lon, double lat) {
         Name = name;
         Longitude = lon;
         Latitude = lat;
         Zoom = -1;
      }

      public PlaceItem(string name, double lon, double lat, double zoom) :
         this(name, lon, lat) {
         Zoom = zoom;
      }

      static public ObservableCollection<PlaceItem> ConvertPlaceList(ObservableCollection<PlaceItem> lst, List<string> txtlst) {
         lst.Clear();
         foreach (string line in txtlst) {
            string[] tmp = line.Split('\t');
            switch (tmp.Length) {
               case 3:
                  lst.Add(new PlaceItem(tmp[0], Convert.ToDouble(tmp[1]), Convert.ToDouble(tmp[2])));
                  break;

               case 4:
                  lst.Add(new PlaceItem(tmp[0], Convert.ToDouble(tmp[1]), Convert.ToDouble(tmp[2]), Convert.ToDouble(tmp[3])));
                  break;
            }
         }
         return lst;
      }

      static public List<string> ConvertPlaceList(ObservableCollection<PlaceItem> lst) {
         List<string> txtlst = new List<string>();
         foreach (var it in lst)
            if (it.Zoom < 0)
               txtlst.Add(it.Name + "\t" + it.Longitude + "\t" + it.Latitude);
            else
               txtlst.Add(it.Name + "\t" + it.Longitude + "\t" + it.Latitude + "\t" + it.Zoom);
         return txtlst;
      }

      public override string ToString() {
         return Name + " (" + Longitude + ", " + Latitude + ")";
      }

   }
}
