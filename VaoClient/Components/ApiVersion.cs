using Vao.Client.Contracts;

namespace Vao.Client.Components
{
   public class ApiVersion : BaseComponent
   {
      #region Private Members

      private static int mCurrentComponentNumber = 0;

      private string mName;
      private int mMajorVersion;
      private int mMinorVersion;


      #endregion

      #region Constructor

      internal ApiVersion(JsonApiVersion jsonObject, VaoClient vaoClient)
         : base(vaoClient, mCurrentComponentNumber)
      {
         mName = jsonObject.name; 
         mMajorVersion = jsonObject.majorVersion;
         mMinorVersion = jsonObject.minorVersion;
      }

      #endregion

      #region Public Properties

      /// <summary>
      /// Name of the api
      /// </summary>
      public string Name
      {
         get { return mName; }
      }

      /// <summary>
      /// Major api version number
      /// </summary>
      public int MajorVersion
      {
         get { return mMajorVersion; }
      }

      /// <summary>
      /// Minor api version number
      /// </summary>
      public int MinorVersion
      {
         get { return mMinorVersion; }
      }
      #endregion
   }
}
