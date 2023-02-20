using System.ComponentModel;
using System.Globalization;

namespace System.Drawing {
   //
   // Zusammenfassung:
   //     Speichert einen Satz von vier ganzen Zahlen, die die Position und Größe eines
   //     Rechtecks angeben.
   public struct Rectangle {
      //
      // Zusammenfassung:
      //     Stellt eine System.Drawing.Rectangle-Struktur mit nicht initialisierten Eigenschaften
      //     dar.
      public static readonly Rectangle Empty;

      private int x;

      private int y;

      private int width;

      private int height;

      //
      // Zusammenfassung:
      //     Ruft die Koordinaten der linken oberen Ecke dieser System.Drawing.Rectangle-Struktur
      //     ab oder legt diese fest.
      //
      // Rückgabewerte:
      //     Ein System.Drawing.Point, der die linke obere Ecke dieser System.Drawing.Rectangle-Struktur
      //     darstellt.
      public Point Location {
         get {
            return new Point(X, Y);
         }
         set {
            X = value.X;
            Y = value.Y;
         }
      }

      //
      // Zusammenfassung:
      //     Ruft die Größe dieses System.Drawing.Rectangle ab oder legt sie fest.
      //
      // Rückgabewerte:
      //     Eine System.Drawing.Size, die die Breite und Höhe dieser System.Drawing.Rectangle-Struktur
      //     darstellt.
      public Size Size {
         get {
            return new Size(Width, Height);
         }
         set {
            Width = value.Width;
            Height = value.Height;
         }
      }

      //
      // Zusammenfassung:
      //     Ruft die x-Koordinate der linken oberen Ecke dieser System.Drawing.Rectangle-Struktur
      //     ab oder legt diese fest.
      //
      // Rückgabewerte:
      //     Die x-Koordinate der linken oberen Ecke dieser System.Drawing.Rectangle-Struktur.
      //     Der Standard ist 0.
      public int X {
         get {
            return x;
         }
         set {
            x = value;
         }
      }

      //
      // Zusammenfassung:
      //     Ruft die y-Koordinate der linken oberen Ecke dieser System.Drawing.Rectangle-Struktur
      //     ab oder legt diese fest.
      //
      // Rückgabewerte:
      //     Die y-Koordinate der linken oberen Ecke dieser System.Drawing.Rectangle-Struktur.
      //     Der Standard ist 0.
      public int Y {
         get {
            return y;
         }
         set {
            y = value;
         }
      }

      //
      // Zusammenfassung:
      //     Ruft die Breite dieser System.Drawing.Rectangle-Struktur ab oder legt diese fest.
      //
      // Rückgabewerte:
      //     Die Breite dieser System.Drawing.Rectangle-Struktur. Der Standard ist 0.
      public int Width {
         get {
            return width;
         }
         set {
            width = value;
         }
      }

      //
      // Zusammenfassung:
      //     Ruft die Höhe dieser System.Drawing.Rectangle-Struktur ab oder legt diese fest.
      //
      // Rückgabewerte:
      //     Die Höhe der System.Drawing.Rectangle-Struktur. Der Standard ist 0.
      public int Height {
         get {
            return height;
         }
         set {
            height = value;
         }
      }

      //
      // Zusammenfassung:
      //     Ruft die x-Koordinate des linken Randes dieser System.Drawing.Rectangle-Struktur
      //     ab.
      //
      // Rückgabewerte:
      //     Die x-Koordinate des linken Randes dieser System.Drawing.Rectangle-Struktur.
      [Browsable(false)]
      public int Left => X;

      //
      // Zusammenfassung:
      //     Ruft die y-Koordinate des oberen Randes dieser System.Drawing.Rectangle-Struktur
      //     ab.
      //
      // Rückgabewerte:
      //     Die y-Koordinate des oberen Randes dieser System.Drawing.Rectangle-Struktur.
      [Browsable(false)]
      public int Top => Y;

