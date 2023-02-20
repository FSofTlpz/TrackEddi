using System.IO;

namespace System.Windows.Forms {
   public class Cursor : IDisposable {
      //
      // Zusammenfassung:
      //     Initialisiert eine neue Instanz der System.Windows.Forms.Cursor-Klasse anhand
      //     des angegebenen Windows-Handles.
      //
      // Parameter:
      //   handle:
      //     Ein System.IntPtr, der das Windows-Handle des zu erstellenden Cursors darstellt.
      //
      // Ausnahmen:
      //   T:System.ArgumentException:
      //     handle ist System.IntPtr.Zero.
      public Cursor(IntPtr handle) { }
      //
      // Zusammenfassung:
      //     Initialisiert eine neue Instanz der System.Windows.Forms.Cursor-Klasse aus der
      //     angegebenen Datei.
      //
      // Parameter:
      //   fileName:
      //     Die zu ladende Cursordatei.
      public Cursor(string fileName) { }
      //
      // Zusammenfassung:
      //     Initialisiert eine neue Instanz der System.Windows.Forms.Cursor-Klasse aus dem
      //     angegebenen Stream.
      //
      // Parameter:
      //   stream:
      //     Der Datenstream, aus dem der System.Windows.Forms.Cursor geladen werden soll.
      public Cursor(Stream stream) { }
      //
      // Zusammenfassung:
      //     Initialisiert eine neue Instanz der System.Windows.Forms.Cursor-Klasse aus der
      //     angegebenen Ressource mit dem angegebenen Ressourcentyp.
      //
      // Parameter:
      //   type:
      //     Die Ressource System.Type.
      //
      //   resource:
      //     Der Name der Ressource.
      public Cursor(Type type, string resource) { }

      //
      // Zusammenfassung:
      //     Gibt einem Objekt Gelegenheit zu dem Versuch, Ressourcen freizugeben und andere
      //     Bereinigungen durchzuführen, bevor es von der Garbage Collection freigegeben
      //     wird.
      ~Cursor() { }

      //
      // Zusammenfassung:
      //     Ruft die Position des Cursors ab oder legt diese fest.
      //
      // Rückgabewerte:
      //     Ein System.Drawing.Point in Bildschirmkoordinaten, der die Position des Cursors
      //     darstellt.
      public static System.Drawing.Point Position { get; set; }
      //
      // Zusammenfassung:
      //     Ruft das Cursorobjekt ab, das den Mauscursor darstellt, oder legt dieses fest.
      //
      // Rückgabewerte:
      //     Ein System.Windows.Forms.Cursor, der den Mauscursor darstellt. Der Standardwert
      //     ist null, wenn der Mauscursor nicht angezeigt wird.
      public static Cursor Current { get; set; }
      ////
      //// Zusammenfassung:
      ////     Ruft die Begrenzungen ab, die das Auswahlrechteck für den Cursor darstellen,
      ////     oder legt diese fest.
      ////
      //// Rückgabewerte:
      ////     Das System.Drawing.Rectangle in Bildschirmkoordinaten, das das Auswahlrechteck
      ////     für den System.Windows.Forms.Cursor darstellt.
      //public static Rectangle Clip { get; set; }
      ////
      //// Zusammenfassung:
      ////     Ruft den Cursorhotspot ab.
      ////
      //// Rückgabewerte:
      ////     Ein System.Drawing.Point, der den Cursorhotspot darstellt.
      //public Drawing.Point HotSpot { get; }
      ////
      //// Zusammenfassung:
      ////     Ruft das Handle des Cursors ab.
      ////
      //// Rückgabewerte:
      ////     Ein System.IntPtr, der das Cursorhandle darstellt.
      ////
      //// Ausnahmen:
      ////   T:System.Exception:
      ////     Dieses Handle ist System.IntPtr.Zero.
      //public IntPtr Handle { get; }
      ////
      //// Zusammenfassung:
      ////     Ruft die Größe des Cursorobjekts ab.
      ////
      //// Rückgabewerte:
      ////     Die System.Drawing.Size, die die Breite und Höhe des System.Windows.Forms.Cursor
      ////     darstellt.
      //public Drawing.Size Size { get; }
      ////
      //// Zusammenfassung:
      ////     Ruft das Objekt ab, das Daten über System.Windows.Forms.Cursor enthält, oder
      ////     legt dieses fest.
      ////
      //// Rückgabewerte:
      ////     Ein System.Object, das Daten über den System.Windows.Forms.Cursor enthält. Die
      ////     Standardeinstellung ist null.
      //[Bindable(true)]
      //[DefaultValue(null)]
      //[Localizable(false)]
      //[SRCategoryAttribute("CatData")]
      //[SRDescriptionAttribute("ControlTagDescr")]
      //[TypeConverter(typeof(StringConverter))]
      //public object Tag { get; set; }

