using Vao.Client.Components.Interfaces;
using Vao.Client.Contracts;
using Vao.Client.Enum;

namespace Vao.Client.Components
{
   public class Alarm : BaseComponent, INamedComponent
   {
      // ReSharper disable once MemberInitializerValueIgnored
      private string mAlarmName = string.Empty;

      private JsonAlarmObject mJsonAlarmObject;
      private AlarmGeneralStatus mStatus;
      private string mExtendedStatus = string.Empty;
      private int mPriority = 0;
      private int mActions = 0;


      internal Alarm(int alarmNumber, JsonAlarmObject alarm, FlexRApiClient flexRApiClient)
         : base(flexRApiClient, alarmNumber)
      {
         mAlarmName = alarm.name;
         Status = System.Enum.TryParse(alarm.status, ignoreCase: true, out AlarmGeneralStatus parsedStatus) ? parsedStatus : AlarmGeneralStatus.Unknown;
         ExtendedStatus = alarm.extendedStatus;
         Priority = alarm.priority;
         Actions = alarm.actions;
         mJsonAlarmObject = alarm;
      }


      /// <summary>
      /// The name of the alarm in the VMS system.
      /// </summary>
      public string Name
      {
         get { return mAlarmName; }
         internal set { mAlarmName = value; }
      }

      /// <summary>
      /// The alarm general status
      /// </summary>
      public AlarmGeneralStatus Status
      {
         get
         {
            return mStatus;
         }
         internal set
         {
            if (mStatus != value)
            {
               mStatus = value;
               NotifyPropertyChanged();
            }
         }
      }

      /// <summary>
      /// The alarm extended status
      /// </summary>
      public string ExtendedStatus
      {
         get
         {
            return mExtendedStatus;
         }
         internal set
         {
            if (mExtendedStatus != value)
            {
               mExtendedStatus = value;
               NotifyPropertyChanged();
            }
         }
      }

      /// <summary>
      /// The priority of the alarm
      /// </summary>
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

      /// <summary>
      /// The number of actions the alarm has
      /// </summary>
      public int Actions
      {
         get
         {
            return mActions;
         }
         internal set
         {
            if (mActions != value)
            {
               mActions = value;
               NotifyPropertyChanged();
            }
         }
      }

      public bool SetName(string newName)
      {
         //TODO implement.
         mAlarmName = newName;
         return false;
      }

      internal void UpdateAlarmStatus(StatusMessage message)
      {
         switch (message.Type)
         {
            case MessageType.AlarmStatusDisabled:
               Status = AlarmGeneralStatus.Disabled;
               break;
            case MessageType.AlarmStatusTampered:
               Status = AlarmGeneralStatus.Tampered;
               break;
            case MessageType.AlarmStatusActive:
               Status = AlarmGeneralStatus.Active;
               break;
            case MessageType.AlarmStatusInactive:
               Status = AlarmGeneralStatus.Inactive;
               break;
            case MessageType.AlarmStatusAcknowledged:
               Status = AlarmGeneralStatus.Acknowledged;
               break;
         }
      }

      private void UpdateData(Alarm alarm)
      {
         if (alarm != null && alarm.ComponentNumber == ComponentNumber)
         {
            JsonAlarmObject jsonAlarmObject = alarm.mJsonAlarmObject;
            if (jsonAlarmObject != null)
            {
               Name = jsonAlarmObject.name;
               Status = System.Enum.TryParse(jsonAlarmObject.status, ignoreCase: true, out AlarmGeneralStatus parsedStatus) ? parsedStatus : AlarmGeneralStatus.Unknown;
               ExtendedStatus = jsonAlarmObject.extendedStatus;
               Priority = jsonAlarmObject.priority;
               Actions = jsonAlarmObject.actions;

               mJsonAlarmObject = jsonAlarmObject;
            }
         }
      }
   }
}
