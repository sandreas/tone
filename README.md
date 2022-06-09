# tone

`tone` is a cross platform audio utility to dump and modify metadata for a wide variety of formats, including `mp3`, `m4b`, `flac` and more. 
It is written in pure `C#`, deployed as single binary and utilizes the awesome [atldotnet] library
to provide support for a wide variety of audio and metadata formats. 

> Important note: `tone` is in a pretty early state of development and may contain 
> bugs or missing features as well as missing documentation, so it might be a good 
> idea to backup your files before using it and prepare for reading code, if 
> documentation is missing.

## TL;DR

### dump tags
```bash
# show help
tone dump --help

# show all tags for single file (input.mp3)
tone dump input.mp3

# show title and artiest tag recursively for all files in directory with extension m4b in FFMETADATA format
tone dump audio-directory/ --include-extension m4b --format ffmetadata --include-property title --include-property artist
```

### modify tags

```bash
# show help
tone tag --help

# change title tag
tone tag input.mp3 --meta-title "a title"

# change a custom field, auto-import covers nearby and show debug info on error (--dry-run simulation)
tone tag --debug --auto-import=covers --meta-additional-field ©st3=testing input.m4b --dry-run

# recursively set tags genre, artist, series, part and title by path pattern (--dry-run simulation)
tone tag --auto-import=covers --auto-import=chapters --path-pattern="audiobooks/%g/%a/%s/%p - %n.m4b" --path-pattern="audiobooks/%g/%a/%z/%n.m4b" audiobooks/ --dry-run
```

## Features
The main purpose of `tone` is to tag `m4b` audio books for myself. It is planned as a successor to [m4b-tool].

- `dump` metadata of audio files
  - different metadata formats (e.g. `ffmetadata`)
  - extensive list of supported tags (*additional fields*, *covers*, *chapters*, etc.)
- `tag` audio files with different kinds of metadata
  - different file formats (e.g. `mp3`, `m4b`, and `flac`)
  - extensive list of supported tags (*additional fields*, *covers*, *chapters*, etc.)
  - filename to tags via `--path-pattern` (see below)

### Future plans

- [ ] `split` large audio files into multiple smaller files (e.g. by chapters) using `ffmpeg`, `fdkaac` and [CliFX]
- [ ] `merge` multiple smaller audio files into large ones auto generating chapters using silence detection with `ffmpeg`, `fdkaac` and [CliFX]
- [ ] publish an official `docker` image with all dependencies
- [ ] write unit tests and more detailed documentation


## Setup

`tone` is a terminal application and deployed as monolithic binary with no dependencies. 
This means, that downloading a single file from the [releases] page.

### Linux / macOS
```bash

# linux-arm
wget https://github.com/sandreas/tone/releases/download/v0.0.3/tone-0.0.3-linux-arm.tar.gz

# linux-arm64
wget https://github.com/sandreas/tone/releases/download/v0.0.3/tone-0.0.3-linux-arm64.tar.gz

# linux-x64
wget https://github.com/sandreas/tone/releases/download/v0.0.3/tone-0.0.3-linux-x64.tar.gz

# macos (m1) - not working atm, see issue #6
wget https://github.com/sandreas/tone/releases/download/v0.0.3/tone-0.0.3-osx-arm64.tar.gz

# macos (intel)
wget https://github.com/sandreas/tone/releases/download/v0.0.3/tone-0.0.3-osx-x64.tar.gz

# untar 
tar xzf tone-*.tar.gz

# install to your $PATH
sudo mv tone*/tone /usr/local/bin/

# test if tone is usable
tone --help
```


### Windows

```bash
# download for windows (powershell)
iwr -outf tone-0.0.3-win-x64.zip https://github.com/sandreas/tone/releases/download/v0.0.3/tone-0.0.3-win-x64.zip

# extract tone
Expand-Archive -LiteralPath tone-0.0.3-win-x64.zip -DestinationPath .

# test if tone is usable
.\tone --help

# open directory in windows explorer to manually put tone in your %PATH%, e.g. C:\Windows
start .
```


## Commands

### `dump` - show audio metadata

```bash
tone dump --help
USAGE:
    tone dump [input] [OPTIONS]

EXAMPLES:
    tone dump --help
    tone dump input.mp3
    tone dump audio-directory/ --include-extension m4b --format ffmetadata --include-property title --include-property artist

ARGUMENTS:
    [input]    Input files or folders

OPTIONS:
    -h, --help                 Prints help information
        --debug                                       
        --format                                      
        --include-extension                           
        --include-property 
```

