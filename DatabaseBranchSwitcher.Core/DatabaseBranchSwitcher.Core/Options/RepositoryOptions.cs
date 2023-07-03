namespace DatabaseBranchSwitcher.Core.Options;

public class RepositoryOptions
{
 public string ConnectionString { get; set; }  =
  "Server=localhost,1433;Database=goportaal;multipleactiveresultsets=True;application name=EntityFramework;User Id=sa;Password=1TBPXJLUk1hr8nuzwIIa3GsYfJqw1s46bX6NBqaF;";

 public string BackupPath { get; set; } =  "/var/backups/";
 public string OriginBackupPath { get; set; } = "/var/backups/origin";
 public string GitFilePath { get; set; } =  "/var/backups/";

}