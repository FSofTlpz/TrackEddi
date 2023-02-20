using System.Drawing;

namespace System.Windows.Forms {

   public class PaintEventArgs {
      public Graphics Graphics { get; }
      public Rectangle ClipRectangle { get; }

      public PaintEventArgs(Graphics graphics, Rectangle clipRect) {
         Graphics = graphics;
         ClipRectangle = clipRect;
      }

   }

}