using SkiaSharp;

namespace System.Drawing {

   public class Font {

      public SKTypeface SKTypeface { get; protected set; }

      public string FontFamilyname => SKTypeface.FamilyName;




      /// <summary>
      /// Die Geviertgröße dieser Font in Punkt.
      /// </summary>
      public int SizeInPoints {
         get {
            return (int)Math.Round(SizeInPointsF);
         }
      }

      /// <summary>
      /// Die Geviertgröße dieser Font in Punkt.
      /// </summary>
      public float SizeInPointsF { get; protected set; }

      public float Height {
         get => GetHeight();
      }

      /// <summary>
      /// Eine FontStyle-Enumeration, die Informationen zum Schriftschnitt für diese Font enthält.
      /// </summary>
      public FontStyle Style {
         get {
            return (Bold ? FontStyle.Bold : FontStyle.Regular) |
                   (Italic ? FontStyle.Italic : FontStyle.Regular);
         }
      }

      public bool Bold {
         get {
            return SKTypeface.IsBold;
         }
         set {
            setTypeface(SKTypeface.FamilyName, value, SKTypeface.IsItalic);
         }
      }

      public bool Italic {
         get {
            return SKTypeface.IsItalic;
         }
         set {
            setTypeface(SKTypeface.FamilyName, SKTypeface.IsBold, value);
         }
      }

      public float Size => SizeInPointsF;



      /// <summary>
      /// 
      /// </summary>
      /// <param name="fontfamily">Eine Zeichenfolgendarstellung der FontFamily für die neue Font.</param>
      /// <param name="emSize">Die Geviertgröße der neuen Schriftart in Punkt.</param>
      public Font(string fontfamily, float emSize) {
         setTypeface(fontfamily, false, false);
         SizeInPointsF = emSize;
      }

      public Font(string fontfamily, float emSize, FontStyle style, GraphicsUnit unit = GraphicsUnit.Pixel) {
         setTypeface(fontfamily, (style | FontStyle.Bold) != 0, (style | FontStyle.Italic) != 0);
         SizeInPointsF = emSize;
      }

      public Font(FontFamily fontfamily, float emSize, FontStyle style, GraphicsUnit unit = GraphicsUnit.Pixel) :
         this(fontfamily.Name, emSize, style, unit) { }

      public Font(SKTypeface typeface, float emSize) {
         SKTypeface = typeface;
         SizeInPointsF = emSize;
      }


      /// <summary>
      /// Gibt den Zeilenabstand dieser Schriftart in Pixel zurück.
      /// </summary>
      /// <returns></returns>
      public float GetHeight() {
         SKPaint sKPaint = new SKPaint() {
            Typeface = SKTypeface,
            TextSize = SizeInPointsF,
         };
         return sKPaint.FontSpacing; // recommend line spacing
      }

      public void Dispose() {
         if (SKTypeface != null) {
            SKTypeface.Dispose();
            SKTypeface = null;
         }
      }


      void setTypeface(string fontfamily, bool bold = false, bool italic = false) {
         if (SKTypeface != null) {
            SKTypeface.Dispose();
            SKTypeface = null;
         }
         SKTypeface = getTypeface(fontfamily, bold, italic);
      }

      SKTypeface getTypeface(string fontfamily, bool bold = false, bool italic = false) {
         return SKTypeface.FromFamilyName(
                              fontfamily,
                              bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,    // Invisible, Thin, ... Normal ... Black
                              SKFontStyleWidth.Normal,                                     // UltraCondensed, ... Normal ... ExtraExpanded
                              italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);

      }


   }
}