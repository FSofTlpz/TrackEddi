using System;
using System.Collections.Generic;
using System.Text;

namespace System.Drawing.Drawing2D {
   //
   // Zusammenfassung:
   //     Kapselt ein benutzerdefiniertes Linienende.
   public class CustomLineCap : IDisposable { //: MarshalByRefObject, ICloneable, IDisposable {
      //
      // Zusammenfassung:
      //     Initialisiert eine neue Instanz der System.Drawing.Drawing2D.CustomLineCap -Klasse
      //     mit dem angegebenen Umriss und ausfüllen.
      //
      // Parameter:
      //   fillPath:
      //     Ein System.Drawing.Drawing2D.GraphicsPath -Objekt, das die Füllung des benutzerdefinierten
      //     Endes definiert.
      //
      //   strokePath:
      //     Ein System.Drawing.Drawing2D.GraphicsPath -Objekt, das den Umriss des benutzerdefinierten
      //     Endes definiert.
      public CustomLineCap(GraphicsPath fillPath, GraphicsPath strokePath) {

      }

      //
      // Zusammenfassung:
      //     Initialisiert eine neue Instanz der der System.Drawing.Drawing2D.CustomLineCap
      //     Klasse aus dem angegebenen vorhandenen System.Drawing.Drawing2D.LineCap Enumeration
      //     mit dem angegebenen Umriss und ausfüllen.
      //
      // Parameter:
      //   fillPath:
      //     Ein System.Drawing.Drawing2D.GraphicsPath -Objekt, das die Füllung des benutzerdefinierten
      //     Endes definiert.
      //
      //   strokePath:
      //     Ein System.Drawing.Drawing2D.GraphicsPath -Objekt, das den Umriss des benutzerdefinierten
      //     Endes definiert.
      //
      //   baseCap:
      //     Das Linienende, aus dem benutzerdefinierten Endes erstellt werden soll.
      //public CustomLineCap(GraphicsPath fillPath, GraphicsPath strokePath, LineCap baseCap) {

      //}

      //
      // Zusammenfassung:
      //     Initialisiert eine neue Instanz der der System.Drawing.Drawing2D.CustomLineCap
      //     Klasse aus dem angegebenen vorhandenen System.Drawing.Drawing2D.LineCap Enumeration
      //     mit dem angegebenen Umriss, füllen, und Inset.
      //
      // Parameter:
      //   fillPath:
      //     Ein System.Drawing.Drawing2D.GraphicsPath -Objekt, das die Füllung des benutzerdefinierten
      //     Endes definiert.
      //
      //   strokePath:
      //     Ein System.Drawing.Drawing2D.GraphicsPath -Objekt, das den Umriss des benutzerdefinierten
      //     Endes definiert.
      //
      //   baseCap:
      //     Das Linienende, aus dem benutzerdefinierten Endes erstellt werden soll.
      //
      //   baseInset:
      //     Der Abstand zwischen dem Ende und der Linie.
      //public CustomLineCap(GraphicsPath fillPath, GraphicsPath strokePath, LineCap baseCap, float baseInset) {

      //}

      ////
      //// Zusammenfassung:
      ////     Ermöglicht eine System.Drawing.Drawing2D.CustomLineCap für den Versuch, Ressourcen
      ////     freizugeben und andere Bereinigungsvorgänge vor der System.Drawing.Drawing2D.CustomLineCap
      ////     durch die Garbagecollection wieder zugänglich gemacht wird.
      //~CustomLineCap();

      ////
      //// Zusammenfassung:
      ////     Ruft ab oder legt der System.Drawing.Drawing2D.LineJoin -Enumeration, der bestimmt,
      ////     wie Zeilen, die diese besteht System.Drawing.Drawing2D.CustomLineCap Objekt verknüpft
      ////     sind.
      ////
      //// Rückgabewerte:
      ////     Die System.Drawing.Drawing2D.LineJoin -Enumeration dies System.Drawing.Drawing2D.CustomLineCap
      ////     -Objekt zum Verbinden von Linien verwendet.
      //public LineJoin StrokeJoin { get; set; }
      ////
      //// Zusammenfassung:
      ////     Ruft ab oder legt die System.Drawing.Drawing2D.LineCap für die diese Enumeration
      ////     System.Drawing.Drawing2D.CustomLineCap basiert.
      ////
      //// Rückgabewerte:
      ////     Die System.Drawing.Drawing2D.LineCap für die diese Enumeration System.Drawing.Drawing2D.CustomLineCap
      ////     basiert.
      //public LineCap BaseCap { get; set; }
      ////
      //// Zusammenfassung:
      ////     Ruft ab oder legt den Abstand zwischen dem Ende und der Linie.
      ////
      //// Rückgabewerte:
      ////     Der Abstand zwischen dem Anfang des Endes und dem Ende der Linie.
      //public float BaseInset { get; set; }
      ////
      //// Zusammenfassung:
      ////     Ruft ab oder legt fest, die durch die Skalierung dieser System.Drawing.Drawing2D.CustomLineCap
      ////     Klassenobjekt in Bezug auf die Breite der System.Drawing.Pen Objekt.
      ////
      //// Rückgabewerte:
      ////     Der Betrag, um den Clientzugriffspunkt skaliert.
      //public float WidthScale { get; set; }

      ////
      //// Zusammenfassung:
      ////     Erstellt eine genaue Kopie von dieser System.Drawing.Drawing2D.CustomLineCap.
      ////
      //// Rückgabewerte:
      ////     Das von dieser Methode erstellte System.Drawing.Drawing2D.CustomLineCap, umgewandelt
      ////     in ein Objekt.
      //public object Clone();
      ////
      //// Zusammenfassung:
      ////     Gibt alle von diesem System.Drawing.Drawing2D.CustomLineCap-Objekt verwendeten
      ////     Ressourcen frei.
      //public void Dispose();
      ////
      //// Zusammenfassung:
      ////     Ruft den Linienanfang und Linien, die dieses benutzerdefinierten Endes bilden.
      ////
      //// Parameter:
      ////   startCap:
      ////     Die System.Drawing.Drawing2D.LineCap -Enumeration am Anfang einer Linie innerhalb
      ////     dieses Endes verwendet wird.
      ////
      ////   endCap:
      ////     Die System.Drawing.Drawing2D.LineCap -Enumeration am Ende einer Linie innerhalb
      ////     dieses Endes verwendet wird.
      //public void GetStrokeCaps(out LineCap startCap, out LineCap endCap);
      ////
      //// Zusammenfassung:
      ////     Legt das Linienanfang und Linien, die dieses benutzerdefinierten Endes bilden.
      ////
      //// Parameter:
      ////   startCap:
      ////     Die System.Drawing.Drawing2D.LineCap -Enumeration am Anfang einer Linie innerhalb
      ////     dieses Endes verwendet wird.
      ////
      ////   endCap:
      ////     Die System.Drawing.Drawing2D.LineCap -Enumeration am Ende einer Linie innerhalb
      ////     dieses Endes verwendet wird.
      //public void SetStrokeCaps(LineCap startCap, LineCap endCap);
      ////
      //// Zusammenfassung:
      ////     Gibt die von System.Drawing.Drawing2D.CustomLineCap verwendeten nicht verwalteten
      ////     Ressourcen und optional die verwalteten Ressourcen frei.
      ////
      //// Parameter:
      ////   disposing:
      ////     true, um sowohl verwaltete als auch nicht verwaltete Ressourcen freizugeben,
      ////     false, um ausschließlich nicht verwaltete Ressourcen freizugeben.
      //protected virtual void Dispose(bool disposing);


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
               


            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion
   }
}