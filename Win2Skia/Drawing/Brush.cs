using SkiaSharp;
using SkiaWrapper;

namespace System.Drawing {
   public class Brush : IDisposable {

      public bool IsSolid => SKBitmap == null && SKShader == null;

      public SKPaint SKPaintSolid { get; protected set; }

      public SKBitmap SKBitmap { get; protected set; }

      public SKShader SKShader { get; protected set; }


      public Brush(Color color) {
         SKPaintSolid = new SKPaint() {
            IsAntialias = true,
            Color = Helper.ConvertColor(color),
         };
      }

      public Brush(Bitmap bitmap) {
         SKBitmap = bitmap.SKBitmap.Copy();
      }

      protected Brush() {
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
               if (!IsSolid) {
                  SKBitmap?.Dispose();
                  SKBitmap = null;
               }
               SKShader?.Dispose();
            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion

   }
}