      //
      // Zusammenfassung:
      //     Ruft die x-Koordinate ab, die die Summe aus dem System.Drawing.Rectangle.X-Eigenschaftswert
      //     und dem System.Drawing.Rectangle.Width-Eigenschaftswert dieser System.Drawing.Rectangle-Struktur
      //     ist.
      //
      // Rückgabewerte:
      //     Die x-Koordinate, die die Summe aus System.Drawing.Rectangle.X und System.Drawing.Rectangle.Width
      //     dieses System.Drawing.Rectangle ist.
      [Browsable(false)]
      public int Right => X + Width;

      //
      // Zusammenfassung:
      //     Ruft die y-Koordinate ab, die die Summe aus dem System.Drawing.Rectangle.Y-Eigenschaftswert
      //     und dem System.Drawing.Rectangle.Height-Eigenschaftswert dieser System.Drawing.Rectangle-Struktur
      //     ist.
      //
      // Rückgabewerte:
      //     Die y-Koordinate, die die Summe aus System.Drawing.Rectangle.Y und System.Drawing.Rectangle.Height
      //     dieses System.Drawing.Rectangle ist.
      [Browsable(false)]
      public int Bottom => Y + Height;

      //
      // Zusammenfassung:
      //     Überprüft, ob alle numerischen Eigenschaften dieses System.Drawing.Rectangle
      //     den Wert 0 (null) haben.
      //
      // Rückgabewerte:
      //     Diese Eigenschaft gibt true zurück, wenn die System.Drawing.Rectangle.Width-Eigenschaft,
      //     die System.Drawing.Rectangle.Height-Eigenschaft, die System.Drawing.Rectangle.X-Eigenschaft
      //     sowie die System.Drawing.Rectangle.Y-Eigenschaft dieses System.Drawing.Rectangle
      //     den Wert 0 (null) haben, andernfalls false.
      [Browsable(false)]
      public bool IsEmpty {
         get {
            if (height == 0 && width == 0 && x == 0) {
               return y == 0;
            }

            return false;
         }
      }

      //
      // Zusammenfassung:
      //     Initialisiert eine neue Instanz der System.Drawing.Rectangle-Klasse mit der angegebenen
      //     Position und Größe.
      //
      // Parameter:
      //   x:
      //     Die x-Koordinate der linken oberen Ecke des Rechtecks.
      //
      //   y:
      //     Die y-Koordinate der linken oberen Ecke des Rechtecks.
      //
      //   width:
      //     Die Breite des Rechtecks.
      //
      //   height:
      //     Die Höhe des Rechtecks.
      public Rectangle(int x, int y, int width, int height) {
         this.x = x;
         this.y = y;
         this.width = width;
         this.height = height;
      }

      //
      // Zusammenfassung:
      //     Initialisiert eine neue Instanz der System.Drawing.Rectangle-Klasse mit der angegebenen
      //     Position und Größe.
      //
      // Parameter:
      //   location:
      //     Ein System.Drawing.Point, der die linke obere Ecke des rechteckigen Bereichs
      //     darstellt.
      //
      //   size:
      //     Eine System.Drawing.Size, die die Breite und Höhe des rechteckigen Bereichs darstellt.
      public Rectangle(Point location, Size size) {
         x = location.X;
         y = location.Y;
         width = size.Width;
         height = size.Height;
      }

      //
      // Zusammenfassung:
      //     Erstellt eine System.Drawing.Rectangle-Struktur mit den angegebenen Randpositionen.
      //
      // Parameter:
      //   left:
      //     Die x-Koordinate der linken oberen Ecke dieser System.Drawing.Rectangle-Struktur.
      //
      //   top:
      //     Die y-Koordinate der linken oberen Ecke dieser System.Drawing.Rectangle-Struktur.
      //
      //   right:
      //     Die x-Koordinate der rechten unteren Ecke dieser System.Drawing.Rectangle-Struktur.
      //
      //   bottom:
      //     Die y-Koordinate der rechten unteren Ecke dieser System.Drawing.Rectangle-Struktur.
      //
      // Rückgabewerte:
      //     Das neue System.Drawing.Rectangle, das von dieser Methode erstellt wird.
      public static Rectangle FromLTRB(int left, int top, int right, int bottom) {
         return new Rectangle(left, top, right - left, bottom - top);
      }

