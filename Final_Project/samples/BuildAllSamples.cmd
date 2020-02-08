@echo off

if not defined MRI_INSTANCE_DIR (
call "%~dp0\..\env.cmd"
)

pushd %~dp0

rem Kinect Samples can only be built if Kinect SDK for Windows is installed.
rem Kinect Samples can only be built if Kinect SDK for Windows is installed.
if exist "%KINECTSDK10_DIR%" (
  set DoBuildKinectSamples=yes
  if exist "%MSSpeechDir%" (
     set DoBuildKinectSpeechSamples=yes
  )
)

if exist "%Silverlight4Dir%" (
	set DoBuildSilverlightSamples=yes
)
if exist "%Silverlight5Dir%" (
	set DoBuildSilverlightSamples=yes
)

if exist "%Silverlight4DirWoW%" (
	set DoBuildSilverlightSamples=yes
)
if exist "%Silverlight5DirWoW%" (
	set DoBuildSilverlightSamples=yes
)

call msbuild buildall.proj


rem Show warning if Silverlight is not present
if not defined DoBuildSilverlightSamples (
	echo .
	echo WARNING! Silverlight SDK installation not detected - the samples that have a 
	echo dependency on Silverlight SDK were not built.
)

rem Show warnings if Kinect samples were not built
if not defined DoBuildKinectSamples ( 
  echo.
  echo WARNING! Kinect for Windows SDK was not detected on this machine - Kinect samples were not built 
  echo Download and install Kinect for Windows SDK if you want Kinect samples to build
)

if not defined DoBuildKinectSpeechSamples (
  echo.
  echo WARNING! Microsoft Speech Platform: Server Runtime or Kinect for Windows Runtime Language Pack is not installed on this machine. Kinect MicArray samples were not built
  echo Microsoft Speech Platform - Server Runtime Version 11 can be downloaded from: "http://www.microsoft.com/download/en/details.aspx?id=27226"
  echo Kinect for Windows SDK can be downloaded from: "http://www.microsoft.com/download/en/details.aspx?id=28782"
)

if exist "%MRI_INSTANCE_DIR%\store\cache\contractDirectoryCache.bin" (
del "%MRI_INSTANCE_DIR%\store\cache\contractDirectoryCache.bin"
)

popd

pause
