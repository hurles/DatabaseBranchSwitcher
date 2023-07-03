namespace DatabaseBranchSwitcher.Core.DataModels;

[Serializable]
public class BranchCache
{
    public Dictionary<string, RepositoryConfig> Repositories { get; set; } = new();
}