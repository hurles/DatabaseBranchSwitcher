using DatabaseBranchSwitcher.Core.DataModels;

namespace DatabaseBranchSwitcher.Core.Abstractions;

public interface ICacheFileManager
{
    void Load();
    
    void SaveChanges();
    
    void AddRepositoryConfigData(string repositoryPath, string backupFolderPath);
    
    void SetCurrentBranchForRepository(string repository, string branchName);
    
    void AddBackup(string repository, string branchName, BranchBackup branchBackup);

    string? GetCurrentBranchForRepository(string repository);

    RepositoryConfig? GetRepositoryConfig(string repository);

    IEnumerable<RepositoryConfig> GetRepositoryConfigs();
    
    IEnumerable<string> GetRepositories();

}