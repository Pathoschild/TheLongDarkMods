using System.Text.Json.Serialization;
using UnityEngine.SceneManagement;

namespace Pathoschild.TheLongDarkMods.FastTravel.Framework.DataModels;

/// <summary>The metadata for a game scene (e.g. a location).</summary>
/// <remarks>This is a JSON-serializable equivalent to <see cref="Scene"/>.</remarks>
internal class SceneModel
{
    /*********
    ** Accessors
    *********/
    /// <summary>The internal scene ID.</summary>
    /// <remarks>For instanced interiors, this is the generic ID (like <c>LakeCabinE</c>) rather than the instanced ID (like <c>LakeCabinE_5da728bb-6b89-4969-913b-d8e3861db68a</c>).</remarks>
    public string Name { get; }

    /// <summary>Unknown.</summary>
    /// <remarks>This always seems to be <c>00000000000000000000000000000000</c>, and doesn't match the instanced interior GUID.</remarks>
    public string Guid { get; }

    /// <summary>The relative path to the scene file in Unity (like <c>Assets/Scenes/_Interiors/LakeCabinE/LakeCabinE.unity</c>).</summary>
    public string Path { get; }

    /// <summary>Unknown.</summary>
    /// <remarks>This always seems to be false.</remarks>
    public bool IsSubScene { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="name"><inheritdoc cref="Name" path="/summary"/></param>
    /// <param name="guid"><inheritdoc cref="Guid" path="/summary"/></param>
    /// <param name="path"><inheritdoc cref="Path" path="/summary"/></param>
    /// <param name="isSubScene"><inheritdoc cref="IsSubScene" path="/summary"/></param>
    [JsonConstructor]
    public SceneModel(string name, string guid, string path, bool isSubScene)
    {
        this.Name = name;
        this.Guid = guid;
        this.Path = path;
        this.IsSubScene = isSubScene;
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="scene">The scene to copy.</param>
    public SceneModel(Scene scene)
        : this(scene.name, scene.guid, scene.path, scene.isSubScene) { }
}
