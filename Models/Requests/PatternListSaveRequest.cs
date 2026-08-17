namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.Requests;

public class PatternListSaveRequest
{
    public IEnumerable<int> PatternIds { get; set; } = Enumerable.Empty<int>();
}