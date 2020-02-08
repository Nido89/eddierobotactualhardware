@echo off

pushd %~dp0

rem set MRI_MAP_DRIVE=R:
set MRI_INSTANCE_DIR=%CD%
set PATH=%PATH%;%MRI_INSTANCE_DIR%\bin\;%WINDIR%\Microsoft.NET\Framework\v4.0.30319\;%programfiles%\Microsoft SDKs\Windows\v7.0A\bin\NETFX 4.0 Tools
set MRI_REFERENCE_PATH=%MRI_INSTANCE_DIR%\bin\;%MRI_INSTANCE_DIR%\bin\cf\
set MriAssemblySampleKeyFile="%MRI_INSTANCE_DIR%\samples\mrisamples.snk"
REM We define this env var so that we can build Kinect samples (image and depth sensor stuff)
set MSSpeechDir=%WINDIR%\assembly\GAC_MSIL\Microsoft.Speech\11*
set Silverlight4Dir=%programfiles%\Microsoft SDKs\Silverlight\v4.0
set Silverlight5Dir=%programfiles%\Microsoft SDKs\Silverlight\v5.0
set Silverlight4DirWoW=%ProgramFiles(x86)%\Microsoft SDKs\Silverlight\v4.0
set Silverlight5DirWoW=%ProgramFiles(x86)%\Microsoft SDKs\Silverlight\v5.0


REM This must happen BEFORE the SETLOCAL
call :CheckPlatformEnvironment

setlocal
REM We expand the name of SN to a full path later depending on the shell we find
set MRI_NET_SN=sn.exe
set MRI_VS=%PROGRAMFILES%\Microsoft Visual Studio 10.0
set MRI_VS_SHELL=%MRI_VS%\VC\vcvarsall.bat

if exist "%MRI_VS_SHELL%" call "%MRI_VS_SHELL%" %1 %2
call :CreateKey %MRI_NET_SN%

REM Map a drive to the application directory
if defined MRI_MAP_DRIVE (
	subst %MRI_MAP_DRIVE% "%MRI_INSTANCE_DIR%" > nul
	endlocal
	pushd %MRI_MAP_DRIVE%
	subst
)
echo Microsoft DSS Command Prompt

goto :eof

:CheckPlatformEnvironment
  if "%PLATFORM%" NEQ "" (

    echo Warning! PLATFORM environment variable is set to "%PLATFORM%". 
    echo          Projects might not compile correctly.
    echo          Deleting PLATFORM environment variable.
    set PLATFORM=

  )
  exit /b 0

:CreateKey %1
set SN_EXE=%~$PATH:1
if "%SN_EXE%"=="" (
  set SN_EXE=%MRI_INSTANCE_DIR%\Tools\sn.exe
)

if not exist "%MRI_INSTANCE_DIR%\samples\mrisamples.snk" (
  if not exist "%SN_EXE%" (
    echo Did not find the mrisamples key and the sn utility. Samples might not compile correctly.
  ) else (
    call "%SN_EXE%" -k "%MRI_INSTANCE_DIR%\samples\mrisamples.snk"
    )
  )
)

goto :eof
