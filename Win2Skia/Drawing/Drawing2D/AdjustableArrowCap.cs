using System;
using System.Collections.Generic;
using System.Text;

namespace System.Drawing.Drawing2D {
   //
   // Zusammenfassung:
   //     Stellt eine veränderbare pfeilförmigen Linienende dar. Diese Klasse kann nicht
   //     vererbt werden.
   public sealed class AdjustableArrowCap : CustomLineCap {
      //
      // Zusammenfassung:
      //     Initialisiert eine neue Instanz der System.Drawing.Drawing2D.AdjustableArrowCap
      //     -Klasse mit der angegebenen Breite und Höhe. Die mit diesem Konstruktor erstellten
      //     Pfeil Linienenden werden immer angegeben.
      //
      // Parameter:
      //   width:
      //     Die Breite des Pfeils.
      //
      //   height:
      //     Die Höhe des Pfeils.
      public AdjustableArrowCap(float width, float height) : base(null, null) {

      }

      //
      // Zusammenfassung:
      //     Initialisiert eine neue Instanz der System.Drawing.Drawing2D.AdjustableArrowCap
      //     -Klasse mit der angegebenen Breite, Höhe und Fill-Eigenschaft. Ob ein Pfeilende
      //     ausgefüllt wird, hängt davon ab, das an übergebene Argument der isFilled Parameter.
      //
      // Parameter:
      //   width:
      //     Die Breite des Pfeils.
      //
      //   height:
      //     Die Höhe des Pfeils.
      //
      //   isFilled:
      //     true um das Pfeilende auszufüllen; andernfalls false.
      //public AdjustableArrowCap(float width, float height, bool isFilled);

      //
      // Zusammenfassung:
      //     Ruft ab oder legt die Höhe der Pfeilende fest.
      //
      // Rückgabewerte:
      //     Die Höhe des das Pfeilende.
      public float Height { get; set; }
      //
      // Zusammenfassung:
      //     Ruft ab oder legt die Breite der Abdeckung Pfeil.
      //
      // Rückgabewerte:
      //     Die Breite des Pfeilendes in Einheiten.
      public float Width { get; set; }
      //
      // Zusammenfassung:
      //     Ruft ab oder legt die Anzahl der Einheiten zwischen dem Umriss des das Pfeilende
      //     und die Füllung fest.
      //
      // Rückgabewerte:
      //     Die Anzahl der Einheiten zwischen dem Umriss des das Pfeilende und die Füllung
      //     für das Pfeilende.
      public float MiddleInset { get; set; }
      //
      // Zusammenfassung:
      //     Ruft ab oder legt fest, ob das Pfeilende ausgefüllt ist.
      //
      // Rückgabewerte:
      //     Diese Eigenschaft ist true ist das Pfeilende gefüllte; andernfalls false.
      public bool Filled { get; set; }
   }
}