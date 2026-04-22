using ModSettings;

namespace Pathoschild.TheLongDarkMods.SaveBackup.Framework;

/// <summary>The mod config model.</summary>
internal class ModConfig : JsonModSettings
{
    /*********
    ** Accessors
    *********/
    [Name("Number of daily backups")]
    [Description("The number of daily backups to keep. Older backups are deleted automatically.")]
    [Slider(0, 30)]
    public int DailyBackupCount = 10;

    [Name("Number of hourly backups")]
    [Description("The number of hourly backups to keep. Older backups are deleted automatically.")]
    [Slider(0, 30)]
    public int HourlyBackupCount = 10;
}
