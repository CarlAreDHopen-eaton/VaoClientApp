using System;
using LibVLCSharp.WinForms;

namespace Vao.Sample
{
   public class VideoViewWithViewerId : VideoView
   {
      public Guid ViewerID { get; } = Guid.NewGuid();
   }
}
