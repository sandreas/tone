# tone

`tone` is a cross platform audio tagger and metadata editor to dump and modify metadata for a wide variety of formats, including `mp3`, `m4b`, `flac` and more. It has no dependencies and can be downloaded as single binary for Windows, macOS, Linux and other common platforms.

The code is written in pure `C#` and utilizes the awesome [atldotnet] library to provide full support for a wide variety of audio and metadata formats.

## Features
The main purpose of `tone` is to tag `m4b` audio books for myself. It is planned as a successor to [m4b-tool].

- `dump` metadata of audio files
  - different metadata formats (e.g. `chptfmtnative`, `ffmetadata`, etc.)
  - file information (bitrate, channels, duration, etc.)
  - support for filterable `json` output (similar to `jq`)
  - extensive list of supported tags (default fields like *album* or *artist as well as *custom fields*, *covers*, *chapters*, etc.)
- `tag` audio files with different kinds of metadata
  - different file formats (e.g. `mp3`, `m4b`, and `flac`)
  - extensive list of supported tags (default fields like *album* or *artist as well as *custom fields*, *covers*, *chapters*, etc.)
  - filename to tags via `--path-pattern` (see below)
  - custom javascript taggers via `--script` and `--script-tagger-parameter`


### Future plans

- [ ] `split` large audio files into multiple smaller files (e.g. by chapters) using `ffmpeg`, `fdkaac` and [CliWrap]
- [ ] `merge` multiple smaller audio files into large ones auto generating chapters using silence detection with `ffmpeg`, `fdkaac` and [CliWrap]
- [ ] write unit tests and more detailed documentation


## Support me via GitHub sponsors

