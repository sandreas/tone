using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ATL;
using HtmlAgilityPack;
using Newtonsoft.Json;
using OperationResult;
using Sandreas.AudioMetadata;
using tone.Metadata.Taggers.IdTaggers.Audible.Models;
using static OperationResult.Helpers;
using ChapterInfo = tone.Metadata.Taggers.IdTaggers.Audible.Models.ChapterInfo;

namespace tone.Metadata.Taggers.IdTaggers.Audible;

public class AudibleIdTagger : IIdTagger
{
    private readonly HttpClient _http;
    private readonly AudibleIdTaggerSettings _settings;
    public string Id { get; set; } = "";

    public AudibleIdTagger(AudibleIdTaggerSettings settings, HttpClient http)
    {
        _settings = settings;
        _http = http;
    }


    public bool SupportsId(string id) =>
        !string.IsNullOrWhiteSpace(_settings.MetadataUrlTemplate) && Regex.IsMatch(id, "^[A-Z0-9]{10}$");


    public async Task<Status<string>> UpdateAsync(IMetadata metadata, IMetadata? originalMetadata = null)
    {
        if (!SupportsId(Id))
        {
            return Ok();
        }

        try
        {

            Product? product = null;
            ChapterInfo? chapters = null;
            if (!string.IsNullOrWhiteSpace(_settings.MetadataUrlTemplate))
            {
                var jsonContent = await LoadUriContents(_settings.MetadataUrlTemplate);

                var audibleResponse = JsonConvert.DeserializeObject<AudibleMetadataResponse>(jsonContent);

                product = audibleResponse?.Product;
                
            }

            if (!string.IsNullOrWhiteSpace(_settings.ChaptersUrlTemplate))
            {
                var chapsUrl = _settings.ChaptersUrlTemplate.Replace("{AUDIBLE_ASIN}", Id);
                var chapsContent = await LoadUriContents(chapsUrl);
                var chaps = JsonConvert.DeserializeObject<AudibleChaptersResponse>(chapsContent);
                chapters = chaps?.CategoryLadders.ChapterInfo;
            }
            var newMeta = await TransferMetaAsync(product, chapters);

            if (_settings.Behaviour.HasFlag(MetadataBehaviour.ForceOverwrite))
            {
                metadata.OverwriteProperties(newMeta);
            } else if(_settings.Behaviour.HasFlag(MetadataBehaviour.ReplaceMeta)) {
                metadata.OverwritePropertiesWhenNotEmpty(newMeta);
            } else if(_settings.Behaviour.HasFlag(MetadataBehaviour.FillEmpty)) {
                metadata.MergeProperties(newMeta);
            } 
            
            if(_settings.Behaviour.HasFlag(MetadataBehaviour.ReplaceChapters))
            {
                metadata.Chapters = newMeta.Chapters;
            }
            
            if(_settings.Behaviour.HasFlag(MetadataBehaviour.ReplacePictures))
            {
                metadata.EmbeddedPictures.Clear();
                foreach (var pic in newMeta.EmbeddedPictures)
                {
                    metadata.EmbeddedPictures.Add(pic);
                }
            }

        }
        catch (Exception e)
        {
            return Error(e.Message);
        }

        return Ok();
    }

