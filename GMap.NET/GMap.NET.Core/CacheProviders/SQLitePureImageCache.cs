using GMap.NET.MapProviders;
using System;
using System.IO;

namespace GMap.NET.CacheProviders {

   /// <summary>
   /// Filesystemcache
   /// </summary>
   public class SQLitePureImageCache : PureImageCache {

      #region useless

      public static void Ping() { }

      public static bool ExportMapDataToDB(string sourceFile, string destFile) => false;

      public static bool VacuumDb(string file) => true;

      public string GtileCache {
         get;
         private set;
      }

      #endregion

      string _cachedir;

      /// <summary>
      /// cache location
      /// </summary>
      public string CacheLocation {
         get => _cachedir;
         set {
            _cachedir = Path.Combine(value, "filecache");
            if (!Directory.Exists(_cachedir))
               Directory.CreateDirectory(_cachedir);
         }
      }

      // _cachedir/type/zoom/X/  Y

      string getFilename(int type, GPoint pos, int zoom, bool dirname = false) {
         return dirname ?
            Path.Combine(_cachedir, type.ToString(), zoom.ToString(), pos.X.ToString()) :
            Path.Combine(_cachedir, type.ToString(), zoom.ToString(), pos.X.ToString(), pos.Y.ToString());
      }

      void updateFiledate(string filename) {
         FileInfo fi = new FileInfo(filename);
         fi.LastWriteTime = DateTime.Now;
      }

      int delete(string path, DateTime date) {
         int count = 0;
         foreach (string file in Directory.GetFiles(path)) {
            FileInfo fi = new FileInfo(file);
            if (fi.LastWriteTime < date) {
               fi.Delete();
               count++;
            }
         }
         foreach (string dir in Directory.GetDirectories(path)) 
            count += delete(dir, date);

         if (Directory.GetFiles(path).Length + Directory.GetDirectories(path).Length == 0)
            Directory.Delete(path);
         
         return count;
      }

      #region PureImageCache Members

      bool PureImageCache.PutImageToCache(byte[] tile, int type, GPoint pos, int zoom) {
         string dirname = getFilename(type, pos, zoom, true);
         if (!Directory.Exists(dirname))
            Directory.CreateDirectory(dirname);
         File.WriteAllBytes(getFilename(type, pos, zoom), tile);
         return true;
      }

      PureImage PureImageCache.GetImageFromCache(int type, GPoint pos, int zoom) {
         PureImage ret = null;
         string filename = getFilename(type, pos, zoom);
         if (File.Exists(filename)) {
            byte[] tile = File.ReadAllBytes(filename);
            updateFiledate(filename);
            if (GMapProvider.TileImageProxy != null)
               ret = GMapProvider.TileImageProxy.FromArray(tile);
         }
         return ret;
      }

      int PureImageCache.DeleteOlderThan(DateTime date, int? type) {
         string startdir = _cachedir;
         if (type != null)
            startdir = Path.Combine(_cachedir, type.ToString());
         return delete(startdir, date);
      }

      #endregion
   }
}
