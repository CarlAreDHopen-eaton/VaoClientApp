using System.Linq;

namespace Vao.Client.Utility
{
   public static class DownloadUrlHelper
   {
      public static string GetFileType(this string url) 
      {
        var ext = url.Split('.').LastOrDefault();
         if (ext != null)
            return ext;

         // fallback, should not happen.
         return "mp4";
      }

      public static string GetFileId(this string url)
      { 
         if (string.IsNullOrWhiteSpace(url))
            return null;
         
         string[] urlParts = url.Split('/');         
         if (urlParts.Length == 0) 
            return null;

         string fileId = urlParts[urlParts.Length - 1];
         fileId = fileId.Trim();
         
         if (string.IsNullOrEmpty(fileId))
            return null;

         return fileId;
      }
   }
}
