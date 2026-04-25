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

# Linux Setup (Fedora)
If you run the Avalonia client on Fedora, VLC/LibVLC must have working H.264 codec support.

## 1. Install required runtime packages
```bash
sudo dnf install vlc vlc-plugin-ffmpeg ffmpeg-free libavcodec-free
```

## 2. Ensure real OpenH264 is installed (important)
Some Fedora installs have `noopenh264` (stub package), which causes errors such as:

- `cannot start codec (libopenh264)`
- `VLC could not decode the format "h264"`

Replace stub package with the real codec package:

```bash
sudo dnf swap noopenh264 openh264 -y
```

## 3. Update multimedia packages
```bash
sudo dnf upgrade ffmpeg-free libavcodec-free vlc vlc-plugin-ffmpeg -y
```

## 4. Clear VLC cache
```bash
rm -rf ~/.cache/vlc ~/.vlc
```

## 5. Verify codec package state
```bash
rpm -q openh264 noopenh264
```

Expected result:
- `openh264` installed
- `noopenh264` not installed

## 6. Run the app
Start the application normally after the package changes.

If playback still fails, validate in standalone VLC first with the same RTSP URL:

```bash
vlc "rtsp://user:password@host:554/path"
```
