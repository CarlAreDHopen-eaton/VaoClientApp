# HERNIS FLEX VAO Demo Application
This is a Demo Application that uses the VAO REST API (https://flexapi.docs.apiary.io) to communicate with a VMS System such as the HERNIS FLEX System.
The application allows the user to select and control cameras. The video streams from the cameras are decoded and displayed using the VLC video control.

![image](https://github.com/user-attachments/assets/afce2c25-07c0-478a-92b6-b2c6a9a0797d)

# Important 3rd party libraries
- RestSharp, communication with the VMS system REST API. (https://github.com/restsharp/RestSharp)
- Avalonia, cross-platform UI framework. (https://github.com/AvaloniaUI/Avalonia)
- FluentFTP, download of video from the VMS system. (https://github.com/robinrodricks/FluentFTP)
- LibVLCSharp, view RTSP video from the VMS system. (https://github.com/videolan/libvlcsharp)
- Newtonsoft.Json, JSON serialization and deserialization. (https://github.com/JamesNK/Newtonsoft.Json)

# Current Features
- Secure and Insecure connection to the VMS System
- User login and access level management
- Camera Selection with search filtering
- Camera Pan/Tilt
- Camera Zoom
- Camera Focus
- Camera Absolute Positioning
- Camera Locking
- RTSP Video Streaming (TCP and UDP)
- Feedback processing and message log with source filtering
- Camera Names
- Camera Renaming
- Preset Positions
- Video Playback (Requires API version 1.1 or later)
- Video Download (Requires API version 1.1 or later)
- Alarm monitoring, activation, acknowledgement, and management
- Configurable themes (dark, light, blue, and more)
- Cross-platform support (Windows, Linux, Android)
- Support for multiple API servers (alternate between servers)

# Future:
- Wipe / WipeWash UI controls

# Build
Built using .NET 8 and Visual Studio 2022 or later. The UI is built with Avalonia and supports cross-platform deployment (Windows, Linux, Android).

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
