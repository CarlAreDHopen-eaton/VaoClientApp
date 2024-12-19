# HERNIS FLEX VAO Demo Application
This is a Demo Application that uses the VAO REST API (https://flexapi.docs.apiary.io) to communicate with a VMS System such as the HERNIS FLEX System.
The application allows the user to select and control cameras. The video streams from the cameras are decoded and displayed using the VLC video control.

![image](https://github.com/user-attachments/assets/afce2c25-07c0-478a-92b6-b2c6a9a0797d)

# Important 3rd party libraries
- RestSharp, communication with the VMS system REST API. (https://github.com/restsharp/RestSharp)
- DarkUI, dark mode user interface for WinForms. (https://github.com/RobinPerris/DarkUI)
- FluentFTP, download of video from the VMS system. (https://github.com/robinrodricks/FluentFTP)
- LibVLCSharp, view RTSP video from the VMS system. (https://github.com/videolan/libvlcsharp)

# Current Features
- Secure and Insecure connection to the VMS System
- Camera Selection
- Camera Pan/Tilt
- Camera Zoom
- Camera Focus
- RTSP Video Streaming (TCP and UDP)
- Feedback processing and message log
- Camera Names
- Camera Renaming
- Preset Positions
- Video Playback (Requires API version 1.1 or later)
- Video Download (Requires API version 1.1 or later)
  
# Future:
- Support for multiple API servers (alernate between servers)
- Wipe / WipeWash

# Build
Built using Visual Studio 2019
