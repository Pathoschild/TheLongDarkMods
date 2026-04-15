using UnityEngine;

namespace Pathoschild.TheLongDarkMods.ShortcutHome.Framework;

/// <summary>A specific location and position in the game.</summary>
/// <param name="Scene">The scene ID for the location.</param>
/// <param name="Position">The three-dimensional position within the scene.</param>
internal record Destination(string Scene, Vector3 Position)
{
    /// <inheritdoc />
    public override string ToString()
    {
        return $"{this.Scene} ({this.Position})";
    }
}
