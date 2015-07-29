@ECHO off
SETLOCAL ENABLEDELAYEDEXPANSION

REM Uninstalls the npnf Platform SDK from your current Unity project
REM The npnf Settings asset will not be deleted

SET curdir=%~dp0
SET del=DEL /F /S /Q

pushd "%curdir%"

IF EXIST npnf_filelist.txt (
	FOR /F %%f in (npnf_filelist.txt) DO (
		SET file=%%f
		SET newfile=!file:/=\!
		%del% "!newfile!" 2>NUL
		%del% "!newfile!.meta" 2>NUL
		%del% /AH "!newfile!" 2>NUL
		%del% /AH "!newfile!.meta" 2>NUL
		FOR %%i IN ("!newfile!") DO IF EXIST %%~si\NUL RMDIR /S /Q !newfile! 2>NUL
	)

	%del% "npnf_filelist.txt" 2>NUL
	%del% "npnf_filelist.txt.meta" 2>NUL
	RMDIR /Q NPNF\Resources 2>NUL
	RMDIR /Q NPNF 2>NUL
	popd
	ECHO The npnf Platform SDK is uninstalled!
)ELSE ( 
ECHO Filelist does not exist!
ECHO Failed to uninstall the npnf Platform SDK. You must manually uninstall the SDK.
)