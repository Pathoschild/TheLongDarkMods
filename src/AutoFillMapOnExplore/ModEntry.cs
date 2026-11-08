using System;
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

    /// <summary>The delay between checks to configure the open map.</summary>
    private readonly DelayTimer MapConfigureDelay = new();

    /// <summary>The pointer for the last map whose player icon was broken and for which <see cref="TryFixArrowSprite"/> was applied (regardless of whether it succeeded).</summary>
    private IntPtr LastMapFixed;


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

        // skip if disabled
        if (config is { AutoFillEnabled: false, ShowPlayerArrow: false })
            return;

        // auto-fill map
        MapReference? mapRef = null;
        if (config.AutoFillEnabled && this.MapAutoFillDelay.Check(config.AutoFillSeconds))
        {
            mapRef ??= GetMapIfAvailable();

            mapRef.Map?.DoNearbyDetailsCheck(
                radius: config.AutoFillRadius,
                forceAddSurveyPosition: false,
                useOverridePosition: false, // use player position
                overridePostion: Vector3.zero
            );

            this.MapAutoFillDelay.Reset();
        }

        // show player arrow
        if (config.ShowPlayerArrow && this.MapConfigureDelay.Check(0.1f))
        {
            mapRef ??= GetMapIfAvailable();

            if (
                mapRef.Map?.IsEnabled() is true                                            // map is open
                && !mapRef.Map.IsWorldMapActive()                                          // not looking at world map
                && mapRef.Map.GetIndexOfCurrentScene() == mapRef.Map.m_RegionSelectedIndex // not looking at a different region's map
                && mapRef.Map.m_PlayerIcon?.gameObject is { } arrow                        // arrow icon is available (should always be)
            )
            {
                arrow.SetActive(true);
                this.TryFixArrowSprite(mapRef.Map, arrow);
            }

            this.MapConfigureDelay.Reset();
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

    /// <summary>Fix the map's player arrow icon if it's linked to the wrong atlas.</summary>
    /// <param name="map">The map whose arrow icon to fix.</param>
    /// <param name="arrow">The arrow icon to fix.</param>
    /// <remarks>The Long Dark 2.58+ removed the arrow's <c>ico_map_directional</c> sprite from the <c>Skills</c> atlas, so it's invisible on the map. This updates the map icon to get it from the correct atlas.</remarks>
    private void TryFixArrowSprite(Panel_Map map, GameObject arrow)
    {
        // skip if nothing to fix
        UISprite? sprite = arrow.GetComponent<UISprite>();
        if (sprite is null || sprite.isValid || string.IsNullOrEmpty(sprite.spriteName))
            return;

        // skip if we already tried and failed
        if (this.LastMapFixed == map.Pointer)
            return;
        this.LastMapFixed = map.Pointer;

        // link sprite to an atlas which has it
        UIAtlas? atlas = this.FindMapAtlasFor(map, sprite.spriteName);
        if (atlas is not null)
            sprite.atlas = atlas;
    }

    /// <summary>Find the atlas reachable from the map which contains the given sprite name, if any.</summary>
    /// <param name="map">The map whose atlases to search.</param>
    /// <param name="spriteName">The sprite name to find.</param>
    /// <returns>Returns the first atlas containing the icon, or <c>null</c> if it wasn't found.</returns>
    private UIAtlas? FindMapAtlasFor(Panel_Map map, string spriteName)
    {
        foreach (UISprite atlasSprite in map.GetComponentsInChildren<UISprite>(includeInactive: true))
        {
            UIAtlas? atlas = atlasSprite.atlas;
            if (atlas?.GetSprite(spriteName) != null)
                return atlas;
        }

        return null;
    }

    /// <summary>A reference to the map panel, if it's available.</summary>
    /// <param name="Map">The map instance if it's available, else <c>null</c>.</param>
    private sealed record MapReference(Panel_Map? Map);
}
