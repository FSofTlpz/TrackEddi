using SkiaSharp;

namespace System.Drawing.Drawing2D {
   public class Region : IDisposable {

      public SKRegion SKRegion { get; protected set; }


      public Region(GraphicsPath path) {
         SKRegion = new SKRegion(path.SKPath);
      }

      public GraphicsPath GetRegionData() {
         return new GraphicsPath(SKRegion.GetBoundaryPath());
      }

      public void Intersect(RectangleF rect) {
         SKRegion.Intersects(new SKRectI((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom));
      }

      public void Intersect(Region region) {
         SKRegion.Intersects(region.SKRegion);
      }

      public void Exclude(RectangleF rect) {
         SKRegion.Op(new SKRectI((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom), SKRegionOperation.Difference);
      }

      public void Exclude(GraphicsPath path) {
         SKRegion.Op(path.SKPath, SKRegionOperation.Difference);
      }

      public void Union(GraphicsPath path) {
         SKRegion.Op(path.SKPath, SKRegionOperation.Union);
      }

      public bool IsEmpty(Graphics canvas) {
         return SKRegion.IsEmpty;
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
               if (SKRegion != null) {
                  SKRegion.Dispose();
                  SKRegion = null;
               }
            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion

   }
}