      //
      // Zusammenfassung:
      //     Überprüft, ob obj eine System.Drawing.Rectangle-Struktur mit derselben Position
      //     und Größe wie diese System.Drawing.Rectangle-Struktur ist.
      //
      // Parameter:
      //   obj:
      //     Der zu überprüfende System.Object.
      //
      // Rückgabewerte:
      //     Diese Methode gibt true zurück, wenn obj eine System.Drawing.Rectangle-Struktur
      //     ist und deren System.Drawing.Rectangle.X-Eigenschaft, System.Drawing.Rectangle.Y-Eigenschaft,
      //     System.Drawing.Rectangle.Width-Eigenschaft und System.Drawing.Rectangle.Height-Eigenschaft
      //     gleich den entsprechenden Eigenschaften dieser System.Drawing.Rectangle-Struktur
      //     sind. Andernfalls gibt die Methode false zurück.
      public override bool Equals(object obj) {
         if (!(obj is Rectangle)) {
            return false;
         }

         Rectangle rectangle = (Rectangle)obj;
         if (rectangle.X == X && rectangle.Y == Y && rectangle.Width == Width) {
            return rectangle.Height == Height;
         }

         return false;
      }

      //
      // Zusammenfassung:
      //     Überprüft, ob zwei System.Drawing.Rectangle-Strukturen die gleiche Position und
      //     Größe haben.
      //
      // Parameter:
      //   left:
      //     Die System.Drawing.Rectangle-Struktur auf der linken Seite des Gleichheitsoperators.
      //
      //   right:
      //     Die System.Drawing.Rectangle-Struktur auf der rechten Seite des Gleichheitsoperators.
      //
      // Rückgabewerte:
      //     Dieser Operator gibt true zurück, wenn die beiden System.Drawing.Rectangle-Strukturen
      //     die gleiche System.Drawing.Rectangle.X-Eigenschaft, System.Drawing.Rectangle.Y-Eigenschaft,
      //     System.Drawing.Rectangle.Width-Eigenschaft und System.Drawing.Rectangle.Height-Eigenschaft
      //     aufweisen.
      public static bool operator ==(Rectangle left, Rectangle right) {
         if (left.X == right.X && left.Y == right.Y && left.Width == right.Width) {
            return left.Height == right.Height;
         }

         return false;
      }

      //
      // Zusammenfassung:
      //     Überprüft, ob sich zwei System.Drawing.Rectangle-Strukturen in Position und Größe
      //     unterscheiden.
      //
      // Parameter:
      //   left:
      //     Die System.Drawing.Rectangle-Struktur auf der linken Seite des Ungleichheitsoperators.
      //
      //   right:
      //     Die System.Drawing.Rectangle-Struktur auf der rechten Seite des Ungleichheitsoperators.
      //
      // Rückgabewerte:
      //     Dieser Operator gibt true zurück, wenn sich die beiden System.Drawing.Rectangle.X-Strukturen
      //     in der System.Drawing.Rectangle.Y-Eigenschaft, der System.Drawing.Rectangle.Width-Eigenschaft,
      //     der System.Drawing.Rectangle.Height-Eigenschaft oder der System.Drawing.Rectangle-Eigenschaft
      //     unterscheiden, andernfalls false.
      public static bool operator !=(Rectangle left, Rectangle right) {
         return !(left == right);
      }

