using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Pathoschild.TheLongDarkMods.Common;
using Pathoschild.TheLongDarkMods.FastTravel.Framework.DataModels;

namespace Pathoschild.TheLongDarkMods.FastTravel.Framework;

/// <summary>Handles checking for fast travel restrictions.</summary>
internal class FastTravelRestrictionHelper
{
    /*********
    ** Fields
    *********/
    /// <summary>The mod settings.</summary>
    private readonly ModConfig Config;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="config">The mod settings.</param>
    public FastTravelRestrictionHelper(ModConfig config)
    {
        this.Config = config;
    }

    /// <summary>Get whether the player's mod settings prohibit a fast travel.</summary>
    /// <param name="from">The scene from which the player would travel.</param>
    /// <param name="to">The scene in which the player would arrive.</param>
    /// <param name="data">The saved fast travel destinations.</param>
    /// <param name="reasonPhrase">If restrictions are in place, a phrase which can fit in the sentence <c>Can't fast travel {0}</c>.</param>
    /// <returns>Returns whether restrictions prohibit this fast travel.</returns>
    public bool IsAllowed(Destination from, Destination? to, SaveModel data, [NotNullWhen(false)] out string? reasonPhrase)
    {
        bool isOutdoors = SceneHelper.IsOutdoors(from.Scene.Name);

        // disabled
        if (!this.Config.CanTravel)
        {
            reasonPhrase = "at all";
            return false;
        }

        // from non-fast travel point
        if (!this.Config.CanTravelFromNonFastTravelPoint && data.Destinations.All(p => p.Value.Scene.Name != from.Scene.Name))
        {
            reasonPhrase = "from a non-saved destination";
            return false;
        }

        // from outside
        if (!this.Config.CanTravelFromOutside && isOutdoors)
        {
            reasonPhrase = "from outside";
            return false;
        }

        // from non-safehouse
        if (!this.Config.CanTravelFromNonSafehouseInterior && !isOutdoors && !SceneHelper.IsCustomizableSafehouse())
        {
            reasonPhrase = "from non-safehouse interior";
            return false;
        }

        // from within scene
        if (!this.Config.CanTravelWithinScene && from.Scene.Name == to?.Scene.Name)
        {
            reasonPhrase = "to the same location";
            return false;
        }

        reasonPhrase = null;
        return true;
    }
}
