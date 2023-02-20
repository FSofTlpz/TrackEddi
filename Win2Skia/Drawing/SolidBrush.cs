using SkiaWrapper;

namespace System.Drawing {
   public class SolidBrush : Brush {

      public Color Color => Helper.ConvertColor(SKPaintSolid.Color);

      public SolidBrush(Color col) : 
         base(col) { }

   }
}