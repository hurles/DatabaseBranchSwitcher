using System.Collections.Concurrent;
using System.Diagnostics;
using DatabaseBranchSwitcher.Core.Abstractions;
using LibGit2Sharp;

namespace DatabaseBranchSwitcher.Core.Services;

public class GitManager : IGitManager
{
    public List<string> RepoPaths { get; init; } = new List<string>();
    public GitManager()
    {
        IndexAllRepositories();
    }
    
    public string GetCurrentBranchName(string repoPath)
    {
        using (var repo = new Repository(repoPath))
        {
            return repo.Head.FriendlyName;
        }
    }

    private void IndexAllRepositories()
    {
        var directoryPaths = Directory.GetDirectories("/");

        var repos = new ConcurrentBag<string>();
        
        Console.WriteLine("Repo search Started");
        var sw = Stopwatch.StartNew();
        
        foreach (var directory in directoryPaths)
        {
            var di = new DirectoryInfo(directory);
            ProcessDirectory(di, repos);
        }
        
        Console.WriteLine($"Repo search Complete in {sw.ElapsedMilliseconds} ms");

        foreach (var repo in repos)
        {
            RepoPaths.Add(repo);
            Console.WriteLine(repo);
        }

    }

    private void ProcessDirectory(DirectoryInfo directory, ConcurrentBag<string> repos)
    {
        Parallel.ForEach(directory.GetFiles("*",  new EnumerationOptions() { IgnoreInaccessible = true, AttributesToSkip = FileAttributes.System }), new ParallelOptions() { MaxDegreeOfParallelism = 128 },
            file => { ParseFilesInFolder(repos, file); });

        Parallel.ForEach(directory.GetDirectories("*", enumerationOptions: new EnumerationOptions() { IgnoreInaccessible = true, AttributesToSkip = FileAttributes.System }), new ParallelOptions() { MaxDegreeOfParallelism = 128 }, dir => { ProcessSubDirectories(repos, dir); });
    }

    private void ProcessSubDirectories(ConcurrentBag<string> repos, DirectoryInfo dir)
    {
        try
        {
            ProcessDirectory(dir, repos);
        }
        catch (UnauthorizedAccessException e)
        {
        }
        catch (DirectoryNotFoundException notfound)
        {
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void ParseFilesInFolder(ConcurrentBag<string> repos, FileInfo file)
    {
        try
        {
            ProcessFile(file, repos);
        }
        catch (UnauthorizedAccessException e)
        {
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void ProcessFile(FileInfo file, ConcurrentBag<string> repos)
    {
        if (file.Name == ".git")
        {
            repos.Add(file.FullName);
            Console.WriteLine($"Found repo at: {file.FullName}");
        }
    }
}