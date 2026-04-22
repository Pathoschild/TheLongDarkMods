using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Il2Cpp;
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
    /// <param name="reasonPhrase">If fast travel is restricted, a phrase which can fit in the sentence <c>Can't fast travel {0}</c>.</param>
    /// <returns>Returns whether restrictions prohibit this fast travel.</returns>
    public bool IsAllowed(Destination from, Destination? to, SaveModel data, [NotNullWhen(false)] out string? reasonPhrase)
    {
        bool isSameScene = from.Scene.Name == to?.Scene.Name;
        bool isFromOutside = SceneHelper.IsOutdoors(from.Scene.Name);

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
        if (!this.Config.CanTravelFromOutside && isFromOutside)
        {
            reasonPhrase = "from outside";
            return false;
        }

        // from non-safehouse
        if (!this.Config.CanTravelFromNonSafehouseInterior && !isFromOutside && !SceneHelper.IsCustomizableSafehouse())
        {
            reasonPhrase = "from non-safehouse interior";
            return false;
        }

        // from within scene
        if (!this.Config.CanTravelWithinScene && isSameScene)
        {
            reasonPhrase = "to the same location";
            return false;
        }

        // restrict by weather
        if (!this.CanTravelDuringWeather(out reasonPhrase))
            return false;

        reasonPhrase = null;
        return true;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get whether the player can travel during the current weather in their departure region.</summary>
    /// <param name="reasonPhrase">If fast travel is restricted, a phrase which can fit in the sentence <c>Can't fast travel {0}</c>.</param>
    /// <returns>Returns whether travel is allowed.</returns>
    private bool CanTravelDuringWeather([NotNullWhen(false)] out string? reasonPhrase)
    {
        switch (GameManager.GetWeatherComponent().GetWeatherStage())
        {
            case WeatherStage.ClearAurora:
                reasonPhrase = "during an aurora";
                return this.Config.CanTravelDuringAurora;

            case WeatherStage.DenseFog:
                reasonPhrase = "during dense fog";
                return this.Config.CanTravelDuringDenseFog;

            case WeatherStage.ElectrostaticFog:
                reasonPhrase = "during glimmer fog";
                return this.Config.CanTravelDuringGlimmerFog;

            case WeatherStage.LightSnow:
                reasonPhrase = "during light snowfall";
                return this.Config.CanTravelDuringLightSnowfall;

            case WeatherStage.HeavySnow:
                reasonPhrase = "during heavy snowfall";
                return this.Config.CanTravelDuringHeavySnowfall;

            case WeatherStage.Blizzard:
                reasonPhrase = "during a blizzard";
                return this.Config.CanTravelDuringBlizzard;

            default:
                reasonPhrase = null;
                return true;
        }
    }
}
