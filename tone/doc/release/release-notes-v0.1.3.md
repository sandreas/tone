# Release Notes

## Fixed

- `M4bFillupTagger` did not work as expected, now it is working again
- Most known command line parameter parsing are fixed by @FrankRay78 (just awesome, thank you so much)
- Fixed invalid chapter length output in `dump` command on `--format=json` (thank you @itzexor)

## Added

- ...

## Changed

- Extracted `spectre.console` helpers to a library `SpectreConsoleHelpers`

## Setup instructions

`tone` is released as single monolithic binary, so you don't need a setup file or any dependencies (not even a `.NET` runtime). Download the `tone` 
release for your platform, extract it and run it via command line. If you need help choosing your download, here are some hints:

- For Windows, only the x64 platform is available... choose `-win-x64.zip`
- For `musl` (an alternative C library) choose your arch prefixed by `musl` (usually this is used in alpine `docker` images and other lightweight distributions)
- For standard Linux (like *Fedora*, *Ubuntu*, etc.), chose your arch without `musl` prefix
- For *macOS* currently only `x64` does work (see known issues below) - choose `osx-x64.tar.gz`

## Known issues

The following issues are known, part of an external library and already reported:

- *M1* / *M2* macOS releases don't seem to work at the moment (see issue #6) - help would be really appreciated


