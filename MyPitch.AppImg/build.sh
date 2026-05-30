#!/usr/bin/env bash

cd ../MyPitch.Desktop
rm -rf bin dist
dotnet publish --output dist
mkdir ../MyPitch.AppImg/usr/bin
cp dist/* ../MyPitch.AppImg/usr/bin
cd ../MyPitch.AppImg
appimagetool .
