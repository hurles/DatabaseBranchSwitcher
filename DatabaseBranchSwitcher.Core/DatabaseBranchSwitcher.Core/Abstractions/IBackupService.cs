using DatabaseBranchSwitcher.Core.DataModels;

namespace DatabaseBranchSwitcher.Core.Abstractions;

public interface IBackupService
{
    Task<BranchBackup?> ExecuteBackupQuery(RepositoryConfig repository, string backupName, CancellationToken cancellationToken);
    
    Task<bool> ExecuteRestoreQuery(RepositoryConfig repository, BranchBackup branchBackup, CancellationToken cancellationToken);
}