namespace Pathoschild.TheLongDarkMods.ShortcutHome.Framework;

/// <summary>The data model for data persisted to ModData.</summary>
internal class DataModel
{
    /*********
    ** Accessors
    *********/
    /// <summary>The scene ID from which the player warped home, or <c>null</c> if there's no warp endpoint set.</summary>
    public string? Scene { get; set; }

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
    public DataModel() { }

    /// <summary>Construct an instance.</summary>
    /// <param name="destination">The destination to copy.</param>
    public DataModel(Destination? destination)
    {
        if (destination is not null)
        {
            this.Scene = destination.Scene;
            this.X = destination.Position.x;
            this.Y = destination.Position.y;
            this.Z = destination.Position.z;
        }
    }
}
