using DatabaseBranchSwitcher.Core;
using DatabaseBranchSwitcher.Core.Abstractions;
using DatabaseBranchSwitcher.Core.Options;
using DatabaseBranchSwitcher.Core.Services;
using DatabaseBranchSwitcherService;


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostBuilder, services) =>
    {
        services.Configure<BranchSwitcherOptions>(hostBuilder.Configuration);
        services.AddSingleton<ICacheFileManager, CacheFileManager>();
        services.AddSingleton<IBackupService, BackupService>();
        services.AddSingleton<IGitManager, GitManager>();
        services.AddHostedService<DatabaseBranchWorker>();
    })
    .Build();

host.Run();