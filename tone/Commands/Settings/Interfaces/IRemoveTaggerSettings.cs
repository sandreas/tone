using tone.Metadata;

namespace tone.Commands.Settings.Interfaces;

public interface IRemoveTaggerSettings
{
    public MetadataProperty[] Remove { get; }
    public string[] RemoveAdditionalFields { get; }
}