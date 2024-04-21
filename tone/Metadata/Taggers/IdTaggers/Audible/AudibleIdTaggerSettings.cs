namespace tone.Metadata.Taggers.IdTaggers.Audible;

public class AudibleIdTaggerSettings
{
    public string MetadataUrlTemplate { get; set; } = ""; // todo: Environment?!
    public string ChaptersUrlTemplate { get; set; } = "";

    public MetadataBehaviour Behaviour = MetadataBehaviour.FillEmpty;
}