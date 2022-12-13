using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ATL;
using Sandreas.AudioMetadata;
using Spectre.Console;

namespace tone.Serializers;

public class SpectreConsoleSerializer : IMetadataSerializer
{
    private readonly IAnsiConsole _console;

    public SpectreConsoleSerializer(IAnsiConsole console)
    {
        _console = console;
    }

    public async Task<string> SerializeAsync(IMetadata metadata)
    {
        var color = Color.Aquamarine1;
        DumpPath(metadata, color);
        DumpFileProperties(metadata, color);
        DumpEmbeddedPictures(metadata, color);
        DumpMetadata(metadata, color);
        DumpAdditionalFields(metadata, color);
        DumpLongText("comment", metadata.Comment, color);
        DumpLyrics(metadata.Lyrics, color);
        DumpLongText("chapters-table-description", metadata.ChaptersTableDescription, color);
        DumpChapters(metadata, color);
        DumpLongText("description", metadata.Description, color);
        DumpLongText("long-description", metadata.LongDescription, color);
        return await Task.FromResult("");
    }

    private void DumpAdditionalFields(IMetadata metadata, Color color)
    {
        var firstCol = new TableColumn("property")
        {
            Width = 17,
        }.RightAligned();
        var properties = new Table()
            .AddColumn(firstCol)
            .AddColumn("value")
            .HideHeaders()
            .BorderColor(color);

        properties.Title = new TableTitle("additional metadata fields", new Style(color));

        var additionalFields = metadata is MetadataTrack m ? m.AdditionalFields.Where(kvp => !m.MappedAdditionalFields.ContainsKey(kvp.Key)) : metadata.AdditionalFields;
        var counter = 0;
        foreach (var (key, value) in additionalFields)
        {
            properties.AddRow(Markup.Escape(key), Markup.Escape(value));
            counter++;
        }

        if (counter > 0)
        {
            _console.Write(properties);
        }
    }

    private void DumpLyrics(LyricsInfo? lyricsContainer, Color color)
    {
        StringifyMulti(lyricsContainer, s =>
        {
            if (s.Length != 3)
            {
                return;
            }

            var languageCode = s[0];
            var description = s[1];
            var lyrics = s[2];

            if (lyrics == "")
            {
                return;
            }

            var title = languageCode is "" or "XXX" ? "lyrics" : "lyrics (" + languageCode + ")";
            // var t = new Table { Title = new TableTitle(title, new Style(color)) }
            var t = new Table { Title = new TableTitle(Markup.Escape(title)) }
                .BorderColor(color)
                .HideHeaders()
                .AddColumn("text");
            if (description == "")
            {
                t.AddRow(Markup.Escape(description));
            }

            // new TableRow(lyrics);
            t.AddRow(Markup.Escape(lyrics));
            // t.Caption = new TableTitle(, new Style(color));
            _console.Write(t);
        });
    }

    private void DumpLongText(string name, string? longText, Color color)
    {
        Stringify(longText,
            s =>
            {
                var title = name + " (" + s.Length + " characters)";
                var t = new Table { Title = new TableTitle(Markup.Escape(title), new Style(color)) }
                    .BorderColor(color)
                    .HideHeaders()
                    .AddColumn("text");

                var borderSize = 2;
                var tableWidth = s.Length + borderSize;
                if (tableWidth < title.Length)
                {
                    t.Width = title.Length + borderSize;
                }

                t.AddRow(Markup.Escape(s));

                // t.Caption = new TableTitle(, new Style(color));
                _console.Write(t);
            }, "");
    }

    private void DumpPath(IMetadata metadata, Color color)
    {
        _console.Write(new Rule($"[red]{Markup.Escape(metadata.Path ?? "")}[/]")
        {
            Justification = Justify.Left,
            Style = new Style(color)
        });
        _console.WriteLine();
    }


