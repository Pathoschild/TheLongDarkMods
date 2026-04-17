using System.Text.Json.Serialization;
using Il2Cpp;
using Pathoschild.TheLongDarkMods.ShortcutHome.Framework.DataModels;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pathoschild.TheLongDarkMods.ShortcutHome.Framework;

/// <summary>The metadata for a player position in the world.</summary>
/// <remarks>This deliberately stores more info than strictly needed, so that we can use other fields in future versions without breaking save changes.</remarks>
internal class Destination
{
    /*********
    ** Accessors
    *********/
    /// <summary>The scene info for the location.</summary>
    public SceneModel Scene { get; }

    /// <summary>The three-dimensional position within the scene.</summary>
    public Vector3Model Position { get; }

    /// <summary>The camera's vertical rotation angle in degrees.</summary>
    public float CameraPitch { get; }

    /// <summary>The camera's horizontal rotation angle in degrees.</summary>
    public float CameraYaw { get; }

    /// <summary>The transition which led to this location.</summary>
    public TransitionModel LastTransition { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="scene"><inheritdoc cref="Scene" path="/summary"/></param>
    /// <param name="position"><inheritdoc cref="Position" path="/summary"/></param>
    /// <param name="cameraPitch"><inheritdoc cref="CameraPitch" path="/summary"/></param>
    /// <param name="cameraYaw"><inheritdoc cref="CameraYaw" path="/summary"/></param>
    /// <param name="lastTransition"><inheritdoc cref="LastTransition" path="/summary"/></param>
    [JsonConstructor]
    public Destination(SceneModel scene, Vector3Model position, float cameraPitch, float cameraYaw, TransitionModel lastTransition)
    {
        this.Scene = scene;
        this.Position = position;
        this.CameraPitch = cameraPitch;
        this.CameraYaw = cameraYaw;
        this.LastTransition = lastTransition;
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="scene"><inheritdoc cref="Scene" path="/summary"/></param>
    /// <param name="position"><inheritdoc cref="Position" path="/summary"/></param>
    /// <param name="cameraPitch"><inheritdoc cref="CameraPitch" path="/summary"/></param>
    /// <param name="cameraYaw"><inheritdoc cref="CameraYaw" path="/summary"/></param>
    /// <param name="lastTransition"><inheritdoc cref="LastTransition" path="/summary"/></param>
    public Destination(Scene scene, Vector3 position, float cameraPitch, float cameraYaw, SceneTransitionData lastTransition)
        : this(new SceneModel(scene), new Vector3Model(position), cameraPitch, cameraYaw, new TransitionModel(lastTransition)) { }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"(scene: '{this.Scene.Name}', position: {this.Position}, pitch: {this.CameraPitch}, yaw: {this.CameraYaw}, lastTransition: from '{this.LastTransition.FromSceneId}' to '{this.LastTransition.ToSceneId}')";
    }
}
