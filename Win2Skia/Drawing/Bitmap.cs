using System.Drawing.Imaging;
using System.IO;
using SkiaSharp;
using SkiaWrapper;

namespace System.Drawing {
   public class Bitmap : IDisposable {

      public SKBitmap SKBitmap { get; protected set; }

      /// <summary>
      /// Ruft die Breite dieses Image in Pixel ab.
      /// </summary>
      public int Width => SKBitmap.Width;

      /// <summary>
      /// Ruft die Höhe dieses Image in Pixel ab.
      /// </summary>
      public int Height => SKBitmap.Height;


      public Bitmap(Bitmap bm) {
         SKBitmap = bm.SKBitmap.Copy();
      }

      public Bitmap(SKBitmap bm) {
         SKBitmap = bm.Copy();
      }

      public Bitmap(int width, int height) {
         SKBitmap = new SKBitmap(width, height);
      }

      /// <summary>
      /// Bitmap aus einem Stream erzeugen
      /// </summary>
      /// <param name="stream"></param>
      public Bitmap(Stream stream) {
         SKBitmap = SKBitmap.Decode(stream);
      }

      public static Bitmap FromStream(MemoryStream ms) {
         return new Bitmap(ms);
      }

      public Bitmap(string filename) {
         using (FileStream stream = new FileStream(filename, FileMode.Open)) {
            SKBitmap = SKBitmap.Decode(stream);
         }
      }

      /// <summary>
      /// Bitmap in einem Stream mit dem angeg. Format speichern
      /// </summary>
      /// <param name="memoryStream"></param>
      /// <param name="format"></param>
      public void Save(MemoryStream memoryStream, ImageFormat format) {
         SKEncodedImageFormat sKEncodedImageFormat = SKEncodedImageFormat.Png;

         switch (format) {
            case ImageFormat.Png:
               sKEncodedImageFormat = SKEncodedImageFormat.Png;
               break;

            case ImageFormat.Bmp:
               sKEncodedImageFormat = SKEncodedImageFormat.Bmp;
               break;

         }
         SKBitmap.Encode(memoryStream, sKEncodedImageFormat, 100);
      }

      public void Save(string filename, ImageFormat format = ImageFormat.Png) {
         using (MemoryStream mem = new MemoryStream()) {
            Save(mem, format);
            using (FileStream stream = new FileStream(filename, FileMode.Create)) {
               stream.Write(mem.ToArray(), 0, (int)mem.Length);
            }
         }
      }

      /// <summary>
      /// Ruft die Farbe des angegebenen Pixels in dieser Bitmap ab.
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      public Color GetPixel(int x, int y) {
         return Helper.ConvertColor(SKBitmap.GetPixel(x, y));
      }

      /// <summary>
      /// Legt die Farbe des angegebenen Pixels in dieser Bitmap fest.
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <param name="color"></param>
      public void SetPixel(int x, int y, Color color) {
         SKBitmap.SetPixel(x, y, Helper.ConvertColor(color));
      }

      /// <summary>
      /// Erstellt eine Kopie des Bitmaps.
      /// </summary>
      /// <returns></returns>
      public Bitmap Clone() {
         return new Bitmap(SKBitmap);
      }


      #region Implementierung der IDisposable-Schnittstelle

      /// <summary>
      /// true, wenn schon ein Dispose() erfolgte
      /// </summary>
      private bool _isdisposed = false;

      /// <summary>
      /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
      /// </summary>
      public void Dispose() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      /// <summary>
      /// überschreibt die Standard-Methode
      /// <para></para>
      /// </summary>
      /// <param name="notfromfinalizer">falls, wenn intern vom Finalizer aufgerufen</param>
      protected virtual void Dispose(bool notfromfinalizer) {
         if (!this._isdisposed) {            // bisher noch kein Dispose erfolgt
            if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben
               if (SKBitmap != null) {
                  SKBitmap.Dispose();
                  SKBitmap = null;
               }
            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion


   }
}
