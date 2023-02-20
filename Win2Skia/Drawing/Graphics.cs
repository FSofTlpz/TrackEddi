using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using SkiaSharp;
using SkiaWrapper;

namespace System.Drawing {

   public class Graphics : IDisposable {

      public SKCanvas SKCanvas { get; protected set; }

      public SmoothingMode SmoothingMode { get; set; }
      public CompositingQuality CompositingQuality { get; set; }
      public InterpolationMode InterpolationMode { get; set; }
      public TextRenderingHint TextRenderingHint { get; set; }

      public int DpiY { get; protected set; }


      public Matrix Transform {
         get {
            return new Matrix(SKCanvas.TotalMatrix);
         }
         set {
            SKCanvas.SetMatrix(value.SKMatrix);
         }
      }


      public Graphics(SKCanvas canvas) {
         SKCanvas = canvas;
      }

      public static Graphics FromImage(Bitmap bm) {
         return new Graphics(new SKCanvas(bm.SKBitmap));
      }

      public void Clear(Color color) {
         SKCanvas.Clear(Helper.ConvertColor(color));
      }

      public void Flush() {
         SKCanvas.Flush();
      }

      #region Draw-Funktionen

      public void DrawLine(Pen pen, float x1, float y1, float x2, float y2) {
         if (pen.IsSolid) {
            SKCanvas.DrawLine(x1, y1, x2, y2, pen.SKPaintSolid);
         } else {
            if (y1 == y2) {
               if (pen.SKBitmap != null) {
                  float y = -pen.SKBitmap.Height / 2;
                  //x1 -= pen.SKBitmap.Height / 2;
                  //x2 += pen.SKBitmap.Height / 2;
                  for (float x = x1; x < x2; x += pen.SKBitmap.Width) {
                     if (x2 - x >= pen.SKBitmap.Width)
                        SKCanvas.DrawBitmap(pen.SKBitmap, x, y);
                     else if (x2 - x > 0)
                        SKCanvas.DrawBitmap(pen.SKBitmap,
                                            new SKRect(0, 0, x2 - x, pen.SKBitmap.Height),
                                            new SKRect(x, y, x2, y + pen.SKBitmap.Height));
                  }
               }

            } else
               throw new NotImplementedException("DrawLine() für TextureBrush z.Z. NUR bei waagerechten Linien möglich.");
         }
      }

      public void DrawLine(Pen pen, PointF p1, PointF p2) {
         DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y);
      }

      public void DrawLines(Pen pen, PointF[] pts) {
         if (pen.IsSolid) {
            SKPath sKPath = new SKPath();
            SKPoint[] skPoints = ConvertPoints(pts);
            sKPath.AddPoly(skPoints, false);
            SKCanvas.DrawPath(sKPath, pen.SKPaintSolid);
            sKPath.Dispose();
         } else {
            // als einzelne Segmente zeichnen ?

            // Nicht nötig?

            throw new NotImplementedException("DrawLines() für TextureBrush fehlt noch.");


         }
      }

      public void DrawLines(Pen pen, Point[] pts) {
         if (pen.IsSolid) {
            SKPath sKPath = new SKPath();
            SKPoint[] skPoints = ConvertPoints(pts);
            sKPath.AddPoly(skPoints, false);
            SKCanvas.DrawPath(sKPath, pen.SKPaintSolid);
            sKPath.Dispose();
         } else {
            // als einzelne Segmente zeichnen ?

            // Nicht nötig?

            throw new NotImplementedException("DrawLines() für TextureBrush fehlt noch.");


         }
      }

      public void DrawRectangle(Pen pen, float x, float y, float width, float height) {
         SKCanvas.DrawRect(x, y, width, height, pen.SKPaintSolid);
      }

      public void DrawRectangle(Pen pen, Rectangle rect) {
         SKCanvas.DrawRect(rect.X, rect.Y, rect.Width, rect.Height, pen.SKPaintSolid);
      }

      public void DrawRectangle(Pen pen, RectangleF rect) {
         SKCanvas.DrawRect(rect.X, rect.Y, rect.Width, rect.Height, pen.SKPaintSolid);
      }

