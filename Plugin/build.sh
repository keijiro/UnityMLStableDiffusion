#!/bin/sh -e -x

MODE="Release"
MODE_LOW=`echo $MODE | tr '[:upper:]' '[:lower:]'`

DEST='../Packages/jp.keijiro.ml-stable-diffusion/Runtime'

# Build directory removal
#rm -rf .build
#rm -rf .xcodebuild

# macOS binary build using SPM
swift build -c $MODE_LOW --arch arm64 --arch x86_64

# iOS binary build using Xcode
xcodebuild build -scheme StableDiffusionPlugin -configuration $MODE -sdk iphoneos \
 -destination generic/platform=iOS -derivedDataPath .xcodebuild ENABLE_BITCODE=YES

# Binary copy to the destination
cp -f .build/apple/Products/$MODE/libStableDiffusionPlugin.dylib $DEST/macOS
cp -rf .xcodebuild/Build/Products/$MODE-iphoneos/PackageFrameworks/StableDiffusionPlugin.framework $DEST/iOS
