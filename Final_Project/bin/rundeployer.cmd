@echo off

REM The PackageDeployer service is required by Microsoft (R) Visual Programming Language (VPL) and Microsoft (R) DSS Manifest Editor (DSSME)
REM in order to launch projects that are distributed across multiple machines.
REM
REM Start this on each machine that you want to use with VPL or DSSME

dsshost.exe /t:55555 /c:"http://schemas.microsoft.com/dss/2008/01/packagedeployer.html"
