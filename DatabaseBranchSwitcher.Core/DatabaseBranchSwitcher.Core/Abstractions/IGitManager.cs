namespace DatabaseBranchSwitcher.Core.Abstractions;

public interface IGitManager
{
    public List<string> RepoPaths { get; init; }

    public string GetCurrentBranchName(string repoPath);
}