      public void DrawPath(Pen pen, GraphicsPath path) {
         if (pen.IsSolid) {

            SKCanvas.DrawPath(path.SKPath, pen.SKPaintSolid);

         } else {
            // als einzelne Segmente zeichnen ?

            //using (SKPath.RawIterator iterator = path.SKPath.CreateRawIterator()) {

            //   SKPathVerb pathVerb = SKPathVerb.Move;
            //   SKPoint[] points = new SKPoint[4];
            //   SKPoint firstPoint = new SKPoint();
            //   SKPoint lastPoint = new SKPoint();

            //   int moves = 0;
            //   int lines = 0;

            //   while ((pathVerb = iterator.Next(points)) != SKPathVerb.Done) {
            //      switch (pathVerb) {
            //         case SKPathVerb.Move:
            //            lines = 0;
            //            moves++;
            //            break;

            //         case SKPathVerb.Line:
            //            lines++;
            //            break;

            //         case SKPathVerb.Close:
            //            Debug.WriteLine("      Close: Lines=" + lines);
            //            break;
            //      }
            //   }
            //   Debug.WriteLine("      End: Lines=" + lines + ", Moves=" + moves);
            //}

            throw new NotImplementedException("DrawPath() für TextureBrush fehlt noch.");

         }
      }

      List<List<SKPoint>> splitPath(SKPath path) {
         List<List<SKPoint>> parts = new List<List<SKPoint>>();

         using (SKPath.RawIterator iterator = path.CreateRawIterator()) {
            SKPathVerb pathVerb = SKPathVerb.Move;
            SKPoint[] points = new SKPoint[4];
            List<SKPoint> part = null;

            while ((pathVerb = iterator.Next(points)) != SKPathVerb.Done) {
               switch (pathVerb) {
                  case SKPathVerb.Move:
                     part = new List<SKPoint>();
                     parts.Add(part);
                     part.Add(new SKPoint(points[0].X, points[0].Y));
                     break;

                  case SKPathVerb.Line:
                     part?.Add(new SKPoint(points[1].X, points[1].Y));
                     break;

                  case SKPathVerb.Close:
                     part?.Add(new SKPoint(points[0].X, points[0].Y));
                     break;
               }
            }
         }
         return parts;
      }

      public void DrawEllipse(Pen pen, float x, float y, float width, float height) {
         if (width == height)
            SKCanvas.DrawCircle(x + width / 2,
                                y + height / 2,
                                width / 2,
                                pen.SKPaintSolid);
         else
            SKCanvas.DrawOval(x + width / 2,
                              y + height / 2,
                              width / 2,
                              height / 2,
                              pen.SKPaintSolid);
      }

      #endregion

      #region Fill-Funktionen

      public void FillPolygon(Brush brush, PointF[] pointFs) {
         SKPoint[] pt = ConvertPoints(pointFs);
         SKPath sKPath = new SKPath();
         sKPath.AddPoly(pt, true);
         if (brush.IsSolid) {

            SKCanvas.DrawPath(sKPath, brush.SKPaintSolid);

         } else {

            SKCanvas.Save();
            SKCanvas.ClipPath(sKPath);
            fillRectWithBrush(brush, getMinMax(pt));
            SKCanvas.Restore();

         }
         sKPath.Dispose();
      }

      public void FillPath(Brush brush, GraphicsPath path) {
         SKCanvas.Save();
         SKCanvas.ClipPath(path.SKPath);
         fillRectWithBrush(brush, path.SKPath.Bounds);
         SKCanvas.Restore();
      }

      public void FillRectangle(Brush brush, RectangleF rect) {
         fillRectWithBrush(brush, Helper.ConvertRect(rect));
      }

      public void FillRectangle(Brush brush, float x1, float y1, float width, float height) {
         FillRectangle(brush, new RectangleF(x1, y1, width, height));
      }

