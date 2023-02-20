using SkiaSharp;
using SkiaWrapper;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;

namespace System.Drawing.Drawing2D {
   public class GraphicsPath : IDisposable {

      public SKPath SKPath { get; protected set; }


      public GraphicsPath() :
         this(new SKPath()) { }

      public GraphicsPath(SKPath path) {
         SKPath = path;
      }

      public GraphicsPath(GraphicsPath path) :
         this(path.SKPath) { }

      /// <summary>
      /// Fügt ein Liniensegment an diesen GraphicsPath an.
      /// </summary>
      /// <param name="x1"></param>
      /// <param name="y1"></param>
      /// <param name="x2"></param>
      /// <param name="y2"></param>
      public void AddLine(float x1, float y1, float x2, float y2) {
         SKPath.AddPoly(new SKPoint[] { new SKPoint(x1, y1), new SKPoint(x2, y2) }, false);
      }

      /// <summary>
      /// Fügt diesem Pfad ein Rechteck hinzu.
      /// </summary>
      /// <param name="rect"></param>
      public void AddRectangle(RectangleF rect) {
         SKPath.AddRect(new SKRect(rect.X, rect.Y, rect.Right, rect.Bottom));
      }

      /// <summary>
      /// Fügt diesem Pfad eine Ellipse hinzu.
      /// </summary>
      /// <param name="x1"></param>
      /// <param name="y1"></param>
      /// <param name="x2"></param>
      /// <param name="y2"></param>
      public void AddEllipse(float x1, float y1, float x2, float y2) {
         SKPath.AddOval(new SKRect(x1, y1, x2, y2));
      }

      /// <summary>
      /// Fügt eine Abfolge verbundener Liniensegmente an das Ende dieses GraphicsPath an.
      /// </summary>
      /// <param name="pt"></param>
      public void AddLines(PointF[] pt) {
         SKPath.AddPoly(Helper.ConvertPointsF(pt), false);
      }

      /// <summary>
      /// Fügt eine Abfolge verbundener Liniensegmente an das Ende dieses GraphicsPath an.
      /// </summary>
      /// <param name="pt"></param>
      public void AddLines(Point[] pt) {
         SKPath.AddPoly(Helper.ConvertPoints(pt), false);
      }

      /// <summary>
      /// Fügt diesem Pfad ein Vieleck hinzu.
      /// </summary>
      /// <param name="pt"></param>
      public void AddPolygon(Point[] pt) {
         SKPath.AddPoly(Helper.ConvertPoints(pt), true);
      }

      /// <summary>
      /// Fügt den angegebenen GraphicsPath an diesen Pfad an.
      /// </summary>
      /// <param name="path"></param>
      /// <param name="connect"></param>
      public void AddPath(GraphicsPath path, bool connect) {
         SKPath.AddPath(path.SKPath, connect ? SKPathAddMode.Append : SKPathAddMode.Extend);
      }

      /// <summary>
      /// Fügt einen Ellipsenbogen an die aktuelle Figur an. (Winkel in Grad)
      /// </summary>
      /// <param name="x">Die x-Koordinate der linken oberen Ecke des rechteckigen Bereichs, der die Ellipse definiert, aus der der zu zeichnende Bogen stammt.</param>
      /// <param name="y">Die y-Koordinate der linken oberen Ecke des rechteckigen Bereichs, der die Ellipse definiert, aus der der zu zeichnende Bogen stammt.</param>
      /// <param name="width">Die Breite des rechteckigen Bereichs, der die Ellipse definiert, aus der der zu zeichnende Bogen stammt.</param>
      /// <param name="height">Die Höhe des rechteckigen Bereichs, der die Ellipse definiert, aus der der zu zeichnende Bogen stammt.</param>
      /// <param name="startAngle">Der Startwinkel des Bogens in Grad, von der x-Achse im Uhrzeigersinn gemessen.</param>
      /// <param name="sweepAngle">Der Winkel zwischen startAngle und dem Ende des Bogens.</param>
      public void AddArc(float x, float y, float width, float height, int startAngle, int sweepAngle) {
         SKPath.AddArc(new SKRect(x, y, x + width, y + height), startAngle, sweepAngle);
      }

      public void AddString(string text, Font font, PointF origin, StringFormat sf) {
         using (var paint = new SKPaint()) {
            paint.IsStroke = false;
            paint.IsAntialias = true;
            paint.Typeface = font.SKTypeface;
            paint.TextSize = font.SizeInPointsF;

            // Es scheint so, dass der mit GetTextPath() erzeugte SKPath IMMER linksbündig und von 0 nach ausgerichtet ist.
            // Das wäre dann ein Fehler in der API.
            // Deshalb wird expliziet ein solcher SKPath gebildet und mit den den Daten des umgebenden Rechtecks korrigiert.

            SKPath textpath = paint.GetTextPath(text, 0, 0);
            SKRect bounds = textpath.Bounds;

            float dx = origin.X, dy = origin.Y;
            switch (sf.Alignment) {
               case StringAlignment.Near:
                  break;

               case StringAlignment.Center:
                  dx -= bounds.Width / 2;
                  break;

               case StringAlignment.Far:
                  dx -= bounds.Width;
                  break;
            }

            switch (sf.LineAlignment) {
               case StringAlignment.Near:
                  dy += paint.FontSpacing;
                  break;

               case StringAlignment.Center:
                  dy += paint.FontSpacing / 2;
                  break;

               case StringAlignment.Far:
                  break;
            }

            SKPath.AddPath(textpath, dx, dy);
         }
      }

