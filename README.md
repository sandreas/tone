# tone

`tone` is a utility to dump and modify audio metadata. It is written in `C#` and utilizes the 
awesome [atldotnet] library. 

> Important note: `tone` is in a pretty early development state and may contain 
bugs or missing features, so it might be a good idea to backup your files before using it.


## Features

- `dump` metadata of audio files
  - different metadata formats (e.g. `ffmetadata`)
  - extensive list of supported tags (*additional fields*, *covers*, *chapters*, etc.)
- `tag` audio files with different kinds of metadata
  - different file formats (e.g. `mp3`, `m4b`, and `flac`)
  - extensive list of supported tags (*additional fields*, *covers*, *chapters*, etc.)


## Setup

`tone` is a terminal application and deployed as monolithic binary with no dependencies. 
This means, that downloading a single file from the [releases] page.

### Linux / macOS
```bash
# linux-x64
wget https://github.com/sandreas/tone/releases/latest/download/tone-linux.tar.gz

# linux-arm
wget https://github.com/sandreas/tone/releases/latest/download/tone-linux-arm.tar.gz

# linux-arm64
wget https://github.com/sandreas/tone/releases/latest/download/tone-linux-arm64.tar.gz

# macos (arm)
wget https://github.com/sandreas/tone/releases/latest/download/tone-macos.tar.gz

# macos-x64
wget https://github.com/sandreas/tone/releases/latest/download/tone-macos-x64.tar.gz


# untar 
tar xzf tone-*.tar.gz

# install
sudo mv tone /usr/local/bin/

# test
tone --help
```


### Windows

```bash
# windows (powershell)
iwr -outf index.html https://github.com/sandreas/tone/releases/latest/download/tone-windows.zip
Expand-Archive -LiteralPath tone-windows.zip -DestinationPath .
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

[releases]: https://github.com/sandreas/tone/releases
[atldotnet]: https://github.com/Zeugma440/atldotnet