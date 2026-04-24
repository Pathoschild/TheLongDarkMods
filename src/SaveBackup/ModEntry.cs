using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Il2Cpp;
using MelonLoader;
using MelonLoader.Utils;
using Pathoschild.TheLongDarkMods.SaveBackup.assets;
using Pathoschild.TheLongDarkMods.SaveBackup.Framework;

namespace Pathoschild.TheLongDarkMods.SaveBackup;

/// <inheritdoc />
public class ModEntry : MelonMod
{
    /*********
    ** Fields
    *********/
    /// <summary>The name of the subfolder for daily backups.</summary>
    private const string DailyFolderName = "daily";

    /// <summary>The name of the subfolder for hourly backups.</summary>
    private const string HourlyFolderName = "hourly";

    /// <summary>The folder containing saves to back up.</summary>
    private readonly string SavesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Hinterland", "TheLongDark");

    /// <summary>The absolute path to the folder in which to store save backups.</summary>
    private readonly string BackupFolder = Path.Combine(MelonEnvironment.ModsDirectory, "SaveBackup");

    /// <summary>The mod settings.</summary>
    private readonly ModConfig Config = new();

    /// <summary>The log instance.</summary>
    private MelonLogger.Instance Log = null!; // set in OnInitializeMelon

    /// <summary>The loaded game version.</summary>
    private string GameVersion = null!; // set in OnLateInitializeMelon

    /// <summary>The loaded MelonLoader API version.</summary>
    private string MelonLoaderVersion = null!; // set in OnLateInitializeMelon

    /// <summary>The next time when the mod should create save backups if needed.</summary>
    private DateTimeOffset NextBackupCheck = DateTimeOffset.MaxValue; // wait until migrations complete


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void OnInitializeMelon()
    {
        this.Log = Melon<ModEntry>.Logger;

        this.Config.AddToModSettings(ModInfo.DisplayName);
    }

    /// <inheritdoc />
    public override void OnLateInitializeMelon()
    {
        this.GameVersion = GameManager.GetVersionString().Trim();
        this.MelonLoaderVersion = BuildInfo.Version;

        Task
            .Run(this.MoveLegacyBackups)
            .ContinueWith(_ => this.NextBackupCheck = DateTimeOffset.UtcNow);
    }

