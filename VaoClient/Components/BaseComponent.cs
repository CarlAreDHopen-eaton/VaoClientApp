using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Vao.Client.Components.Interfaces
{
   public class BaseComponent : INotifyPropertyChanged
   {
      #region Private Members

      private VaoClient mVaoClient;
      private int mComponentNumber;

      #endregion

      #region Constructors

      public BaseComponent(VaoClient client, int componentNumber)
      { 
         mVaoClient = client;
         mComponentNumber = componentNumber;
      }

      #endregion

      #region Public Properties

      /// <summary>
      /// The clinet that this object belong to.
      /// </summary>
      public VaoClient VaoClient { get { return mVaoClient; } }

      /// <summary>
      /// The component number that identifies this component.
      /// </summary>
      public int ComponentNumber { get { return mComponentNumber; } }

      #endregion

      #region Events 
      
      /// <summary>
      /// Event fired when properties of interest are changed.
      /// </summary>
      public event PropertyChangedEventHandler PropertyChanged;

      #endregion

      #region Protected Methods

      // This method is called by the Set accessor of each property.  
      // The CallerMemberName attribute that is applied to the optional propertyName  
      // parameter causes the property name of the caller to be substituted as an argument.  
      protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      #endregion
   }
}
