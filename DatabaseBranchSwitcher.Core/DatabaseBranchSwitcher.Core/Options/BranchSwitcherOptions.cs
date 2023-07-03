namespace DatabaseBranchSwitcher.Core.Options;

public class BranchSwitcherOptions
{
    public List<RepositoryOptions> Repositories { get; set; } = new();
}