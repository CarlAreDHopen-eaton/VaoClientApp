﻿using System;
using System.Collections.Generic;
using Json.Net;
using RestSharp;
using Vao.Client.Contracts;

namespace Vao.Client.Components
{
   public class Camera
   {
      // ReSharper disable once MemberInitializerValueIgnored
      private string mCameraName = string.Empty;
      private int mCameraNumber = 0;
      private VaoClient mVaoClient;
      private bool? mCameraVideoStream1Ok;
      private bool? mCameraVideoStream2Ok;
      private bool mCameraDataOk;

      internal Camera(int cameraNumber, JsonCameraObject camera, VaoClient vaoClient)
      {
         mCameraNumber = cameraNumber;
         mVaoClient = vaoClient;
         mCameraName = camera.name;
      }

      /// <summary>
      /// The name of the camera in the VMS system.
      /// </summary>
      public string Name
      {
         get { return mCameraName; }
         internal set { mCameraName = value; }
      }

      /// <summary>
      /// The number of the camera in the VMS system.
      /// </summary>
      public int CameraNumber
      {
         get { return mCameraNumber; }
      }

      /// <summary>
      /// Get the camera data status for this camera.
      /// </summary>
      public bool? CameraDataOk { get { return mCameraDataOk; } }

      /// <summary>
      /// Get the camera video status for stream 1 for this camera.
      /// </summary>
      public bool? CameraVideoStream1Ok { get { return mCameraVideoStream1Ok; } }

      /// <summary>
      /// Get the camera video status for stream 2 for this camera.
      /// </summary>
      public bool? CameraVideoStream2Ok { get { return mCameraVideoStream2Ok; } }

      /// <summary>
      /// Gets the live stream RTSP url for the camera.
      /// Note, always call this method before connecting to the camera as the URL may change between calls.
      /// </summary>
      /// <param name="iStream">The stream number/quality, typically 1 for high quality and 2 for low quality.</param>
      /// <returns>The RTSP url for the camera live stream.</returns>
      public string GetCameraLiveStreamUrl(int iStream)
      {
         RestResponse response = mVaoClient.GetVaoCameraInternal(CameraNumber);
         if (response != null && response.IsSuccessful)
         {
            JsonCameraObject oCameraObject = JsonNet.Deserialize<JsonCameraObject>(response.Content);
            if (iStream == 1)
            {
               return oCameraObject.stream1url;
            }
            if (iStream == 2)
            {
               // If the 2nd stream is available we revert back to stream 1.
               if (string.IsNullOrEmpty(oCameraObject.stream2url))
                  return oCameraObject.stream1url;

               return oCameraObject.stream2url;
            }
         }
         return "";
      }

      /// <summary>
      /// Starts panning the camera to the left.
      /// </summary>
      public void PanLeft()
      {
         RestResponse response = mVaoClient.PanTiltStart(CameraNumber, -1, 0);
      }

      /// <summary>
      /// Starts panning the camnera to the right.
      /// </summary>
      public void PanRight()
      {
         RestResponse response = mVaoClient.PanTiltStart(CameraNumber, 1, 0);
      }

      /// <summary>
      /// Stops the Pan/Tilt operation.
      /// </summary>
      public void PanTiltStop()
      {
         RestResponse response = mVaoClient.PanTiltStop(CameraNumber);         
      }

      /// <summary>
      /// Tilts the camera up.
      /// </summary>
      public void TiltUp()
      {
         RestResponse response = mVaoClient.PanTiltStart(CameraNumber, 0, 1);
      }

      /// <summary>
      /// Tilts the camera down.
      /// </summary>
      public void TiltDown()
      {
         RestResponse response = mVaoClient.PanTiltStart(CameraNumber, 0, -1);
      }      
   }
}