    private void DumpFileProperties(IMetadata metadata, Color color)
    {
        if (metadata is MetadataTrack track)
        {
            var firstCol = new TableColumn("property")
            {
                Width = 17,
            }.RightAligned();
            var fileTable = new Table()
                .AddColumn(firstCol)
                .AddColumn("value")
                .HideHeaders()
                .BorderColor(color);
            // fileTable.Caption = new TableTitle("properties"); // is below table
            // fileTable.Expand = true; // full with
            var title = new TableTitle("properties", new Style(color));

            fileTable.Title = title;

            // fileTable.NoSafeBorder();
            // fileTable.NoSafeBorder();
            
            Stringify(track.AudioFormat, s => fileTable.AddRow("format", Markup.Escape(s)));
            Stringify(track.Bitrate, s => fileTable.AddRow("bitrate", Markup.Escape(s)));
            Stringify(track.SampleRate, s => fileTable.AddRow("sample-rate", Markup.Escape(s)));
            Stringify(track.TotalDuration, s => fileTable.AddRow("duration", Markup.Escape(s)));
            Stringify(track.IsVBR, s => fileTable.AddRow("vbr", Markup.Escape(s)));
            Stringify(track.ChannelsArrangement, s => fileTable.AddRow("channels", Markup.Escape(s)));
            Stringify(track.EmbeddedPictures.Count, s => fileTable.AddRow("embedded pictures", Markup.Escape(s)));

            var metadataFormats = string.Join(Environment.NewLine, track.MetadataFormats.Select(m => m.Name));
            var formatString = track.MetadataFormats.Count == 1 ? "format" : "formats";
            Stringify(track.MetadataFormats.Count, _ => fileTable.AddRow($"{track.MetadataFormats.Count} meta {formatString}", Markup.Escape(metadataFormats)));

            _console.Write(fileTable);
        }
    }

    private void DumpEmbeddedPictures(IMetadata metadata, Color color)
    {
        if (metadata.EmbeddedPictures.Count == 0)
        {
            return;
        }

        var firstCol = new TableColumn("position")
        {
            Width = 17,
        }.RightAligned();
        var pictures = new Table()
            .AddColumn(firstCol)
            .AddColumn("type")
            .AddColumn("description")
            .AddColumn("mimetype")
            .AddColumn("size")
            // .HideHeaders()
            .BorderColor(color);
        pictures.Title = new TableTitle("embedded pictures", new Style(color));
        foreach (var pic in metadata.EmbeddedPictures)
        {
            StringifyMulti(pic, values =>
            {
                if (values.Length == 5)
                {
                    pictures.AddRow(values.Select(Markup.Escape).ToArray());
                }
            });
        }

        _console.Write(pictures);
    }

    private void DumpMetadata(IMetadata metadata, Color color)
    {
        var firstCol = new TableColumn("property")
        {
            Width = 17,
        }.RightAligned();
        var properties = new Table()
            .AddColumn(firstCol)
            .AddColumn("value")
            .HideHeaders()
            .BorderColor(color);
        properties.Title = new TableTitle("metadata", new Style(color));

        Stringify(metadata.Genre, s => properties.AddRow("genre", Markup.Escape(s)));
        Stringify(metadata.Artist, s => properties.AddRow("artist", Markup.Escape(s)));
        Stringify(metadata.SortArtist, s => properties.AddRow("sort-artist", Markup.Escape(s)));
        Stringify(metadata.AlbumArtist, s => properties.AddRow("album-artist", Markup.Escape(s)));
        Stringify(metadata.SortAlbumArtist, s => properties.AddRow("sort-album-artist", Markup.Escape(s)));
        Stringify(metadata.OriginalArtist, s => properties.AddRow("original-artist", Markup.Escape(s)));
        Stringify(metadata.Narrator, s => properties.AddRow("narrator", Markup.Escape(s)));
        Stringify(metadata.Composer, s => properties.AddRow("composer", Markup.Escape(s)));
        Stringify(metadata.SortComposer, s => properties.AddRow("sort-composer", Markup.Escape(s)));
        Stringify(metadata.Publisher, s => properties.AddRow("publisher", Markup.Escape(s)));
        Stringify(metadata.Album, s => properties.AddRow("album", Markup.Escape(s)));
        Stringify(metadata.SortAlbum, s => properties.AddRow("sort-album", Markup.Escape(s)));
        Stringify(metadata.OriginalAlbum, s => properties.AddRow("original-album", Markup.Escape(s)));
        Stringify(metadata.Title, s => properties.AddRow("title", Markup.Escape(s)));
        Stringify(metadata.Subtitle, s => properties.AddRow("subtitle", Markup.Escape(s)));
        Stringify(metadata.SortTitle, s => properties.AddRow("sort-title", Markup.Escape(s)));
        Stringify(metadata.MovementName, s => properties.AddRow("movement-name", Markup.Escape(s)));
        Stringify(metadata.Part, s => properties.AddRow("part", Markup.Escape(s)));
        Stringify(metadata.Movement, s => properties.AddRow("movement", Markup.Escape(s)));
        Stringify(metadata.DiscNumber, s => properties.AddRow("disc-number", Markup.Escape(s)), 0);
        Stringify(metadata.DiscTotal, s => properties.AddRow("disc-total", Markup.Escape(s)), 0);
        Stringify(metadata.TrackNumber, s => properties.AddRow("track-number", Markup.Escape(s)), 0);
        Stringify(metadata.TrackTotal, s => properties.AddRow("track-total", Markup.Escape(s)), 0);
        Stringify(metadata.Copyright, s => properties.AddRow("copyright", Markup.Escape(s)));
        Stringify(metadata.PublishingDate, s => properties.AddRow("publishing-date", Markup.Escape(s)),
            DateTime.MinValue);
        Stringify(metadata.RecordingDate, s => properties.AddRow("recording-date", Markup.Escape(s)),
            DateTime.MinValue);
        Stringify(metadata.PurchaseDate, s => properties.AddRow("purchase-date", Markup.Escape(s)), DateTime.MinValue);
        Stringify(metadata.Group, s => properties.AddRow("group", Markup.Escape(s)));
        Stringify(metadata.EncodingTool, s => properties.AddRow("encoding-tool", Markup.Escape(s)));
        Stringify(metadata.EncodedBy, s => properties.AddRow("encoded-by", Markup.Escape(s)));
        Stringify(metadata.EncoderSettings, s => properties.AddRow("encoder-settings", Markup.Escape(s)));
        Stringify(metadata.ItunesMediaType, s => properties.AddRow("itunes-media-type", Markup.Escape(s)));
        Stringify(metadata.ItunesPlayGap, s => properties.AddRow("itunes-play-gap", Markup.Escape(s)));
        //Stringify(metadata.ItunesCompilation, s => properties.AddRow("itunes-compilation", Markup.Escape(s)));
        Stringify(metadata.Popularity, s => properties.AddRow("popularity", Markup.Escape(s)), 0);
        Stringify(metadata.Conductor, s => properties.AddRow("conductor", Markup.Escape(s)));
        Stringify(metadata.Bpm, s => properties.AddRow("bpm", Markup.Escape(s)), 0);
        _console.Write(properties);
    }

