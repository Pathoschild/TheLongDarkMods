using System;
using System.Collections.Generic;

namespace Pathoschild.TheLongDarkMods.FastTravel.Framework.DataModels;

/// <summary>The data model for data persisted to ModData.</summary>
internal class SaveModel
{
    /*********
    ** Accessors
    *********/
    /// <summary>The player's location before their most recent fast travel.</summary>
    public Destination? ReturnPoint { get; set; }

    /// <summary>The saved destinations.</summary>
    public Dictionary<int, Destination> Destinations { get; set; } = [];


    /*********
    ** Public methods
    *********/
    /// <summary>Get a saved destination, if it exists.</summary>
    /// <param name="index">The slot index.</param>
    public Destination? Get(int index)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), $"Invalid fast travel slot index {index}.");

        return this.Destinations.GetValueOrDefault(index);
    }

    /// <summary>Save a destination to a slot.</summary>
    /// <param name="index">The slot index.</param>
    /// <param name="destination">The destination to set, or <c>null</c> to delete it.</param>
    public void Set(int index, Destination? destination)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), $"Invalid fast travel slot index {index}.");

        if (destination is null)
            this.Destinations.Remove(index);
        else
            this.Destinations[index] = destination;
    }
}
