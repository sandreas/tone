# tone
Audio converter and  tagger

to release:
git tag -a v0.1 -m "tag message"
git push origin v0.1

# todo
- grok (https://github.com/Marusyk/grok.net) - debugger: https://grokdebug.herokuapp.com/
  - ```
    input/Fantasy/J.K. Rowling/Harry Potter/1 - Harry Potter and the Philosopher's Stone/
    input/%{NONSLASH:genre}/%{NONSLASH:author}/%{NONSLASH:series}/%{WORD:part} - %{NONSLASH:title}
    // Add custom patterns Keep Empty Captures Named Captures Only Singles
    NONSLASH [^/\\]*
    NONSPACE [^ ]* 
    ```
- https://github.com/Zeugma440/atldotnet/wiki/3.-Usage-_-Code-snippets#base
- Libs
  - OperationResult (https://github.com/gnaeus/OperationResult)
  - System.IO.Abstractions (https://github.com/TestableIO/System.IO.Abstractions)
  - ATL (https://github.com/Zeugma440/atldotnet)
  - CommandLine (https://github.com/commandlineparser/commandline)
- 
- Deployment
  - Raspberry pi
  - Docker image
- Write articles
  - Github actions
  - Dependency injection for command line


# xxx
```

//     private void UpdatedAdditionalFieldsFromPropertiesForMp3()
//     {
//         // https://id3.org/id3v2.3.0
//         
//         MakeAdditionalField("TIT1", Group, JoinSeparatedString(" ", SeriesTitle, SeriesPart));
//         MakeAdditionalField("TIT3", Subtitle);
//         MakeAdditionalField("TSOT", SortTitle, Title);
//         MakeAdditionalField("TSOA", SortAlbum, Album);
//         MakeAdditionalField("TSOP", SortArtist, Artist);
//         // MakeAdditionalField("????", SortAlbumArtist, Artist);
//
//         if ((Comment ?? "") == "")
//         {
//             // MakeAdditionalField("????", LongDescription, Description);
//             Comment = LongDescription;
//         }
//         
//         // TENC
//         // MakeAdditionalField("©enc",EncodedBy);
//         
//         // TSSM
//         // MakeAdditionalField("©too",EncodingTool);
//         
//         // maybe TOWN? (ownership frame)
//         // MakeAdditionalField("purd",PurchaseDate?.ToString("yyyy/MM/dd"));
//         
//         // not possible
//         // MakeAdditionalField("stik",MediaType);
//         
//         // put narrator to conductor if set and conductor is empty (TPE3
//         if (Narrator?.Length > 0 && (Conductor?.Length ?? 0) == 0)
//         {
//             Conductor = Narrator;
//         }
//         MakeAdditionalField("MVNM",SeriesTitle);
//         MakeAdditionalField("MVIN",SeriesPart);
//         MakeAdditionalField("TXXX:SERIES",SeriesTitle);
//         MakeAdditionalField("TXXX:SERIES-PART",SeriesPart);
//         MakeAdditionalField("TXXX:TMP_GENRE1",Genre);
//         // todo:
//         // MakeAdditionalField("TXXX:TMP_GENRE2",Genre);
//
//     }
//
//     private void UpdatedAdditionalFieldsFromPropertiesForMp4()
//     {
//         
// ;FFMETADATA1
// major_brand=M4A 
// minor_version=512
// compatible_brands=isomiso2
// track=1/1
// disc=1/1
// gapless_playback=1
// description=He was a husband, a father, a preacher - and the preeminent leader of a movement that continues to transform America and the world. Martin Luther King, Jr. was one of the twentieth century's most influential men and lived one of its most extraordinary lives. Now, in a special program commissioned and authorized by his family, here is the life and times of Martin Luther King, Jr. drawn from a comprehensive collection of writings, recordings, and documentary materials, many of which have never before been made public. This history-making autobiography is Martin Luther King in his own words as read by others and in his own words and voice: the mild-mannered, inquisitive child and student who chafed under and eventually rebelled against segregation\; the dedicated young minister who continually questioned the depths of his faith and the limits of his wisdom\; the loving husband and father who sought to balance his family's needs with those of a growing, nationwide movement\; and the reflective, world-famous leader who was fired by a vision of equality for people everywhere. In original recordings of his own vivid, compassionate voice, here at last is Martin Luther King, Jr.'s unforgettable chronicle of his life and his legacy. Years in the making, and woven together from thousands of recordings and documents, including letters to his family and diary entries, this program is a unique compilation which includes many rare recordings of Martin Luther King, Jr. delivering sermons, speeches, lectures, and addresses. Highlights include "I Have a Dream," "Letter from a Birmingham Jail," and "The Nobel Prize Acceptance Speech."
// genre=Biographies & Memoirs/Cultural, Ethnic & Regional
// media_type=2
// date=1998
// WWWAUDIOFILE=https://www.audible.com/pd/The-Autobiography-of-Martin-Luther-King-Jr-Audiobook/B002V01BJQ?ipRedirectOverride\=true&overrideBaseCountry
// album=The Autobiography of Martin Luther King, Jr.
// tmp_Genre2=Cultural, Ethnic & Regional
// tmp_Genre1=Biographies & Memoirs
// title=The Autobiography of Martin Luther King, Jr.
// RELEASETIME=&\#16
// RATING WMP=4.7
// Publisher=\;1998 Time Warner AudioBooks (Packaging Only), A Division of Time Warner Trade Publishing \; 1998 Intellectual Properties Management, Inc., All Rights Reserved\; 16 9\; 1998 Time Warner AudioBooks (Packaging Only), A Division of Time Warner Trade Publishing
// copyright=1998 Intellectual Properties Management, Inc., All Rights Reserved
// composer=Levar Burton
// comment=He was a husband, a father, a preacher - and the preeminent leader of a movement that continues to transform America and the world. Martin Luther King, Jr. was one of the twentieth century's most influential men and lived one of its most extraordinary lives. Now, in a special program commissioned and authorized by his family, here is the life and times of Martin Luther King, Jr. drawn from a comprehensive collection of writings, recordings, and documentary materials, many of which have never before been made public. This history-making autobiography is Martin Luther King in his own words as read by others and in his own words and voice: the mild-mannered, inquisitive child and student who chafed under and eventually rebelled against segregation\; the dedicated young minister who continually questioned the depths of his faith and the limits of his wisdom\; the loving husband and father who sought to balance his family's needs with those of a growing, nationwide movement\; and the reflective, world-famous leader who was fired by a vision of equality for people everywhere. In original recordings of his own vivid, compassionate voice, here at last is Martin Luther King, Jr.'s unforgettable chronicle of his life and his legacy. Years in the making, and woven together from thousands of recordings and documents, including letters to his family and diary entries, this program is a unique compilation which includes many rare recordings of Martin Luther King, Jr. delivering sermons, speeches, lectures, and addresses. Highlights include "I Have a Dream," "Letter from a Birmingham Jail," and "The Nobel Prize Acceptance Speech."
// ASIN=B002V01BJQ
// artist=Dr. Martin Luther King Jr., Levar Burton
// sort_album=The Autobiography of Martin Luther King, Jr.
// album_artist=Dr. Martin Luther King Jr.
// encoder=Lavf57.83.100
//          
//         
//         MakeAdditionalField("©grp", Group, JoinSeparatedString(" ", SeriesTitle, SeriesPart));
//         MakeAdditionalField("----:com.apple.iTunes:SUBTITLE", Subtitle);
//         MakeAdditionalField("sonm", SortTitle, Title);
//         MakeAdditionalField("soal", SortAlbum, Album);
//         MakeAdditionalField("soar", SortArtist, Artist);
//         MakeAdditionalField("soaa", SortAlbumArtist, AlbumArtist);
//         MakeAdditionalField("ldes", LongDescription, Description);
//         MakeAdditionalField("©enc", EncodedBy, "tone");
//         MakeAdditionalField("©too", EncodingTool, "tone");
//         MakeAdditionalField("purd", PurchaseDate?.ToString("yyyy/MM/dd"));
//         MakeAdditionalField("stik", MediaType);
//         
//         if (MediaType == "2")
//         {
//             // M4B Gapless album = 1
//             MakeAdditionalField("pgap", "1");
//         }
//         if (SeriesTitle?.Length > 0)
//         {
//             // Show Movement (M4B), if Series then = 1 else blank
//             MakeAdditionalField("shwm", "1");
//         }
//         
//         MakeAdditionalField("©nrt", Narrator, Composer);
//         MakeAdditionalField("----:com.pilabor.tone:SERIES_TITLE", SeriesTitle);
//         MakeAdditionalField("----:com.pilabor.tone:SERIES_PART", SeriesPart);
//     }

    /*
var list = new[]
        {
            ("Harry Potter 1 - Harry Potter und der Stein der Weisen",
                new[] { "Harry Potter", "1", "Harry Potter und der Stein der Weisen" }),
            ("Harry Potter - Harry Potter und der Stein der Weisen",
                new[] { "Harry Potter", "", "Harry Potter und der Stein der Weisen" }),
            ("Harry Potter und der Stein der Weisen",
                new[] { "", "", "Harry Potter und der Stein der Weisen" }),
            ("Harry Potter",
                new[] { "Harry Potter", "", "" }),
            ("Harry Potter 1",
                new[] { "Harry Potter", "1", "" }),
            ("1 - Harry Potter und der Stein der Weisen",
                new[] { "", "1", "Harry Potter und der Stein der Weisen" }),
        };
        var separators = new[] { " ", " - " };

        foreach (var (expected, input) in list)
        {
            var actual = ConcatMulti(separators, input);
            if (expected != actual)
            {
                _console.Error.WriteLine($"error: <{expected}>!=<{actual}>");
            }
        }        
     */
     
     
    private static string? JoinSeparatedString(string separator, params string?[] values)
    {
        return JoinSeparatedString(new[] { separator }, values);
    }

    private static string JoinSeparatedString(IReadOnlyList<string> separator, params string?[] values)
    {
        var concatValues = new List<string?>();
        for (var i = 0; i < values.Length; i++)
        {
            if (string.IsNullOrEmpty(values[i]))
            {
                continue;
            }
            var separatorIndex = i - 1;
            if (separatorIndex >= 0 && separatorIndex < separator.Count && concatValues.Count > 0)
            {
                concatValues.Add(separator[separatorIndex]);
            }
            concatValues.Add(values[i]);
        }
        return string.Join("", concatValues);
    }
    private void MakeAdditionalField(string key, params string?[] values)
    {
        foreach (var value in values)
        {
            if (value != null)
            {
                AdditionalFields[key] = value;
                return;
            }
        }
    }


    protected new void Update(bool onlyReadEmbeddedPictures = false)
    {
        base.Update(onlyReadEmbeddedPictures);
        switch (AudioFormat.ID)
        {
            case AudioDataIOFactory.CID_MP3:
                // todo: LoadPropertiesFromAdditionalFieldsForMp3
                break;
            case AudioDataIOFactory.CID_MP4:
                // todo: LoadPropertiesFromAdditionalFieldsForMp4
                break;
        }
    }
    
    
/*
ItunesGapless (pgap, =1)
ShowMovement (shwm, if Series then = 1 else blank)
MovementName (MVNM, SeriesTitle)
Movement (MVIN, SeriesPart)
TXXX (SERIES)** 	SeriesTitle
TXXX (SERIES-PART)** 	SeriesPart
TXXX (TMP_GENRE1)** 	Genre 1
TXXX (TMP_GENRE2)** 	Genre 2
TIT2 (TITLE) 	Not Scraped, but used for Chapter Title If no chapter data available set to filename
 */

// https://wiki.hydrogenaud.io/index.php?title=Tag_Mapping
// https://help.mp3tag.de/main_tags.html
// https://github.com/seanap/Plex-Audiobook-Guide#tags-that-are-being-set

// todo:
// - write series to group: TIT1 (CONTENTGROUP)
/*
TPE2 (ALBUMARTIST) 	Author
TIT1 (CONTENTGROUP) 	Series, Book #
TIT3 (SUBTITLE) 	Subtitle
COMM (COMMENT) 	Publisher's Summary (MP3)
TSOA (ALBUMSORT) 	If ALBUM only, then %Title%, If ALBUM and SUBTITLE, then %Title% - %Subtitle%, If Series, then %Series% %Series-part% - %Title%
stik (ITUNESMEDIATYPE) 	M4B Media type = Audiobook (Normal, Audiobook, Music Video, Movie, TV Show, Booklet)
pgap (ITUNESGAPLESS) 	M4B Gapless album = 1
shwm (SHOWMOVEMENT) 	Show Movement (M4B), if Series then = 1 else blank
MVNM (MOVEMENTNAME) 	Series
MVIN (MOVEMENT) 	Series Book #
TXXX (SERIES)** 	Series
TXXX (SERIES-PART)** 	Series Book #
TXXX (TMP_GENRE1)** 	Genre 1
TXXX (TMP_GENRE2)** 	Genre 2
TIT2 (TITLE) 	Not Scraped, but used for Chapter Title - If no chapter data available set to filename

ASIN (ASIN) 	Amazon Standard Identification Number
POPM (RATING WMP) 	Audible Rating
WOAF (WWWAUDIOFILE) 	Audible Album URL
 */
/*
TIT1 (CONTENTGROUP) 	Series, Book #
TALB (ALBUM) 	Title
TIT3 (SUBTITLE) 	Subtitle
TPE1 (ARTIST) 	Author, Narrator
TPE2 (ALBUMARTIST) 	Author
TCOM (COMPOSER) 	Narrator
TCON (GENRE) 	Genre1/Genre2
TYER (YEAR) 	Copyright Year*
COMM (COMMENT) 	Publisher's Summary (MP3)
desc (DESCRIPTION) 	Publisher's Summary (M4B)
TSOA (ALBUMSORT) 	If ALBUM only, then %Title%, If ALBUM and SUBTITLE, then %Title% - %Subtitle%, If Series, then %Series% %Series-part% - %Title%
TDRL (RELEASETIME) 	Audiobook Release Year
TPUB (PUBLISHER) 	Publisher
TCOP (COPYRIGHT) 	Copyright
ASIN (ASIN) 	Amazon Standard Identification Number
POPM (RATING WMP) 	Audible Rating
WOAF (WWWAUDIOFILE) 	Audible Album URL
stik (ITUNESMEDIATYPE) 	M4B Media type = Audiobook
pgap (ITUNESGAPLESS) 	M4B Gapless album = 1
shwm (SHOWMOVEMENT) 	Show Movement (M4B), if Series then = 1 else blank
MVNM (MOVEMENTNAME) 	Series
MVIN (MOVEMENT) 	Series Book #
TXXX (SERIES)** 	Series
TXXX (SERIES-PART)** 	Series Book #
TXXX (TMP_GENRE1)** 	Genre 1
TXXX (TMP_GENRE2)** 	Genre 2
CoverUrl 	Album Cover Art
TIT2 (TITLE) 	Not Scraped, but used for Chapter Title - If no chapter data available set to filename
 */

```

# notes
```
        <OutputType>Exe</OutputType>
        <TargetFramework>net6.0</TargetFramework>
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>
        <DockerDefaultTargetOS>Linux</DockerDefaultTargetOS>
        <InvariantGlobalization>true</InvariantGlobalization>
        <PublishReadyToRun>true</PublishReadyToRun>
        <!--
        <PublishTrimmed>true</PublishTrimmed>
        <PublishSingleFile>true</PublishSingleFile>
```

## Dependency injection
see https://www.youtube.com/watch?v=dgJ1nS2CLpQ
- Install `Microsoft.Extensions.Hosting`


## general
- `dotnet publish tone/tone.csproj --framework net5.0 --runtime linux-x64 -c Release -o "tone/dist/"`
- Runtime identifer catalog: https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
- Project settings

```xml
  <OutputType>Exe</OutputType>
  <TargetFramework>net5.0</TargetFramework>
  <DockerDefaultTargetOS>Linux</DockerDefaultTargetOS>
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <PublishReadyToRun>true</PublishReadyToRun>
  <PublishTrimmed>true</PublishTrimmed>
```




# deployment
```bash
#linux (linux-x64)
dotnet publish tone/tone.csproj --framework net6.0 --runtime linux-x64 -o "tone/dist/linux-x64/" -p:PublishSingleFile=true --self-contained true -p:PublishReadyToRun=true -p:PublishReadyToRunShowWarnings=true -p:PublishTrimmed=true -c Release

# macOS (osx-x64)
# dotnet publish tone/tone.csproj --runtime osx-x64 -o "tone/dist/osx-x64/" -p:PublishSingleFile=true --self-contained true -p:PublishReadyToRun=true -p:PublishTrimmed=true --framework net5.0 -c Release
dotnet publish tone/tone.csproj --runtime osx-x64 -o "tone/dist/osx-x64/" -p:PublishSingleFile=true --self-contained true -p:PublishTrimmed=true --framework net5.0 -c Release


# win
dotnet publish tone/tone.csproj --runtime win-x64 -o "tone/dist/win-x64/" -p:PublishSingleFile=true --self-contained true -p:PublishReadyToRun=true -p:PublishTrimmed=true --framework net5.0 -c Release

```