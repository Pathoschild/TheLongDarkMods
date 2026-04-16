using System.Collections.Generic;
using UnityEngine;

namespace Pathoschild.TheLongDarkMods.ShortcutHome.Framework;

/// <summary>The data model for data persisted to ModData.</summary>
internal class DataModel
{
    /*********
    ** Accessors
    *********/
    /// <summary>The saved destinations.</summary>
    public Dictionary<DestinationType, DataDestinationModel> Destinations { get; set; } = [];


    /*********
    ** Public methods
    *********/
    /// <summary>Get the saved home location, if it exists.</summary>
    public Destination? GetHome()
    {
        return this.GetDestination(DestinationType.Home);
    }

    /// <summary>Get the saved return point, if it exists.</summary>
    public Destination? GetReturnPoint()
    {
        return this.GetDestination(DestinationType.ReturnPoint);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get a saved destination, if it exists.</summary>
    /// <param name="type">The destination type to get.</param>
    private Destination? GetDestination(DestinationType type)
    {
        return this.Destinations.TryGetValue(type, out DataDestinationModel? raw)
            ? new Destination(raw.Scene, new Vector3(raw.X, raw.Y, raw.Z), raw.CameraPitch, raw.CameraYaw)
            : null;
    }
}