      void fillRectWithBrush(Brush brush, SKRect rect) {
         if (brush.IsSolid) {

            SKCanvas.DrawRect(rect, brush.SKPaintSolid);

         } else {

            if (brush.SKBitmap != null) {

               SizeF translation = brush is TextureBrush ?
                                       (brush as TextureBrush).Translation :
                                       new SizeF();
               for (float x = rect.Left; x < rect.Right; x += brush.SKBitmap.Width) {
                  for (float y = rect.Top; y < rect.Bottom; y += brush.SKBitmap.Height) {
                     SKCanvas.DrawBitmap(brush.SKBitmap, translation.Width + x, translation.Height + y);
                  }
               }

            } else if (brush.SKShader != null) {

               SKPaint sKPaint = new SKPaint() {
                  Shader = brush.SKShader,
               };
               SKCanvas.DrawRect(rect, sKPaint);
               sKPaint.Dispose();

            }
         }
      }

      public void FillEllipse(Brush brush, float x1, float y1, float width, float height) {
         SKRect rect = new SKRect(x1, y1, x1 + width, y1 + height);

         if (brush.IsSolid) {

            SKCanvas.DrawOval(rect, brush.SKPaintSolid);

         } else {

            SizeF translation = brush is TextureBrush ?
                                    (brush as TextureBrush).Translation :
                                    new SizeF();
            for (float x = rect.Left; x < rect.Right; x += brush.SKBitmap.Width) {
               for (float y = rect.Top; y < rect.Bottom; y += brush.SKBitmap.Height) {
                  SKCanvas.DrawBitmap(brush.SKBitmap, translation.Width + x, translation.Height + y);
               }
            }

         }
      }


      #endregion

      #region Bitmap zeichnen

      public void DrawImageUnscaled(Bitmap bitmap, int x, int y) {
         DrawImage(bitmap, x, y);
      }

      public void DrawImage(Bitmap bitmap, float x, float y, float width, float height) {
         SKCanvas.DrawBitmap(bitmap.SKBitmap, new SKRect(x, y, x + width, y + height));
      }

      public void DrawImage(Bitmap bitmap, int x, int y) {
         SKCanvas.DrawBitmap(bitmap.SKBitmap, x, y);
      }

      public void DrawImage(Bitmap bitmap, RectangleF dstRect, RectangleF srcRect, GraphicsUnit graphicsunit) {
         if (graphicsunit != GraphicsUnit.Pixel)
            throw new NotImplementedException();
         SKCanvas.DrawBitmap(bitmap.SKBitmap, convertRect(srcRect), convertRect(dstRect));
      }

      #endregion

      /// <summary>
      /// Misst die angegebene Zeichenfolge, wenn diese mit der angegebenen Font gezeichnet und mit dem angegebenen StringFormat formatiert wird.
      /// </summary>
      /// <param name="text"></param>
      /// <param name="font"></param>
      /// <param name="lefttop"></param>
      /// <param name="sf"></param>
      /// <returns></returns>
      public SizeF MeasureString(string text, Font font, PointF lefttop, StringFormat sf) {
         return MeasureString(text, font);
      }

      public SizeF MeasureString(string text, Font font) {
         SKPaint sKPaint = new SKPaint() {
            Typeface = font.SKTypeface,
            TextSize = font.SizeInPointsF,
         };
         return new SizeF(sKPaint.MeasureText(text),
                          sKPaint.FontSpacing);          // recommend line spacing
                                                         // sKPaint.TextSize);          // in Pixel!
      }

      #region Textausgabe

