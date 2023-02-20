using System.Drawing.Drawing2D;
using SkiaSharp;
using SkiaWrapper;

namespace System.Drawing {
   public class Pen {

      public SKPaint SKPaintSolid { get; protected set; }
      public SKBitmap SKBitmap { get; protected set; }
      //public TextureBrush InternalBrush { get; protected set; }

      public bool IsSolid => SKBitmap == null;

      public Color Color => Helper.ConvertColor(SKPaintSolid.Color);

      public float Width {
         get {
            return SKPaintSolid.StrokeWidth;
         }
         set {
            SKPaintSolid.StrokeWidth = value;
         }
      }

      public LineJoin LineJoin {
         get {
            return ConvertJoin(SKPaintSolid.StrokeJoin);
         }
         set {
            SKPaintSolid.StrokeJoin = ConvertJoin(value);
         }
      }

      LineCap _startCap = LineCap.Flat;

      public LineCap StartCap {
         get {
            return _startCap; // ConvertCap(SKPaintSolid.StrokeCap);
         }
         set {
            _startCap = value;
            SKPaintSolid.StrokeCap = ConvertCap(_startCap);
         }
      }

      LineCap _endCap = LineCap.Flat;

      public LineCap EndCap {
         get {
            return _endCap; // ConvertCap(SKPaintSolid.StrokeCap);
         }
         set {
            _endCap = value;
            SKPaintSolid.StrokeCap = ConvertCap(_endCap);
         }
      }

      public PenAlignment Alignment { get; set; }


      DashStyle _dashStyle = DashStyle.Solid;

      public DashStyle DashStyle {
         get => _dashStyle;
         set {
            switch (value) {
               //case DashStyle.Solid:
               //case DashStyle.Custom:
               default:
                  SKPaintSolid.PathEffect = null;
                  _dashStyle = value;
                  break;

               case DashStyle.Dash:
                  SKPaintSolid.PathEffect = SKPathEffect.CreateDash(new float[] { 17, 8 }, 25);
                  _dashStyle = value;
                  break;
               case DashStyle.Dot:
                  SKPaintSolid.PathEffect = SKPathEffect.CreateDash(new float[] { 5, 5 }, 10);
                  _dashStyle = value;
                  break;
               case DashStyle.DashDot:
                  SKPaintSolid.PathEffect = SKPathEffect.CreateDash(new float[] { 20, 5, 5, 5 }, 35);
                  _dashStyle = value;
                  break;
               case DashStyle.DashDotDot:
                  SKPaintSolid.PathEffect = SKPathEffect.CreateDash(new float[] { 20, 5, 5, 5, 5, 5 }, 35);
                  _dashStyle = value;
                  break;
            }
         }
      }

      //
      // Zusammenfassung:
      //     Ruft ein benutzerdefiniertes Ende ab, das am Anfang der mit diesem System.Drawing.Pen
      //     gezeichneten Linien verwendet werden soll, oder legt dieses fest.
      //
      // Rückgabewerte:
      //     Ein System.Drawing.Drawing2D.CustomLineCap, das das Ende darstellt, das am Anfang
      //     der mit diesem System.Drawing.Pen gezeichneten Linien verwendet wird.
      //
      // Ausnahmen:
      //   T:System.ArgumentException:
      //     Die System.Drawing.Pen.CustomStartCap festgelegt wird auf einen unveränderlichen
      //     System.Drawing.Pen, z. B. diejenigen, die von der System.Drawing.Pens Klasse.
      public CustomLineCap CustomStartCap { get; set; }
      //
      // Zusammenfassung:
      //     Ruft ein benutzerdefiniertes Ende ab, das am Ende der mit diesem System.Drawing.Pen
      //     gezeichneten Linien verwendet werden soll, oder legt dieses fest.
      //
      // Rückgabewerte:
      //     Ein System.Drawing.Drawing2D.CustomLineCap, das das Ende darstellt, das am Ende
      //     der mit diesem System.Drawing.Pen gezeichneten Linien verwendet wird.
      //
      // Ausnahmen:
      //   T:System.ArgumentException:
      //     Die System.Drawing.Pen.CustomEndCap festgelegt wird auf einen unveränderlichen
      //     System.Drawing.Pen, z. B. diejenigen, die von der System.Drawing.Pens Klasse.
      public CustomLineCap CustomEndCap { get; set; }


