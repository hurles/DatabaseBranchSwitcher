namespace DatabaseBranchSwitcher.Core.DataModels;

[Serializable]
public class RepositoryConfig
{
    public string CurrentBranch { get; set; }
    public string GitFilePath { get; set; }
    
    public string OriginBackupPath { get; set; }
    public string BackupPath { get; set; }

    public string ConnectionString { get; set; }
    public Dictionary<string, BranchBackupConfig> Branches { get; set; } = new();

}