      ////
      //// Zusammenfassung:
      ////     Blendet den Cursor aus.
      //public static void Hide() { }
      ////
      //// Zusammenfassung:
      ////     Zeigt den Cursor an.
      //public static void Show() { }
      ////
      //// Zusammenfassung:
      ////     Kopiert das Handle dieses System.Windows.Forms.Cursor.
      ////
      //// Rückgabewerte:
      ////     Ein System.IntPtr, der das Cursorhandle darstellt.
      //public IntPtr CopyHandle();
      ////
      // Zusammenfassung:
      //     Gibt alle vom System.Windows.Forms.Cursor verwendeten Ressourcen frei.
      public void Dispose() { }
      ////
      //// Zusammenfassung:
      ////     Zeichnet den Cursor auf der angegebenen Oberfläche innerhalb der angegebenen
      ////     Begrenzungen.
      ////
      //// Parameter:
      ////   g:
      ////     Die System.Drawing.Graphics-Oberfläche, auf der der System.Windows.Forms.Cursor
      ////     gezeichnet werden soll.
      ////
      ////   targetRect:
      ////     Das System.Drawing.Rectangle, das die Begrenzungen des System.Windows.Forms.Cursor
      ////     darstellt.
      //public void Draw(Graphics g, Rectangle targetRect);
      ////
      //// Zusammenfassung:
      ////     Zeichnet den Cursor in gestrecktem Format auf der angegebenen Oberfläche innerhalb
      ////     der angegebenen Begrenzungen.
      ////
      //// Parameter:
      ////   g:
      ////     Die System.Drawing.Graphics-Oberfläche, auf der der System.Windows.Forms.Cursor
      ////     gezeichnet werden soll.
      ////
      ////   targetRect:
      ////     Das System.Drawing.Rectangle, das die Begrenzungen des System.Windows.Forms.Cursor
      ////     darstellt.
      //public void DrawStretched(Graphics g, Rectangle targetRect);
      ////
      //// Zusammenfassung:
      ////     Gibt einen Wert zurück, der angibt, ob dieser Cursor mit dem angegebenen System.Windows.Forms.Cursor
      ////     übereinstimmt.
      ////
      //// Parameter:
      ////   obj:
      ////     Der zu vergleichende System.Windows.Forms.Cursor.
      ////
      //// Rückgabewerte:
      ////     true, wenn dieser Cursor dem angegebenen System.Windows.Forms.Cursor entspricht,
      ////     andernfalls false.
      //public override bool Equals(object obj);
      ////
      //// Zusammenfassung:
      ////     Ruft den Hashcode für den aktuellen System.Windows.Forms.Cursor ab.
      ////
      //// Rückgabewerte:
      ////     Ein Hashcode für die aktuelle System.Windows.Forms.Cursor.
      //public override int GetHashCode();
      ////
      //// Zusammenfassung:
      ////     Ruft eine lesbare Zeichenfolge ab, die diesen System.Windows.Forms.Cursor darstellt.
      ////
      //// Rückgabewerte:
      ////     Ein System.String, der diesen System.Windows.Forms.Cursor darstellt.
      //public override string ToString();

      ////
      //// Zusammenfassung:
      ////     Gibt einen Wert zurück, der angibt, ob zwei Instanzen der System.Windows.Forms.Cursor-Klasse
      ////     gleich sind.
      ////
      //// Parameter:
      ////   left:
      ////     Ein zu vergleichender System.Windows.Forms.Cursor.
      ////
      ////   right:
      ////     Ein zu vergleichender System.Windows.Forms.Cursor.
      ////
      //// Rückgabewerte:
      ////     true, wenn zwei Instanzen der System.Windows.Forms.Cursor-Klasse gleich sind,
      ////     andernfalls false.
      //public static bool operator ==(Cursor left, Cursor right);
      ////
      //// Zusammenfassung:
      ////     Gibt einen Wert zurück, der angibt, ob zwei Instanzen der System.Windows.Forms.Cursor-Klasse
      ////     ungleich sind.
      ////
      //// Parameter:
      ////   left:
      ////     Ein zu vergleichender System.Windows.Forms.Cursor.
      ////
      ////   right:
      ////     Ein zu vergleichender System.Windows.Forms.Cursor.
      ////
      //// Rückgabewerte:
      ////     true, wenn zwei Instanzen der System.Windows.Forms.Cursor-Klasse ungleich sind,
      ////     andernfalls false.
      //public static bool operator !=(Cursor left, Cursor right);

   }
}
