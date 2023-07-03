using DatabaseBranchSwitcher.Core.Abstractions;
using DatabaseBranchSwitcher.Core.DataModels;
using DatabaseBranchSwitcher.Core.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace DatabaseBranchSwitcher.Core.Services;

public class BackupService : IBackupService
{
    private readonly IOptions<BranchSwitcherOptions> _options;
    private readonly ICacheFileManager _cacheFileManager;

    public const string DefaultBackupPath = "/var/backups/";

    public BackupService(IOptions<BranchSwitcherOptions> options, ICacheFileManager cacheFileManager)
    {
        _options = options;
        _cacheFileManager = cacheFileManager;
    }

    public async Task<BranchBackup?> ExecuteBackupQuery(RepositoryConfig repository, string backupName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(repository.ConnectionString))
        {
            Console.WriteLine($"Skipping backup check - ConnectionString for {repository.GitFilePath} is empty");
            return null;
        }
        
        
        var sqlConnectionString = repository.ConnectionString;
       if (!sqlConnectionString.Contains("TrustServerCertificate=True;"))
           sqlConnectionString += "TrustServerCertificate=True;";
       
       await using SqlConnection connection = new SqlConnection(sqlConnectionString);

       var dbName = connection.Database;
       
       backupName = string.Join("_", backupName.Split(Path.GetInvalidFileNameChars()));    
       backupName = string.Join("_", backupName.Split(Path.GetInvalidPathChars()));

       var path = Path.Combine(!string.IsNullOrWhiteSpace(repository.BackupPath) ? repository.BackupPath : DefaultBackupPath, backupName);
       
       var backupQuery = @$"BACKUP DATABASE {dbName} TO DISK = '{path}.bak';";
       var command = new SqlCommand(backupQuery, connection);
           
       try
       {
           Console.WriteLine($"Executing command: {backupQuery}");

           await connection.OpenAsync(cancellationToken);
           var rowCount = await command.ExecuteNonQueryAsync(cancellationToken);
           Console.WriteLine($"{rowCount} Rows affected");

           return new BranchBackup()
           {
               BackupFileName = path,
               Date = DateTimeOffset.Now
           };

       }
       catch (Exception ex)
       {
           Console.WriteLine(ex.Message);
       }

       return null;
    }

    public async Task<bool> ExecuteRestoreQuery(RepositoryConfig repository, BranchBackup branchBackup, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(repository.ConnectionString))
        {
            Console.WriteLine($"Skipping restore - ConnectionString for {repository.GitFilePath} is empty");
            return false;
        }
        
        
        var sqlConnectionString = repository.ConnectionString;
        if (!sqlConnectionString.Contains("TrustServerCertificate=True;"))
            sqlConnectionString += "TrustServerCertificate=True;";
       
        await using SqlConnection connection = new SqlConnection(sqlConnectionString);

        var dbName = connection.Database;
        
        var backupQuery = @$"
USE master;
ALTER DATABASE {dbName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE {dbName} FROM DISK ='{branchBackup.BackupFileName}.bak' WITH REPLACE;
ALTER DATABASE {dbName} SET MULTI_USER;";
        
        var command = new SqlCommand(backupQuery, connection);
           
        try
        {
            Console.WriteLine($"Executing command: {backupQuery}");

            await connection.OpenAsync(cancellationToken);
            var rowCount = await command.ExecuteNonQueryAsync(cancellationToken);
            Console.WriteLine($"{rowCount} Rows affected");

            return true;

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return false;
    }
}