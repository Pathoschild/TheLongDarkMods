namespace Pathoschild.TheLongDarkMods.FastTravel.Framework;

/// <summary>The state for a fast travel transition across multiple ticks.</summary>
internal class FastTravelTransition
{
    /*********
    ** Accessors
    *********/
    /// <summary>The location we departed from.</summary>
    public Destination From { get; }

    /// <summary>The location we're arriving in.</summary>
    public Destination To { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="from"><inheritdoc cref="From" path="/summary"/></param>
    /// <param name="to"><inheritdoc cref="To" path="/summary"/></param>
    public FastTravelTransition(Destination from, Destination to)
    {
        this.From = from;
        this.To = to;
    }
}
