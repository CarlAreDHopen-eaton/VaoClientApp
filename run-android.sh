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

echo "==> Selecting device..."
if [ -n "$ANDROID_SERIAL" ]; then
    echo "    Using ANDROID_SERIAL=$ANDROID_SERIAL"
    ADB_TARGET="-s $ANDROID_SERIAL"
else
    ADB_DEVICES=$("$ADB" devices)
    # Prefer physical devices over emulators
    PHYSICAL=$(echo "$ADB_DEVICES" | awk 'NR>1 && $2=="device" && $1!~/^emulator/ {print $1}')
    EMULATORS=$(echo "$ADB_DEVICES" | awk 'NR>1 && $2=="device" && $1~/^emulator/ {print $1}')
    UNAUTHORIZED=$(echo "$ADB_DEVICES" | awk 'NR>1 && $2=="unauthorized" && $1!~/^emulator/ {print $1}')
    COUNT=$(echo "$PHYSICAL" | grep -c . || true)
    if [ "$COUNT" -eq 1 ]; then
        SERIAL="$PHYSICAL"
        echo "    Auto-selected physical device: $SERIAL"
        ADB_TARGET="-s $SERIAL"
    elif [ "$COUNT" -gt 1 ]; then
        echo "ERROR: Multiple physical devices connected. Set ANDROID_SERIAL to one of:"
        echo "$PHYSICAL"
        exit 1
    else
        # Warn about unauthorized physical devices before falling back
        UNAUTH_COUNT=$(echo "$UNAUTHORIZED" | grep -c . || true)
        if [ "$UNAUTH_COUNT" -gt 0 ]; then
            echo "WARNING: Physical device(s) found but not authorized:"
            echo "$UNAUTHORIZED"
            echo "         Accept the 'Allow USB debugging' prompt on the device, then re-run."
        fi
        # Fall back to emulator
        EMU_COUNT=$(echo "$EMULATORS" | grep -c . || true)
        if [ "$EMU_COUNT" -eq 1 ]; then
            SERIAL="$EMULATORS"
            echo "    Auto-selected emulator: $SERIAL"
            ADB_TARGET="-s $SERIAL"
        elif [ "$EMU_COUNT" -gt 1 ]; then
            echo "ERROR: Multiple emulators connected and no physical device. Set ANDROID_SERIAL."
            exit 1
        else
            echo "ERROR: No connected devices found. Connect a device or start an emulator."
            echo "       Connected devices:"
            echo "$ADB_DEVICES"
            exit 1
        fi
    fi
fi

echo "==> Detecting device ABI..."
DEVICE_ABI=$("$ADB" $ADB_TARGET shell getprop ro.product.cpu.abi 2>/dev/null | tr -d '\r')
case "$DEVICE_ABI" in
    arm64-v8a)   RUNTIME_ID="android-arm64" ;;
    armeabi-v7a) RUNTIME_ID="android-arm" ;;
    x86_64)      RUNTIME_ID="android-x64" ;;
    x86)         RUNTIME_ID="android-x86" ;;
    *)
        echo "WARNING: Unknown ABI '$DEVICE_ABI', defaulting to android-arm64"
        RUNTIME_ID="android-arm64" ;;
esac
echo "    Device ABI: $DEVICE_ABI → RuntimeIdentifier: $RUNTIME_ID"

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
        -p:RuntimeIdentifier="$RUNTIME_ID" \
        -p:RuntimeIdentifiers="$RUNTIME_ID" \
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
            -p:RuntimeIdentifier="$RUNTIME_ID" \
            -p:RuntimeIdentifiers="$RUNTIME_ID" \
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

echo "==> Locating APK..."
FOUND_APK=$(find VaoClientApp/obj/Release/net8.0-android -name "com.vao.clientapp.apk" 2>/dev/null \
    | grep -v "/lp/" | head -1)
if [ -z "$FOUND_APK" ]; then
    FOUND_APK=$(find VaoClientApp/bin/Release -name "com.vao.clientapp.apk" 2>/dev/null | head -1)
fi
if [ -n "$FOUND_APK" ]; then
    APK="$FOUND_APK"
    APK_DIR=$(dirname "$APK")
    SIGNED_APK="$APK_DIR/com.vao.clientapp-signed.apk"
    echo "    Found APK: $APK"
else
    echo "ERROR: Could not find com.vao.clientapp.apk after build."
    find VaoClientApp/obj/Release/net8.0-android -name "*.apk" 2>/dev/null || true
    exit 1
fi

echo "==> Validating APK launcher..."
AAPT=$(find "$ANDROID_SDK_ROOT/build-tools" -type f -name aapt | sort | tail -n 1)
if ! "$AAPT" dump badging "$APK" 2>/dev/null | grep -q "launchable-activity: name='com.vao.clientapp.MainActivity'"; then
    echo "WARNING: Could not confirm launchable activity — proceeding anyway."
    "$AAPT" dump badging "$APK" 2>/dev/null | grep -E "package:|launchable-activity|application-label" || true
fi

echo "==> Signing APK..."
APKSIGNER=$(find "$ANDROID_SDK_ROOT/build-tools" -type f -name apksigner | sort | tail -n 1)
cp "$APK" "$SIGNED_APK"
"$APKSIGNER" sign \
    --ks "$KEYSTORE" \
    --ks-pass pass:android \
    --key-pass pass:android \
    --ks-key-alias androiddebugkey \
    "$SIGNED_APK"

echo "==> Installing..."
"$ADB" $ADB_TARGET install -r "$SIGNED_APK"

echo "==> Launching..."
"$ADB" $ADB_TARGET shell monkey -p com.vao.clientapp -c android.intent.category.LAUNCHER 1

echo "==> Done."
