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

// Derived from https://github.com/Pathoschild/SMAPI (see src/SMAPI.Mods.SaveBackup/ModEntry.cs).
// I'm the only author for all copied changes, so this is multi-licensed under MIT for this repo.

/// <inheritdoc />
public class ModEntry : MelonMod
{
    /*********
    ** Fields
    *********/
    /// <summary>The folder containing saves to back up.</summary>
    private readonly string SavesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Hinterland", "TheLongDark");

    /// <summary>The absolute path to the folder in which to store save backups.</summary>
    private readonly string BackupFolder = Path.Combine(MelonEnvironment.ModsDirectory, "SaveBackup");

    /// <summary>The mod settings.</summary>
    private readonly ModConfig Config = new();

    /// <summary>The log instance.</summary>
    private MelonLogger.Instance Log = null!; // set in OnInitializeMelon


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
        try
        {
            // init backup folder
            DirectoryInfo backupFolder = new(this.BackupFolder);
            backupFolder.Create();

            // back up & prune saves
            string backupName = $"{DateTime.Now:yyyy-MM-dd} (MelonLoader {BuildInfo.Version}, The Long Dark {GameManager.GetVersionString().Trim()})";
            Task
                .Run(() => this.CreateBackup(backupFolder, backupName))
                .ContinueWith(_ => this.PruneBackups(backupFolder, this.Config.BackupCount));
        }
        catch (Exception ex)
        {
            this.Log.Error("Error backing up saves.", ex);
        }
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Back up the current saves.</summary>
    /// <param name="backupFolder">The folder containing save backups.</param>
    /// <param name="backupName">The name of the backup folder or zip to create.</param>
    private void CreateBackup(DirectoryInfo backupFolder, string backupName)
    {
        try
        {
            // get target path
            FileInfo targetFile = new(Path.Combine(backupFolder.FullName, $"{backupName}.zip"));
            DirectoryInfo fallbackDir = new(Path.Combine(backupFolder.FullName, backupName));
            if (targetFile.Exists || fallbackDir.Exists)
            {
                this.Log.Msg("Already backed up today.");
                return;
            }

            // copy saves to fallback directory
            DirectoryInfo savesDir = new(this.SavesPath);
            if (!this.RecursiveCopy(savesDir, fallbackDir, copyRoot: false))
            {
                this.Log.Warning("No saves found to back up.");
                return;
            }

            // compress backup if possible
            if (!this.TryCompressDir(fallbackDir.FullName, targetFile, out Exception? compressError))
                this.Log.Msg($"Backed up to {fallbackDir.FullName}. Couldn't compress backup:\n{compressError}");
            else
            {
                this.Log.Msg($"Backed up to {targetFile.FullName}.");
                fallbackDir.Delete(recursive: true);
            }
        }
        catch (Exception ex)
        {
            this.Log.Error("Couldn't back up saves.", ex);
        }
    }

    /// <summary>Remove old backups if we've exceeded the limit.</summary>
    /// <param name="backupFolder">The folder containing save backups.</param>
    /// <param name="backupsToKeep">The number of backups to keep.</param>
    private void PruneBackups(DirectoryInfo backupFolder, int backupsToKeep)
    {
        try
        {
            var oldBackups = backupFolder
                .GetFileSystemInfos()
                .OrderByDescending(p => p.CreationTimeUtc)
                .Skip(backupsToKeep);

            foreach (FileSystemInfo entry in oldBackups)
            {
                try
                {
                    this.Log.Msg($"Deleting {entry.Name}...");
                    if (entry is DirectoryInfo folder)
                        folder.Delete(recursive: true);
                    else
                        entry.Delete();
                }
                catch (Exception ex)
                {
                    this.Log.Error($"Error deleting old save backup '{entry.Name}'.", ex);
                }
            }
        }
        catch (Exception ex)
        {
            this.Log.Error("Couldn't remove old backups.", ex);
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
