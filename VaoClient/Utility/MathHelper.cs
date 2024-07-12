using System.Text.Encodings.Web;

namespace Vao.Client.Utility
{
   public static class MathHelper
   {
      public static int Clamp(int value, int min, int max)
      {
         return (value <= min) ? min : (value >= max) ? max : value;
      }
   }
}
