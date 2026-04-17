using UnityEngine;

namespace Pathoschild.TheLongDarkMods.ShortcutHome.Framework;

/// <summary>A specific location and position in the game.</summary>
/// <param name="Scene">The scene ID for the location.</param>
/// <param name="Position">The three-dimensional position within the scene.</param>
/// <param name="CameraPitch">The camera's vertical rotation angle in degrees.</param>
/// <param name="CameraYaw">The camera's horizontal rotation angle in degrees.</param>
internal record Destination(string Scene, Vector3 Position, float CameraPitch, float CameraYaw)
{
    /// <inheritdoc />
    public override string ToString()
    {
        return $"(scene: '{this.Scene}', position: {this.Position}, pitch: {this.CameraPitch}, yaw: {this.CameraYaw})";
    }
}
