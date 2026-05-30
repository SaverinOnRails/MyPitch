#!/usr/bin/env bash

cd ../MyPitch.Desktop
rm -rf bin dist
dotnet publish --output dist
cp dist/* ../MyPitch.AppImg/usr/bin
cd ../MyPitch.AppImg
appimagetool .
