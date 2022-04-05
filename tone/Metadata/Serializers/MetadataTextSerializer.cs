using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ATL;
using tone.Common.Extensions.Stream;
using tone.Metadata.Formats;

namespace tone.Metadata.Serializers;

public class MetadataTextSerializer : IMetadataSerializer
{
    private readonly ChptFmtNativeMetadataFormat _chapterFormat;

    public MetadataTextSerializer(ChptFmtNativeMetadataFormat chapterFormat)
    {
        _chapterFormat = chapterFormat;
    }

    public async Task<string> SerializeAsync(IMetadata metadata)
    {
        var result = "";
        if (metadata is MetadataTrack track)
        {
            result += await Task.FromResult(DumpTrack(track));
        } 
        return result + await DumpMetadata(metadata, false);
    }

    private string DumpTrack(Track? track)
    {
        return track == null ? "" : DumpOptionalTag("file: ", track.Path) +
                                    DumpOptionalTag("\naudio-format: ", track.AudioFormat) +
               DumpOptionalTag("\nvbr: ", track.IsVBR) +
               DumpOptionalTag("\nchannels: ", track.ChannelsArrangement) +
               DumpOptionalTag("\nbitrate: ", track.Bitrate) +
               DumpOptionalTag("\nsample-rate: ", track.SampleRate)+
                DumpOptionalTag( "codec family: ", track.CodecFamily) + "\n";
        
        // await ;

    }
    
    private async Task<string> DumpMetadata(IMetadata track, bool shortDump)
    {
        var result = "";
        result += DumpOptionalTag("\nduration: ", track.TotalDuration)+
                  DumpOptionalTag("\ngenre: ", track.Genre)+
                  DumpOptionalTag("\nartist: ", track.Artist)+
                  DumpOptionalTag("\nsort-artist: ", track.SortArtist)+
                  DumpOptionalTag("\nalbum-artist: ", track.AlbumArtist)+
                  DumpOptionalTag("\nsort-album-artist: ", track.SortAlbumArtist)+
                  DumpOptionalTag("\noriginal-artist: ", track.OriginalArtist)+
                  DumpOptionalTag("\nnarrator: ", track.Narrator)+
                  DumpOptionalTag("\ncomposer: ", track.Composer)+
                  DumpOptionalTag("\npublisher: ", track.Publisher)+
                  DumpOptionalTag("\nalbum: ", track.Album)+
                  DumpOptionalTag("\nsort-album: ", track.SortAlbum)+
                  DumpOptionalTag("\noriginal-album: ", track.OriginalAlbum)+
                  DumpOptionalTag("\ntitle: ", track.Title)+
                  DumpOptionalTag("\nsort-title: ", track.SortTitle)+
                  DumpOptionalTag("\nseries-title: ", track.MovementName)+
                  DumpOptionalTag("\nseries-part: ", track.Movement)+
                  DumpOptionalTag("\ndisc-number: ", track.DiscNumber, 0)+
                  DumpOptionalTag("\ndisc-total: ", track.DiscTotal, 0)+
                  DumpOptionalTag("\ntrack-number: ", track.TrackNumber, 0)+
                  DumpOptionalTag("\ntrack-total: ", track.TrackTotal, 0)+
                  DumpOptionalTag("\ncopyright: ", track.Copyright)+
                  DumpOptionalTag("\npublishing-date: ", track.PublishingDate, DateTime.MinValue)+
                  DumpOptionalTag("\nrecording-date: ", track.RecordingDate, DateTime.MinValue)+
                  DumpOptionalTag("\npurchase-date: ", track.PurchaseDate, DateTime.MinValue)+
                  DumpOptionalTag("\ngroup: ", track.Group)+
                  DumpOptionalTag("\nencoding-tool: ", track.EncodingTool)+
                  DumpOptionalTag("\nmedia-type: ", track.ItunesMediaType)+
                  DumpOptionalTag("\npopularity: ", track.Popularity, 0)+
                  DumpOptionalTag("\nconductor: ", track.Conductor)
                     ;
        if (track.AdditionalFields != null)
        {
            if (shortDump)
            {
                result += DumpOptionalTag("\nadditional-fields: ", string.Join(", ", track.AdditionalFields.Keys));
            }
            else
            {
                result += DumpAdditionalFields("\nadditional-fields:\n", track.AdditionalFields) + "\n";
            }
        }

        if (track.EmbeddedPictures != null)
        {
            if (shortDump)
            {
                result += DumpOptionalTag("\nembedded-pictures: ", track.EmbeddedPictures?.Count);
            }
            else
            {
                result += DumpEmbeddedPictures("\nembedded-pictures:", track.EmbeddedPictures);
            }

            result += "\n";
        }
        
        result += DumpOptionalTag("\ncomment:\n", track.Comment);
        if ((track.Comment?.Trim() ?? "") != "")
        {
            result += "\n";
        }
        if (shortDump)
        {
            result += DumpOptionalTag("\ndescription (length): ", track.Description?.Length) +
                      DumpOptionalTag("\nlong-description (length): ", track.LongDescription?.Length, 0) +
                      DumpOptionalTag("\nlyrics (length): ", track.Lyrics?.UnsynchronizedLyrics.Length, 0) +
                      DumpOptionalTag("\nchapters (count): ", track.Chapters?.Count);
        }
        else
        {
            
            var bigDump = DumpOptionalTag("\n\ndescription:\n", track.Description) +
                      DumpOptionalTag("\n\nlong-description:\n", track.LongDescription) +
                      DumpLyrics("\n\nlyrics:\n", track.Lyrics) +
                      await DumpChapters("\n\nchapters:\n", track);

            if (bigDump != "")
            {
                result += bigDump + "\n\n";
            }
        }

        return result;
    }

