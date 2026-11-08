using Il2Cpp;
using MelonLoader;
using Pathoschild.TheLongDarkMods.AutoFillMapOnExplore.Framework;
using Pathoschild.TheLongDarkMods.Common;
using UnityEngine;

namespace Pathoschild.TheLongDarkMods.AutoFillMapOnExplore;

/// <inheritdoc />
public class ModEntry : MelonMod
{
    /*********
    ** Fields
    *********/
    /// <summary>The mod settings.</summary>
    private readonly ModConfig Config = new();

    /// <summary>The delay between map auto-fills.</summary>
    private readonly DelayTimer MapAutoFillDelay = new();


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void OnInitializeMelon()
    {
        this.Config.AddToModSettings(ModInfo.DisplayName);
    }

    /// <inheritdoc />
    public override void OnUpdate()
    {
        ModConfig config = this.Config;

        // auto-fill map
        if (config.AutoFillEnabled && this.MapAutoFillDelay.Check(config.AutoFillSeconds))
        {
            MapReference mapRef = GetMapIfAvailable();

            mapRef.Map?.DoNearbyDetailsCheck(
                radius: config.AutoFillRadius,
                forceAddSurveyPosition: false,
                useOverridePosition: false, // use player position
                overridePostion: Vector3.zero
            );

            this.MapAutoFillDelay.Reset();
        }
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the map, if the current scene has one.</summary>
    private static MapReference GetMapIfAvailable()
    {
        if (
            SceneHelper.IsPlayableScene()
            && InterfaceManager.GetPanel<Panel_Map>() is { } map
            && map.SceneCanBeMapped(map.GetMapNameOfCurrentScene())
        )
            return new MapReference(map);

        return new MapReference(null);
    }

    /// <summary>A reference to the map panel, if it's available.</summary>
    /// <param name="Map">The map instance if it's available, else <c>null</c>.</param>
    private sealed record MapReference(Panel_Map? Map);
}
