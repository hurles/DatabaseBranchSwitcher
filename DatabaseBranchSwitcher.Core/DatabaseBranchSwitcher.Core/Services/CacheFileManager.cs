using System.Text.Json;
using DatabaseBranchSwitcher.Core.Abstractions;
using DatabaseBranchSwitcher.Core.DataModels;

namespace DatabaseBranchSwitcher.Core.Services;

public class CacheFileManager : ICacheFileManager
{
    private BranchCache _brancheCache = new BranchCache();
    private readonly string _cacheFolderPath;
    private readonly string _branchFileName = "BrancheCache.json";
    private readonly string _branchFilePath;

    public CacheFileManager()
    {
        _cacheFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DatabaseBranchSwitcher");
        _branchFilePath = Path.Combine(_cacheFolderPath, _branchFileName);
        Load();
    }

    public void Load()
    {
        if (!File.Exists(_branchFilePath))
        {
            Directory.CreateDirectory(_cacheFolderPath);
            File.WriteAllText(_branchFilePath, JsonSerializer.Serialize(_brancheCache, new JsonSerializerOptions()
            {
                WriteIndented = true
            }));
        }

        try
        {
            _brancheCache = JsonSerializer.Deserialize<BranchCache>(File.ReadAllText(_branchFilePath)) ??
                            throw new JsonException("Could not deserialize config file");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void AddRepositoryConfigData(string repositoryPath, string backupFolderPath)
    {
        _brancheCache.Repositories.TryAdd(repositoryPath, new RepositoryConfig()
        {
            GitFilePath = repositoryPath,
            BackupPath = backupFolderPath,
        });
        
        SaveChanges();
    }
    
    public void SetCurrentBranchForRepository(string repository, string branchName)
    {
        if (_brancheCache.Repositories.TryGetValue(repository, out var repo))
        {
            if (!repo.Branches.TryGetValue(branchName, out var branch))
            {
                repo.Branches.Add(branchName, new BranchBackupConfig()
                {
                    BranchName = branchName,
                    Backups = new List<BranchBackup>()
                });
            }

            repo.CurrentBranch = branchName;
            
            SaveChanges();
        }
        
    }

    public void AddBackup(string repository, string branchName, BranchBackup branchBackup)
    {
        if (_brancheCache.Repositories.TryGetValue(repository, out var repo))
        {
            if (repo.Branches.TryGetValue(branchName, out var branch))
            {
                var existing = branch.Backups.FirstOrDefault(x => x.BackupFileName == branchBackup.BackupFileName);
                if (existing is not null)
                    existing.Date = branchBackup.Date;
                else
                    branch.Backups.Add(branchBackup);

                SaveChanges();
            }
        }    
    }

    public string? GetCurrentBranchForRepository(string repository)
    {
        if (_brancheCache.Repositories.TryGetValue(repository, out var repo))
        {
            return repo.CurrentBranch;
        }

        return null;
    }

    public RepositoryConfig? GetRepositoryConfig(string repository)
    {
        _brancheCache.Repositories.TryGetValue(repository, out var repositoryConfig);
        
        return repositoryConfig;
    }
    
    public IEnumerable<RepositoryConfig> GetRepositoryConfigs()
    {
        return _brancheCache.Repositories.Values.ToList();
    }

    public IEnumerable<string> GetRepositories()
    {
        return _brancheCache.Repositories.Keys;
    }

    public void SaveChanges()
    {
        try
        {
            Directory.CreateDirectory(_cacheFolderPath);
            File.WriteAllText(_branchFilePath, JsonSerializer.Serialize(_brancheCache, new JsonSerializerOptions()
            {
                WriteIndented = true
            }));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}