      //
      // Zusammenfassung:
      //     Konvertiert die angegebene System.Drawing.RectangleF-Struktur in eine System.Drawing.Rectangle-Struktur,
      //     indem die System.Drawing.RectangleF-Werte auf die nächste ganze Zahl aufgerundet
      //     werden.
      //
      // Parameter:
      //   value:
      //     Die zu konvertierende System.Drawing.RectangleF-Struktur.
      //
      // Rückgabewerte:
      //     Gibt einen Wert vom Typ System.Drawing.Rectangle zurück.
      public static Rectangle Ceiling(RectangleF value) {
         return new Rectangle((int)Math.Ceiling(value.X), (int)Math.Ceiling(value.Y), (int)Math.Ceiling(value.Width), (int)Math.Ceiling(value.Height));
      }

      //
      // Zusammenfassung:
      //     Konvertiert das angegebene System.Drawing.RectangleF in ein System.Drawing.Rectangle,
      //     indem die System.Drawing.RectangleF-Werte abgeschnitten werden.
      //
      // Parameter:
      //   value:
      //     Der zu konvertierende System.Drawing.RectangleF.
      //
      // Rückgabewerte:
      //     Der gekürzte Wert von System.Drawing.Rectangle.
      public static Rectangle Truncate(RectangleF value) {
         return new Rectangle((int)value.X, (int)value.Y, (int)value.Width, (int)value.Height);
      }

      //
      // Zusammenfassung:
      //     Konvertiert das angegebene System.Drawing.RectangleF in ein System.Drawing.Rectangle,
      //     indem die System.Drawing.RectangleF-Werte auf die nächste ganze Zahl gerundet
      //     werden.
      //
      // Parameter:
      //   value:
      //     Der zu konvertierende System.Drawing.RectangleF.
      //
      // Rückgabewerte:
      //     Der gerundete Ganzzahl-Wert von System.Drawing.Rectangle.
      public static Rectangle Round(RectangleF value) {
         return new Rectangle((int)Math.Round(value.X), (int)Math.Round(value.Y), (int)Math.Round(value.Width), (int)Math.Round(value.Height));
      }

      //
      // Zusammenfassung:
      //     Bestimmt, ob der angegebene Punkt in dieser System.Drawing.Rectangle-Struktur
      //     enthalten ist.
      //
      // Parameter:
      //   x:
      //     Die x-Koordinate des Punktes, der überprüft werden soll.
      //
      //   y:
      //     Die y-Koordinate des Punktes, der überprüft werden soll.
      //
      // Rückgabewerte:
      //     Diese Methode gibt true zurück, wenn der von x und y definierte Punkt in dieser
      //     System.Drawing.Rectangle-Struktur enthalten ist, andernfalls false.
      public bool Contains(int x, int y) {
         if (X <= x && x < X + Width && Y <= y) {
            return y < Y + Height;
         }

         return false;
      }

      //
      // Zusammenfassung:
      //     Bestimmt, ob der angegebene Punkt in dieser System.Drawing.Rectangle-Struktur
      //     enthalten ist.
      //
      // Parameter:
      //   pt:
      //     Der zu überprüfende System.Drawing.Point.
      //
      // Rückgabewerte:
      //     Diese Methode gibt true zurück, wenn der von pt dargestellte Punkt in dieser
      //     System.Drawing.Rectangle-Struktur enthalten ist, andernfalls false.
      public bool Contains(Point pt) {
         return Contains(pt.X, pt.Y);
      }

      //
      // Zusammenfassung:
      //     Bestimmt, ob der von rect dargestellte rechteckige Bereich vollständig in dieser
      //     System.Drawing.Rectangle-Struktur enthalten ist.
      //
      // Parameter:
      //   rect:
      //     Der zu überprüfende System.Drawing.Rectangle.
      //
      // Rückgabewerte:
      //     Diese Methode gibt true zurück, wenn der von rect dargestellte rechteckige Bereich
      //     vollständig in dieser System.Drawing.Rectangle-Struktur enthalten ist, andernfalls
      //     false.
      public bool Contains(Rectangle rect) {
         if (X <= rect.X && rect.X + rect.Width <= X + Width && Y <= rect.Y) {
            return rect.Y + rect.Height <= Y + Height;
         }

         return false;
      }

