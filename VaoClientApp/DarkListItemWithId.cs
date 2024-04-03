using System;
using DarkUI.Controls;

namespace Vao.Sample
{
   public class DarkListItemWithId : DarkListItem
   {
      public DarkListItemWithId() :
         base()
      {
      }

      public DarkListItemWithId(string text)
         : base(text)
      {
      }

      public DarkListItemWithId(string text, Guid downloadId)
   : base(text)
      {
         DownloadId = downloadId;
      }


      public Guid DownloadId { get; set; }
   }
}
