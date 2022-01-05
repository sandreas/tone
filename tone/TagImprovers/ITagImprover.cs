using ATL;

namespace tone.TagImprovers;

public interface ITagImprover
{
    public Track Improve(Track track);
}