      //
      // Zusammenfassung:
      //     Gibt den Hashcode für diese System.Drawing.Rectangle-Struktur zurück. Informationen
      //     über die Verwendung von Hashcodes finden Sie unter System.Object.GetHashCode.
      //
      // Rückgabewerte:
      //     Eine ganze Zahl, die den Hashcode für dieses Rechteck darstellt.
      public override int GetHashCode() {
         return X ^ ((Y << 13) | (int)((uint)Y >> 19)) ^ ((Width << 26) | (int)((uint)Width >> 6)) ^ ((Height << 7) | (int)((uint)Height >> 25));
      }

      //
      // Zusammenfassung:
      //     Vergrößert dieses System.Drawing.Rectangle um den angegebenen Betrag.
      //
      // Parameter:
      //   width:
      //     Der Betrag, um den dieses System.Drawing.Rectangle horizontal vergrößert werden
      //     soll.
      //
      //   height:
      //     Der Betrag, um den dieses System.Drawing.Rectangle vertikal vergrößert werden
      //     soll.
      public void Inflate(int width, int height) {
         X -= width;
         Y -= height;
         Width += 2 * width;
         Height += 2 * height;
      }

      //
      // Zusammenfassung:
      //     Vergrößert dieses System.Drawing.Rectangle um den angegebenen Betrag.
      //
      // Parameter:
      //   size:
      //     Der Betrag, um den das Rechteck vergrößert werden soll.
      public void Inflate(Size size) {
         Inflate(size.Width, size.Height);
      }

      //
      // Zusammenfassung:
      //     Erstellt eine vergrößerte Kopie der angegebenen System.Drawing.Rectangle-Struktur
      //     und gibt die Kopie zurück. Die Kopie wird um den angegebenen Betrag vergrößert.
      //     Die ursprüngliche System.Drawing.Rectangle-Struktur wird nicht geändert.
      //
      // Parameter:
      //   rect:
      //     Das Ausgangs-System.Drawing.Rectangle. Dieses Rechteck wird nicht geändert.
      //
      //   x:
      //     Der Betrag, um den dieses System.Drawing.Rectangle horizontal vergrößert werden
      //     soll.
      //
      //   y:
      //     Der Betrag, um den dieses System.Drawing.Rectangle vertikal vergrößert werden
      //     soll.
      //
      // Rückgabewerte:
      //     Das vergrößerte System.Drawing.Rectangle.
      public static Rectangle Inflate(Rectangle rect, int x, int y) {
         Rectangle result = rect;
         result.Inflate(x, y);
         return result;
      }

      //
      // Zusammenfassung:
      //     Ersetzt dieses System.Drawing.Rectangle durch die Schnittmenge dieses Rechtecks
      //     und des angegebenen System.Drawing.Rectangle.
      //
      // Parameter:
      //   rect:
      //     Das System.Drawing.Rectangle, mit dem die Schnittmenge gebildet werden soll.
      public void Intersect(Rectangle rect) {
         Rectangle rectangle = Intersect(rect, this);
         X = rectangle.X;
         Y = rectangle.Y;
         Width = rectangle.Width;
         Height = rectangle.Height;
      }