    private void DumpChapters(IMetadata metadata, Color color)
    {
        if (metadata.Chapters.Count == 0)
        {
            return;
        }

        var firstCol = new TableColumn("position")
        {
            Width = 17,
        }.RightAligned();
        var chapters = new Table()
            .AddColumn(firstCol)
            .AddColumn("name")
            //.AddColumn("subtitle")
            //.AddColumn("pictures")
            .HideHeaders()
            .BorderColor(color);
        chapters.Title = new TableTitle("chapters", new Style(color));
        foreach (var chap in metadata.Chapters)
        {
            StringifyMulti(chap, values =>
            {
                if (values.Length == 2)
                {
                    chapters.AddRow(values.Select(Markup.Escape).ToArray());
                }
            });
        }

        _console.Write(chapters);
    }


    private static void StringifyMulti<T>(T value, Action<string[]> act)
    {
        var result = value switch
        {
            PictureInfo p => new[] { p.Position.ToString(), p.PicType.ToString(), p.Description, p.MimeType, p.PictureData.Length + " bytes" },
            ChapterInfo c => new[]
            {
                Stringify(TimeSpan.FromMilliseconds(c.StartTime), null, TimeSpan.FromMilliseconds(uint.MaxValue)),
                c.Title, 
                //c.Subtitle,
                //c.Picture == null ? "" : c.Picture.ToString()
            },
            LyricsInfo l => new[]
            {
                l.LanguageCode,
                l.Description,
                l.SynchronizedLyrics.Count == 0
                    ? l.UnsynchronizedLyrics
                    : string.Join("\n",
                        l.SynchronizedLyrics.Select(phrase =>
                            Stringify(TimeSpan.FromMilliseconds(phrase.TimestampMs), null,
                                TimeSpan.FromMilliseconds(int.MaxValue)) + " - " + phrase.Text))
            },
            _ => Array.Empty<string>()
        };

        if (result.Length > 0)
        {
            act(result);
        }
    }

    private static string Stringify<T>(T value, Action<string>? act = null, T? ignoreValue = default)
    {
        if (EqualityComparer<T>.Default.Equals(value, ignoreValue))
        {
            return "";
        }

        var result = value switch
        {
            Format f => f.Name + ": " + string.Join(", ", f.MimeList.FirstOrDefault()),
            ChannelsArrangements.ChannelsArrangement ch => ch.NbChannels + " (" + ch.Description + ")",
            TimeSpan duration =>
                Math.Floor(duration.TotalHours).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0') +
                duration.ToString(@"\:mm\:ss\.fff"),
            DateTime dateTime => dateTime.ToString(CultureInfo.InvariantCulture).Replace(" 00:00:00", ""),
            Enum e => (int)(object)e + " (" + e + ")",
            _ => value?.ToString() ?? ""
        };

        if (result != "")
        {
            act?.Invoke(result);
        }

        return result;
    }
}