### `tag` - modify audio metadata

```bash
tone tag --help 
USAGE:
    tone tag [input] [OPTIONS]

EXAMPLES:
    tone tag --help
    tone tag input.mp3 --meta-title "a title"
    tone tag --debug --auto-import=covers --meta-additional-field ©st3=testing input.m4b --dry-run
    tone tag --auto-import=covers --auto-import=chapters --path-pattern="audiobooks/%g/%a/%s/%p - %n.m4b" --path-pattern="audiobooks/%g/%a/%z/%n.m4b" audiobooks/ --dry-run

ARGUMENTS:
    [input]    Input files or folders

OPTIONS:
    -h, --help                               Prints help information
        --debug                                                     
        --include-extension                                         
        --meta-artist                                               
        --meta-album                                                
        --meta-album-artist                                         
        --meta-bpm                                                  
        --meta-chapters-table-description                           
        --meta-comment                                              
        --meta-composer                                             
        --meta-conductor                                            
        --meta-copyright                                            
        --meta-description                                          
        --meta-disc-number                                          
        --meta-disc-total                                           
        --meta-encoded-by                                           
        --meta-encoder-settings                                     
        --meta-encoding-tool                                        
        --meta-genre                                                
        --meta-group                                                
        --meta-itunes-compilation                                   
        --meta-itunes-media-type                                    
        --meta-itunes-play-gap                                      
        --meta-long-description                                     
        --meta-part                                                 
        --meta-movement                                             
        --meta-movement-name                                        
        --meta-narrator                                             
        --meta-original-album                                       
        --meta-original-artist                                      
        --meta-popularity                                           
        --meta-publisher                                            
        --meta-publishing-date                                      
        --meta-purchase-date                                        
        --meta-recording-date                                       
        --meta-sort-album                                           
        --meta-sort-album-artist                                    
        --meta-sort-artist                                          
        --meta-sort-composer                                        
        --meta-sort-title                                           
        --meta-subtitle                                             
        --meta-title                                                
        --meta-track-number                                         
        --meta-track-total                                          
        --meta-additional-field                                     
        --auto-import                                               
        --meta-chapters-file                                        
        --meta-cover-file                                           
    -p, --path-pattern                                              
        --path-pattern-extension                                    
        --meta-equate                                               
        --meta-remove-additional-field                              
        --meta-remove-property                                      
    -y, --assume-yes                                                
        --dry-run     
```

#### filename to tag via `--path-pattern` / `-p`

It is possible to use the `tag` subcommand with multiple `--path-pattern` arguments to read metadata from path names. Please note:

- If multiple path patterns are present, the first matching one is preferred
- Path patterns can be applied recursively for a whole directory tree as well as for single files
- It is recommended use the `--dry-run` flag to see a diff before changing anything
  - there is an [issue with flags] like `--dry-run`, that they sometimes not work depending on the position - sometimes shifting them around helps
- Path pattern matching is based on [grok.net], so all metadata properties could be read from a path name and there are a lot of things yet to be documented 
  - For now it is recommended to use the short hands below

**short hands**

All short hands are configured to match non-slash (`/`) or part numbers (`[0-9-.IVXLCDM]+`).

- `%a` -  `Artist`
- `%A` -  `SortArtist`
- `%c` -  `Comment`
- `%C` -  `Copyright`
- `%d` -  `Description`
- `%D` -  `LongDescription`
- `%g` -  `Genre`
- `%m` -  `Album`
- `%M` -  `SortAlbum`
- `%n` -  `Title`
- `%N` -  `SortTitle` 
- `%p` -  `Part` (only matching part numbers)
- `%s` -  `MovementName`
- `%t` -  `AlbumArtist`
- `%w` -  `Composer`
- `%y` -  `ReleaseDate`
- `%z` -  `IgnoreDummy`
- `%Z` -  `IgnoreDummy` (only matching part numbers)

[releases]: https://github.com/sandreas/tone/releases
[atldotnet]: https://github.com/Zeugma440/atldotnet
[issue with flags]: https://github.com/spectreconsole/spectre.console/issues/825
[grok.net]: https://github.com/Marusyk/grok.net
[CliFX]: https://github.com/Tyrrrz/CliFx