namespace Pathoschild.TheLongDarkMods.ShortcutHome.Framework;

/// <summary>The data model for a destination in <see cref="DataModel"/>.</summary>
internal class DataDestinationModel
{
    /*********
    ** Accessors
    *********/
    /// <summary>The scene ID for the location.</summary>
    public string Scene { get; set; } = string.Empty;

    /// <summary>The X position within the <see cref="Scene"/>.</summary>
    public float X { get; set; }

    /// <summary>The Y position within the <see cref="Scene"/>.</summary>
    public float Y { get; set; }

    /// <summary>The Z position within the <see cref="Scene"/>.</summary>
    public float Z { get; set; }

    /// <summary>The camera's vertical rotation angle in degrees.</summary>
    public float CameraPitch { get; set; }

    /// <summary>The camera's horizontal rotation angle in degrees.</summary>
    public float CameraYaw { get; set; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    public DataDestinationModel() { }

    /// <summary>Construct an instance.</summary>
    /// <param name="destination">The destination to copy.</param>
    public DataDestinationModel(Destination destination)
    {
        this.Scene = destination.Scene;
        this.X = destination.Position.x;
        this.Y = destination.Position.y;
        this.Z = destination.Position.z;
        this.CameraPitch = destination.CameraPitch;
        this.CameraYaw = destination.CameraYaw;
    }
}
