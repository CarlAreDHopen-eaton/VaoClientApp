using Vao.Client.Contracts;

namespace Vao.Client.Components
{
   public class ImplementationVersion : BaseComponent
   {
      #region Private Members

      private static int mCurrentComponentNumber = 0;

      private string mName;
      private string mVersion;

      #endregion

      #region Constructor

      internal ImplementationVersion(JsonImplementationVersion jsonObject, FlexApiClient flexApiClient)
         : base(flexApiClient, mCurrentComponentNumber)
      {
         mName = jsonObject.name;
         mVersion = jsonObject.version;
      }

      #endregion

      #region Public Properties

      /// <summary>
      /// Name of the implementation (e.g. "Hernis")
      /// </summary>
      public string Name
      {
         get { return mName; }
      }

      /// <summary>
      /// Implementation version string (e.g. "6.5.0.0")
      /// </summary>
      public string Version
      {
         get { return mVersion; }
      }

      #endregion
   }
}
