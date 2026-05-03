#!/usr/bin/env bash
set -e

export ANDROID_SDK_ROOT="$HOME/Android/Sdk"
export DOTNET_ROOT="$HOME/.dotnet-microsoft"
export FLATPAK_JBR="/var/lib/flatpak/app/com.google.AndroidStudio/x86_64/stable/dc3668614b503ccfc4e05b437f9c91b9329472a7e1156e628d6b4d84d84f48f5/files/extra/jbr"

ADB="$ANDROID_SDK_ROOT/platform-tools/adb"
APK_DIR="VaoClientApp/obj/Release/net8.0-android/android/bin"
APK="$APK_DIR/com.vao.clientapp.apk"
SIGNED_APK="$APK_DIR/com.vao.clientapp-signed.apk"
KEYSTORE="$HOME/.android/debug.keystore"

cd "$(dirname "$0")"

echo "==> Cleaning..."
"$DOTNET_ROOT/dotnet" build-server shutdown 2>/dev/null || true
rm -rf VaoClientApp/bin/Release/net8.0-android
rm -rf VaoClientApp/obj/Release/net8.0-android
mkdir -p "$APK_DIR"

"$DOTNET_ROOT/dotnet" restore VaoClientApp/VaoClientApp.csproj \
    -p:EnableAndroidTarget=true \
    -p:JavaSdkDirectory="$FLATPAK_JBR" \
    -p:AndroidSdkDirectory="$ANDROID_SDK_ROOT" \
    -v minimal

echo "==> Building and packaging..."
set +e
"$DOTNET_ROOT/dotnet" build VaoClientApp/VaoClientApp.csproj \
        -c Release -f net8.0-android \
        -t:PackageForAndroid \
        -m:1 \
        -p:EnableAndroidTarget=true \
        -p:JavaSdkDirectory="$FLATPAK_JBR" \
        -p:AndroidSdkDirectory="$ANDROID_SDK_ROOT" \
        -v minimal
BUILD_EXIT=$?

if [ "$BUILD_EXIT" -ne 0 ]; then
    echo "==> First build attempt failed (exit $BUILD_EXIT). Retrying with AAPT2 daemon disabled..."
    mkdir -p "$APK_DIR"
    "$DOTNET_ROOT/dotnet" build VaoClientApp/VaoClientApp.csproj \
            -c Release -f net8.0-android \
            -t:PackageForAndroid \
            -m:1 \
            -p:AndroidUseAapt2Daemon=false \
            -p:EnableAndroidTarget=true \
            -p:JavaSdkDirectory="$FLATPAK_JBR" \
            -p:AndroidSdkDirectory="$ANDROID_SDK_ROOT" \
            -v minimal
    BUILD_EXIT=$?
fi
set -e

if [ "$BUILD_EXIT" -ne 0 ]; then
    echo "ERROR: Android build/package failed after retry (exit $BUILD_EXIT)."
    exit "$BUILD_EXIT"
fi

echo "==> Validating APK launcher..."
AAPT=$(find "$ANDROID_SDK_ROOT/build-tools" -type f -name aapt | sort | tail -n 1)
if ! "$AAPT" dump badging "$APK" | grep -q "launchable-activity: name='com.vao.clientapp.MainActivity'"; then
    echo "ERROR: APK does not contain expected launchable activity com.vao.clientapp.MainActivity"
    "$AAPT" dump badging "$APK" | grep -E "package:|launchable-activity|application-label" || true
    exit 1
fi

echo "==> Signing APK..."
if [ ! -f "$APK" ]; then
  echo "ERROR: APK not found at $APK"
  ls -la "$APK_DIR" 2>/dev/null || echo "APK_DIR does not exist"
  exit 1
fi
APKSIGNER=$(find "$ANDROID_SDK_ROOT/build-tools" -type f -name apksigner | sort | tail -n 1)
cp "$APK" "$SIGNED_APK"
"$APKSIGNER" sign \
    --ks "$KEYSTORE" \
    --ks-pass pass:android \
    --key-pass pass:android \
    --ks-key-alias androiddebugkey \
    "$SIGNED_APK"

echo "==> Installing..."
"$ADB" install -r "$SIGNED_APK"

echo "==> Launching..."
"$ADB" shell monkey -p com.vao.clientapp -c android.intent.category.LAUNCHER 1

echo "==> Done."
