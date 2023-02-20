namespace System.Drawing {
   public class StringFormat : IDisposable {
      public StringAlignment Alignment { get; set; }
      public StringAlignment LineAlignment { get; set; }

      public void Dispose() { }
   }
}