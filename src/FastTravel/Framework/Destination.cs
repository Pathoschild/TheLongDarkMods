using System.Text.Json.Serialization;
using Il2Cpp;
using Il2CppTLD.Scenes;
using Pathoschild.TheLongDarkMods.Common;
using Pathoschild.TheLongDarkMods.FastTravel.Framework.DataModels;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pathoschild.TheLongDarkMods.FastTravel.Framework;

/// <summary>The metadata for a player position in the world.</summary>
/// <remarks>This deliberately stores more info than strictly needed, so that we can use other fields in future versions without breaking save changes.</remarks>
internal class Destination
{
    /*********
    ** Accessors
    *********/
    /// <summary>The region containing the location, if found.</summary>
    public RegionModel? Region { get; }

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
    /// <param name="region"><inheritdoc cref="Region" path="/summary"/></param>
    /// <param name="scene"><inheritdoc cref="Scene" path="/summary"/></param>
    /// <param name="position"><inheritdoc cref="Position" path="/summary"/></param>
    /// <param name="cameraPitch"><inheritdoc cref="CameraPitch" path="/summary"/></param>
    /// <param name="cameraYaw"><inheritdoc cref="CameraYaw" path="/summary"/></param>
    /// <param name="lastTransition"><inheritdoc cref="LastTransition" path="/summary"/></param>
    [JsonConstructor]
    public Destination(RegionModel? region, SceneModel scene, Vector3Model position, float cameraPitch, float cameraYaw, TransitionModel lastTransition)
    {
        this.Region = region;
        this.Scene = scene;
        this.Position = position;
        this.CameraPitch = cameraPitch;
        this.CameraYaw = cameraYaw;
        this.LastTransition = lastTransition;
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="region"><inheritdoc cref="Region" path="/summary"/></param>
    /// <param name="scene"><inheritdoc cref="Scene" path="/summary"/></param>
    /// <param name="position"><inheritdoc cref="Position" path="/summary"/></param>
    /// <param name="cameraPitch"><inheritdoc cref="CameraPitch" path="/summary"/></param>
    /// <param name="cameraYaw"><inheritdoc cref="CameraYaw" path="/summary"/></param>
    /// <param name="lastTransition"><inheritdoc cref="LastTransition" path="/summary"/></param>
    public Destination(RegionSpecification? region, Scene scene, Vector3 position, float cameraPitch, float cameraYaw, SceneTransitionData lastTransition)
        : this(
            region: region != null ? new RegionModel(region) : null,
            scene: new SceneModel(scene),
            position: new Vector3Model(position),
            cameraPitch: cameraPitch,
            cameraYaw: cameraYaw,
            lastTransition: new TransitionModel(lastTransition)
        )
    { }

    /// <summary>Get the destination's translated display name, including the region if it's not the one containing the player.</summary>
    public string GetDisplayName()
    {
        string name = SceneHelper.GetDisplayName(this.Scene.Name);

        if (this.Region != null && !SceneHelper.IsOutdoors(this.Scene.Name) && this.Region.Id != SceneHelper.TryGetRegion()?.GetName())
            name += $" in {Localization.Get(this.Region.NameLocalizationId)}"; // don't use `this.LastTransition.LastOutdoorScene`, since it sometimes shows the wrong location

        return name;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"(scene: '{this.Scene.Name}' in '{this.Region?.Name ?? "<unknown>"}', position: {this.Position}, camera: ({this.CameraPitch}, {this.CameraYaw}), lastTransition: from '{this.LastTransition.FromSceneId}' to '{this.LastTransition.ToSceneId}')";
    }
}
