using System.Text.Json;
using Il2Cpp;
using MelonLoader;
using ModData;
using Pathoschild.TheLongDarkMods.Common;
using Pathoschild.TheLongDarkMods.FastTravel.Framework.DataModels;
using UnityEngine;

namespace Pathoschild.TheLongDarkMods.FastTravel.Framework;

/// <summary>Tracks persisted destination endpoints.</summary>
internal class DestinationManager
{
    /*********
    ** Fields
    *********/
    /// <summary>The log instance.</summary>
    private readonly MelonLogger.Instance Log;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="log">The log instance.</param>
    public DestinationManager(MelonLogger.Instance log)
    {
        this.Log = log;
    }

    /// <summary>Get the saved data on disk.</summary>
    public SaveModel GetData()
    {
        ModDataManager dataManager = this.CreateDataManager();
        SaveModel? data = this.DeserializeRaw(dataManager.Load());

        return data ?? new SaveModel();
    }

    /// <summary>Save data back to disk.</summary>
    /// <param name="data">The data to save.</param>
    public void SaveData(SaveModel data)
    {
        this
            .CreateDataManager()
            .Save(this.Serialize(data));
    }

    /// <summary>Get the destination info for the player's current position.</summary>
    public Destination GetCurrentLocation()
    {
        vp_FPSCamera camera = GameManager.GetVpFPSCamera();
        Transform player = GameManager.GetPlayerObject().transform;

        return new Destination(
            region: SceneHelper.TryGetRegion(),
            scene: SceneHelper.GetScene(),
            position: player.position,
            cameraPitch: camera.m_Pitch,
            cameraYaw: camera.m_Yaw,
            lastTransition: GameManager.m_SceneTransitionData
        );
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Create a mod data manager.</summary>
    private ModDataManager CreateDataManager()
    {
        return new ModDataManager("FastTravel");
    }

    /// <summary>Deserialize raw data into the data model, if it's valid.</summary>
    /// <param name="rawData">The raw data to deserialize.</param>
    private SaveModel? DeserializeRaw(string? rawData)
    {
        if (rawData is not null)
        {
            try
            {
                SaveModel? data = JsonSerializer.Deserialize<SaveModel>(rawData);
                if (data?.Destinations != null)
                    return data;
            }
            catch (JsonException ex)
            {
                this.Log.Error("Can't load saved destinations; the data will be reset.", ex);
            }
        }

        return null;
    }

    /// <summary>Serialize a data model into raw data.</summary>
    /// <param name="data">The data to serialize.</param>
    private string Serialize(SaveModel data)
    {
        return JsonSerializer.Serialize(data);
    }
}
