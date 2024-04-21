# Release Notes

## todo
- Finish Id-Tagger + docs (for tagging via remotes with id, e.g. musicbrainz, ASIN, etc.)
  - AudibleIdTagger
  - 
- Try to use "year only" on date properties: https://github.com/sandreas/tone/issues/45
- Fix missing metadata specs #46: https://github.com/sandreas/DotnetLibAudioMetadata/blob/0feeda2978cff3d84abd0644c1ae3ccf185d63df/Sandreas.AudioMetadata/AudioMetadata/MetadataTrack.cs#L263

# later
- More ffmetadata support (https://github.com/sandreas/tone/issues/39)


## Fixed

- tone falsely reported as malware (#41)
- movement tag is not removed (#44)



## Changed
- Upgrated `atldotnet` library
- Upgrated `Jint` library (https://github.com/sandreas/tone/issues/40)

## Added

- `--export` option for `dump` command - allows you to export metadata in file (can be used together with `--format`, defaults to `json`)


## Setup instructions

`tone` is released as single monolithic binary, so you don't need a setup file or any dependencies (not even a `.NET` runtime). Download the `tone` 
release for your platform, extract it and run it via command line. If you need help choosing your download, here are some hints:

- For Windows, only the x64 platform is available... choose `-win-x64.zip`
- For `musl` (an alternative C library) choose your arch prefixed by `musl` (usually this is used in alpine `docker` images and other lightweight distributions)
- For standard Linux (like *Fedora*, *Ubuntu*, etc.), chose your arch without `musl` prefix
- For *macOS* you might need to run `xattr -rd com.apple.quarantine tone` after extracting to remove `quarantine` flag


