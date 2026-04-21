namespace Pathoschild.TheLongDarkMods.FastTravel;

/// <summary>The build info for MelonLoader.</summary>
internal class ModInfo
{
    /// <summary>The human-readable display name for the mod.</summary>
    public const string DisplayName = "Fast Travel";

    /// <summary>The semantic version for the mod.</summary>
    /// <remarks>This affects both the MelonLoader mod version and DLL version.</remarks>
    public const string Version = "0.2.0";

    /// <summary>The assembly version, if different from the <see cref="Version"/>.</summary>
    /// <remarks>This is only needed when the version has a pre-release tag, which isn't valid in assembly versions.</remarks>
    public const string AssemblyVersion = Version;

    /// <summary>The author name.</summary>
    public const string Author = "Pathoschild";

    /// <summary>A short human-readable description of the mod.</summary>
    public const string Description = "Lets you save up to 9 places (like your home base), and fast travel to them anytime at the press of a button.";

    /// <summary>The URL of the page where the player can find the mod.</summary>
    public const string? DownloadLink = "https://www.nexusmods.com/thelongdark/mods/54";
}
