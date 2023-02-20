using System.ComponentModel;
using System.Globalization;

namespace System.Drawing {
   //
   // Zusammenfassung:
   //     Stellt ein geordnetes Paar von x- und y-Koordinaten als Gleitkommazahlen dar,
   //     das einen Punkt in einem zweidimensionalen Raum definiert.
   public struct PointF {
      //
      // Zusammenfassung:
      //     Stellt eine neue Instanz der System.Drawing.PointF-Klasse ohne Initialisierung
      //     der Memberdaten dar.
      public static readonly PointF Empty;

      private float x;

      private float y;

      //
      // Zusammenfassung:
      //     Ruft einen Wert ab, der angibt, ob dieser System.Drawing.PointF leer ist.
      //
      // Rückgabewerte:
      //     true, wenn sowohl System.Drawing.PointF.X als auch System.Drawing.PointF.Y 0
      //     sind, andernfalls false.
      [Browsable(false)]
      public bool IsEmpty {
         get {
            if (x == 0f) {
               return y == 0f;
            }

            return false;
         }
      }

      //
      // Zusammenfassung:
      //     Ruft die x-Koordinate dieses System.Drawing.PointF ab oder legt diese fest.
      //
      // Rückgabewerte:
      //     Die x-Koordinate für diesen System.Drawing.PointF.
      public float X {
         get {
            return x;
         }
         set {
            x = value;
         }
      }

      //
      // Zusammenfassung:
      //     Ruft die y-Koordinate dieses System.Drawing.PointF ab oder legt diese fest.
      //
      // Rückgabewerte:
      //     Die y-Koordinate für diesen System.Drawing.PointF.
      public float Y {
         get {
            return y;
         }
         set {
            y = value;
         }
      }

      //
      // Zusammenfassung:
      //     Initialisiert eine neue Instanz der System.Drawing.PointF-Klasse mit den angegebenen
      //     Koordinaten.
      //
      // Parameter:
      //   x:
      //     Die horizontale Position des Punkts.
      //
      //   y:
      //     Die vertikale Position des Punkts.
      public PointF(float x, float y) {
         this.x = x;
         this.y = y;
      }

      //
      // Zusammenfassung:
      //     Verschiebt einen System.Drawing.PointF um eine angegebene System.Drawing.Size.
      //
      // Parameter:
      //   pt:
      //     Der zu verschiebende System.Drawing.PointF.
      //
      //   sz:
      //     Eine System.Drawing.Size, die das zu den Koordinaten von pt zu addierende Zahlenpaar
      //     angibt.
      //
      // Rückgabewerte:
      //     Der verschobene System.Drawing.PointF.
      public static PointF operator +(PointF pt, Size sz) {
         return Add(pt, sz);
      }

      //
      // Zusammenfassung:
      //     Verschiebt einen System.Drawing.PointF um den negativen Wert einer angegebenen
      //     System.Drawing.Size.
      //
      // Parameter:
      //   pt:
      //     Der zu verschiebende System.Drawing.PointF.
      //
      //   sz:
      //     Eine System.Drawing.Size, die die von den Koordinaten von pt zu subtrahierenden
      //     Zahlen angibt.
      //
      // Rückgabewerte:
      //     Der verschobene System.Drawing.PointF.
      public static PointF operator -(PointF pt, Size sz) {
         return Subtract(pt, sz);
      }

      //
      // Zusammenfassung:
      //     Verschiebt den System.Drawing.PointF um die angegebene System.Drawing.SizeF.
      //
      // Parameter:
      //   pt:
      //     Der zu verschiebende System.Drawing.PointF.
      //
      //   sz:
      //     Die System.Drawing.SizeF, die die Zahlen angibt, die zu den x- und y-Koordinaten
      //     des System.Drawing.PointF addiert werden sollen.
      //
      // Rückgabewerte:
      //     Der verschobene System.Drawing.PointF.
      public static PointF operator +(PointF pt, SizeF sz) {
         return Add(pt, sz);
      }

      //
      // Zusammenfassung:
      //     Verschiebt einen System.Drawing.PointF um den negativen Wert einer angegebenen
      //     System.Drawing.SizeF.
      //
      // Parameter:
      //   pt:
      //     Der zu verschiebende System.Drawing.PointF.
      //
      //   sz:
      //     Eine System.Drawing.SizeF, die die von den Koordinaten von pt zu subtrahierenden
      //     Zahlen angibt.
      //
      // Rückgabewerte:
      //     Der verschobene System.Drawing.PointF.
      public static PointF operator -(PointF pt, SizeF sz) {
         return Subtract(pt, sz);
      }

      //
      // Zusammenfassung:
      //     Vergleicht zwei System.Drawing.PointF-Strukturen. Das Ergebnis gibt an, ob die
      //     Werte der System.Drawing.PointF.X-Eigenschaft und der System.Drawing.PointF.Y-Eigenschaft
      //     der beiden System.Drawing.PointF-Strukturen gleich sind.
      //
      // Parameter:
      //   left:
      //     Ein zu vergleichender System.Drawing.PointF.
      //
      //   right:
      //     Ein zu vergleichender System.Drawing.PointF.
      //
      // Rückgabewerte:
      //     true, wenn der System.Drawing.PointF.X-Wert und der System.Drawing.PointF.Y-Wert
      //     der linken und rechten System.Drawing.PointF-Strukturen gleich sind, andernfalls
      //     false.
      public static bool operator ==(PointF left, PointF right) {
         if (left.X == right.X) {
            return left.Y == right.Y;
         }

         return false;
      }

