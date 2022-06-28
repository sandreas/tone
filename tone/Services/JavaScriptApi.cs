using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ATL;
using Jint;
using Newtonsoft.Json;
using tone.Api.JavaScript;
using tone.Metadata.Taggers;

namespace tone.Services;

public class JavaScriptApi
{
    private readonly Engine _jint;
    private readonly TaggerComposite _tagger;
    private readonly IEnumerable<string> _customTaggerParameters = Array.Empty<string>();
    private readonly HttpClient _http;
    private readonly FileSystem _fs;

    public JavaScriptApi()
    {
        // fallback "dummy" constructor in case of missing prerequisites
        _jint = new Engine();
        _tagger = new TaggerComposite();
        _fs = new FileSystem();
        _http = new HttpClient();
        
    }
    public JavaScriptApi(Engine jint, FileSystem fs, HttpClient http, TaggerComposite tagger, IEnumerable<string> customTaggerParameters)
    {
        _jint = jint;
        _tagger = tagger;
        _customTaggerParameters = customTaggerParameters;
        _fs = fs;
        _http = http;
    }

    public void RegisterTagger(string name)
    {
        _tagger.Taggers.Add(new ScriptTagger(_jint, name, _customTaggerParameters));
    }
    
    public bool Download(string url, string destination, object? data=null){
        var fetchData = NormalizeFetchData(data);
        fetchData.DownloadPath = destination;
        var result = _fetch(url, fetchData, data);
        return result == destination;
    }
    public string Fetch(string url, object? data=null){
        var fetchData = NormalizeFetchData(data);
        return _fetch(url, fetchData, data);
    }
    
    private string _fetch(string url, FetchData fetchData, object? data=null)
    {
        var httpRequestMessage = BuildRequestMessage(url, data, fetchData);
        
        // this is a really bad async2sync hack 
        // jint does not provide async/await, so async2sync is the only option here
        var task = Task.Run(async () =>
        {
            var response = await _http.SendAsync(httpRequestMessage).ConfigureAwait(false);
            if(fetchData.DownloadPath == "") {
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            var destinationFile = _fs.FileInfo.FromFileName(fetchData.DownloadPath);
            if(destinationFile.Exists && !fetchData.Overwrite)
            {
                return "";
            }  
            
            if(!_fs.Directory.Exists(destinationFile.DirectoryName))
            {
                _fs.Directory.CreateDirectory(destinationFile.DirectoryName);
            }
            
            await _fs.File.WriteAllBytesAsync(destinationFile.FullName, await response.Content.ReadAsByteArrayAsync());
            
            return await Task.FromResult(fetchData.DownloadPath);
        });
        return task.Result;
    }

    
    private static FetchData NormalizeFetchData(object? data=null){
        if(data == null)
        {
            return new FetchData();
        }
        var dataSerialized = JsonConvert.SerializeObject(data);
        return JsonConvert.DeserializeObject<FetchData>(dataSerialized) ?? new FetchData();
    }
    
    private HttpRequestMessage BuildRequestMessage(string url, object? data, FetchData fetchData){
        var httpRequestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(url),
            
        };

        if(data != null){
            httpRequestMessage = new HttpRequestMessage()        {
                Method = ConvertStringToHttpMethod(fetchData.Method),
                RequestUri = new Uri(url),
                Content = new StringContent(fetchData.Body)
            };
        
            foreach(var (key, value) in fetchData.Headers)
            {
                var lowerKey = key.ToLower();
                if(lowerKey == "content-type")
                {
                    httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(value);
                    httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(value));
                    continue;
                }
                
                httpRequestMessage.Headers.Add(key, value);
            }
        }
        // httpRequestMessage.Headers.UserAgent.Add( new ProductInfoHeaderValue("Mozilla", "5.0"));
        return httpRequestMessage;
    }
    private static HttpMethod ConvertStringToHttpMethod(string fetchDataMethod) => fetchDataMethod.ToLower() switch
    {
        "delete" => HttpMethod.Delete,
        "head" => HttpMethod.Head,
        "options" => HttpMethod.Options,
        "patch" => HttpMethod.Patch,
        "post" => HttpMethod.Post,
        "put" => HttpMethod.Put,
        "trace" => HttpMethod.Trace,
        _ => HttpMethod.Get
    };

    public string ReadTextFile(string path)
    {
        return _fs.File.Exists(path) ? _fs.File.ReadAllText(path) : "";
    }
    
    public DateTime? CreateDateTime(string dateString)
    {
        return DateTime.TryParse(dateString, out var dateTime) ? dateTime : null;
    }
    
    public TimeSpan CreateDateTime(int milliseconds)
    {
        return TimeSpan.FromMilliseconds(milliseconds);
    }
    public ChapterInfo CreateChapter(string title, uint start, uint length, PictureInfo? picture=null, string subtitle="",string uniqueId="")    {
        return new ChapterInfo()
        {
            Title = title,
            StartTime = start,
            EndTime = start + length,
            UniqueID = uniqueId,
            Subtitle = subtitle,
            Picture = picture
        };
    }
    
    public PictureInfo CreatePicture(string path, PictureInfo.PIC_TYPE type=PictureInfo.PIC_TYPE.Generic)
    {
        var pic = PictureInfo.fromBinaryData(_fs.File.ReadAllBytes(path));
        pic.PicType = type;
        pic.ComputePicHash();
        return pic;
    }
}