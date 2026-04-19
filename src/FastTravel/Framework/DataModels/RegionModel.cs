using System.Text.Json.Serialization;
using Il2Cpp;
using Il2CppTLD.Scenes;

namespace Pathoschild.TheLongDarkMods.FastTravel.Framework.DataModels;

/// <summary>The metadata for a game region.</summary>
/// <remarks>This is a JSON-serializable equivalent to <see cref="RegionSpecification"/>.</remarks>
internal class RegionModel
{
    /*********
    ** Accessors
    *********/
    /// <summary>The internal region ID, like <c>TracksRegion</c>.</summary>
    public string Id { get; }

    /// <summary>The name localization ID, like <c>SCENENAME_Railroad</c>.</summary>
    public string NameLocalizationId { get; }

    /// <summary>The region display name at the time this model was saved, like <c>Broken Railroad</c>.</summary>
    /// <remarks>Most code should call <see cref="Localization.Get"/> with the <see cref="NameLocalizationId"/> instead.</remarks>
    public string Name { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="id"><inheritdoc cref="Id" path="/summary"/></param>
    /// <param name="nameLocalizationId"><inheritdoc cref="NameLocalizationId" path="/summary"/></param>
    /// <param name="name"><inheritdoc cref="Name" path="/summary"/></param>
    [JsonConstructor]
    public RegionModel(string id, string nameLocalizationId, string name)
    {
        this.Id = id;
        this.NameLocalizationId = nameLocalizationId;
        this.Name = name;
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="region">The region to copy.</param>
    public RegionModel(RegionSpecification region)
        : this(region.GetName(), region.ZoneNameId, region.ZoneName) { }
}
