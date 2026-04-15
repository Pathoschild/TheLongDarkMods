using Il2Cpp;
using MelonLoader;
using Pathoschild.TheLongDarkMods.ShortcutHome.Framework;
using UnityEngine;

namespace Pathoschild.TheLongDarkMods.ShortcutHome;

/// <inheritdoc />
public class ModEntry : MelonMod
{
    /*********
    ** Fields
    *********/
    /// <summary>The mod settings.</summary>
    private readonly ModConfig Config = new();

    /// <summary>Tracks the destination endpoint which the player last warped from, if any.</summary>
    private readonly DestinationManager DestinationManager = new();

    /// <summary>The destination which the player is currently warping back to, if applicable.</summary>
    private Destination? WarpingBackTo;


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
        // initiate warp
        if (InputManager.GetKeyDown(InputManager.m_CurrentContext, this.Config.FastTravelKey) && this.Config.GetHomeScene() is { } homeScene)
        {
            if (GameManager.m_Instance is not null && !GameManager.IsMainMenuActive() && GameManager.m_ActiveScene is not (null or "" or "MainMenu"))
            {

                // warp home
                if (GameManager.m_ActiveScene != homeScene)
                {
                    this.DestinationManager.SetDestination();
                    GameManager.LoadSceneWithLoadingScreen(homeScene);
                }

                // else warp back
                else
                {
                    Destination? destination = this.DestinationManager.GetDestination();
                    if (destination != null)
                    {
                        this.WarpingBackTo = destination;
                        GameManager.LoadSceneWithLoadingScreen(destination.Scene);
                    }
                }
            }
        }
    }

    /// <inheritdoc />
    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        // ignore intermediate scene
        if (sceneName is "Empty")
            return;

        // update position after warp
        Destination? destination = this.WarpingBackTo;
        if (destination is not null)
        {
            if (sceneName != destination.Scene)
                MelonLogger.Warning($"Failed setting position after warp back: arrived in scene '{sceneName}' instead of the expected '{destination.Scene}'.");
            else
            {
                Transform player = GameManager.GetPlayerObject().transform;
                player.position = destination.Position;
            }

            this.WarpingBackTo = null;
        }
    }
}
