using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Alamo.CLI;
using Microsoft.Extensions.Logging;

namespace MegCreatorCLI
{
    internal sealed class Packer : IPacker
    {
        private readonly ILogger? _logger;
        public PackOptions Options { get; }

        private MegaFile MegFile { get; }

        public Packer(PackOptions options, ILogger? logger = null)
        {
            _logger = logger;
            Options = options ?? throw new ArgumentNullException(nameof(options));
            MegFile = new MegaFile();
        }

        public void Pack()
        {
            var realWorkingDirectory = Options.WorkingDirectory is null
                ? Directory.GetCurrentDirectory()
                : GetAbsolutePath(Options.WorkingDirectory);
            
            var files = CollectFiles(Options.InputLocations, realWorkingDirectory).Distinct();

            _logger.LogInformation("Inserting Files...");
            InsertFiles(files);
            _logger.LogInformation("Finished inserting Files.");
            Save(MegFile, GetAbsolutePath(Options.Output, realWorkingDirectory));
            _logger?.LogInformation($"Saved meg to {Options.Output}.");
        }

        public void Dispose()
        {
            MegFile?.Dispose();
        }

        private static void Save(MegaFile meg, string outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            using var stream = File.Open(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            meg.Close(stream, MegaFile.Format.V1, new MegaFile.EncryptionKey?());
        }

        private IEnumerable<MegFileEntry> CollectFiles(IEnumerable<string> inputs, string workingDir)
        {

            var dirs = new HashSet<string>();


            foreach (var path in inputs)
            {

                var absolutePath = GetAbsolutePath(path, out var wasAbsolute, workingDir);

                if (!IsDirectory(absolutePath))
                {
                    var fileName = wasAbsolute ? absolutePath : absolutePath.Substring(workingDir.Length + 1);
                    yield return new MegFileEntry(new FileInfo(absolutePath), fileName);
                }
                else
                {
                    var dirFiles = GetFilesFromDirectory(absolutePath, Options.IncludeSubDirectories, dirs,
                        filePath => GetEntryName(filePath, workingDir, wasAbsolute));
                    foreach (var fileInfo in dirFiles)
                    {
                        yield return fileInfo;
                    }
                }
            }
        }



        private string GetEntryName(string fullPath, string workingDir, bool wasAbsolute)
        {
            if (wasAbsolute)
                return fullPath;
            _logger?.LogTrace("Making meg-entry name a relative path");
            return fullPath.Substring(workingDir.Length + 1);
        }


        private void InsertFiles(IEnumerable<MegFileEntry> entries)
        {
            foreach (var entry in entries)
            {
                var fs = File.Open(entry.File.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                MegFile.InsertFile(entry.Name, fs);
            }
        }

        private IEnumerable<MegFileEntry> GetFilesFromDirectory(string directory, bool includeSubs,
            ICollection<string> visitedDirs, Func<string, string> createFileName)
        {
            var normalized = Path.GetFullPath(directory);
            if (visitedDirs.Contains(normalized))
                return Enumerable.Empty<MegFileEntry>();
            visitedDirs.Add(normalized);

            _logger?.LogInformation($"Getting files from directory {directory}");

            var directoryInfo = new DirectoryInfo(normalized);
            var files = directoryInfo.EnumerateFiles();

            var entries = new List<MegFileEntry>();


            foreach (var fileInfo in files)
            {
                var name = createFileName(fileInfo.FullName);
                var entry = new MegFileEntry(fileInfo, name);
                _logger?.LogInformation($"Added entry: {entry}");
                entries.Add(entry);
            }
            
            if (!includeSubs)
            {
                _logger?.LogTrace("Do not include sub directories.");
                return entries;
            }


            var subDirs = directoryInfo.EnumerateDirectories("*", SearchOption.AllDirectories);
            foreach (var subDir in subDirs)
            {
                entries = entries.Union(GetFilesFromDirectory(subDir.FullName, true, visitedDirs, createFileName))
                    .ToList();
            }

            return entries;
        }


        private static bool IsDirectory(string path)
        {
            var attr = File.GetAttributes(path);
            return attr.HasFlag(FileAttributes.Directory);
        }


        private  string GetAbsolutePath(string path, out bool wasAbsolute, string? workingDirectory = null)
        {
            wasAbsolute = true;
            if (Path.IsPathRooted(path))
                return path;
            wasAbsolute = false;
            if (workingDirectory != null)
            {
                _logger?.LogTrace($"Converting relative path '{path}' to absolute path form WorkingDir {workingDirectory}.");
                return Path.Combine(workingDirectory, path);
            }
            _logger?.LogTrace($"Converting relative path '{path}' to absolute path.");
            return Path.Combine(Directory.GetCurrentDirectory(), path);
        }

        private string GetAbsolutePath(string path, string? workingDirectory = null)
        {
            return GetAbsolutePath(path, out _, workingDirectory);
        }

        private class FileInfoEqualityComparer : IEqualityComparer<FileInfo>
        {
            public static FileInfoEqualityComparer Instance = new FileInfoEqualityComparer();


            public bool Equals(FileInfo? x, FileInfo? y)
            {
                if (x is null || y is null)
                    return false;
                var xp = Path.GetFullPath(x.FullName);
                var yp = Path.GetFullPath(y.FullName);
                return xp.Equals(yp, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(FileInfo? obj)
            {
                return obj == null ? 0 : Path.GetFullPath(obj.FullName).GetHashCode();
            }
        }

        private readonly struct MegFileEntry : IEquatable<MegFileEntry>, IEquatable<FileInfo>
        {
            public string Name { get; }

            public FileInfo File { get; }

            public MegFileEntry(FileInfo file, string? fileName = null)
            {
                File = file;
                Name = fileName ?? file.FullName;
            }

            public override bool Equals(object obj)
            {
                return obj switch
                {
                    null => false,
                    MegFileEntry otherEntry => Equals(otherEntry),
                    FileInfo otherFileInfo => Equals(otherFileInfo),
                    _ => false
                };
            }


            public bool Equals(MegFileEntry other)
            {
                return FileInfoEqualityComparer.Instance.Equals(this.File, other.File);
            }

            public bool Equals(FileInfo other)
            {
                return FileInfoEqualityComparer.Instance.Equals(this.File, other);
            }

            public override int GetHashCode()
            {
                return FileInfoEqualityComparer.Instance.GetHashCode(File);
            }

            public override string ToString()
            {
                return $"NAME: {Name}, PATH: {File.FullName}";
            }
        }
    }
}
