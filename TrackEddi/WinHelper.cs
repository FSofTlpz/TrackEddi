using System.Drawing;
using System.IO;

namespace TrackEddi {
   internal static class WinHelper {

      /// <summary>
      /// erzeugt zum Bitmap eine <see cref="Xamarin.Forms.ImageSource"/> und liefert die dazu weiterhin (!)
      /// benötigten Bilddaten
      /// </summary>
      /// <param name="bm"></param>
      /// <param name="ims"></param>
      /// <returns></returns>
      public static byte[] GetImageSource4WindowsBitmap(Bitmap bm, out Xamarin.Forms.ImageSource ims) {
         MemoryStream mem = new MemoryStream();
         bm.Save(mem, System.Drawing.Imaging.ImageFormat.Png);
         mem.Position = 0;
         byte[] pictdata = mem.ToArray();
         mem.Dispose();

         ims = Xamarin.Forms.ImageSource.FromStream(() => {
            return new MemoryStream(pictdata);  // MS: "The delegate provided to must return a new stream on every invocation."
         });

         return pictdata;
      }

   }
}
