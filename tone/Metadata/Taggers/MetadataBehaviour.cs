using System;

namespace tone.Metadata.Taggers;

[Flags]
public enum MetadataBehaviour
{
    FillEmpty = 1 << 0,
    ReplaceMeta = 1 << 1,
    ReplacePictures = 1 << 2,
    ReplaceChapters = 1 << 3,
    
    ForceOverwrite = int.MaxValue
}