    private async Task<IMetadata> TransferMetaAsync(Product? product, ChapterInfo? chapters)
    {
        IMetadata newMeta = new MetadataTrack();

        if (product != null)
        {

            newMeta.Album = product.Title;
            // newMeta.AlbumArtist = JoinPersonNames(product.Authors);
            newMeta.Artist = JoinPersonNames(product.Authors);
            // newMeta.Bpm = 0;
            // newMeta.ChaptersTableDescription = "";
            newMeta.Composer = JoinPersonNames(product.Narrators);
            newMeta.Comment = StripTags(product.PublisherSummary);
            // newMeta.Conductor = "";
            // newMeta.Copyright = "";
            newMeta.Description = newMeta.Comment;
            // newMeta.DiscNumber = 0;
            // newMeta.DiscTotal = 0;
            // newMeta.EncodedBy = "";
            // newMeta.EncoderSettings = "";
            // newMeta.EncodingTool = "";
            var genre = product.CategoryLadders?.FirstOrDefault()?.Ladder.FirstOrDefault()?.Name;
            if (genre == null)
            {
                newMeta.Genre = genre;
            }
            // newMeta.Group = ""
            // newMeta.ItunesCompilation = ""
            // newMeta.ItunesMediaType = ""
            // newMeta.ItunesPlayGap = ""
            newMeta.LongDescription = newMeta.Description;
            // newMeta.Lyrics = null;
            var series = product.Series.FirstOrDefault();
            if (series != null)
            {
                newMeta.MovementName = series.Title;
                newMeta.Part = series.Sequence;
            }
            newMeta.Narrator = newMeta.Composer;
            // OriginalAlbum { get; set; }
            // OriginalArtist { get; set; }
            // Popularity { get; set; }
            newMeta.Publisher = product.PublisherName;
            newMeta.PublishingDate = product.PublicationDatetime;
            // PurchaseDate { get; set; }
            // newMeta.RecordingDate = product.IssueDate
            // SortTitle { get; set; }
            // SortAlbum { get; set; }
            // SortArtist { get; set; }
            // SortAlbumArtist { get; set; }
            // SortComposer { get; set; }
            newMeta.Subtitle = product.Subtitle;
            newMeta.Title = product.Title;
            // TrackNumber { get; set; }
            // TrackTotal { get; set; }
            newMeta.AdditionalFields["----:com.pilabor.tone:AUDIBLE_ASIN"] = product.Asin;

            /*
            if (product.ProductImages.Count > 0)
            {
                var image = await LoadCoverAsync(product.ProductImages.FirstOrDefault().Value);
                if (image != null)
                {                
                    newMeta.EmbeddedPictures.Add(image);
                }
            }
            */
        }

        TransferChapters(newMeta, chapters);

        return newMeta;
    }

    private void TransferChapters(IMetadata newMeta, ChapterInfo? chapters)
    {
        if (chapters == null)
        {
            return;
        }

        if (chapters.Chapters.Count == 0)
        {
            return;
        }

        newMeta.Chapters = ChaptersToChapterInfo(chapters.Chapters);
    }

    private List<ATL.ChapterInfo> ChaptersToChapterInfo(List<Chapter> chapters)
    {
        var chapterInfos = new List<ATL.ChapterInfo>();
        foreach (var chapter in chapters)
        {

            var startMs = (uint)chapter.StartOffsetMs;
            var lengthMs = (uint)chapter.LengthMs;
            var endMs = startMs + lengthMs;
            var atlChapter = new ATL.ChapterInfo()
            {
                StartTime = startMs,
                EndTime = endMs,
                Title = chapter.Title
            };
            chapterInfos.Add(atlChapter);

            if (chapter.Chapters.Count <= 0)
            {
                continue;
            }
            // handle subchapters
            var subChapters = ChaptersToChapterInfo(chapter.Chapters);
            var firstSubChapter = subChapters.ElementAt(0);
            if (firstSubChapter.StartTime == startMs)
            {
                subChapters.RemoveAt(0);
                if (!string.IsNullOrWhiteSpace(firstSubChapter.Title))
                {
                    atlChapter.Title += ": " + firstSubChapter.Title;
                }
            }

            chapterInfos.AddRange(subChapters);
        }

        return chapterInfos;
    }

    private async Task<PictureInfo?> LoadCoverAsync(string? imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return null;
            }
            var bytes = await _http.GetByteArrayAsync(imageUrl);
            if (bytes.Length > 0)
            {
                return PictureInfo.fromBinaryData(bytes);
            }
        }
        catch (Exception e)
        {
            return null;
        }
        return null;
    }
    
    private string StripTags(string input)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(input);
        return htmlDoc.DocumentNode.InnerText;
    }

    private string? JoinPersonNames(IEnumerable<Person> persons)
    {
        
        var authorNames = persons.Select(a => a.Name).ToList();
        if (authorNames.Count == 0)
        {
            return null;
        }
        return string.Join(", ", authorNames);
    }

    private async Task<string> LoadUriContents(string uriTemplate)
    {
        try
        {
            var uri = new Uri(uriTemplate.Replace("{AUDIBLE_ASIN}", Id));
            if (uri.IsFile)
            {
                return await File.ReadAllTextAsync(uri.AbsolutePath);
            }

            var chapsResponse = await _http.GetAsync(uri);
            return await chapsResponse.Content.ReadAsStringAsync();
        }
        catch (Exception)
        {
            return "";
        }
    }
}