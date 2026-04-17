using ModSettings;

namespace Pathoschild.TheLongDarkMods.SaveBackup.Framework;

/// <summary>The mod config model.</summary>
internal class ModConfig : JsonModSettings
{
    /*********
    ** Accessors
    *********/
    [Name("Number of backups")]
    [Description("The number of daily backups to keep.")]
    [Slider(1, 30)]
    public int BackupCount = 10;
}
