namespace System.Drawing {

   public enum GraphicsUnit {
      World = 0,        // Gibt die Einheit des globalen Koordinatensystems als Maßeinheit an.
      Display = 1,      // Gibt die Maßeinheit des Anzeigegerätes an. Für Bildschirme ist dies i. d. R. Pixel und 1/100 Zoll für Drucker.
      Pixel = 2,        // Gibt ein Gerätepixel als Maßeinheit an.
      Point = 3,        // Gibt einen Druckerpunkt (1/72 Zoll) als Maßeinheit an.
      Inch = 4,         // Gibt Zoll als Maßeinheit an.
      Document = 5,     // Gibt die Dokumenteinheit (1/300 Zoll) als Maßeinheit an.
      Millimeter = 6,   // Gibt Millimeter als Maßeinheit an.
   }

   public enum StringAlignment {
      Near,
      Center,
      Far
   }

   [Flags]
   public enum FontStyle {
      Regular = 0,      // Normaler Text.
      Bold = 1,         // Fett formatierter Text.
      Italic = 2,       // Kursiv formatierter Text.
      Underline = 4,    // Unterstrichener Text.
      Strikeout = 8,    // Durchgestrichener Text.
   }

}
