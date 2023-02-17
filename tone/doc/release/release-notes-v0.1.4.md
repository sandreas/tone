# Release Notes

**Caution**: This release contains some major refactorings and although I did test all the changes extensively, it still may have broken features under specific circumstances. Be sure to backup your data and do extensive testing before upgrading to `v0.1.4`.

## Fixed

- additional fields are no longer removed but merged when using `--meta-additional-field="..." (#11)
- Duplicate `dump` of additional fields (#7)
- Description should now work like expected in `dump` (#17)

## Changed

- Release mode for macOS-arm64 targets temporarily switched from `Release` to `Debug`, maybe this fixes issue #6
- Improved documentation (issues #20 and #30)
- Bumped libraries to newest versions 

## Added

- `tag` command now supports `--auto-import=ffmetadata` (#27)
- `tag` command can now output json, when `--format=json` is specified (full tag output, no diff, see #29)

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


