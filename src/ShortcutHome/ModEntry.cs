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

        // log debug info
        if (this.Config.LogDebugInfo)
        {
            Destination location = this.DestinationManager.GetCurrentLocation();

            this.Log.Msg(
                $"""
                Scene initialized:
                    buildIndex: {buildIndex}
                    sceneName: '{sceneName}'

                    location: {location}
                    save name: '{SaveGameSystem.GetCurrentSaveName()}'
                    m_SceneWasRestored: {GameManager.m_SceneWasRestored}

                    Unity scene:
                        name: {location.Scene.Name}
                        guid: {location.Scene.Guid}
                        path: {location.Scene.Path}
                        isSubScene: {location.Scene.IsSubScene}

                    TravelingTo: {this.TravelingTo?.ToString() ?? "null"}

                {this.GetTransitionDebugSummary("transition", location.LastTransition)}
                """
            );
        }

        // update position after travel
        Destination? destination = this.TravelingTo;
        if (destination is not null)
        {
            if (sceneName != destination.Scene.Name)
                this.Log.Warning($"Failed setting position after warp back: arrived in scene '{sceneName}' instead of the expected '{destination.Scene.Name}'.");
            else
            {
                Transform player = GameManager.GetPlayerObject().transform;
                vp_FPSCamera camera = GameManager.GetVpFPSCamera();

                player.position = destination.Position.ToVector3();

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
        if (savedHome != null && savedHome.Scene.Name == sceneId)
        {
            this.InteractionHelper.ShowConfirmDialogue(
                "This is already home! Do you want to update the arrival position?",
                () => this.DestinationManager.SetDestination(DestinationType.Home)
            );
        }

        // else replace home
        else
        {
            string question = $"Set {SceneHelper.GetSceneDisplayName(sceneId)} as your home?";
            if (savedHome != null)
                question += $"\n\nThis will replace your previous home ({SceneHelper.GetSceneDisplayName(savedHome.Scene.Name)}).";

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
        if (home.Scene.Name != sceneId)
        {
            string question = $"Travel home to {SceneHelper.GetSceneDisplayName(home.Scene.Name)}?";
            if (returnPoint != null && returnPoint.Scene.Name != sceneId)
                question += $"\n\nWhen you travel back later, you'll arrive here instead of {SceneHelper.GetSceneDisplayName(returnPoint.Scene.Name)}.";

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
            $"Travel back to {SceneHelper.GetSceneDisplayName(returnPoint.Scene.Name)}?",
            () => this.FastTravelTo(returnPoint)
        );
    }

    /// <summary>Fast travel to the given destination.</summary>
    /// <param name="destination">The destination to travel to.</param>
    private void FastTravelTo(Destination destination)
    {
        // trigger autosave
        // (This is needed to persist any changes made to the location; otherwise they'd be discarded when we leave.)
        string currentSceneName = SceneHelper.GetSceneName();
        SaveGameSystem.SaveGame("autosave", currentSceneName);

        // fade out and warp
        CameraFade.FadeOut(
            time: GameManager.m_SceneTransitionFadeOutTime,
            onFadeFinished: (System.Action)(() =>
            {
                TransitionModel original = destination.LastTransition;

                // recreate saved transition
                GameManager.m_SceneTransitionData = new SceneTransitionData
                {
                    m_SceneSaveFilenameCurrent = original.FromSceneId, // note: deliberately reuse original departure point (not our current scene) to avoid confusing the game
                    m_SceneSaveFilenameNextLoad = original.ToSceneId,
                    m_ForceNextSceneLoadTriggerScene = original.ForceNextSceneLoadTriggerScene,
                    m_SceneLocationLocIDOverride = original.SceneLocationLocIdOverride,
                    m_GameRandomSeed = original.GameRandomSeed,
                    m_Location = original.Location,
                    m_LastOutdoorScene = original.LastOutdoorScene,
                    m_PosBeforeInteriorLoad = original.LastOutdoorPosition.ToVector3(),
                    m_TeleportPlayerSaveGamePosition = true // mark as normal transition (e.g. not a new-game spawn)

                    // deliberately don't set m_SpawnPointName/m_SpawnPointAudio, since we want to restore the saved position
                };

                // log debug info
                if (this.Config.LogDebugInfo)
                {
                    this.Log.Msg(
                        $"""
                        Starting fast travel:
                            from scene: '{currentSceneName}'
                            save name: '{SaveGameSystem.GetCurrentSaveName()}'

                            destination: {destination}

                        {this.GetTransitionDebugSummary("transition", new TransitionModel(GameManager.m_SceneTransitionData))}
                        """
                    );
                }

                // start transition
                this.TravelingTo = destination;
                GameManager.LoadScene(destination.Scene.Name, SaveGameSystem.GetCurrentSaveName()); // need to load the Unity scene name; the game will get the instance ID from the transition data
            })
        );
    }

    /// <summary>Get a debug log representation of a scene transition.</summary>
    /// <param name="label">The label for the section.</param>
    /// <param name="transition">The scene transition to dump.</param>
    /// <param name="indent">The left indent with which to prefix each line.</param>
    private string GetTransitionDebugSummary(string label, TransitionModel transition, string indent = "    ")
    {
        return $"""
        {indent}{label}:
        {indent}    FromSceneId: {transition.FromSceneId ?? "<null>"}
        {indent}    ToSceneId: {transition.ToSceneId ?? "<null>"}
        {indent}    ToSpawnPoint: {transition.ToSpawnPoint ?? "<null>"}
        {indent}    ToSpawnPointAudio: {transition.ToSpawnPointAudio ?? "<null>"}
        {indent}    RestorePlayerPosition: {transition.RestorePlayerPosition}
        {indent}    LastOutdoorScene: {transition.LastOutdoorScene ?? "<null>"}
        {indent}    LastOutdoorPosition: {transition.LastOutdoorPosition}
        {indent}    GameRandomSeed: {transition.GameRandomSeed}
        {indent}    ForceNextSceneLoadTriggerScene: {transition.ForceNextSceneLoadTriggerScene ?? "<null>"}
        {indent}    SceneLocationLocIdOverride: {transition.SceneLocationLocIdOverride ?? "<null>"}
        {indent}    Location: {transition.Location ?? "<null>"}
        """;
    }
}
