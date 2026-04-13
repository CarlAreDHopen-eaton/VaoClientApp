using Vao.Client.Components.Interfaces;
using Vao.Client.Contracts;
using Vao.Client.Enum;

namespace Vao.Client.Components
{
   public class User : BaseComponent, INamedComponent
   {
      // ReSharper disable once MemberInitializerValueIgnored
      
      private JsonUserObject mJsonUserObject;
      private string mUserName = string.Empty;
      private int mPriority = 0;
      private UserPrivilege mPrivilege;
      private bool mPlaybackAccess = false;
      private bool mDownloadAccess = false;


      internal User(int alarmNumber, JsonUserObject user, VaoClient vaoClient)
         : base(vaoClient, alarmNumber)
      {
         mUserName = user.name;
         Privilege = System.Enum.TryParse(user.privilege, ignoreCase: true, out UserPrivilege parsedUserPrivilege) ? parsedUserPrivilege : UserPrivilege.None;
         Priority = user.priority;
         PlaybackAccess = user.playbackAccess;
         DownloadAccess = user.downloadAccess;
         mJsonUserObject = user;
      }

      public string Name
      {
         get { return mUserName; }
         internal set { mUserName = value; }
      }

      public int Priority
      {
         get
         {
            return mPriority;
         }
         internal set
         {
            if (mPriority != value)
            {
               mPriority = value;
               NotifyPropertyChanged();
            }
         }
      }

      public UserPrivilege Privilege
      {
         get
         {
            return mPrivilege;
         }
         internal set
         {
            if (mPrivilege != value)
            {
               mPrivilege = value;
               NotifyPropertyChanged();
            }
         }
      }

      public bool PlaybackAccess
      {
         get
         {
            return mPlaybackAccess;
         }
         internal set
         {
            if (mPlaybackAccess != value)
            {
               mPlaybackAccess = value;
               NotifyPropertyChanged();
            }
         }
      }

      public bool DownloadAccess
      {
         get
         {
            return mDownloadAccess;
         }
         internal set
         {
            if (mDownloadAccess != value)
            {
               mDownloadAccess = value;
               NotifyPropertyChanged();
            }
         }
      }

      /// <summary>
      /// Determines whether the user has privilege equal to or above the specified access level.
      /// </summary>
      /// <param name="access">The access level.</param>
      /// <returns>
      ///   <c>true</c> if the user has privilege equal to or above the specified access level; otherwise, <c>false</c>.
      /// </returns>
      public bool HasAccessLevel(UserPrivilege access)
      {
         return Privilege >= access;
      }

      public bool SetName(string newName)
      {
         //TODO implement.
         mUserName = newName;
         return false;
      }
   }
}
