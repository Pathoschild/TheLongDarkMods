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

    /// <summary>Tracks the persisted fast travel destinations.</summary>
    private readonly DestinationManager DestinationManager = new();

    /// <summary>The destination which the player is currently traveling to, if applicable.</summary>
    private Destination? TravelingTo;


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
        if (InteractionHelper.IsKeyDown(this.Config.SetHomeKey) && this.IsSaveLoaded())
            this.OnInteractivelySetHome();

        else if (InteractionHelper.IsKeyDown(this.Config.FastTravelKey) && this.IsSaveLoaded())
            this.OnInteractivelyFastTravel();
    }

    /// <inheritdoc />
    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        // ignore intermediate scene
        if (sceneName is "Empty")
            return;

        // update position after travel
        Destination? destination = this.TravelingTo;
        if (destination is not null)
        {
            if (sceneName != destination.Scene)
                MelonLogger.Warning($"Failed setting position after warp back: arrived in scene '{sceneName}' instead of the expected '{destination.Scene}'.");
            else
            {
                Transform player = GameManager.GetPlayerObject().transform;
                player.position = destination.Position;
            }

            this.TravelingTo = null;
        }
    }

    /*********
    ** Private methods
    *********/
    /// <summary>Handle the player requesting to set their home location.</summary>
    private void OnInteractivelySetHome()
    {
        string sceneId = GameManager.m_ActiveScene;
        Destination? savedHome = this.DestinationManager.GetData().GetHome();

        // update position in same home
        if (savedHome != null && savedHome.Scene == sceneId)
        {
            InteractionHelper.ShowConfirmDialogue(
                "This is already home! Do you want to update the arrival position?",
                () => this.DestinationManager.SetDestination(DestinationType.Home)
            );
        }

        // else replace home
        else
        {
            string question = $"Set {this.GetSceneDisplayName(sceneId)} as your home?";
            if (savedHome != null)
                question += $"\n\nThis will replace your previous home ({this.GetSceneDisplayName(savedHome.Scene)}).";

            InteractionHelper.ShowConfirmDialogue(
                question,
                () => this.DestinationManager.SetDestination(DestinationType.Home)
            );
        }
    }

    /// <summary>Handle the player requesting to fast travel.</summary>
    private void OnInteractivelyFastTravel()
    {
        string sceneId = GameManager.m_ActiveScene;
        DataModel data = this.DestinationManager.GetData();
        Destination? home = data.GetHome();
        Destination? returnPoint = data.GetReturnPoint();

        // not set up yet
        if (home is null)
        {
            InteractionHelper.ShowMessageBox($"You haven't set your home yet.\n\nPress {this.Config.SetHomeKey} to set your current location as home.");
            return;
        }

        // travel home
        if (home.Scene != sceneId)
        {
            string question = $"Travel home to {this.GetSceneDisplayName(home.Scene)}?";
            if (returnPoint != null && returnPoint.Scene != sceneId)
                question += $"\n\nWhen you travel back later, you'll arrive here instead of {this.GetSceneDisplayName(returnPoint.Scene)}.";

            InteractionHelper.ShowConfirmDialogue(
                question,
                () =>
                {
                    this.DestinationManager.SetDestination(DestinationType.ReturnPoint);
                    this.FastTravelTo(home);
                }
            );
            return;
        }

        // travel to return point
        if (returnPoint is null)
        {
            InteractionHelper.ShowMessageBox("You're already home, and haven't fast traveled yet.\n\nAfter you fast travel home at least once, you'll be able to fast travel back to your departure point.");
            return;
        }
        InteractionHelper.ShowConfirmDialogue(
            $"Travel back to {this.GetSceneDisplayName(returnPoint.Scene)}?",
            () => this.FastTravelTo(returnPoint)
        );
    }

    /// <summary>Whether the save is loaded and ready.</summary>
    private bool IsSaveLoaded()
    {
        return
            GameManager.m_Instance is not null
            && !GameManager.IsMainMenuActive()
            && GameManager.m_ActiveScene is not (null or "" or "MainMenu");
    }

    /// <summary>Fast travel to the given destination.</summary>
    /// <param name="destination">The destination to travel to.</param>
    private void FastTravelTo(Destination destination)
    {
        this.TravelingTo = destination;
        GameManager.LoadSceneWithLoadingScreen(destination.Scene);
    }

    /// <summary>Get the localized name for a scene.</summary>
    /// <param name="name">The internal scene name.</param>
    private string GetSceneDisplayName(string name)
    {
        return InterfaceManager.GetNameForScene(name);
    }
}
