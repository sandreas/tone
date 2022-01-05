using System.Collections.Generic;
using CommandLine;

namespace tone.Options;

[Verb("merge", HelpText = "merge audio files")]
public class MergeOptions: OptionsBase
{
    [Option('i', "input", HelpText = "Input file(s) to process")]
    public IEnumerable<string> Input { get; set; } = new List<string>();

    [Option('o', "output",  HelpText = "Output file or directory")]
    public string Output { get; set; } = "";
    
    [Option('b', "batch-pattern", HelpText = "Batch patterns")]
    public IEnumerable<string> BatchPatterns { get; set; } = new List<string>();
    
    [Option('l',"chapter-max-length", HelpText = "Maximum chapter length")]
    public string ChapterMaxLenght { get; set; } = "300,900";

    [Option("include-extensions", Separator = ',', Required = false, HelpText = "Set output to verbose messages.")]
    public IEnumerable<string> IncludeExtensions { get; set; } = new List<string>();

    [Option("audio-channels", Required = false, HelpText = "Set output to verbose messages.")]
    public int AudioChannels { get; set; } = 1;
  
    // todo: 64k? => enum?
    [Option("audio-bitrate", Required = false, HelpText = "Set output to verbose messages.")]
    public int AudioBitrate { get; set; } = 64000;

    [Option("audio-sample-rate", Required = false, HelpText = "Set output to verbose messages.")]
    public int AudioSampleRate { get; set; } = 64000;
    
    [Option("ipod-adjust",  Required = false, HelpText = "Set output to verbose messages.")]
    public bool IpodAdjust { get; set; } = false;    
    
    
    /*
    --audio-codec=libfdk_aac 
    --audio-profile=aac_he 
*/

    /*
    [Option( "audio-format", HelpText = "")]
    public string AudioFormat { get; set; } = "";
    */

    /*
     *       --logfile[=LOGFILE]                        file to log all output [default: ""]
      --debug                                    enable debug mode - sets verbosity to debug, logfile to m4b-tool.log and temporary encoded files are not deleted
  -f, --force                                    force overwrite of existing files
      --no-cache                                 clear cache completely before doing anything
      --ffmpeg-threads[=FFMPEG-THREADS]          specify -threads parameter for ffmpeg - you should also consider --jobs when merge is used [default: ""]
      --platform-charset[=PLATFORM-CHARSET]      Convert from this filesystem charset to utf-8, when tagging files (e.g. Windows-1252, mainly used on Windows Systems) [default: ""]
      --ffmpeg-param[=FFMPEG-PARAM]              Add argument to every ffmpeg call, append after all other ffmpeg parameters (e.g. --ffmpeg-param="-max_muxing_queue_size" --ffmpeg-param="1000" for ffmpeg [...] -max_muxing_queue_size 1000) (multiple values allowed)
  -a, --silence-min-length[=SILENCE-MIN-LENGTH]  silence minimum length in milliseconds [default: 1750]
  -b, --silence-max-length[=SILENCE-MAX-LENGTH]  silence maximum length in milliseconds [default: 0]
      --max-chapter-length[=MAX-CHAPTER-LENGTH]  maximum chapter length in seconds - its also possible to provide a desired chapter length in form of 300,900 where 300 is desired and 900 is max - if the max chapter length is exceeded, the chapter is placed on the first silence between desired and max chapter length [default: "0"]
      --name[=NAME]                              custom name, otherwise the existing metadata will be used
      --sortname[=SORTNAME]                      custom sortname, that is used only for sorting
      --album[=ALBUM]                            custom album, otherwise the existing metadata for name will be used
      --sortalbum[=SORTALBUM]                    custom sortalbum, that is used only for sorting
      --artist[=ARTIST]                          custom artist, otherwise the existing metadata will be used
      --sortartist[=SORTARTIST]                  custom sortartist, that is used only for sorting
      --genre[=GENRE]                            custom genre, otherwise the existing metadata will be used
      --writer[=WRITER]                          custom writer, otherwise the existing metadata will be used
      --albumartist[=ALBUMARTIST]                custom albumartist, otherwise the existing metadata will be used
      --year[=YEAR]                              custom year, otherwise the existing metadata will be used
      --description[=DESCRIPTION]                custom short description, otherwise the existing metadata will be used
      --longdesc[=LONGDESC]                      custom long description, otherwise the existing metadata will be used
      --comment[=COMMENT]                        custom comment, otherwise the existing metadata will be used
      --copyright[=COPYRIGHT]                    custom copyright, otherwise the existing metadata will be used
      --encoded-by[=ENCODED-BY]                  custom encoded-by, otherwise the existing metadata will be used
      --cover[=COVER]                            custom cover, otherwise the existing metadata will be used
      --skip-cover                               skip extracting and embedding covers
      --series[=SERIES]                          custom series, this pseudo tag will be used to auto create sort order (e.g. Harry Potter or The Kingkiller Chronicles)
      --series-part[=SERIES-PART]                custom series part, this pseudo tag will be used to auto create sort order (e.g. 1 or 2.5)
      --audio-format[=AUDIO-FORMAT]              output format, that ffmpeg will use to create files [default: "m4b"]
      --audio-channels[=AUDIO-CHANNELS]          audio channels, e.g. 1, 2 [default: ""]
      --audio-bitrate[=AUDIO-BITRATE]            audio bitrate, e.g. 64k, 128k, ... [default: ""]
      --audio-samplerate[=AUDIO-SAMPLERATE]      audio samplerate, e.g. 22050, 44100, ... [default: ""]
      --audio-codec[=AUDIO-CODEC]                audio codec, e.g. libmp3lame, aac, ... [default: ""]
      --audio-profile[=AUDIO-PROFILE]            audio profile, when using extra low bitrate - valid values: aac_he, aac_he_v2 [default: ""]
      --adjust-for-ipod                          auto adjust bitrate and sampling rate for ipod, if track is too long (may result in low audio quality)
      --fix-mime-type                            try to fix MIME-type (e.g. from video/mp4 to audio/mp4) - this is needed for some players to prevent an empty video window
  -o, --output-file=OUTPUT-FILE                  output file
      --include-extensions[=INCLUDE-EXTENSIONS]  comma separated list of file extensions to include (others are skipped) [default: "aac,alac,flac,m4a,m4b,mp3,oga,ogg,wav,wma,mp4"]
  -m, --musicbrainz-id=MUSICBRAINZ-ID            musicbrainz id so load chapters from
      --no-conversion                            skip conversion (destination file uses same encoding as source - all encoding specific options will be ignored)
      --batch-pattern[=BATCH-PATTERN]            multiple batch patterns that can be used to merge all audio books in a directory matching the given patterns (e.g. %a/%t for author/title) - parameter --output-file must be a directory (multiple values allowed)
      --dry-run                                  perform a dry run without converting all the files in batch mode (requires --batch-pattern)
      --jobs[=JOBS]                              Specifies the number of jobs (commands) to run simultaneously [default: 1]
      --use-filenames-as-chapters                Use filenames for chapter titles instead of tag contents
      --no-chapter-reindexing                    Do not perform any reindexing for index-only chapter names (by default m4b-tool will try to detect index-only chapters like Chapter 1, Chapter 2 and reindex it with its numbers only)
  -h, --help                                     Display this help message
  -q, --quiet                                    Do not output any message
  -V, --version                                  Display this application version
      --ansi                                     Force ANSI output
      --no-ansi                                  Disable ANSI output
     */
}