If you are using any of my projects and find them helpful, please consider [donating to support me](https://github.com/sponsors/sandreas). I plan to use the money to support other open source projects or charitable purposes. Thank you!

<p align="center">
<a href="https://github.com/sponsors/sandreas"><img src="./assets/help.svg" width="300" alt="sponsor me and donate" style="margin:auto;"></a>
</p>

## TL;DR

### dump tags
```bash
# show help
tone dump --help

# show all tags for single file (input.mp3)
tone dump input.mp3

# show title and artiest tag recursively for all files in directory with extension m4b in FFMETADATA format
tone dump audio-directory/ --include-extension m4b --format ffmetadata --include-property title --include-property artist

# show album only via json format and JSONPath query
tone dump "input.mp3" --format json --query "$.meta.album"

# show audio stream information via JSONPath query
tone dump "input.mp3" --format json --query "$.audio"

```

### modify tags

```bash
# show help
tone tag --help

# change title tag
tone tag input.mp3 --meta-title "a title"

# change a custom field, auto-import covers nearby and show debug info on error (--dry-run simulation)
tone tag --debug --auto-import=covers --meta-additional-field "©st3=testing" input.m4b --dry-run

# recursively set tags genre, artist, series, part and title by path pattern (--dry-run simulation)
tone tag --auto-import=covers --auto-import=chapters --path-pattern="audiobooks/%g/%a/%s/%p - %n.m4b" --path-pattern="audiobooks/%g/%a/%z/%n.m4b" audiobooks/ --dry-run

# write your own custom JavaScript tagger and call this function with parameters to modify metadata on your own
tone tag "harry-potter-1.m4b" --taggers="musicbrainz" --script="musicbrainz.js" --script-tagger-parameter="e2310769-2e68-462f-b54f-25ac8e3f1a21"
```


## Setup

`tone` is a terminal application and deployed as monolithic binary with no dependencies.
This means, that downloading a single file from the [releases] page.

### Linux / macOS
```bash

# linux-arm
wget https://github.com/sandreas/tone/releases/download/v0.1.2/tone-0.1.2-linux-arm.tar.gz

# linux-arm64
wget https://github.com/sandreas/tone/releases/download/v0.1.2/tone-0.1.2-linux-arm64.tar.gz

# linux-x64
wget https://github.com/sandreas/tone/releases/download/v0.1.2/tone-0.1.2-linux-x64.tar.gz

# macos (m1) - not working atm, see issue #6
wget https://github.com/sandreas/tone/releases/download/v0.1.2/tone-0.1.2-osx-arm64.tar.gz

# macos (intel)
wget https://github.com/sandreas/tone/releases/download/v0.1.2/tone-0.1.2-osx-x64.tar.gz

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
iwr -outf tone-0.1.2-win-x64.zip https://github.com/sandreas/tone/releases/download/v0.1.2/tone-0.1.2-win-x64.zip

# extract tone
Expand-Archive -LiteralPath tone-0.1.2-win-x64.zip -DestinationPath .

# test if tone is usable
.\tone --help

# open directory in windows explorer to manually put tone in your %PATH%, e.g. C:\Windows
start .
```

## Input and output formats

`tone` already supports some common input and output formats for metadata, as well as a `tone` specific one. Here are some examples:

### ToneJson format
The *ToneJson* format is specific for `tone`, can contain all supported metadata (including binary images) and looks similar to this example...


**Example**

```json
{
  "audio": {
    "bitrate": 320,
    "format": "MPEG Audio (Layer III)",
    "formatShort": "MPEG",
    "sampleRate": 44100.0,
    "duration": 255920.0,
    "channels": {
      "count": 2,
      "description": "Joint Stereo"
    },
    "frames": {
      "offset": 20749,
      "length": 10236864
    },
    "metaFormat": [
      "id3V24"
    ]
  },
  "meta": {
    "album": "Back in Black",
    "albumArtist": "AC/DC",
    "artist": "AC/DC",
    "discNumber": 1,
    "discTotal": 1,
    "encodedBy": "LAME 3.99.5",
    "genre": "Hard Rock",
    "itunesCompilation": "no",
    "publisher": "Atlantic",
    "recordingDate": "1986-01-01T00:00:00",
    "sortArtist": "AC/DC",
    "title": "Back in Black",
    "trackNumber": 6,
    "trackTotal": 10,
    "embeddedPictures": [
      {
        "type": 2,
        "code": 3,
        "mimetype": "image/jpeg",
        "data": "/9j/4AAQSkZJRgA...9k="
      }
    ],
    "additionalFields": {
      "grP1": "5",
      "tmed": "CD",
      "tlan": "eng",
      "tipl": "arranger",
      "tdor": "1980-07-25",
      "script": "Latn",
      "artist Credit": "AC/DC",
      "albumartistsort": "AC/DC",
      "catalognumber": "16018-2",
      "album Artist Credit": "AC/DC",
      "musicBrainz Album Type": "album",
      "replaygaiN_ALBUM_GAIN": "-8.43 dB",
      "replaygaiN_ALBUM_PEAK": "1.064363",
      "replaygaiN_TRACK_GAIN": "-8.38 dB",
      "replaygaiN_TRACK_PEAK": "1.051585",
      "musicBrainz Album Status": "Official",
      "musicBrainz Album Release Country": "DE",
      "acoustid Id": "8b379144-9a9d-4fc1-897a-a7c0771f8ebb",
      "musicBrainz Album Id": "fdabb997-b984-4097-bd3b-89fafd5e2e75",
      "ufid": "http://musicbrainz.org\u0000ef71afb6-5e51-41df-999b-9e7c7306063a",
      "musicBrainz Artist Id": "66c662b6-6e2f-4930-8610-912e24c63ed1",
      "musicBrainz Album Artist Id": "66c662b6-6e2f-4930-8610-912e24c63ed1",
      "musicBrainz Release Group Id": "d3bc1a64-7561-3787-b680-0003aa50f8f1",
      "musicBrainz Release Track Id": "cf05ab29-27c7-47ed-9450-9f4de676cded",
      "acoustid Fingerprint": "AQADtE...oIIYgRUChBhABIEeWAA0AQR4hSDg",
      "iTunNORM": " 00001AE7 00001AE7 00004340 00004340 00000000 00000000 0000869A 0000869A 00000000 00000000"
    }
  },
  "file": {
    "size": 10257613,
    "created": "2019-06-12T18:50:37.5527895+02:00",
    "modified": "2019-06-12T18:50:37.5527895+02:00",
    "accessed": "2023-02-14T09:21:29.2261032+01:00",
    "path": "music/album/AC_DC/Back in Black",
    "name": "06 - Back in Black.mp3"
  }
}

```

### ChptFmtNative format (also CHPT_FMT_NATIVE)
The *ChptFmtNative* format was initially used in `mp4v2`, but never fully specified. However, there is a loose [spec here](https://github.com/enzo1982/mp4v2/files/8103210/ToolGuide.txt).

**Example**

```
## artist: Cœur de pirate
## album: Blonde
##
## total-duration: 00:38:37.034
##
00:00:00.000 Lève les voiles
00:01:12.709 Adieu
00:03:40.346 Danse et danse
00:06:50.775 Golden Baby
00:09:57.772 Ava
00:13:14.657 Loin d'ici
00:15:58.494 Les amours dévouées
00:18:26.443 Place de la république
00:22:37.664 Cap diamant
00:25:20.925 Verseau
00:29:14.722 Saint-Laurent
00:32:29.519 La petite mort
00:36:19.140 Hôtel amour
```

### ffmetadata format
The *ffmetadata* format was designed for `ffmpeg`, a versatile media encoder and it is [specified here](https://ffmpeg.org/ffmpeg-formats.html#toc-Metadata-1).

**Example**

```
;FFMETADATA1
title=Back in Black
artist=AC/DC
track=6/10
album=Back in Black
disc=1/1
date=1986
genre=Hard Rock
TBPM=0
compilation=0
TMED=CD
language=eng
album_artist=AC/DC
artist-sort=AC/DC
publisher=Atlantic
TIPL=arranger
TDOR=1980-07-25
encoded_by=LAME 3.99.5
Script=Latn
Artist Credit=AC/DC
ALBUMARTISTSORT=AC/DC
CATALOGNUMBER=16018-2
```


## Commands

The features of `tone` are divided by commands. You can `dump` information or `tag` a file and so on. To do so, run

```bash
tone <command> <parameters>
```

Example:
```
tone dump "my-audio-file.mp3"
```



**global options**

There are some global options, that can be used to change the behaviour of the file iterator. These options apply for all commands:

- `--order-by`: Sort files by attribute (defaults to `path`, available options are `path`, `size`, `filename`, `extension`, `created`, `modified`, `accessed`, combine via `,`, descending via `!`), examples:
  - `--order-by="!created"` - sort by create date descending
  - `--order-by="extension,created"` - sort by extension, then by created
  - `--order-by="size,!extension,modified"` - sort by size, then extension descending, then by modification date
- `--limit`: Limit results
  - one value (e.g. `--limit=10`) - top `10` results
  - two values with comma (e.g. `--limit=10,20`) - offset `10` fetch `20` results
- `--include-extensions`: Filter for these extensions
- `--debug`: Enable debug mode (for development or issue reporting)
- `--force`: Try to force action (e.g. overwrite existing files, etc.)

### `dump` - show audio metadata

The `dump` command can be used to show metadata for a wide variety of audio files. You can either specify a single file or a directory, 
which will be traversed recursively. Several output `--format` options are supported. By default a terminal user interface library is used, 
but it is also possible to use `json` or `ffmetadata`.


#### Options reference 
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
        --force                                       
        --include-extension                           
        --order-by                                    
        --limit                                       
        --include-property                            
        --format                                      
        --query 
```

### `tag` - modify audio metadata

The `tag` command can be used to modify audio metadata. Besides using predefined parameters like `--meta-album` it is also possible to 
add or modify custom fields via `--meta-additional-field`, e.g. `--meta-additional-field "©st3=testing"` as well as pictures or chapters.

#### The `--taggers` option
The `--taggers` option allows you to specify a custom set or a different order of internal taggers (NOT input formats), which are gonna be applied. In most cases
changing the order of the *taggers* does not make a huge difference, but fully understanding this option 
requires a bit of technical knowledge. Lets go through a use case to see what you can do with it.

> Note: Internal taggers are applied in a sane order by default and not meant for beginners. Most of the time you don't need to change the order and this usually is for very specific experts use cases. So if you don't fully understand this option, just leave it as is.

*Use case: re-tag `sorttitle` / `sortalbum`*
The following taggers are relevant for this use case:

- `remove` - Removes metadata fields or sets it to an empty value
- `m4bfillup` - Fills up missing or relevant special fields for audio books (e.g. `sorttitle` / `sortalbum`)
- `*` - Represents all remaining taggers, that are not already provided by name

Usually, the `remove` tagger is applied at last. If you provide `--meta-remove-property=sorttitle`, this ensures an existing value will really be 
removed after all taggers have been applied. The `m4bfillup` tagger will automatically generate `sorttitle` / `sortalbum` from `movementname`,
`movement` and `title` / `album` if AND ONLY IF the current value is empty. 

So if you change the `movementname` (e.g. `Harray Potter` to `Harry Potter` because of a typo), `sorttitle` / `sortalbum` will not be updated, 
because these fields already have a value. If you `remove` the `sorttitle` / `sortalbum`, it will not be auto-updated but only removed, 
since `remove` is applied after `m4bfillup`.

This can be solved by reordering the taggers:
- First apply `remove` tagger to remove `sorttitle` / `sortalbum` completely
- Then apply `m4bfillup` to rebuild `sorttitle` / `sortalbum`

```bash
tone tag harry-potter-1.m4b --taggers="remove,m4bfillup" --meta-movement-name="Harry Potter" --meta-remove-property="sortalbum" --meta-remove-property="sorttitle"
```

As you see, most of the time, you only care about one special tagger to be applied first or last. This is why `tone` has an option to add all
remaining taggers to the list using a `*`:

```bash
tone tag harry-potter-1.m4b --taggers="remove,*" --meta-movement-name="Harry Potter" --meta-remove-property="sortalbum" --meta-remove-property="sorttitle"
```

The following taggers are available at the moment (names can be applied case insensitive):

- `ToneJson` - sets metadata values from `tone.json` file
- `Metadata` - sets metadata values from input parameters `--meta-...`
- `Cover` - sets cover from cover files
- `PathPattern` - sets metadata values from path pattern
- `ChptFmtNative` - sets chapters from `chapters.txt` file
- `Equate` - equates 2 or more metadata fields from `--meta-equate`
- `M4BFillUp` - auto fill `album`, `title`, `iTunesMediaType` from existing fields if possible
- `PrependMovementToDescription` - prepends `movement` to all description fields, if set
- `Remove` - removes metadata values from input parameter `--meta-remove-property` and `--meta-remove-additional-field`
- `ScriptTagger` - your personal custom JavaScript taggers (see below)

#### Options reference
```bash
tone tag --help                                                                                                                                                                     
USAGE:
    tone tag [input] [OPTIONS]

EXAMPLES:
    tone tag --help
    tone tag input.mp3 --meta-title "a title"
    tone tag --debug --auto-import=covers --meta-additional-field ©st3=testing input.m4b --dry-run
    tone tag --auto-import=covers --auto-import=chapters --path-pattern="audiobooks/%g/%a/%s/%p - %n.m4b" --path-pattern="audiobooks/%g/%a/%z/%n.m4b" audiobooks/ --dry-run
    tone tag input.mp3 --script musicbrainz.js --script-tagger-parameter e2310769-2e68-462f-b54f-25ac8e3f1a21

ARGUMENTS:
    [input]    Input files or folders

OPTIONS:
    -h, --help                               Prints help information
        --debug                                                     
        --force                                                     
        --include-extension                                         
        --order-by                                                  
        --limit                                                     
    -y, --assume-yes                                                
        --dry-run                                                   
        --taggers                                                   
        --script                                                    
        --script-tagger-parameter                                   
        --prepend-movement-to-description                           
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
        --meta-tone-json-file                                       
    -p, --path-pattern                                              
        --path-pattern-extension                                    
        --meta-equate                                               
        --meta-remove-additional-field                              
        --meta-remove-property
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


#### Custom scripted taggers (experimental)

With `tone v0.0.4` it is possible to use *scripted taggers*. Long story short: You can now use JavaScript
to hook into the tagging mechanism and write your own *extensions* for `tone`.

> Note: script support is limited to a specific subset of JavaScript and does not support every feature that is supported in modern browsers. If you would like to know more, take a look at [jint]

##### create a javascript file

Lets say you would like to consume an external API to set some tags, in our example we use http://musicbrainz.org to tag the audiobook *Harry Potter and the Philosophers Stone* :

```js
// musicbrainz.js
function musicbrainz(metadata, parameters) {
  // e2310769-2e68-462f-b54f-25ac8e3f1a21
  var id = parameters.length > 0 ? parameters[0] : null;
  if(id === null) {
    console.log("Please provide a valid musicbrainz release id to use this tagger");
    return;
  }
  var url = "http://musicbrainz.org/ws/2/release/" + id + "?inc=recordings&fmt=json";
  console.log("fetching url:", url);
  
  // User-Agent header is required for musicbrainz to provide a response
  var json = tone.Fetch(url, {
      headers: {
        'User-Agent': 'Mozilla/5.0 (iPad; U; CPU OS 3_2 like Mac OS X; en-us) AppleWebKit/531.21.10 (KHTML, like Gecko) Version/4.0.4'
      }
  });
  // you could also read a text file in the base path of the audio file
  // json = tone.ReadTextFile(metadata.BasePath + "/musicbrainz.json");
  
  var result = JSON.parse(json);
  metadata.Title = result.title;
  console.log("new title:", result.title);

  if('barcode' in result) {
    metadata.AdditionalFields["ISBN"] = result.barcode;
    console.log("new barcode:", result.barcode);
  }
}

// register your function name as tagger
tone.RegisterTagger("musicbrainz");
```

##### run your tagger

Now you can use the `--script` parameter to load your custom `JavaScript` and furthermore
the `--script-tagger-parameter` to provide the `parameters` array used in the tagger function.
If you would like to prevent the default `tone` taggers to be applied, you can also limit the
them to your scripted one via `--taggers=musicbrainz`.

```bash
tone tag "harry-potter-1.m4b" --taggers="musicbrainz" --script="musicbrainz.js" --script-tagger-parameter="e2310769-2e68-462f-b54f-25ac8e3f1a21"
```

##### Tagger API

To get an overview of fields, that can be accessed or modified via the `metadata` object, you should take a look at the [`IMetadata` interface](https://github.com/sandreas/tone/blob/main/tone/Metadata/IMetadata.cs). Not all of them are primitive types, but there are API at least some helper methods to overcome this problem (more are planned):

| Method                                                                                                                                    | Description                                                                                                                                                                | Notes                                                                                                                     |
|-------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------|
| `tone.RegisterTagger(string functionName):void`                                                                                           | Registers a custom tagger function with `functionName`                                                                                                                     | - |
| `tone.Fetch(string url [, object? options]):string`                                                                                       | Fetches remote `url` contents using `options` inspired by [original fetch API](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API/Using_Fetch)                     | Only a small subset of options is implemented (mainly `method`, `body` and `headers`)                                     |
| `tone.Download(string url, string destinationPath [, object options]):bool`                                                               | Downloads a remote `<url>` to `<destinationFile>` using `options` inspired by [original fetch API](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API/Using_Fetch) | Returns `true` on success, `false` on error<br/>Directories will be created recursively<br/>Files are not overwritten by default |
| `tone.ReadTextFile(string path):string`                                                                                                   | Reads a text file completely as string                                                                                                                                     | - |
| `tone.WriteTextFile(string path, string content):void`                                                                                    | Writes text to a file (create file if not exists, overwrite contents)                                                                                                      | - |
| `tone.AppendTextFile(string path, string content):void`                                                                                   | Appends text to a file  (create file if not exists, append contents)                                                                                                       | - |
| `tone.LimitByteLength(string message, int maxLength):string`                                                                              | Limites text to byte length (not char length)                                                                                                                              | - |
| `tone.CreateDateTime(string dateString):DateTime`                                                                                         | Creates a `DateTime` value from string                                                                                                                                     | e.g. for `metadata.PublishingDate`|
| `tone.CreateTimeSpan(number milliseconds):TimeSpan`                                                                                       | Creates a `TimeSpan` value from string                                                                                                                                     | e.g. for `metadata.TotalDuration`|  
| `tone.CreatePicture(string path):PictureInfo`                                                                                             | Creates a `PictureInfo` value from a path (refer to `Download`)                                                                                                            | for `metadata.EmbeddedPictures`|   
| `tone.CreateChapter(string title, number startMs, number lengthMs [, PictureInfo picture, string subtitle, string uniqueID]):ChapterInfo` | Creates a `ChapterInfo`                                                                                                                                                    | for `metadata.Chapters`|  


# known issues

The following issues are known, part of an external library and already reported:

- flag options (e.g. `--dry-run`) cannot be followed by arguments (e.g. `tone tag --meta-album="album" --dry-run input.mp3`) ([spectre.console 825])
  - workaround: append flag options at the end (`tone tag --meta-album="album" input.mp3 --dry-run`)
- `--meta-*` options cannot be set to empty values ([spectre.console 842])
  - workaround: use `--meta-remove-property` instead
- Value starting with `-` is mistreated as extra option (e.g. `--meta-description "-5 degrees"`)  ([spectre.console 890])
  - workaround: use `--meta-description="-5 degrees"` instead (with `=`)
- Invalid handling of parameter values starting with double quotes ("), e.g. `--meta-description'"quoted" value'` ([spectre.console 891])
- Invalid handling of `--meta-recording-date="2022-07-05"` ([atldotnet 155])


[spectre.console 825]: https://github.com/spectreconsole/spectre.console/issues/825
[spectre.console 842]: https://github.com/spectreconsole/spectre.console/issues/842
[spectre.console 890]: https://github.com/spectreconsole/spectre.console/issues/890
[spectre.console 891]: https://github.com/spectreconsole/spectre.console/issues/891
[atldotnet 155]: https://github.com/Zeugma440/atldotnet/issues/155

[releases]: https://github.com/sandreas/tone/releases
[atldotnet]: https://github.com/Zeugma440/atldotnet
[issue with flags]: https://github.com/spectreconsole/spectre.console/issues/825
[grok.net]: https://github.com/Marusyk/grok.net
[CliWrap]: https://github.com/Tyrrrz/CliWrap
[jint]: https://github.com/sebastienros/jint