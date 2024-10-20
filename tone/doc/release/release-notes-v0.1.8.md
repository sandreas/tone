# Release Notes

## Fixed

- #68 - json output was broken when redirected
- #66 - added instructions for developers 
- #65 - copy from one field to another (more detailed documentation)
- #55 - import cover as type `front` instead of `generic`

## Changed
- Upgraded `atldotnet` library
- Upgraded `Jint` library
- Upgraded `HtmlAgilityPack` library
- Upgraded `Serilog` library


## Setup instructions

`tone` is released as single monolithic binary, so you don't need a setup file or any dependencies (not even a `.NET` runtime). Download the `tone` 
release for your platform, extract it and run it via command line. If you need help choosing your download, here are some hints:

- For Windows, only the x64 platform is available... choose `-win-x64.zip`
- For `musl` (an alternative C library) choose your arch prefixed by `musl` (usually this is used in alpine `docker` images and other lightweight distributions)
- For standard Linux (like *Fedora*, *Ubuntu*, etc.), it is recommended choose your arch without `musl` prefix
- For *macOS* you might need to run `xattr -rd com.apple.quarantine tone` after extracting to remove `quarantine` flag


