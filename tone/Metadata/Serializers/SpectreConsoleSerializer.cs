using System.Threading.Tasks;
using Spectre.Console;

namespace tone.Metadata.Serializers;

public class SpectreConsoleSerializer: IMetadataSerializer
{
    private readonly IAnsiConsole _console;

    public SpectreConsoleSerializer(IAnsiConsole console)
    {
        _console = console;
    }
    
    public async Task<string> SerializeAsync(IMetadata metadata)
    {
        var tree = new Tree(metadata.Path ?? "metadata");
        if (metadata is MetadataTrack track)
        {
            var fileTable = new Table()
                .AddColumn(new TableColumn("property").RightAligned())
                .AddColumn("value")
                .HideHeaders()
                .BorderColor(Color.Aquamarine1);
            // fileTable.Caption = new TableTitle("properties"); // is below table
            // fileTable.Expand = true; // full with

            fileTable.AddRow("format", track.AudioFormat?.ToString()??"unknown");
            tree.AddNode(fileTable);
        }
        
        // var meta = tree.AddNode("[yellow]metadata[/]");
        
        
        
        
        _console.Write(tree);
        return await Task.FromResult("");
    }
}