      public Pen(Color col) :
         this(col, 1) { }

      public Pen(Color col, float width) {
         SKPaintSolid = new SKPaint() {
            Color = Helper.ConvertColor(col),
            IsAntialias = true,
            IsStroke = true,
            StrokeWidth = width,
            StrokeCap = SKStrokeCap.Butt,          // https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/paths/lines
            StrokeJoin = SKStrokeJoin.Miter,       // https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/paths/paths
         };
         SKBitmap = null;
      }

      public Pen(TextureBrush tb, float width) :
         this(Color.Black, width) {
         SKBitmap = tb.SKBitmap.Copy();
      }

      public Pen(Brush brush, float width) :
         this(brush.IsSolid ?
                  Helper.ConvertColor(brush.SKPaintSolid.Color) :
                  Color.Black,
              width) {
      }


      /*
Windows:    https://learn.microsoft.com/de-de/dotnet/api/system.drawing.drawing2d.linecap?view=dotnet-plat-ext-6.0
            (http://www.java2s.com/Code/CSharp/2D-Graphics/AllLineCapillustration.htm)
   AnchorMask 	      240 	Gibt ein Maske an, mit der überprüft wird, ob es sich bei einem Linienende um das Ende eines Ankers handelt.
   ArrowAnchor 	   20 	Gibt ein pfeilförmiges Ankerende an.
   Custom 	         255 	Gibt ein benutzerdefiniertes Linienende an.
   DiamondAnchor 	   19 	Gibt ein rautenförmiges Ankerende an.
   Flat 	            0 	   Gibt ein abgeflaches Linienende an.
   NoAnchor 	      16 	Gibt an, dass kein Anker verwendet wird.
   Round 	         2 	   Gibt ein rundes Linienende an.
   RoundAnchor 	   18 	Gibt ein rundes Ankerende an.
   Square 	         1 	   Gibt ein quadratisches Linienende an.
   SquareAnchor 	   17 	Gibt ein quadratisches Ankerlinienende an.
   Triangle 	      3 	   Gibt ein dreieckiges Linienende an.

Skia:          https://learn.microsoft.com/de-de/xamarin/xamarin-forms/user-interface/graphics/skiasharp/paths/lines

   Butt (the default)      flach, direkt am Linienende            <-> Flat
   Square                  flach, zusätzlich zum Linienende       <-> Square
   Round                   abgerundet                             <-> Round
       */


      static SKStrokeCap ConvertCap(LineCap cap) {
         switch (cap) {
            case LineCap.Round:
               return SKStrokeCap.Round;

            case LineCap.Square:
               return SKStrokeCap.Square;

            case LineCap.Flat:
               return SKStrokeCap.Butt;

            default:
               return SKStrokeCap.Butt;
         }
      }

      static LineCap ConvertCap(SKStrokeCap cap) {
         switch (cap) {
            case SKStrokeCap.Round:
               return LineCap.Round;

            case SKStrokeCap.Butt:
               return LineCap.Flat;

            case SKStrokeCap.Square:
               return LineCap.Square;

            default:
               return LineCap.Flat;
         }
      }

      static SKStrokeJoin ConvertJoin(LineJoin join) {
         switch (join) {
            case LineJoin.Miter:
               return SKStrokeJoin.Miter;

            case LineJoin.Round:
               return SKStrokeJoin.Round;

            case LineJoin.Bevel:
               return SKStrokeJoin.Bevel;
         }
         return SKStrokeJoin.Miter;
      }

      static LineJoin ConvertJoin(SKStrokeJoin join) {
         switch (join) {
            case SKStrokeJoin.Miter:
               return LineJoin.Miter;

            case SKStrokeJoin.Round:
               return LineJoin.Round;

            case SKStrokeJoin.Bevel:
               return LineJoin.Bevel;
         }
         return LineJoin.Miter;
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
               if (SKPaintSolid != null) {
                  SKPaintSolid.Dispose();
                  SKPaintSolid = null;
               }
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