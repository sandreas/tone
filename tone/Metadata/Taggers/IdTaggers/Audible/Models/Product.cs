using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace tone.Metadata.Taggers.IdTaggers.Audible.Models;

public class Product
{
    public string Asin { get; set; } = "";
    
    // [JsonProperty("asset_details")]
    // public List<object> AssetDetails { get; set; } = new();!t
    public List<Person> Authors { get; set; } = new();
    
    // [JsonProperty("available_codecs")]
    // public List<object> AvailableCodecs { get; set; } = new();
    [JsonProperty("category_ladders")]
    public List<CategoryLadder> CategoryLadders { get; set; } = new();
    // [JsonProperty("content_delivery_type")]
    // public ContentDeliveryType ContentDeliveryType { get; set; } = new();
    // [JsonProperty("content_type")]
    // public ContentType ContentType { get; set; } = new();
    
    [JsonProperty("format_type")]
    public FormatType FormatType { get; set; }
    [JsonProperty("has_children")]
    public bool HasChildren { get; set; }
    [JsonProperty("is_adult_product")]
    public bool IsAdultProduct { get; set; }
    [JsonProperty("is_listenable")]
    public bool IsListenable { get; set; }
    [JsonProperty("is_purchasability_suppressed")]
    public bool IsPurchasabilitySuppressed { get; set; }
    [JsonProperty("issue_date")]
    public DateOnly IssueDate { get; set; } = DateOnly.MinValue;
    public string Language { get; set; } = "";
    [JsonProperty("merchandising_summary")]
    public string MerchandisingSummary { get; set; } = "";
    public List<Person> Narrators { get; set; } = new();
    // public List<object> Plans { get; set; } = new();
    [JsonProperty("product_images")]
    public Dictionary<string, string> ProductImages { get; set; } = new();
    [JsonProperty("publication_datetime")]
    public DateTime PublicationDatetime { get; set; } = DateTime.MinValue;
    [JsonProperty("publication_name")]
    public string PublicationName { get; set; } = "";
    [JsonProperty("publisher_name")]
    public string PublisherName { get; set; } = "";
    [JsonProperty("publisher_summary")]
    public string PublisherSummary { get; set; } = "";
    // public object Rating { get; set; } = new();
    [JsonProperty("release_date")]
    public DateOnly ReleaseDate { get; set; } = DateOnly.MinValue;
    
    [JsonProperty("runtime_length_min")]
    public int RuntimeLengthMin { get; set; }

    [JsonProperty("sample_url")] 
    public string SampleUrl { get; set; } = "";
    
    public List<Series> Series { get; set; } = new();

    public string Sku { get; set; } = "";
    [JsonProperty("sku_lite")]
    public string SkuLite { get; set; } = "";
    
    [JsonProperty("social_media_images")]
    public Dictionary<string, string> SocialMediaImages { get; set; } = new();
    public string Subtitle { get; set; } = "";
    
    [JsonProperty("thesaurus_subject_keywords")]
    public List<string> ThesaurusSubjectKeywords { get; set; } = new();

    public string Title { get; set; } = "";

    [JsonProperty("voice_description")]
    public string VoiceDescription { get; set; } = "";
    


}