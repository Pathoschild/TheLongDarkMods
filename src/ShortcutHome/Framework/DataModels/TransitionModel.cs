using System.Text.Json.Serialization;
using Il2Cpp;

namespace Pathoschild.TheLongDarkMods.ShortcutHome.Framework.DataModels;

/// <summary>The metadata for a transition from one scene to another.</summary>
/// <remarks>This is a JSON-serializable equivalent to <see cref="SceneTransitionData"/>.</remarks>
internal class TransitionModel
{
    /*********
    ** Accessors
    *********/
    /// <summary>The scene ID we're departing from.</summary>
    /// <remarks>Equivalent to <see cref="SceneTransitionData.m_SceneSaveFilenameCurrent"/>.</remarks>
    public string FromSceneId { get; }

    /// <summary>The scene ID we're arriving in.</summary>
    /// <remarks>Equivalent to <see cref="SceneTransitionData.m_SceneSaveFilenameNextLoad"/>.</remarks>
    public string ToSceneId { get; }

    /// <summary>The named spawn point at which to arrive.</summary>
    /// <remarks>Equivalent to <see cref="SceneTransitionData.m_SpawnPointName"/>.</remarks>
    public string? ToSpawnPoint { get; }

    /// <summary>The audio to play on arrival.</summary>
    /// <remarks>Equivalent to <see cref="SceneTransitionData.m_SpawnPointAudio"/>.</remarks>
    public string? ToSpawnPointAudio { get; }

    /// <summary>Whether to restore the player's original position on arrival.</summary>
    /// <remarks>Equivalent to <see cref="SceneTransitionData.m_TeleportPlayerSaveGamePosition"/>.</remarks>
    public bool RestorePlayerPosition { get; }

    /// <summary>The last outdoor scene ID before the current location. This is used to return to the correct exterior when leaving instanced interiors.</summary>
    /// <remarks>Equivalent to <see cref="SceneTransitionData.m_LastOutdoorScene"/>.</remarks>
    public string? LastOutdoorScene { get; }

    /// <summary>The player position in the <see cref="LastOutdoorScene"/>.</summary>
    /// <remarks>Equivalent to <see cref="SceneTransitionData.m_PosBeforeInteriorLoad"/>.</remarks>
    public Vector3Model LastOutdoorPosition { get; }

    /// <summary>The save-specific seed used for randomization.</summary>
    /// <remarks>Equivalent to <see cref="SceneTransitionData.m_GameRandomSeed"/>.</remarks>
    public int GameRandomSeed { get; }

    /// <summary>Unknown. Possibly the location to return to on exit?</summary>
    /// <remarks>Equivalent to <see cref="SceneTransitionData.m_ForceNextSceneLoadTriggerScene"/>.</remarks>
    public string? ForceNextSceneLoadTriggerScene { get; }

    /// <summary>Unknown.</summary>
    /// <remarks>Equivalent to <see cref="SceneTransitionData.m_SceneLocationLocIDOverride"/>.</remarks>
    public string? SceneLocationLocIdOverride { get; }

    /// <summary>Unknown.</summary>
    /// <remarks>Equivalent to <see cref="SceneTransitionData.m_Location"/>.</remarks>
    public string? Location { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="fromSceneId"><inheritdoc cref="FromSceneId" path="/summary"/></param>
    /// <param name="toSceneId"><inheritdoc cref="ToSceneId" path="/summary"/></param>
    /// <param name="toSpawnPoint"><inheritdoc cref="ToSpawnPoint" path="/summary"/></param>
    /// <param name="toSpawnPointAudio"><inheritdoc cref="ToSpawnPointAudio" path="/summary"/></param>
    /// <param name="restorePlayerPosition"><inheritdoc cref="RestorePlayerPosition" path="/summary"/></param>
    /// <param name="lastOutdoorScene"><inheritdoc cref="LastOutdoorScene" path="/summary"/></param>
    /// <param name="lastOutdoorPosition"><inheritdoc cref="LastOutdoorPosition" path="/summary"/></param>
    /// <param name="gameRandomSeed"><inheritdoc cref="GameRandomSeed" path="/summary"/></param>
    /// <param name="forceNextSceneLoadTriggerScene"><inheritdoc cref="ForceNextSceneLoadTriggerScene" path="/summary"/></param>
    /// <param name="sceneLocationLocIdOverride"><inheritdoc cref="SceneLocationLocIdOverride" path="/summary"/></param>
    /// <param name="location"><inheritdoc cref="Location"/></param>
    [JsonConstructor]
    public TransitionModel(string fromSceneId, string toSceneId, string? toSpawnPoint, string? toSpawnPointAudio, bool restorePlayerPosition, string? lastOutdoorScene, Vector3Model lastOutdoorPosition, int gameRandomSeed, string? forceNextSceneLoadTriggerScene, string? sceneLocationLocIdOverride, string? location)
    {
        this.FromSceneId = fromSceneId;
        this.ToSceneId = toSceneId;
        this.ToSpawnPoint = toSpawnPoint;
        this.ToSpawnPointAudio = toSpawnPointAudio;
        this.RestorePlayerPosition = restorePlayerPosition;
        this.LastOutdoorScene = lastOutdoorScene;
        this.LastOutdoorPosition = lastOutdoorPosition;
        this.GameRandomSeed = gameRandomSeed;
        this.ForceNextSceneLoadTriggerScene = forceNextSceneLoadTriggerScene;
        this.SceneLocationLocIdOverride = sceneLocationLocIdOverride;
        this.Location = location;
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="transition">The transition to copy.</param>
    public TransitionModel(SceneTransitionData transition)
        : this(
            fromSceneId: transition.m_SceneSaveFilenameCurrent,
            toSceneId: transition.m_SceneSaveFilenameNextLoad,
            toSpawnPoint: transition.m_SpawnPointName,
            toSpawnPointAudio: transition.m_SpawnPointAudio,
            restorePlayerPosition: transition.m_TeleportPlayerSaveGamePosition,
            lastOutdoorScene: transition.m_LastOutdoorScene,
            lastOutdoorPosition: new Vector3Model(transition.m_PosBeforeInteriorLoad),
            gameRandomSeed: transition.m_GameRandomSeed,
            forceNextSceneLoadTriggerScene: transition.m_ForceNextSceneLoadTriggerScene,
            sceneLocationLocIdOverride: transition.m_SceneLocationLocIDOverride,
            location: transition.m_Location
        )
    { }
}