      //
      // Zusammenfassung:
      //     Bestimmt, ob die Koordinaten der angegebenen Punkte ungleich sind.
      //
      // Parameter:
      //   left:
      //     Ein zu vergleichender System.Drawing.PointF.
      //
      //   right:
      //     Ein zu vergleichender System.Drawing.PointF.
      //
      // Rückgabewerte:
      //     true, um anzugeben, dass der System.Drawing.PointF.X-Wert und der System.Drawing.PointF.Y-Wert
      //     von left und right ungleich sind, andernfalls false.
      public static bool operator !=(PointF left, PointF right) {
         return !(left == right);
      }

      //
      // Zusammenfassung:
      //     Verschiebt einen angegebenen System.Drawing.PointF um die angegebene System.Drawing.Size.
      //
      // Parameter:
      //   pt:
      //     Der zu verschiebende System.Drawing.PointF.
      //
      //   sz:
      //     Eine System.Drawing.Size, die die zu den Koordinaten von pt zu addierenden Zahlen
      //     angibt.
      //
      // Rückgabewerte:
      //     Der verschobene System.Drawing.PointF.
      public static PointF Add(PointF pt, Size sz) {
         return new PointF(pt.X + (float)sz.Width, pt.Y + (float)sz.Height);
      }

      //
      // Zusammenfassung:
      //     Verschiebt einen System.Drawing.PointF um den negativen Wert einer angegebenen
      //     Größe.
      //
      // Parameter:
      //   pt:
      //     Der zu verschiebende System.Drawing.PointF.
      //
      //   sz:
      //     Eine System.Drawing.Size, die die von den Koordinaten von pt zu subtrahierenden
      //     Zahlen angibt.
      //
      // Rückgabewerte:
      //     Der verschobene System.Drawing.PointF.
      public static PointF Subtract(PointF pt, Size sz) {
         return new PointF(pt.X - (float)sz.Width, pt.Y - (float)sz.Height);
      }

      //
      // Zusammenfassung:
      //     Verschiebt einen angegebenen System.Drawing.PointF um eine angegebene System.Drawing.SizeF.
      //
      // Parameter:
      //   pt:
      //     Der zu verschiebende System.Drawing.PointF.
      //
      //   sz:
      //     Eine System.Drawing.SizeF, die die zu den Koordinaten von pt zu addierenden Zahlen
      //     angibt.
      //
      // Rückgabewerte:
      //     Der verschobene System.Drawing.PointF.
      public static PointF Add(PointF pt, SizeF sz) {
         return new PointF(pt.X + sz.Width, pt.Y + sz.Height);
      }

      //
      // Zusammenfassung:
      //     Verschiebt einen System.Drawing.PointF um den negativen Wert einer angegebenen
      //     Größe.
      //
      // Parameter:
      //   pt:
      //     Der zu verschiebende System.Drawing.PointF.
      //
      //   sz:
      //     Eine System.Drawing.SizeF, die die von den Koordinaten von pt zu subtrahierenden
      //     Zahlen angibt.
      //
      // Rückgabewerte:
      //     Der verschobene System.Drawing.PointF.
      public static PointF Subtract(PointF pt, SizeF sz) {
         return new PointF(pt.X - sz.Width, pt.Y - sz.Height);
      }

      //
      // Zusammenfassung:
      //     Gibt an, ob dieser System.Drawing.PointF dieselben Koordinaten wie das angegebene
      //     System.Object enthält.
      //
      // Parameter:
      //   obj:
      //     Der zu überprüfende System.Object.
      //
      // Rückgabewerte:
      //     Diese Methode gibt true zurück, wenn obj ein System.Drawing.PointF ist und dieselben
      //     Koordinaten wie dieser System.Drawing.Point hat.
      public override bool Equals(object obj) {
         if (!(obj is PointF)) {
            return false;
         }

         PointF pointF = (PointF)obj;
         if (pointF.X == X && pointF.Y == Y) {
            return pointF.GetType().Equals(GetType());
         }

         return false;
      }

      //
      // Zusammenfassung:
      //     Gibt einen Hashcode für diese System.Drawing.PointF-Struktur zurück.
      //
      // Rückgabewerte:
      //     Ein ganzzahliger Wert, der einen Hashwert für diese System.Drawing.PointF-Struktur
      //     angibt.
      public override int GetHashCode() {
         return base.GetHashCode();
      }

      //
      // Zusammenfassung:
      //     Konvertiert diesen System.Drawing.PointF in eine Klartextzeichenfolge.
      //
      // Rückgabewerte:
      //     Eine Zeichenfolge, die diese System.Drawing.PointF darstellt.
      public override string ToString() {
         return string.Format(CultureInfo.CurrentCulture, "{{X={0}, Y={1}}}", new object[2] { x, y });
      }
   }
}