      //
      // Zusammenfassung:
      //     Gibt eine dritte System.Drawing.Rectangle-Struktur zurück, die die Schnittmenge
      //     zweier anderer System.Drawing.Rectangle-Strukturen darstellt. Wenn keine Schnittmenge
      //     vorliegt, wird ein leeres System.Drawing.Rectangle zurückgegeben.
      //
      // Parameter:
      //   a:
      //     Ein Rechteck, mit dem eine Schnittmenge gebildet werden soll.
      //
      //   b:
      //     Ein Rechteck, mit dem eine Schnittmenge gebildet werden soll.
      //
      // Rückgabewerte:
      //     Ein System.Drawing.Rectangle, das die Schnittmenge von a und b darstellt.
      public static Rectangle Intersect(Rectangle a, Rectangle b) {
         int num = Math.Max(a.X, b.X);
         int num2 = Math.Min(a.X + a.Width, b.X + b.Width);
         int num3 = Math.Max(a.Y, b.Y);
         int num4 = Math.Min(a.Y + a.Height, b.Y + b.Height);
         if (num2 >= num && num4 >= num3) {
            return new Rectangle(num, num3, num2 - num, num4 - num3);
         }

         return Empty;
      }

      //
      // Zusammenfassung:
      //     Bestimmt, ob dieses Rechteck eine Schnittmenge mit rect bildet.
      //
      // Parameter:
      //   rect:
      //     Das zu überprüfende Rechteck.
      //
      // Rückgabewerte:
      //     Diese Methode gibt true zurück, wenn eine Schnittmenge vorliegt, andernfalls
      //     false.
      public bool IntersectsWith(Rectangle rect) {
         if (rect.X < X + Width && X < rect.X + rect.Width && rect.Y < Y + Height) {
            return Y < rect.Y + rect.Height;
         }

         return false;
      }

      //
      // Zusammenfassung:
      //     Ruft eine System.Drawing.Rectangle-Struktur ab, die die Gesamtmenge zweier System.Drawing.Rectangle-Strukturen
      //     enthält.
      //
      // Parameter:
      //   a:
      //     Ein Rechteck, mit dem die Gesamtmenge gebildet werden soll.
      //
      //   b:
      //     Ein Rechteck, mit dem die Gesamtmenge gebildet werden soll.
      //
      // Rückgabewerte:
      //     Eine System.Drawing.Rectangle-Struktur, die die Gesamtmenge der beiden System.Drawing.Rectangle-Strukturen
      //     umgrenzt.
      public static Rectangle Union(Rectangle a, Rectangle b) {
         int num = Math.Min(a.X, b.X);
         int num2 = Math.Max(a.X + a.Width, b.X + b.Width);
         int num3 = Math.Min(a.Y, b.Y);
         int num4 = Math.Max(a.Y + a.Height, b.Y + b.Height);
         return new Rectangle(num, num3, num2 - num, num4 - num3);
      }

      //
      // Zusammenfassung:
      //     Passt die Position dieses Rechtecks um den angegebenen Betrag an.
      //
      // Parameter:
      //   pos:
      //     Betrag für den Offset der Position.
      public void Offset(Point pos) {
         Offset(pos.X, pos.Y);
      }

      //
      // Zusammenfassung:
      //     Passt die Position dieses Rechtecks um den angegebenen Betrag an.
      //
      // Parameter:
      //   x:
      //     Der horizontale Offset.
      //
      //   y:
      //     Der vertikale Offset.
      public void Offset(int x, int y) {
         X += x;
         Y += y;
      }

      //
      // Zusammenfassung:
      //     Konvertiert die Attribute für dieses System.Drawing.Rectangle in eine Klartextzeichenfolge.
      //
      // Rückgabewerte:
      //     Eine Zeichenfolge, die die Position, Breite und Höhe dieser System.Drawing.Rectangle-Struktur
      //     enthält, z. B. "{X=20, Y=20, Width=100, Height=50}".
      public override string ToString() {
         return "{X=" + X.ToString(CultureInfo.CurrentCulture) + ",Y=" + Y.ToString(CultureInfo.CurrentCulture) + ",Width=" + Width.ToString(CultureInfo.CurrentCulture) + ",Height=" + Height.ToString(CultureInfo.CurrentCulture) + "}";
      }
   }
}
