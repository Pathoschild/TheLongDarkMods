using Il2Cpp;
using MelonLoader;
using Pathoschild.TheLongDarkMods.Common;
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

    /// <summary>The log instance.</summary>
    private MelonLogger.Instance Log = null!; // set in OnInitializeMelon

    /// <summary>Tracks the persisted fast travel destinations.</summary>
    private DestinationManager DestinationManager = null!; // set in OnInitializeMelon

    /// <summary>Provides utility methods for reading input and showing UI.</summary>
    private InteractionHelper InteractionHelper = null!; // set in OnInitializeMelon

    /// <summary>The destination which the player is currently traveling to, if applicable.</summary>
    private Destination? TravelingTo;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void OnInitializeMelon()
    {
        this.Log = Melon<ModEntry>.Logger;
        this.DestinationManager = new DestinationManager(this.Log);
        this.InteractionHelper = new InteractionHelper(this.Log);

        this.Config.AddToModSettings(ModInfo.DisplayName);
    }

    /// <inheritdoc />
    public override void OnUpdate()
    {
        if (this.InteractionHelper.IsKeyDown(this.Config.SetHomeKey) && SceneHelper.IsSaveLoaded())
            this.OnInteractivelySetHome();

        else if (this.InteractionHelper.IsKeyDown(this.Config.FastTravelKey) && SceneHelper.IsSaveLoaded())
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
                this.Log.Warning($"Failed setting position after warp back: arrived in scene '{sceneName}' instead of the expected '{destination.Scene}'.");
            else
            {
                Transform player = GameManager.GetPlayerObject().transform;
                vp_FPSCamera camera = GameManager.GetVpFPSCamera();

                player.position = destination.Position;

                camera.m_Pitch = destination.CameraPitch;
                camera.m_TargetPitch = destination.CameraPitch;
                camera.m_CurrentPitch = destination.CameraPitch;

                camera.m_Yaw = destination.CameraYaw;
                camera.m_TargetYaw = destination.CameraYaw;
                camera.m_CurrentYaw = destination.CameraYaw;
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
        string sceneId = SceneHelper.GetSceneName();
        Destination? savedHome = this.DestinationManager.GetData().GetHome();

        // update position in same home
        if (savedHome != null && savedHome.Scene == sceneId)
        {
            this.InteractionHelper.ShowConfirmDialogue(
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

            this.InteractionHelper.ShowConfirmDialogue(
                question,
                () => this.DestinationManager.SetDestination(DestinationType.Home)
            );
        }
    }

    /// <summary>Handle the player requesting to fast travel.</summary>
    private void OnInteractivelyFastTravel()
    {
        string sceneId = SceneHelper.GetSceneName();
        DataModel data = this.DestinationManager.GetData();
        Destination? home = data.GetHome();
        Destination? returnPoint = data.GetReturnPoint();

        // not set up yet
        if (home is null)
        {
            this.InteractionHelper.ShowMessageBox($"You haven't set your home yet.\n\nPress {this.Config.SetHomeKey} to set your current location as home.");
            return;
        }

        // travel home
        if (home.Scene != sceneId)
        {
            string question = $"Travel home to {this.GetSceneDisplayName(home.Scene)}?";
            if (returnPoint != null && returnPoint.Scene != sceneId)
                question += $"\n\nWhen you travel back later, you'll arrive here instead of {this.GetSceneDisplayName(returnPoint.Scene)}.";

            this.InteractionHelper.ShowConfirmDialogue(
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
            this.InteractionHelper.ShowMessageBox("You're already home, and haven't fast traveled yet.\n\nAfter you fast travel home at least once, you'll be able to fast travel back to your departure point.");
            return;
        }
        this.InteractionHelper.ShowConfirmDialogue(
            $"Travel back to {this.GetSceneDisplayName(returnPoint.Scene)}?",
            () => this.FastTravelTo(returnPoint)
        );
    }

    /// <summary>Fast travel to the given destination.</summary>
    /// <param name="destination">The destination to travel to.</param>
    private void FastTravelTo(Destination destination)
    {
        // collect info
        Transform player = GameManager.GetPlayerObject().transform;
        string fromSceneId = SceneHelper.GetSceneName();
        bool fromOutside = GameManager.IsOutDoorsScene(fromSceneId);

        // trigger autosave
        // (This is needed to persist any changes made to the location; otherwise they'd be discarded when we leave.)
        SaveGameSystem.SaveGame("autosave", fromSceneId);

        // fade out and warp
        CameraFade.FadeOut(
            time: GameManager.m_SceneTransitionFadeOutTime,
            onFadeFinished: (System.Action)(() =>
            {
                var transition = new SceneTransitionData
                {
                    m_SceneSaveFilenameCurrent = fromSceneId,
                    m_SceneSaveFilenameNextLoad = destination.Scene,
                    m_TeleportPlayerSaveGamePosition = true // mark as normal transition (e.g. not a new-game spawn)
                };

                if (GameManager.m_SceneTransitionData is { } prevTransition)
                {
                    transition.m_PosBeforeInteriorLoad = prevTransition.m_PosBeforeInteriorLoad;
                    transition.m_GameRandomSeed = prevTransition.m_GameRandomSeed;
                    transition.m_LastOutdoorScene = prevTransition.m_LastOutdoorScene;
                }

                if (fromOutside)
                {
                    transition.m_PosBeforeInteriorLoad = player.position;
                    transition.m_LastOutdoorScene = fromSceneId;
                }

                GameManager.m_SceneTransitionData = transition;

                this.TravelingTo = destination;
                GameManager.LoadScene(destination.Scene, SaveGameSystem.GetCurrentSaveName());
            })
        );
    }

    /// <summary>Get the localized name for a scene.</summary>
    /// <param name="name">The internal scene name.</param>
    private string GetSceneDisplayName(string name)
    {
        return InterfaceManager.GetNameForScene(name);
    }
}
