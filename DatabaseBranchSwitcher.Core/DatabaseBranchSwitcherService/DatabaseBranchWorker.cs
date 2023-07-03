using DatabaseBranchSwitcher.Core;
using DatabaseBranchSwitcher.Core.Abstractions;
using DatabaseBranchSwitcher.Core.DataModels;
using DatabaseBranchSwitcher.Core.Options;
using DatabaseBranchSwitcher.Core.Services;
using Microsoft.Extensions.Options;

namespace DatabaseBranchSwitcherService;

public class DatabaseBranchWorker : BackgroundService
{
    private readonly ILogger<DatabaseBranchWorker> _logger;
    private readonly IOptions<BranchSwitcherOptions> _configuration;

    private readonly ICacheFileManager _cacheFileManager;
    private readonly IGitManager _gitManager;
    private readonly IBackupService _backupService;

    private const string DefaultBackupPath = "/var/backups/";

    public DatabaseBranchWorker(ILogger<DatabaseBranchWorker> logger, IOptions<BranchSwitcherOptions> configuration, IBackupService backupService, ICacheFileManager cacheFileManager, IGitManager gitManager)
    {
        _logger = logger;
        _configuration = configuration;
        _backupService = backupService;
        _cacheFileManager = cacheFileManager;
        _gitManager = gitManager;
    }
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Init(cancellationToken);
        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(10000, cancellationToken);

            foreach (var repository in _cacheFileManager.GetRepositoryConfigs())
            {
                _cacheFileManager.AddRepositoryConfigData(repository.GitFilePath, repository.BackupPath);
                
                var repo = _cacheFileManager.GetRepositoryConfig(repository.GitFilePath);
                if (repo is null)
                    continue;
                
                if (CheckBranchChanged(repository.GitFilePath, out var branchName))
                {
                    await InitiateBackup(cancellationToken, repository);
                    await InitiateRestore(repository.GitFilePath, branchName ?? "", cancellationToken);
                }
                
            }
        }
    }

    private async Task Init(CancellationToken cancellationToken)
    {
        var repositories = _cacheFileManager.GetRepositoryConfigs();

        foreach (var repoPath in _gitManager.RepoPaths)
        {
            _cacheFileManager.AddRepositoryConfigData(repoPath, DefaultBackupPath);
        }
    }

    private async Task InitiateBackup(CancellationToken cancellationToken, RepositoryConfig repo)
    {
        var currentBranch = _gitManager.GetCurrentBranchName(repo.GitFilePath);
        _logger.LogInformation($"{repo.GitFilePath} - Current branch: " + currentBranch);

        try
        {
            var backup = await _backupService.ExecuteBackupQuery(repo, currentBranch, cancellationToken);

            if (backup is not null)
            {
                _cacheFileManager.SetCurrentBranchForRepository(repo.GitFilePath, currentBranch);
                _cacheFileManager.AddBackup(repo.GitFilePath, currentBranch, backup);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred: {error}", e.Message);
        }
    }

    private async Task InitiateRestore(string repo, string branchName, CancellationToken cancellationToken)
    {
        var repository = _cacheFileManager.GetRepositoryConfig(repo);
        if (repository is not null)
        {
            if (repository.Branches.TryGetValue(branchName, out var branch))
            {
                var backup = branch.Backups.MaxBy(x => x.Date);
                if (backup is not null)
                {
                    await _backupService.ExecuteRestoreQuery(repository, backup, cancellationToken);
                }
            }
        }
    }

    private bool CheckBranchChanged(string repoPath, out string? branchName)
    {
        branchName = null;

        var currentBranch = _gitManager.GetCurrentBranchName(repoPath);

        if (currentBranch != _cacheFileManager.GetCurrentBranchForRepository(repoPath))
        {
            branchName = currentBranch;
            return true;
        }
        
        return false;
    }
}