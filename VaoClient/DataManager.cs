using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vao.Client.Components;
using Monitor = Vao.Client.Components.Monitor;

namespace Vao.Client
{
    public class DataManager
    {
        private readonly FlexApiClient mApiClient;
        private readonly Dictionary<int, Camera> mCameraList = new Dictionary<int, Camera>();
        private readonly Dictionary<int, Monitor> mMonitorList = new Dictionary<int, Monitor>();
        private readonly Dictionary<int, Alarm> mAlarmList = new Dictionary<int, Alarm>();
        private readonly object mUpdateCameraListLocker = new object();

        public DataManager(FlexApiClient apiClient)
        {
            mApiClient = apiClient;
        }

        internal Camera GetCamera(int cameraNo)
        {
            lock (mUpdateCameraListLocker)
            {
                if (mCameraList.TryGetValue(cameraNo, out var camera))
                    return camera;
            }
            return null;
        }

        internal List<Camera> GetCameraList()
        {
            lock (mUpdateCameraListLocker)
            {
                if (mCameraList.Count > 0)
                    return mCameraList.Values.ToList();
            }
            return null;
        }

        internal void AddOrUpdateCamera(Camera camera)
        {
            var key = camera.ComponentNumber;
            lock (mUpdateCameraListLocker)
            {
                if (!mCameraList.ContainsKey(key))
                {
                    mCameraList.Add(key, camera);
                }
                else
                {
                    mCameraList[key].UpdateData(camera);
                }
            }
        }

        internal List<Monitor> GetMonitorList()
        {
            lock (mUpdateCameraListLocker)
            {
                if (mMonitorList.Count > 0)
                    return mMonitorList.Values.ToList();
            }
            return null;
        }

        internal int GetMonitorCount()
        {
            lock (mUpdateCameraListLocker)
            {
                return mMonitorList.Count;
            }
        }

        internal bool ContainsMonitor(int key)
        {
            lock (mUpdateCameraListLocker)
            {
                return mMonitorList.ContainsKey(key);
            }
        }

        internal void AddMonitor(int key, Monitor monitor)
        {
            lock (mUpdateCameraListLocker)
            {
                mMonitorList.Add(key, monitor);
            }
        }

        internal void AddOrUpdateMonitor(Monitor monitor, Camera camera)
        {
            var key = monitor.ComponentNumber;
            lock (mUpdateCameraListLocker)
            {
                if (!mMonitorList.ContainsKey(key))
                {
                    mMonitorList.Add(key, monitor);
                    monitor.ActiveCamera = camera;
                }
                else
                {
                    mMonitorList[key].Name = monitor.Name;
                }
            }
        }

        internal void ClearDataManager()
        {
            lock (mUpdateCameraListLocker)
            {
                mCameraList.Clear();
                mMonitorList.Clear();
                mAlarmList.Clear();
            }
        }

        internal List<Alarm> GetAlarmList()
        {
            lock (mUpdateCameraListLocker)
            {
                if (mAlarmList.Count > 0)
                    return mAlarmList.Values.ToList();
            }
            return null;
        }

        internal void AddOrUpdateAlarm(Alarm alarm)
        {
            var key = alarm.ComponentNumber;
            lock (mUpdateCameraListLocker)
            {
                if (!mAlarmList.ContainsKey(key))
                {
                    mAlarmList.Add(key, alarm);
                }
                else
                {
                    mAlarmList[key].Name = alarm.Name;
                }
            }
        }
    }
}
