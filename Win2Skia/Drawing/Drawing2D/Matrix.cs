using SkiaSharp;

namespace System.Drawing.Drawing2D {
   public class Matrix {

      public SKMatrix SKMatrix { get; protected set; }

      /// <summary>
      /// erzeugt eine Identitätsmatrix
      /// </summary>
      public Matrix() {
         SKMatrix = SKMatrix.CreateIdentity();
      }

      public Matrix(SKMatrix matrix) {
         SKMatrix = matrix;
      }

      /// <summary>
      /// Weist dieser Matrix eine Drehung im Uhrzeigersinn um den angegebenen Punkt zu, wobei die Drehung vorangestellt wird.
      /// </summary>
      /// <param name="angle"></param>
      /// <param name="referencePoint"></param>
      /// <param name="order"></param>
      public void RotateAt(float angle, PointF referencePoint, MatrixOrder order = MatrixOrder.Prepend) {
         switch (order) {
            case MatrixOrder.Append:
               SKMatrix = SKMatrix.PostConcat(SKMatrix.CreateRotation(angle / 180 * (float)Math.PI, referencePoint.X, referencePoint.Y));
               break;

            case MatrixOrder.Prepend:
               SKMatrix = SKMatrix.PreConcat(SKMatrix.CreateRotation(angle / 180 * (float)Math.PI, referencePoint.X, referencePoint.Y));
               break;
         }
      }

      /// <summary>
      /// Weist die von dieser Matrix dargestellte geometrische Transformation einem Punktearray zu.
      /// </summary>
      /// <param name="tt"></param>
      public void TransformPoints(Point[] pt) {
         SKPoint[] skiapt = SKMatrix.MapPoints(SkiaWrapper.Helper.ConvertPoints(pt));
         SkiaWrapper.Helper.ConvertPoints(skiapt, ref pt);
      }

      public void TransformPoints(PointF[] pt) {
         SKPoint[] skiapt = SKMatrix.MapPoints(SkiaWrapper.Helper.ConvertPointsF(pt));
         SkiaWrapper.Helper.ConvertPointsF(skiapt, ref pt);
      }

      /// <summary>
      /// Multipliziert jeden Vektor in einem Array mit der Matrix. Die zu verschiebenden Elemente dieser Matrix (dritte Zeile) werden ignoriert.
      /// </summary>
      /// <param name="p"></param>
      public void TransformVectors(Point[] pt) {
         SKPoint[] skiapt = SKMatrix.MapVectors(SkiaWrapper.Helper.ConvertPoints(pt));
         SkiaWrapper.Helper.ConvertPoints(skiapt, ref pt);
      }

      /// <summary>
      /// Setzt diese Matrix zurück, sodass sie die Elemente der Identitätsmatrix enthält.
      /// </summary>
      public void Reset() {
         SKMatrix = SKMatrix.Identity;
      }

      /// <summary>
      /// Invertiert diese Matrix, sofern sie invertierbar ist.
      /// </summary>
      public void Invert() {
         // org.: bool SkMatrix::invert(SkMatrix *inverse) 	
         SKMatrix.Invert();
         // oder:
         //Internal= Internal.Invert();
      }

      public override string ToString() {
         return string.Format("ScaleX={0}, SkewX={1}, TransX={2}, SkewY={3}, ScaleY={4}, TransY={5}, Persp0={6}, Persp1={7}, Persp2={8}",
                              SKMatrix.ScaleX,
                              SKMatrix.SkewX,
                              SKMatrix.TransX,
                              SKMatrix.SkewY,
                              SKMatrix.ScaleY,
                              SKMatrix.TransY,
                              SKMatrix.Persp0,
                              SKMatrix.Persp1,
                              SKMatrix.Persp2);
      }
   }
}