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
    }
}
