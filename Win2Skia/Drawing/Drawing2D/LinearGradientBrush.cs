using SkiaSharp;
using SkiaWrapper;

namespace System.Drawing.Drawing2D {
   public class LinearGradientBrush : Brush {

      public LinearGradientBrush(PointF point1, PointF point2, Color color1, Color color2) {
         SKShader = SKShader.CreateLinearGradient(Helper.ConvertPoint(point1),
                                                  Helper.ConvertPoint(point2),
                                                  new SKColor[] {
                                                     Helper.ConvertColor(color1),
                                                     Helper.ConvertColor(color2),
                                                  },
                                                  SKShaderTileMode.Clamp);

      }

   }
}
