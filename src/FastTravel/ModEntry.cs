using System;
using System.Diagnostics.CodeAnalysis;
using Il2Cpp;
using MelonLoader;
using Pathoschild.TheLongDarkMods.Common;
using Pathoschild.TheLongDarkMods.FastTravel.Framework;
using Pathoschild.TheLongDarkMods.FastTravel.Framework.DataModels;
using UnityEngine;

namespace Pathoschild.TheLongDarkMods.FastTravel;

/// <inheritdoc />
public class ModEntry : MelonMod
{
    /*********
    ** Fields
    *********/
    /// <summary>The maximum number of destinations which the player can save.</summary>
    private const int MaxDestinations = 9;

    /// <summary>The mod settings.</summary>
    private readonly ModConfig Config = new();

    /// <summary>The log instance.</summary>
    private MelonLogger.Instance Log = null!; // set in OnInitializeMelon

    /// <summary>Tracks the persisted destinations.</summary>
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
        if (InputManager.HasPressedKey() && SceneHelper.IsSaveLoaded())
        {
            if (this.InteractionHelper.IsKeyJustPressed(this.Config.ReturnPointKey))
                this.InteractivelyReturn();
            else
            {
                for (int i = 0; i < MaxDestinations; i++)
                {
                    // skip if not pressed
                    KeyCode key = this.GetKeyForSlot(i);
                    if (!this.InteractionHelper.IsKeyJustPressed(key))
                        continue;

                    // apply
                    if (this.InteractionHelper.IsKeyDown(this.Config.SaveModifierKey))
                        this.InteractivelySave(i);
                    else if (this.InteractionHelper.IsKeyDown(this.Config.DeleteModifierKey))
                        this.InteractivelyDelete(i);
                    else
                        this.InteractivelyFastTravel(i);
                    break;
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
    /// <summary>Delete a destination with player interaction.</summary>
    /// <param name="slotIndex">The destination index.</param>
    private void InteractivelyDelete(int slotIndex)
    {
        if (!this.Config.CanEditDestinations)
        {
            this.Log.Warning("Can't edit fast travel destinations (per your mod settings).");
            return;
        }

        SaveModel data = this.DestinationManager.GetData();
        Destination? slot = data.Get(slotIndex);

        if (slot is null)
            return; // nothing to delete

        this.InteractionHelper.ShowConfirmDialogue(
            $"Do you want to forget fast travel point {slotIndex + 1} ({slot.GetDisplayName()})?",
            () =>
            {
                data.Set(slotIndex, null);
                this.DestinationManager.SaveData(data);
            }
        );
    }

    /// <summary>Save a destination with player interaction.</summary>
    /// <param name="slotIndex">The destination index.</param>
    private void InteractivelySave(int slotIndex)
    {
        // check restriction
        if (!this.Config.CanEditDestinations)
        {
            this.Log.Warning("Can't edit fast travel destinations (per your mod settings).");
            return;
        }

        // apply
        SaveModel data = this.DestinationManager.GetData();
        Destination here = this.DestinationManager.GetCurrentLocation();
        Destination? slot = data.Get(slotIndex);

        string question = $"Save {here.GetDisplayName()} as fast travel point {slotIndex + 1}?";
        if (slot != null)
            question += $"\n\nThis will replace your previous saved point ({slot.GetDisplayName()}).";

        this.InteractionHelper.ShowConfirmDialogue(
            question,
            () =>
            {
                data.Set(slotIndex, here);
                this.DestinationManager.SaveData(data);
            }
        );
    }

    /// <summary>Fast travel to a saved destination with player interaction.</summary>
    /// <param name="slotIndex">The destination index.</param>
    private void InteractivelyFastTravel(int slotIndex)
    {
        SaveModel data = this.DestinationManager.GetData();
        Destination here = this.DestinationManager.GetCurrentLocation();
        Destination? destination = data.Get(slotIndex);
        Destination? returnPoint = data.ReturnPoint;

        // check restrictions
        if (this.HasFastTravelRestrictions(here, destination, out string? reasonPhrase))
        {
            this.Log.Warning($"Can't fast travel {reasonPhrase} (per your mod settings).");
            return;
        }

        // not set yet
        if (destination is null)
        {
            string message = $"You haven't saved anywhere as fast travel point {slotIndex + 1} yet.";
            if (this.Config.ShowUsageHints)
                message += $"\n\nPress {this.Config.SaveModifierKey} + {this.GetKeyForSlot(slotIndex)} to save your current location to it.";

            this.InteractionHelper.ShowMessageBox(message);
            return;
        }

        // else travel
        string question = $"Travel to {destination.GetDisplayName()}?";
        if (this.Config.ReturnPointKey != KeyCode.None)
        {
            if (returnPoint != null && returnPoint.Scene.Name != here.Scene.Name)
                question += $"\n\nThis will replace your previous return point ({returnPoint.GetDisplayName()}).";
            if (this.Config.ShowUsageHints)
                question += $"\n\nYou can return here later by pressing {this.Config.ReturnPointKey}.";
        }

        this.InteractionHelper.ShowConfirmDialogue(
            question,
            () =>
            {
                data.ReturnPoint = here;
                this.DestinationManager.SaveData(data);
                this.FastTravelTo(destination);
            }
        );
    }

    /// <summary>Handle the player requesting to fast travel to their last return point.</summary>
    private void InteractivelyReturn()
    {
        SaveModel data = this.DestinationManager.GetData();
        Destination here = this.DestinationManager.GetCurrentLocation();
        Destination? returnPoint = data.ReturnPoint;

        if (returnPoint is null)
        {
            string message = "You haven't fast traveled anywhere yet.";
            if (this.Config.ShowUsageHints)
                message += $"\n\nAfter you fast travel at least once, you'll be able to return to your departure point by pressing {this.Config.ReturnPointKey}.";

            this.InteractionHelper.ShowMessageBox(message);
            return;
        }

        string question = $"Travel back to {returnPoint.GetDisplayName()}?";
        if (here.Scene.Name != returnPoint.Scene.Name)
            question += $"\n\nThis will set {here.GetDisplayName()} as your new return point.";

        this.InteractionHelper.ShowConfirmDialogue(
            question,
            () =>
            {
                data.ReturnPoint = here;
                this.DestinationManager.SaveData(data);
                this.FastTravelTo(returnPoint);
            }
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
            onFadeFinished: (Action)(() =>
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

    /// <summary>Get the key bound to a given fast travel slot.</summary>
    /// <param name="slotIndex">The fast travel slot index.</param>
    private KeyCode GetKeyForSlot(int slotIndex)
    {
        return slotIndex switch
        {
            0 => this.Config.Destination1,
            1 => this.Config.Destination2,
            2 => this.Config.Destination3,
            3 => this.Config.Destination4,
            4 => this.Config.Destination5,
            5 => this.Config.Destination6,
            6 => this.Config.Destination7,
            7 => this.Config.Destination8,
            8 => this.Config.Destination9,
            _ => throw new InvalidOperationException($"Unsupported destination slot {slotIndex}.")
        };
    }

    /// <summary>Get whether the player's mod settings prohibit a fast travel.</summary>
    /// <param name="from">The scene from which the player would travel.</param>
    /// <param name="to">The scene in which the player would arrive.</param>
    /// <param name="restrictionPhrase">If restrictions are in place, a phrase which can fit in the sentence <c>Can't fast travel {0}</c>.</param>
    /// <returns>Returns whether restrictions prohibit this fast travel.</returns>
    private bool HasFastTravelRestrictions(Destination from, Destination? to, [NotNullWhen(true)] out string? restrictionPhrase)
    {
        // disabled
        if (!this.Config.CanTravel)
        {
            restrictionPhrase = "at all";
            return true;
        }

        // travel from
        if (!this.Config.CanTravelFromOutside || !this.Config.CanTravelFromNonSafehouseInterior)
        {
            bool isOutdoors = GameManager.IsOutDoorsScene(from.Scene.Name);

            if (!this.Config.CanTravelFromOutside && isOutdoors)
            {
                restrictionPhrase = "from outside";
                return true;
            }

            if (!this.Config.CanTravelFromNonSafehouseInterior && !isOutdoors && !GameManager.GetSafehouseManager().InCustomizableSafehouse())
            {
                restrictionPhrase = "from non-safehouse interior";
                return true;
            }
        }

        // travel to same scene
        if (!this.Config.CanTravelToSameScene && from.Scene.Name == to?.Scene.Name)
        {
            restrictionPhrase = "to the same location";
            return true;
        }

        restrictionPhrase = null;
        return false;
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
