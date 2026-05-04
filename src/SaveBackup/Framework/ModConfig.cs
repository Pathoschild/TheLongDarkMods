using ModSettings;
using UnityEngine;

namespace Pathoschild.TheLongDarkMods.SaveBackup.Framework;

/// <summary>The mod config model.</summary>
internal class ModConfig : JsonModSettings
{
    /*********
    ** Accessors
    *********/
    /****
    ** Automatic backups
    ****/
    [Section("Automatic backups")]
    [Name("Number of daily backups")]
    [Description("The number of daily backups to keep. Older backups are deleted automatically.")]
    [Slider(0, 30)]
    public int DailyBackupCount = 10;

    [Name("Number of hourly backups")]
    [Description("The number of hourly backups to keep. Older backups are deleted automatically.")]
    [Slider(0, 30)]
    public int HourlyBackupCount = 10;

    /****
    ** Manual backups
    ****/
    [Section("Manual backups")]
    [Name("Backup key")]
    [Description("The button to press to create an immediate backup. This doesn't trigger a save, it only backs up any current saves on disk.")]
    public KeyCode ManualBackupKey = KeyCode.None;

    [Name("Number of backups")]
    [Description("The number of manual backups to keep. Older backups are deleted automatically.")]
    [Slider(0, 30)]
    public int ManualBackupCount = 10;


    /****
    ** What to back up
    ****/
    [Section("What to back up")]
    [Name("Include survival saves")]
    [Description("Whether to back up saves created in survival mode.")]
    public bool IncludeSurvival = true;

    [Name("Include Wintermute saves")]
    [Description("Whether to back up saves created in Wintermute.\n\nNote: Wintermute saves are only backed up when you launch the game, unless you add MelonLoader to the Wintermute version.")]
    public bool IncludeWintermute = true;
}
