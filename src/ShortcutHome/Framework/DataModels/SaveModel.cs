using System.Collections.Generic;

namespace Pathoschild.TheLongDarkMods.ShortcutHome.Framework.DataModels;

/// <summary>The data model for data persisted to ModData.</summary>
internal class SaveModel
{
    /*********
    ** Accessors
    *********/
    /// <summary>The saved destinations.</summary>
    public Dictionary<DestinationType, Destination> Destinations { get; set; } = [];


    /*********
    ** Public methods
    *********/
    /// <summary>Get the saved home location, if it exists.</summary>
    public Destination? GetHome()
    {
        return this.Destinations.GetValueOrDefault(DestinationType.Home);
    }

    /// <summary>Get the saved return point, if it exists.</summary>
    public Destination? GetReturnPoint()
    {
        return this.Destinations.GetValueOrDefault(DestinationType.ReturnPoint);
    }
}
