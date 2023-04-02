# Release Notes

This release is a bugfix release mainly to fix one important bug, where `tone` did not work on `arm64` Macs.

## Fixed

- `tone` now works on `M1` and `M2` Macs (#6 - thank you @remmycat)

## Setup instructions

`tone` is released as single monolithic binary, so you don't need a setup file or any dependencies (not even a `.NET` runtime). Download the `tone` 
release for your platform, extract it and run it via command line. If you need help choosing your download, here are some hints:

- For Windows, only the x64 platform is available... choose `-win-x64.zip`
- For `musl` (an alternative C library) choose your arch prefixed by `musl` (usually this is used in alpine `docker` images and other lightweight distributions)
- For standard Linux (like *Fedora*, *Ubuntu*, etc.), chose your arch without `musl` prefix
- For *macOS* you might need to run `xattr -rd com.apple.quarantine tone` after extracting to remove `quarantine` flag


