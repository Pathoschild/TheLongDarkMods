using System;
using System.Diagnostics.CodeAnalysis;
using Il2Cpp;
using MelonLoader;
using Pathoschild.TheLongDarkMods.AutoFillMapOnExplore.Framework;
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
            && this.TryGetMapPanel(out Panel_Map? map)
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


    /*********
    ** Private methods
    *********/
    /// <summary>Get the map panel, if the game is ready.</summary>
    /// <param name="map">The map panel, if applicable.</param>
    /// <returns>Returns whether the map is available.</returns>
    private bool TryGetMapPanel([NotNullWhen(true)] out Panel_Map? map)
    {
        if (GameManager.m_Instance is not null && !GameManager.IsMainMenuActive() && GameManager.m_ActiveScene is not (null or "" or "MainMenu"))
        {
            map = InterfaceManager.GetPanel<Panel_Map>();
            return map != null;
        }

        map = null;
        return false;
    }
}