    /// <inheritdoc />
    public override void OnFixedUpdate()
    {
        // wait until next backup window
        DateTimeOffset now = DateTimeOffset.UtcNow;
        if (now < this.NextBackupCheck)
            return;
        this.NextBackupCheck = now.Add(new TimeSpan(hours: 1, minutes: -now.Minute, seconds: -now.Second)); // set to <hour + 1>:00:00

        // create backups
        Task.Run(this.UpdateBackups);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Move 1.0.0 backups into the newer <see cref="DailyFolderName"/>.</summary>
    private void MoveLegacyBackups()
    {
        try
        {
            DirectoryInfo rootFolder = new DirectoryInfo(this.BackupFolder);
            if (!rootFolder.Exists)
                return;

            FileInfo[] legacyFiles = rootFolder.GetFiles();
            if (legacyFiles.Length < 1)
                return;

            DirectoryInfo dailyFolder = new DirectoryInfo(Path.Combine(this.BackupFolder, DailyFolderName));
            dailyFolder.Create();

            foreach (FileInfo file in legacyFiles)
                file.MoveTo(Path.Combine(dailyFolder.FullName, file.Name));

            this.Log.Msg($"Moved {legacyFiles.Length} root backups into the '{DailyFolderName}' folder.");
        }
        catch (Exception ex)
        {
            this.Log.Error($"Couldn't move pre-1.1.0 backup files into the '{DailyFolderName}' folder.", ex);
        }
    }

    /// <summary>Back up the current saves and prune older backups as needed.</summary>
    private void UpdateBackups()
    {
        string backupLabel = this.GetBackupLabel();
        DateTimeOffset now = DateTimeOffset.Now;

        if (this.Config.DailyBackupCount > 0)
            this.UpdateBackupsOfType(DailyFolderName, $"{now:yyyy-MM-dd} ({backupLabel})");
        if (this.Config.HourlyBackupCount > 0)
            this.UpdateBackupsOfType(HourlyFolderName, $"{now:yyyy-MM-dd HH} ({backupLabel})");

        this.PruneBackups(DailyFolderName, this.Config.DailyBackupCount);
        this.PruneBackups(HourlyFolderName, this.Config.HourlyBackupCount);
    }

    /// <summary>Get the name for a backup taken now (excluding the data).</summary>
    private string GetBackupLabel()
    {
        string backupLabel = $"MelonLoader {this.MelonLoaderVersion}, The Long Dark {this.GameVersion}";

        switch (this.Config)
        {
            case { IncludeSurvival: true, IncludeWintermute: false }:
                backupLabel += ", survival only";
                break;

            case { IncludeSurvival: false, IncludeWintermute: true }:
                backupLabel += ", Wintermute only";
                break;

            case { IncludeSurvival: false, IncludeWintermute: false }:
                backupLabel += ", mod data only";
                break;
        }

        return backupLabel;
    }

    /// <summary>Back up the current saves of a given type and prune its older backups as needed.</summary>
    /// <param name="type">The backup type (e.g. <see cref="DailyFolderName"/>).</param>
    /// <param name="name">The unique name for the backup, excluding the path and extension.</param>
    private void UpdateBackupsOfType(string type, string name)
    {
        try
        {
            // get paths
            string backupRoot = Path.Combine(this.BackupFolder, type);
            FileInfo targetFile = new(Path.Combine(backupRoot, $"{name}.zip"));
            DirectoryInfo fallbackDir = new(Path.Combine(backupRoot, name));

            // skip: nothing to do
            if (targetFile.Exists || fallbackDir.Exists || !this.TryGetFilesToBackUp(out Dictionary<string, FileInfo> backupFiles))
                return;

            // copy files into backup folder
            foreach ((string relativePath, FileInfo file) in backupFiles)
            {
                string copyToPath = Path.Combine(fallbackDir.FullName, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(copyToPath)!);
                File.Copy(file.FullName, copyToPath);
            }

            // add README
            File.WriteAllText(
                Path.Combine(fallbackDir.FullName, "README.txt"),
                Resources.SaveFolderReadme
            );

            // zip & delete folder
            if (!this.TryCompressDir(fallbackDir.FullName, targetFile, out Exception? compressError))
                this.Log.Msg($"Added {type} backup at {fallbackDir.FullName}. Couldn't compress backup:\n{compressError}");
            else
            {
                this.Log.Msg($"Added {type} backup at {targetFile.FullName}.");
                fallbackDir.Delete(recursive: true);
            }
        }
        catch (Exception ex)
        {
            this.Log.Error($"Couldn't create save backup '{name}'.", ex);
        }
    }

    /// <summary>Get the files that should be backed up, indexed by their relative path within the backup.</summary>
    /// <param name="files">The files to back up.</param>
    /// <returns>Returns whether any files were found.</returns>
    private bool TryGetFilesToBackUp(out Dictionary<string, FileInfo> files)
    {
        files = [];

        // save files
        DirectoryInfo savesDir = new(this.SavesPath);
        if (savesDir.Exists)
        {
            foreach (FileSystemInfo entry in savesDir.GetFileSystemInfos())
            {
                bool isSurvival = entry is DirectoryInfo && entry.Name.Equals("Survival", StringComparison.OrdinalIgnoreCase);
                bool include = isSurvival ? this.Config.IncludeSurvival : this.Config.IncludeWintermute;
                if (!include)
                    continue;

                foreach (FileInfo file in this.GetFilesRecursively(entry))
                {
                    string relativePath = Path.GetRelativePath(savesDir.FullName, file.FullName);
                    files[Path.Combine("Saves", relativePath)] = file;
                }
            }
        }

        // mod data
        DirectoryInfo modDataDir = new DirectoryInfo(Path.Combine(MelonEnvironment.ModsDirectory, "ModData"));
        foreach (FileInfo file in this.GetFilesRecursively(modDataDir))
        {
            string relativePath = Path.GetRelativePath(MelonEnvironment.ModsDirectory, file.FullName);
            files[relativePath] = file;
        }

        return files.Count > 0;
    }

    /// <summary>Remove old backups if we've exceeded the limit.</summary>
    /// <param name="type">The backup type (e.g. <see cref="DailyFolderName"/>).</param>
    /// <param name="backupsToKeep">The number of backups to keep.</param>
    private void PruneBackups(string type, int backupsToKeep)
    {
        try
        {
            DirectoryInfo backupFolder = new DirectoryInfo(
                Path.Combine(this.BackupFolder, type)
            );

            if (!backupFolder.Exists)
                return;

            var oldBackups = backupFolder
                .GetFileSystemInfos()
                .OrderByDescending(p => p.CreationTimeUtc)
                .Skip(backupsToKeep);

            foreach (FileSystemInfo entry in oldBackups)
            {
                try
                {
                    this.Log.Msg($"Deleting {type} backup {entry.Name}...");
                    if (entry is DirectoryInfo folder)
                        folder.Delete(recursive: true);
                    else
                        entry.Delete();
                }
                catch (Exception ex)
                {
                    this.Log.Error($"Error deleting old {type} backup '{entry.Name}'.", ex);
                }
            }
        }
        catch (Exception ex)
        {
            this.Log.Error($"Couldn't remove old {type} backups.", ex);
        }
    }

    /// <summary>Try to create a compressed zip file for a directory.</summary>
    /// <param name="sourcePath">The directory path to zip.</param>
    /// <param name="destination">The destination file to create.</param>
    /// <param name="error">The error which occurred trying to compress, if applicable. This is <see cref="NotSupportedException"/> if compression isn't supported on this platform.</param>
    /// <returns>Returns whether compression succeeded.</returns>
    private bool TryCompressDir(string sourcePath, FileInfo destination, [NotNullWhen(false)] out Exception? error)
    {
        try
        {
            ZipFile.CreateFromDirectory(sourcePath, destination.FullName, CompressionLevel.Fastest, false);

            error = null;
            return true;
        }
        catch (Exception ex)
        {
            error = ex;
            return false;
        }
    }

    /// <summary>Recursively get all files starting from a root (including the root itself if it's a file).</summary>
    /// <param name="root">The file or directory from which to start.</param>
    private FileInfo[] GetFilesRecursively(FileSystemInfo root)
    {
        if (!root.Exists)
            return [];

        switch (root)
        {
            case FileInfo sourceFile:
                return [sourceFile];

            case DirectoryInfo sourceDir:
                return sourceDir.GetFiles("*.*", SearchOption.AllDirectories);

            default:
                throw new NotSupportedException($"Unknown filesystem info type '{root.GetType().FullName}'.");
        }
    }
}
