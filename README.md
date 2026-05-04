# 🎬 FrameFlux - V1.0

FrameFlux is a lightweight Windows desktop tool built with C# WinForms that simplifies **video slow-motion generation using FFmpeg**.  
It provides a clean GUI for applying advanced frame interpolation, motion smoothing, and speed control without needing to manually write FFmpeg commands.

---

## ✨ Features

- 🎥 Video file selection with instant preview info
- 🧠 FFmpeg-powered slow motion processing
- ⚡ Frame interpolation (minterpolate)
- 🎛️ Adjustable:
  - FPS output
  - Slow factor (speed multiplier)
- 🖼️ Thumbnail generation from video
- ⏱️ Video duration detection (ffprobe)
- 📊 Real-time progress display
- 📜 Live FFmpeg logs viewer
- 📂 Auto output file naming or custom filename
- 🔁 Optional auto-open output after processing
- 🧾 Full debug log file saving (`ffmpeg_log.txt`)
- 🪟 Clean modern dark UI

---

## 🖥️ UI Overview

FrameFlux includes a simple workflow:

1. Select a video file
2. Configure settings:
   - FPS
   - Slow factor
   - Interpolation / blending options
3. Click **Apply SlowMo**
4. Monitor progress and logs in real-time
5. Get your processed output instantly

---

## ⚙️ Processing Engine

FrameFlux uses FFmpeg with advanced filters:

- `minterpolate` → frame interpolation (smooth motion)
- `setpts` → time stretching for slow motion
- `libx264` → high-quality encoding
- `aac` → audio encoding

Example command:

```bash
ffmpeg -i input.mp4 -vf "minterpolate=fps=60:mi_mode=mci:mc_mode=aobmc:me_mode=bidir,setpts=10*PTS" -c:v libx264 -crf 18 -preset slow -c:a aac output.mp4
```

## 📊 Logging System

- FrameFlux automatically logs:

- Full FFmpeg command
- Errors and warnings
- Encoding progress
- Exit code
- Performance metrics

-- Logs are saved to:   ffmpeg_log.txt



## ⚠️ Notes:

Processing speed depends heavily on CPU/GPU
First run may download FFmpeg automatically
Large videos may take significant time due to interpolation
