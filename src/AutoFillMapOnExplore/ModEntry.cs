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

    /// <summary>When the map was last filled.</summary>
    private DateTime LastMapAutoFill = DateTime.UtcNow;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void OnInitializeMelon()
    {
        this.Config.AddToModSettings(ModInfo.DisplayName);
    }

    /// <inheritdoc />
    public override void OnFixedUpdate()
    {
        ModConfig config = this.Config;

        if (
            config.Enabled
            && (DateTime.UtcNow - this.LastMapAutoFill).TotalSeconds > config.AutoFillSeconds
            && SceneHelper.IsPlayableScene()
            && InterfaceManager.GetPanel<Panel_Map>() is { } map
            && map.SceneCanBeMapped(map.GetMapNameOfCurrentScene())
        )
        {
            this.LastMapAutoFill = DateTime.UtcNow;

            map.DoNearbyDetailsCheck(
                radius: config.AutoFillRadius,
                forceAddSurveyPosition: false,
                useOverridePosition: false, // use player position
                overridePostion: Vector3.zero
            );
        }
    }
}
