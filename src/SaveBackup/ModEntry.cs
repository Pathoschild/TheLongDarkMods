using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Il2Cpp;
using MelonLoader;
using MelonLoader.Utils;
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

    /// <summary>A brief name for the backup (excluding the date).</summary>
    private string BackupLabel = null!; // set in OnLateInitializeMelon

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
        this.BackupLabel = $"MelonLoader {BuildInfo.Version}, The Long Dark {GameManager.GetVersionString().Trim()}";

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
        DateTimeOffset now = DateTimeOffset.Now;

        this.UpdateBackupsOfType(
            rootBackupsPath: this.BackupFolder,
            type: DailyFolderName,
            name: $"{now:yyyy-MM-dd} ({this.BackupLabel})",
            backupsToKeep: this.Config.DailyBackupCount
        );

        this.UpdateBackupsOfType(
            rootBackupsPath: this.BackupFolder,
            type: HourlyFolderName,
            name: $"{now:yyyy-MM-dd HH} ({this.BackupLabel})",
            backupsToKeep: this.Config.HourlyBackupCount
        );
    }

    /// <summary>Back up the current saves of a given type and prune its older backups as needed.</summary>
    /// <param name="rootBackupsPath">The root folder in which to store backup subfolders.</param>
    /// <param name="type">The backup type (e.g. <see cref="DailyFolderName"/>).</param>
    /// <param name="name">The unique name for the backup, excluding the path and extension.</param>
    /// <param name="backupsToKeep">The number of backups to keep of this type.</param>
    private void UpdateBackupsOfType(string rootBackupsPath, string type, string name, int backupsToKeep)
    {
        try
        {
            string folderPath = Path.Combine(rootBackupsPath, type);

            // add backup
            if (backupsToKeep > 0)
            {
                Directory.CreateDirectory(folderPath);

                // get target path
                FileInfo targetFile = new(Path.Combine(folderPath, $"{name}.zip"));
                DirectoryInfo fallbackDir = new(Path.Combine(folderPath, name));
                if (targetFile.Exists || fallbackDir.Exists)
                    return;

                // copy saves to fallback directory
                DirectoryInfo savesDir = new(this.SavesPath);
                if (!this.RecursiveCopy(savesDir, fallbackDir, copyRoot: false))
                    return;

                // compress backup if possible
                if (!this.TryCompressDir(fallbackDir.FullName, targetFile, out Exception? compressError))
                    this.Log.Msg($"Added {type} backup at {fallbackDir.FullName}. Couldn't compress backup:\n{compressError}");
                else
                {
                    this.Log.Msg($"Added {type} backup at {targetFile.FullName}.");
                    fallbackDir.Delete(recursive: true);
                }
            }

            // prune backups
            this.PruneBackups(type, new DirectoryInfo(folderPath), backupsToKeep);
        }
        catch (Exception ex)
        {
            this.Log.Error($"Couldn't create save backup '{name}'.", ex);
        }
    }

    /// <summary>Remove old backups if we've exceeded the limit.</summary>
    /// <param name="type">The backup type (e.g. <see cref="DailyFolderName"/>).</param>
    /// <param name="backupFolder">The folder containing save backups of the type being pruned.</param>
    /// <param name="backupsToKeep">The number of backups to keep.</param>
    private void PruneBackups(string type, DirectoryInfo backupFolder, int backupsToKeep)
    {
        try
        {
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

    /// <summary>Recursively copy a directory or file.</summary>
    /// <param name="source">The file or folder to copy.</param>
    /// <param name="targetFolder">The folder to copy into.</param>
    /// <param name="copyRoot">Whether to copy the root folder itself, or <c>false</c> to only copy its contents.</param>
    /// <returns>Returns whether any files were copied.</returns>
    private bool RecursiveCopy(FileSystemInfo source, DirectoryInfo targetFolder, bool copyRoot = true)
    {
        if (!source.Exists)
            return false;

        bool anyCopied = false;

        switch (source)
        {
            case FileInfo sourceFile:
                targetFolder.Create();
                sourceFile.CopyTo(Path.Combine(targetFolder.FullName, sourceFile.Name));
                anyCopied = true;
                break;

            case DirectoryInfo sourceDir:
                DirectoryInfo targetSubfolder = copyRoot ? new DirectoryInfo(Path.Combine(targetFolder.FullName, sourceDir.Name)) : targetFolder;
                foreach (var entry in sourceDir.EnumerateFileSystemInfos())
                    anyCopied = this.RecursiveCopy(entry, targetSubfolder) || anyCopied;
                break;

            default:
                throw new NotSupportedException($"Unknown filesystem info type '{source.GetType().FullName}'.");
        }

        return anyCopied;
    }
}
