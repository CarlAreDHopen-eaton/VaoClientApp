using System;
using System.Collections.Generic;
using System.Text;

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
