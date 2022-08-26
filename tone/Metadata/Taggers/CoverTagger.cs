using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using ATL;
using OperationResult;
using Sandreas.AudioMetadata;
using tone.Commands.Settings.Interfaces;
using tone.Metadata.Extensions;

namespace tone.Metadata.Taggers;
using static Helpers;

public class CoverTagger: INamedTagger
{
    public string Name => nameof(CoverTagger);

    private readonly List<IFileInfo> _covers;
    private readonly bool _autoload;
    private readonly IFileSystem _fs;
    private static readonly string[] AllowedImageExtensions = {"jpg", "png" };

    // todo
    public CoverTagger(IFileSystem fs, IEnumerable<string> covers, bool autoload=false)
    {
        _fs = fs;
        _covers = covers.Select(f => fs.FileInfo.FromFileName(f)).ToList();
        _autoload = autoload;
    }
    
    public CoverTagger(IFileSystem fs, ICoverTaggerSettings settings)    {
        _fs = fs;
        _covers = settings.Covers.Select(f => fs.FileInfo.FromFileName(f)).ToList();
        _autoload = settings.AutoImportCovers;
    }
    public async Task<Status<string>> UpdateAsync(IMetadata metadata, IMetadata? originalMetadata = null)
    {
        if(_autoload && _covers.Count == 0 && metadata.BasePath!=null)
        {
            var dir = _fs.GetContainingDirectory(metadata.BasePath);
            if(dir.Exists){
                // autoload covers
                // (<audiofilename>.)cover[0-100].(jpg|png)
                // 1 - MyTitle.cover.jpg
                // 1 - MyTitle.cover.png
                // 1 - MyTitle.cover[0].jpg
                _covers.AddRange( _fs.Directory
                    .GetFiles(dir.FullName)
                    .Select(p => _fs.FileInfo.FromFileName(p))
                    .Where(HasCoverExtension));
            }
        }
        
        
        var potentialCovers = _covers.Where(HasCoverExtension).ToImmutableArray();

        var validGroups = potentialCovers.ToLookup(IsValidCover);
        var validCovers = validGroups[true].ToImmutableArray();
        var invalidCovers = validGroups[false].ToImmutableArray();

        if (validCovers.Length == 0)
        {
            return Ok();
        }
        
        // remove existing covers before import
        metadata.EmbeddedPictures.Clear();
        
        var prefixFile = metadata.Path == null ? null : _fs.FileInfo.FromFileName(metadata.Path);
        var fileNamePrefix = prefixFile == null ? "" : prefixFile.Name.TrimSuffix(prefixFile.Extension.TrimStart('.'));
        var prefixedCoverGroups = validCovers.ToLookup(c => c.Name.StartsWith(fileNamePrefix));
        var prefixedCovers = prefixedCoverGroups[true].ToImmutableArray();
        var unprefixedCovers = prefixedCoverGroups[false].ToImmutableArray();
        
        // prefixed covers are always preferred over non-prefixed ones
        // cover in filename is the preferred generic type
        var prefixedPreferredCovers = prefixedCovers.Where(f => f.Name.Contains(".cover."));
        var unprefixedPreferredCovers = prefixedCovers.Where(f => f.Name.Contains("cover."));
        
        var embeddedCoverTypes = new List<PictureInfo.PIC_TYPE>();
        await EmbedCovers(metadata, prefixedPreferredCovers, embeddedCoverTypes);
        await EmbedCovers(metadata, prefixedCovers, embeddedCoverTypes);
        await EmbedCovers(metadata, unprefixedPreferredCovers, embeddedCoverTypes);
        await EmbedCovers(metadata, unprefixedCovers, embeddedCoverTypes);

        if(invalidCovers.Length > 0)
        {
            return Error($"{embeddedCoverTypes.Count} covers worked, {invalidCovers.Length} covers did not exist or where invalid: " + string.Join(",", invalidCovers.Select(f => f.FullName)));
        }

        return Ok();
    }

    private async Task EmbedCovers(IMetadata metadata, IEnumerable<IFileInfo> covers,
        ICollection<PictureInfo.PIC_TYPE> embeddedCoverTypes)
    {
        var chapterMaxIndex = metadata.Chapters.Count - 1;
        foreach (var cover in covers)
        {
            if (IsChapterCover(cover))
            {
                var chapterIndex = ParseChapterIndex(cover, chapterMaxIndex);
                if (chapterIndex != -1 && metadata.Chapters[chapterIndex].Picture == null)
                {
                    metadata.Chapters[chapterIndex].Picture = await LoadPictureInfoAsync(cover, PictureInfo.PIC_TYPE.Generic);
                }
                continue;
            }

            var picInfo = await LoadPictureInfoAsync(cover);
            if (embeddedCoverTypes.Contains(picInfo.PicType) || metadata.EmbeddedPictures.Any(p => p.PicType == picInfo.PicType))
            {
                continue;
            }

            metadata.EmbeddedPictures.Add(picInfo);
            embeddedCoverTypes.Add(picInfo.PicType);
        }
    }

