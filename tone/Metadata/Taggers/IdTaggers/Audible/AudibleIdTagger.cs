using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OperationResult;
using Sandreas.AudioMetadata;
using static OperationResult.Helpers;

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
        !string.IsNullOrWhiteSpace(_settings.MetadataUrlTemplate) && Regex.IsMatch( id,"^[A-Z0-9]{10}$");


    public async Task<Status<string>> UpdateAsync(IMetadata metadata, IMetadata? originalMetadata = null)
    {
        if (!SupportsId(Id))
        {
            return Ok();
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(_settings.MetadataUrlTemplate))
            {
                var metaUrl = _settings.MetadataUrlTemplate.Replace("{AUDIBLE_ASIN}", Id);
                var metaResponse = await _http.GetAsync(metaUrl);
                var metaContent = await metaResponse.Content.ReadAsStringAsync();
                var meta = JsonConvert.DeserializeObject<AudibleMetadataResponse>(metaContent);
            }

            if (!string.IsNullOrWhiteSpace(_settings.ChaptersUrlTemplate))
            {
                var chapsUrl = _settings.ChaptersUrlTemplate.Replace("{AUDIBLE_ASIN}", Id);
                var chapsResponse = await _http.GetAsync(chapsUrl);
                var chapsContent = await chapsResponse.Content.ReadAsStringAsync();
                var chaps = JsonConvert.DeserializeObject<AudibleChaptersResponse>(chapsContent);
            }
        }
        catch (Exception e)
        {
            return Error(e.Message);
        }

        return Ok();
    }
}