      /// <summary>
      /// Fügt diesem Pfad eine Textzeichenfolge hinzu.
      /// </summary>
      /// <param name="text"></param>
      /// <param name="family"></param>
      /// <param name="style"></param>
      /// <param name="emSize"></param>
      /// <param name="origin"></param>
      /// <param name="sf"></param>
      public void AddString(string text, string family, int style, float emSize, PointF origin, StringFormat sf) {
         using (var paint = new SKPaint()) {
            paint.IsStroke = false;
            paint.IsAntialias = true;
            paint.Typeface = SKTypeface.FromFamilyName(
                              family,
                              (style & (int)FontStyle.Bold) != 0 ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,   // Invisible, Thin, ... Normal ... Black
                              SKFontStyleWidth.Normal,                                                                  // UltraCondensed, ... Normal ... ExtraExpanded
                              (style & (int)FontStyle.Italic) != 0 ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
            paint.TextSize = emSize;         // ????     canvas.DpiY * font.SizeInPoints / 72,       // point -> em size

            //switch (sf.Alignment) {
            //   case StringAlignment.Near:
            //      paint.TextAlign = SKTextAlign.Left;
            //      break;

            //   case StringAlignment.Center:
            //      paint.TextAlign = SKTextAlign.Center;
            //      break;

            //   case StringAlignment.Far:
            //      paint.TextAlign = SKTextAlign.Right;
            //      break;
            //}

            //switch (sf.LineAlignment) {
            //   case StringAlignment.Near:
            //      break;

            //   case StringAlignment.Center:
            //      origin.Y -= paint.FontSpacing / 2;
            //      break;

            //   case StringAlignment.Far:
            //      origin.Y -= paint.FontSpacing;
            //      break;
            //}

            //Internal.AddPath(paint.GetTextPath(text, origin.X, origin.Y));


            // Es scheint so, dass der mit GetTextPath() erzeugte SKPath IMMER linksbündig und von 0 nach ausgerichtet ist.
            // Das wäre dann ein Fehler in der API.
            // Deshalb wird expliziet ein solcher SKPath gebildet und mit den den Daten des umgebenden Rechtecks korrigiert.

            SKPath textpath = paint.GetTextPath(text, 0, 0);
            SKRect bounds = textpath.Bounds;

            float dx = origin.X, dy = origin.Y;
            switch (sf.Alignment) {
               case StringAlignment.Near:
                  break;

               case StringAlignment.Center:
                  dx -= bounds.Width / 2;
                  break;

               case StringAlignment.Far:
                  dx -= bounds.Width;
                  break;
            }

            switch (sf.LineAlignment) {
               case StringAlignment.Near:
                  dy += paint.FontSpacing;
                  break;

               case StringAlignment.Center:
                  dy += paint.FontSpacing / 2;
                  break;

               case StringAlignment.Far:
                  break;
            }

            SKPath.AddPath(textpath, dx, dy);

         }
      }

      /// <summary>
      /// Leert das PathPoints-Array und das PathTypes-Array und legt FillMode auf Alternate fest.
      /// </summary>
      public void Reset() {
         SKPath.Reset();
         //Internal.FillType= SKPathFillType.
      }

      /// <summary>
      /// Schließt die aktuelle Figur und beginnt eine neue. Wenn die aktuelle Figur eine Abfolge verbundener Linien und Kurven enthält, schließt die 
      /// Methode die Schleife, indem End- und Anfangspunkt durch eine Linie verbunden werden.
      /// </summary>
      public void CloseFigure() {
         SKPath.Close();
      }

      /// <summary>
      /// MS: Gets the last point in the PathPoints array of this GraphicsPath.
      /// </summary>
      /// <returns></returns>
      public PointF GetLastPoint() {
         Helper.ConvertPoint(SKPath.LastPoint, out PointF pt);
         return pt;
      }

      /// <summary>
      /// Weist diesem GraphicsPath eine Transformationsmatrix zu.
      /// </summary>
      /// <param name="m"></param>
      public void Transform(Matrix m) {
         SKPath.Transform(m.SKMatrix);
      }

      /// <summary>
      /// Gibt ein Rechteck zurück, das diesen GraphicsPath umschließt.
      /// </summary>
      /// <returns></returns>
      public RectangleF GetBounds() {
         SKRect rect = SKPath.Bounds;
         return new RectangleF(rect.Left, rect.Top, rect.Width, rect.Height);
      }

      /// <summary>
      /// Gibt an, dass der angegebene Punkt in diesem GraphicsPath enthalten ist.
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      public bool IsVisible(int x, int y) {
         return SKPath.Contains(x, y);
      }

      /// <summary>
      /// Zeigt an, ob sich der angegebene Punkt auf bzw. unter dem Umriss dieses GraphicsPath befindet, wenn er mit dem angegebenen Pen gezeichnet wurde.
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <param name="pen"></param>
      /// <returns></returns>
      //public bool IsOutlineVisible1(int x, int y, Pen pen) {
      //   SKPath testpath = new SKPath();
      //   // Überscheidet sich der Pfad mit einem Quadrat um den gewünschten Punkt mit der Seitenlänge des Pen?
      //   testpath.AddRect(new SKRect(x - pen.Width / 2, y - pen.Width / 2, x + pen.Width / 2, y + pen.Width / 2));
      //   SKPath result = SKPath.Op(testpath, SKPathOp.Intersect);
      //   return !result.IsEmpty;
      //}


      #region Implementierung der IDisposable-Schnittstelle

      /// <summary>
      /// true, wenn schon ein Dispose() erfolgte
      /// </summary>
      private bool _isdisposed = false;

      /// <summary>
      /// kann explizit für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
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
               if (SKPath != null) {
                  SKPath.Dispose();
                  SKPath = null;
               }
            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion

   }
}