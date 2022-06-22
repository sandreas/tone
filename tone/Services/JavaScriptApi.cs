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
    
    public string Fetch(string url, object? data=null)
    {
        var httpRequestMessage = new HttpRequestMessage()        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(url)
        };
        
        if(data != null){
            var dataSerialized = JsonConvert.SerializeObject(data);
            var fetchData = JsonConvert.DeserializeObject<FetchData>(dataSerialized);
            httpRequestMessage = new HttpRequestMessage()        {
                Method = ConvertStringToHttpMethod(fetchData.Method),
                RequestUri = new Uri(url),
                Content = new StringContent(fetchData.Body)
            };
        
            foreach(var (key, value) in fetchData.Headers){
                if(key.ToLower() == "content-type")
                {
                    httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(value);
                    httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(value));
                    continue;
                }
                
                httpRequestMessage.Headers.Add(key, value);
            }
        }
        
        // this is a really bad async2sync hack 
        // jint does not provide async/await, so async2sync is the only option here
        var task = Task.Run(async () =>
        {
            var response = await _http.SendAsync(httpRequestMessage).ConfigureAwait(false);
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        });
        return task.Result;
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