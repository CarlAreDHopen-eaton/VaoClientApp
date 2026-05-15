using Vao.Client.Contracts;

namespace Vao.Client.Components
{
   public class SystemInformation : BaseComponent
   {
      #region Private Members

      private static int mCurrentComponentNumber = 0;

      private string mName;
      private string mVersion;
      private string mSystemName;
      private string mSystemId;
      #endregion

      #region Constructor

      internal SystemInformation(JsonImplementationVersion jsonObject, FlexApiClient flexApiClient)
         : base(flexApiClient, mCurrentComponentNumber)
      {
         mName = jsonObject.name;
         mVersion = jsonObject.version;
         mSystemName = jsonObject.systemName;
         mSystemId = jsonObject.systemId;
      }

      #endregion

      #region Public Properties

      /// <summary>
      /// Name of the implementation (e.g. "HERNIS FLEX System")
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

      /// <summary>
      /// Gets the name of the system.
      /// </summary>
      public string SystemName
      {
         get { return mSystemName; }
      }

      /// <summary>
      /// Gets the unique identifier for the system.
      /// </summary>
      public string SystemId
      {
         get { return mSystemId; }
      }

      #endregion
   }
}
