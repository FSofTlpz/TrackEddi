namespace System.Drawing {

   /// <summary>
   /// Dieses Prinzip ist in Skia nicht bekannt und dient hier nur als Platzhalter!
   /// </summary>
   public class FontFamily {

      public readonly static FontFamily GenericSansSerif = new FontFamily() { Name = "Arial", };
      // public readonly static FontFamily GenericMonospace = new FontFamily() { Name = "Monospace", };
      // public readonly static FontFamily GenericSerif = new FontFamily() { Name = "TimesNewRoman", };

      public string Name { get; protected set; }


      public FontFamily() { }

   }

}
