namespace DatabaseBranchSwitcher.Core.DataModels;

[Serializable]
public class BranchBackupConfig
{
    public string BranchName { get; set; }
    
    public List<BranchBackup> Backups { get; set; }
}