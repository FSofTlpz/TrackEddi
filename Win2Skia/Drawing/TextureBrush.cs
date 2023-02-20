using System.Drawing.Drawing2D;

namespace System.Drawing {
   public class TextureBrush : Brush, IDisposable {

      public WrapMode WrapMode { get; set; }

      public Bitmap Image => new Bitmap(SKBitmap);

      /// <summary>
      /// Verschiebung des Koordinatenursprungs des Bitmaps
      /// </summary>
      public SizeF Translation { get; protected set; }


      public TextureBrush(Bitmap bm) :
         base(bm) {
         Translation = new SizeF();
      }

      public void TranslateTransform(int deltax, float deltay) {
         Translation = new SizeF(
            Translation.Width + deltax,
            Translation.Height + deltay);
      }

      #region Implementierung der IDisposable-Schnittstelle

      /// <summary>
      /// true, wenn schon ein Dispose() erfolgte
      /// </summary>
      private bool _isdisposed = false;

      /// <summary>
      /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
      /// </summary>
      public new void Dispose() {
         Dispose(true);
         base.Dispose();
      }

      /// <summary>
      /// überschreibt die Standard-Methode
      /// <para></para>
      /// </summary>
      /// <param name="notfromfinalizer">falls, wenn intern vom Finalizer aufgerufen</param>
      protected override void Dispose(bool notfromfinalizer) {
         if (!this._isdisposed) {            // bisher noch kein Dispose erfolgt
            if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben
               Image.Dispose();

            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
            base.Dispose(notfromfinalizer);
         }
      }

      #endregion


   }
}