    private async Task<string> DumpChapters(string chapters, IMetadata track)
    {
        if (track.Chapters?.Count == 0)
        {
            return "";
        }

        await using var chapterStream = new MemoryStream();
        if (await _chapterFormat.WriteAsync(track, chapterStream))
        {
            return DumpOptionalTag("\nchapters-table-description:\n", track.ChaptersTableDescription) +
                    DumpOptionalTag(chapters, chapterStream.StreamToString());
        }

        return "";
    }

    private static string DumpLyrics(string prefix, LyricsInfo? trackLyrics)
    {
        if (trackLyrics == null)
        {
            return "";
        }

        List<string> parts = new();
        if (!string.IsNullOrWhiteSpace(trackLyrics.LanguageCode))
        {
            parts.Add("lyrics-language: " + trackLyrics.LanguageCode);
        }

        if (!string.IsNullOrWhiteSpace(trackLyrics.Description))
        {
            parts.Add("lyrics-description: " + trackLyrics.Description);
        }

        if (trackLyrics.SynchronizedLyrics.Count > 0)
        {
            parts.Add("lyrics (parts with timestamp): " + trackLyrics.SynchronizedLyrics.Count);
        }

        if (!string.IsNullOrWhiteSpace(trackLyrics.UnsynchronizedLyrics))
        {
            parts.Add("lyrics:\n" + trackLyrics.UnsynchronizedLyrics);
        }

        return DumpOptionalTag(prefix, string.Join("\n", parts));
    }

    private static string DumpAdditionalFields(string prefix,
        IDictionary<string, string> trackAdditionalFields)
    {
        return PrefixedString(prefix, string.Join("\n",
            trackAdditionalFields
                .Select(kvp => " - " + kvp.Key + ": " + kvp.Value)));
    }

    private static string DumpEmbeddedPictures(string prefix,
        IEnumerable<PictureInfo?> trackEmbeddedPictures)
    {
        return PrefixedString(prefix, string.Join("\n", trackEmbeddedPictures
            .Select(pic => pic == null
                ? ""
                : "\n  " + pic.Position + ".) " + pic.MimeType + " (" +
                  pic.PictureData.Length + " bytes)")));
    }

    private static string PrefixedString(string prefix, string result)
    {
        return result == "" ? result : prefix + result;
    }

    private static string DumpOptionalTag<T>(string prefix, T value, T? ignoreValue = default)
    {
        if (value == null || EqualityComparer<T>.Default.Equals(value, ignoreValue))
        {
            return "";
        }

        if (value is string str && string.IsNullOrEmpty(str))
        {
            return "";
        }

        return DumpTag(prefix, value);
    }

    private static string DumpTag<T>(string prefix, T value) => value switch
    {
        Format f => prefix + f.Name,
        TimeSpan duration => prefix +
                             Math.Floor(duration.TotalHours).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0') +
                             duration.ToString(@"\:mm\:ss\.fff"),
        DateTime dateTime => prefix + dateTime.ToString(CultureInfo.InvariantCulture).Replace(" 00:00:00", ""),
        _ => prefix + (value == null ? "null" : value) 
    };
}