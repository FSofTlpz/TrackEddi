namespace System.Drawing.Drawing2D {
   //
   // Zusammenfassung:
   //     Gibt den Stil der mit gezeichneten gestrichelten Linien ein System.Drawing.Pen
   //     Objekt.
   public enum DashStyle {
      //
      // Zusammenfassung:
      //     Gibt eine durchgehende Linie.
      Solid = 0,
      //
      // Zusammenfassung:
      //     Gibt eine Zeile aus Strichen besteht.
      Dash = 1,
      //
      // Zusammenfassung:
      //     Gibt eine Zeile aus Punkten besteht.
      Dot = 2,
      //
      // Zusammenfassung:
      //     Gibt eine bestehend aus einer sich wiederholenden Strich-Punkt-Linie.
      DashDot = 3,
      //
      // Zusammenfassung:
      //     Gibt eine Zeile aus einer sich wiederholenden Strich-Punkt-Punkt besteht.
      DashDotDot = 4,
      //
      // Zusammenfassung:
      //     Gibt eine benutzerdefinierte Strichformat.
      Custom = 5
   }
}