      /// <summary>
      /// Zeichnet die angegebene Zeichenfolge an der angegebenen Position mit dem angegebenen Brush-Objekt und dem angegebenen Font-Objekt 
      /// unter Verwendung der Formatierungsattribute vom angegebenen StringFormat.
      /// </summary>
      /// <param name="text"></param>
      /// <param name="font"></param>
      /// <param name="brush"></param>
      /// <param name="pt">Skia-Koordinaten</param>
      /// <param name="sf"></param>
      public void DrawString(string text, Font font, Brush brush, PointF pt, StringFormat sf) {
         if (brush.IsSolid) {
            using (var paint = new SKPaint()) {
               paint.Color = brush.SKPaintSolid.Color;
               paint.IsStroke = false;
               paint.IsAntialias = true;
               paint.IsDither = true;
               paint.Typeface = font.SKTypeface;
               paint.TextSize = font.SizeInPointsF;
               paint.FilterQuality = SKFilterQuality.High;

               switch (sf.Alignment) {
                  case StringAlignment.Near:
                     paint.TextAlign = SKTextAlign.Left;
                     break;

                  case StringAlignment.Center:
                     paint.TextAlign = SKTextAlign.Center;
                     break;

                  case StringAlignment.Far:
                     paint.TextAlign = SKTextAlign.Right;
                     break;
               }

               switch (sf.LineAlignment) {
                  case StringAlignment.Near:
                     pt.Y += paint.FontSpacing;
                     break;

                  case StringAlignment.Center:
                     pt.Y += paint.FontSpacing / 2;
                     break;

                  case StringAlignment.Far:
                     break;
               }


               SKCanvas.DrawText(text, pt.X, pt.Y, paint);
            }
         } else {

            throw new NotImplementedException("DrawString() für TextureBrush fehlt noch.");

         }
      }

      public void DrawString(string text, Font font, Brush brush, PointF pt) {
         DrawString(text,
                    font,
                    brush,
                    pt,
                    new StringFormat() {
                       Alignment = StringAlignment.Near,
                       LineAlignment = StringAlignment.Near
                    });
      }

      public void DrawString(string text, Font font, Brush brush, float x, float y) {
         DrawString(text,
                    font,
                    brush,
                    new PointF(x, y),
                    new StringFormat() {
                       Alignment = StringAlignment.Near,
                       LineAlignment = StringAlignment.Near
                    });
      }

