@echo off

:: Set the version variable
set version=1.1.1

:: Use the version variable in the command
c:\opt\butler\butler.exe push LastBreath_v%version%.zip diogoandrade/last-breath:windows --userversion %version%
