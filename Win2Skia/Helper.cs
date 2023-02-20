using System;
using System.Drawing;
using System.IO;
using SkiaSharp;

namespace SkiaWrapper {
   public class Helper {

      public static SKPoint ConvertPoint(Point pt) {
         return new SKPoint(pt.X, pt.Y);
      }

      public static SKPoint ConvertPoint(PointF pt) {
         return new SKPoint(pt.X, pt.Y);
      }

      public static Point ConvertPoint(SKPoint pt) {
         return new Point((int)pt.X, (int)pt.Y);
      }

      public static void ConvertPoint(SKPoint pt, out PointF ptf) {
         ptf = new PointF(pt.X, pt.Y);
      }

      public static SKPoint[] ConvertPoints(Point[] pt) {
         SKPoint[] skiapt = new SKPoint[pt.Length];
         for (int i = 0; i < skiapt.Length; i++) {
            skiapt[i].X = pt[i].X;
            skiapt[i].Y = pt[i].Y;
         }
         return skiapt;
      }

      public static Point[] ConvertPoints(SKPoint[] skiapt) {
         Point[] pt = new Point[skiapt.Length];
         for (int i = 0; i < skiapt.Length; i++) {
            pt[i].X = (int)skiapt[i].X;
            pt[i].Y = (int)skiapt[i].Y;
         }
         return pt;
      }

      public static void ConvertPoints(SKPoint[] skiapt, ref Point[] dest) {
         for (int i = 0; i < skiapt.Length; i++) {
            dest[i].X = (int)skiapt[i].X;
            dest[i].Y = (int)skiapt[i].Y;
         }
      }

      public static SKPoint[] ConvertPointsF(PointF[] pt) {
         SKPoint[] skiapt = new SKPoint[pt.Length];
         for (int i = 0; i < skiapt.Length; i++) {
            skiapt[i].X = pt[i].X;
            skiapt[i].Y = pt[i].Y;
         }
         return skiapt;
      }

      public static void ConvertPointsF(SKPoint[] skiapt, ref PointF[] dest) {
         for (int i = 0; i < skiapt.Length; i++) {
            dest[i].X = (int)skiapt[i].X;
            dest[i].Y = (int)skiapt[i].Y;
         }
      }

      public static SKRect ConvertRect(Rectangle rect) {
         return new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
      }

      public static SKRect ConvertRect(RectangleF rect) {
         return new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
      }



      /// <summary>
      /// konvertiert die Farbe
      /// </summary>
      /// <param name="col"></param>
      /// <returns></returns>
      public static Color ConvertColor(SKColor col) {
         return Color.FromArgb(col.Alpha, col.Red, col.Green, col.Blue);
      }

      /// <summary>
      /// konvertiert die Farbe
      /// </summary>
      /// <param name="col"></param>
      /// <returns></returns>
      public static SKColor ConvertColor(Color col) {
         return new SKColor(col.R, col.G, col.B, col.A);
      }


      public static bool AppendLog(string txt, string filename) {
         bool res = false;
         using (StreamWriter stream = File.AppendText(filename)) {
            stream.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToString("G"), txt));
            stream.Flush();
            res = true;
         }
         return res;
      }

      public static bool SaveBitmap(SKBitmap bm, string filename) {
         bool res = false;
         using (MemoryStream memStream = new MemoryStream()) {
            using (SKManagedWStream wstream = new SKManagedWStream(memStream)) {
               bm.Encode(wstream, SKEncodedImageFormat.Png, 100);
               byte[] data = memStream.ToArray();
               if (data != null && data.Length > 0) {
                  if (File.Exists(filename))
                     File.Delete(filename);
                  using (FileStream stream = File.Create(filename)) {
                     stream.Write(data, 0, data.Length);
                     res = true;
                  }
               }
            }
         }
         return res;
      }

      public static SKBitmap LoadBitmap(string filename) {
         SKBitmap bm = null;
         using (FileStream stream = File.OpenRead(filename)) {
            bm = SKBitmap.Decode(stream);
         }
         return bm;
      }


   }
}