      /// <summary>
      /// Zeichnet die angegebene Textzeichenfolge in dem angegebenen Rechteck mit dem angegebenen Brush-Objekt und dem angegebenen Font-Objekt.
      /// </summary>
      /// <param name="text"></param>
      /// <param name="font"></param>
      /// <param name="brush"></param>
      /// <param name="rect">Skia-Koordinaten</param>
      public void DrawString(string text, Font font, SolidBrush brush, RectangleF rect) {
         DrawString(text,
                    font,
                    brush,
                    rect,
                    new StringFormat() {
                       Alignment = StringAlignment.Near,
                       LineAlignment = StringAlignment.Near,
                    });
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="text"></param>
      /// <param name="font"></param>
      /// <param name="brush"></param>
      /// <param name="rect">Skia-Koordinaten</param>
      /// <param name="sf"></param>
      public void DrawString(string text, Font font, SolidBrush brush, RectangleF rect, StringFormat sf) {
         SKCanvas.Save();
         SKCanvas.ClipRect(convertRect(rect));
         float x = 0;
         float y = 0;
         switch (sf.Alignment) {
            case StringAlignment.Near:
               x = rect.Left;
               break;
            case StringAlignment.Center:
               x = rect.Left + rect.Width / 2;
               break;
            case StringAlignment.Far:
               x = rect.Right;
               break;
         }
         switch (sf.LineAlignment) {
            case StringAlignment.Near:
               y = rect.Top;
               break;
            case StringAlignment.Center:
               y = rect.Top + rect.Height / 2;
               break;
            case StringAlignment.Far:
               y = rect.Bottom;
               break;
         }

         DrawString(text,
                    font,
                    brush,
                    new PointF(x, y),
                    sf);
         SKCanvas.Restore();
      }

      public void DrawStringWithOutline(string text, Font font, Brush brush, Pen pen, PointF pt, StringFormat sf) {
         if (brush.IsSolid) {
            using (var paint = new SKPaint()) {
               paint.Color = brush.SKPaintSolid.Color;
               paint.IsStroke = false;
               paint.IsAntialias = true;
               paint.IsDither = true;
               paint.Typeface = font.SKTypeface;
               paint.TextSize = font.SizeInPointsF;

               switch (sf.Alignment) {
                  case StringAlignment.Near:
                     paint.TextAlign = SKTextAlign.Left;
                     break;

                  case StringAlignment.Center:
                     paint.TextAlign = SKTextAlign.Center;
                     break;

                  case StringAlignment.Far:
                     paint.TextAlign = SKTextAlign.Right;
                     break;
               }

               switch (sf.LineAlignment) {
                  case StringAlignment.Near:
                     break;

                  case StringAlignment.Center:
                     pt.Y -= paint.FontSpacing / 2;
                     break;

                  case StringAlignment.Far:
                     pt.Y -= paint.FontSpacing;
                     break;
               }

               SKCanvas.DrawText(text, pt.X, pt.Y, paint);
            }
         } else {

            throw new NotImplementedException("DrawString() für TextureBrush fehlt noch.");

         }
      }

      #endregion

      public static SKPoint[] ConvertPoints(PointF[] pt) {
         SKPoint[] sKPoints = new SKPoint[pt.Length];
         for (int i = 0; i < pt.Length; i++) {
            sKPoints[i].X = pt[i].X;
            sKPoints[i].Y = pt[i].Y;
         }
         return sKPoints;
      }

      public static SKPoint[] ConvertPoints(Point[] pt) {
         SKPoint[] sKPoints = new SKPoint[pt.Length];
         for (int i = 0; i < pt.Length; i++) {
            sKPoints[i].X = pt[i].X;
            sKPoints[i].Y = pt[i].Y;
         }
         return sKPoints;
      }


      static SKRect convertRect(RectangleF rect) {
         return new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
      }

      static SKRect getMinMax(IList<SKPoint> pt) {
         float minx, maxx, miny, maxy;

         minx = miny = float.MaxValue;
         maxx = maxy = float.MinValue;
         foreach (var item in pt) {
            minx = Math.Min(minx, item.X);
            miny = Math.Min(miny, item.Y);
            maxx = Math.Max(maxx, item.X);
            maxy = Math.Max(maxy, item.Y);
         }
         return new SKRect(minx, miny, maxx, maxy);
      }

      #region Transformation-Matrix

      SKMatrix appenMatrix(SKMatrix m) {
         return SKMatrix.Concat(m, SKCanvas.TotalMatrix);
      }

      public void ScaleTransform(float sx, float sy, MatrixOrder append) {
         if (append == MatrixOrder.Prepend)
            SKCanvas.Scale(sx, sy);
         else
            SKCanvas.SetMatrix(appenMatrix(SKMatrix.CreateScale(sx, sy)));
      }

      public void ScaleTransform(float sx, float sy, float pivotx, float pivoty, MatrixOrder append) {
         if (append == MatrixOrder.Prepend)
            SKCanvas.Scale(sx, sy, pivotx, pivoty);
         else
            SKCanvas.SetMatrix(appenMatrix(SKMatrix.CreateScale(sx, sy, pivotx, pivoty)));
      }

      public void TranslateTransform(float x, float y, MatrixOrder append = MatrixOrder.Prepend) {
         if (append == MatrixOrder.Prepend)
            SKCanvas.Translate(x, y);
         else
            SKCanvas.SetMatrix(appenMatrix(SKMatrix.CreateTranslation(x, y)));
      }

      /// <summary>
      /// Drehungswinkel in Grad
      /// </summary>
      /// <param name="angle"></param>
      public void RotateTransform(float angle) {
         SKCanvas.RotateDegrees(angle);
      }

      public void ResetTransform() {
         SKCanvas.ResetMatrix();
      }

      public string GetMatrixString() {
         return string.Format("{0} {1} / {2} {3} / {4} {5}",
                              SKCanvas.TotalMatrix.ScaleX, SKCanvas.TotalMatrix.SkewY,
                              SKCanvas.TotalMatrix.SkewX, SKCanvas.TotalMatrix.ScaleY,
                              SKCanvas.TotalMatrix.TransX, SKCanvas.TotalMatrix.TransY);
      }

      #endregion


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
               SKCanvas.Dispose();
            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion
   }
}