    private static int ParseChapterIndex(IFileSystemInfo chapterCover, int maxIndex)
    {
        var name = chapterCover.Name;
        var markerIndex = name.IndexOf(".chapter", StringComparison.Ordinal);
        if(markerIndex == -1)
        {
            return -1;
        }
        var indexAsString = name[markerIndex..];
        var markerStartIndex = indexAsString.IndexOf("[", StringComparison.Ordinal);
        if(markerStartIndex == -1)
        {
            return -1;
        }

        markerStartIndex++;
        indexAsString = indexAsString[markerStartIndex..];

        var markerEndIndex = indexAsString.IndexOf("]", StringComparison.Ordinal);
        if(markerEndIndex == -1)
        {
            return -1;
        }
        indexAsString = indexAsString[..markerEndIndex];
        
        if(int.TryParse(indexAsString, out var index) && index > -1 && index <= maxIndex)
        {
            return index;
        }

        return -1;
    }

    private async Task<PictureInfo> LoadPictureInfoAsync(IFileSystemInfo cover, PictureInfo.PIC_TYPE? picType=null)        {
        var picInfo = PictureInfo.fromBinaryData(await _fs.File.ReadAllBytesAsync(cover.FullName));
        picInfo.PicType = picType ?? SwitchPicType(cover);
        picInfo.Description = picType.ToString();
        picInfo.ComputePicHash();
        return picInfo;
    }
    
    private static PictureInfo.PIC_TYPE SwitchPicType(IFileSystemInfo cover) => cover.Name.ToLower() switch
    {
        { } name when name.Contains("front.") => PictureInfo.PIC_TYPE.Front,
        { } name when name.Contains("back.") => PictureInfo.PIC_TYPE.Back,
        { } name when name.Contains("cd.") => PictureInfo.PIC_TYPE.CD,
        { } name when name.Contains("icon.") => PictureInfo.PIC_TYPE.Icon,
        { } name when name.Contains("leaflet.") => PictureInfo.PIC_TYPE.Leaflet,
        { } name when name.Contains("leadartist.") => PictureInfo.PIC_TYPE.LeadArtist,
        { } name when name.Contains("artist.") => PictureInfo.PIC_TYPE.Artist,
        { } name when name.Contains("conductor.") => PictureInfo.PIC_TYPE.Conductor,
        { } name when name.Contains("band.") => PictureInfo.PIC_TYPE.Band,
        { } name when name.Contains("composer.") => PictureInfo.PIC_TYPE.Composer,
        { } name when name.Contains("lyricist.") => PictureInfo.PIC_TYPE.Lyricist,
        { } name when name.Contains("recordinglocation.") => PictureInfo.PIC_TYPE.RecordingLocation,
        { } name when name.Contains("duringrecording.") => PictureInfo.PIC_TYPE.DuringRecording,
        { } name when name.Contains("duringperformance.") => PictureInfo.PIC_TYPE.DuringPerformance,
        { } name when name.Contains("moviecapture.") => PictureInfo.PIC_TYPE.MovieCapture,
        { } name when name.Contains("fishie.") => PictureInfo.PIC_TYPE.Fishie,
        { } name when name.Contains("illustration.") => PictureInfo.PIC_TYPE.Illustration,
        { } name when name.Contains("bandlogo.") => PictureInfo.PIC_TYPE.BandLogo,
        { } name when name.Contains("publisherlogo.") => PictureInfo.PIC_TYPE.PublisherLogo,
        _ => PictureInfo.PIC_TYPE.Generic
    };

    private static bool IsChapterCover(IFileSystemInfo cover)
    {
        var lowerName = cover.Name.ToLower();
        return lowerName.Contains(".chapter") && lowerName.Contains('[') && lowerName.Contains(']');
    }

    private static bool HasCoverExtension(IFileSystemInfo f)
    {
        return AllowedImageExtensions.Contains(f.Extension.TrimStart('.').ToLower());
    }
    
    private static bool IsValidCover(IFileInfo f)
    {
        return f.Exists && f is { Length: > 0 };
    }
}
