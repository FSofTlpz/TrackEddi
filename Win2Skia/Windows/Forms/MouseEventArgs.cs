namespace System.Windows.Forms {

   public class MouseEventArgs {
      /// <summary>
      /// Ruft die x-Koordinate der Maus während des generierten Mausereignisses ab.
      /// </summary>
      public int X => Location.X;
      /// <summary>
      /// Ruft die y-Koordinate der Maus während des generierten Mausereignisses ab.
      /// </summary>
      public int Y => Location.Y;
      /// <summary>
      /// Ruft die Position der Maus während des generierten Mausereignisses ab.
      /// </summary>
      public System.Drawing.Point Location { get; protected set; }
      /// <summary>
      /// Ruft ab, welche Maustaste gedrückt wurde.
      /// </summary>
      public MouseButtons Button { get; protected set; }
      /// <summary>
      /// Ruft einen Zähler mit Vorzeichen für die Anzahl der Arretierungen ab, um die das Mausrad gedreht wurde, der mit der Konstanten WHEEL_DELTA multipliziert wird. 
      /// Eine Arretierung (Rastpunkt) ist eine Kerbe des Mausrades.
      /// </summary>
      public int Delta { get; protected set; }

      public MouseEventArgs(MouseButtons button, int x, int y, int delta) {
         Location = new System.Drawing.Point(x, y);
         Delta = delta;
         Button = button;
      }

      public MouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta) {
         Location = new System.Drawing.Point(x, y);
         Delta = delta;
         Button = button;
      }

   }
}