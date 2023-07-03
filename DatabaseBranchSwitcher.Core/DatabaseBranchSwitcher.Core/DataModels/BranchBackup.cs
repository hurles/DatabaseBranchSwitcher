namespace DatabaseBranchSwitcher.Core.DataModels;

[Serializable]
public class BranchBackup
{
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;

    public string BackupFileName { get; set; }
}