using System.Text.Json;
using Il2Cpp;
using MelonLoader;
using ModData;
using UnityEngine;

namespace Pathoschild.TheLongDarkMods.ShortcutHome.Framework;

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

    /// <summary>Get the saved data.</summary>
    public DataModel GetData()
    {
        ModDataManager dataManager = this.CreateDataManager();
        DataModel? state = this.DeserializeRaw(dataManager.Load());

        return state?.Destinations != null
            ? state
            : new DataModel();
    }

    /// <summary>Set the player's current position as the tracked destination.</summary>
    /// <param name="type">The destination type to set.</param>
    public void SetDestination(DestinationType type)
    {
        vp_FPSCamera camera = GameManager.GetVpFPSCamera();
        Transform player = GameManager.GetPlayerObject().transform;
        var destination = new Destination(GameManager.m_ActiveScene, player.position, camera.m_Pitch, camera.m_Yaw);

        ModDataManager dataManager = this.CreateDataManager();
        DataModel data = this.DeserializeRaw(dataManager.Load()) ?? new DataModel();

        data.Destinations[type] = new DataDestinationModel(destination);

        dataManager.Save(
            this.Serialize(data)
        );
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Create a mod data manager.</summary>
    private ModDataManager CreateDataManager()
    {
        return new ModDataManager("ShortcutHome");
    }

    /// <summary>Deserialize raw data into the data model, if it's valid.</summary>
    /// <param name="rawData">The raw data to deserialize.</param>
    private DataModel? DeserializeRaw(string? rawData)
    {
        if (rawData is not null)
        {
            try
            {
                DataModel? state = JsonSerializer.Deserialize<DataModel>(rawData);
                if (state?.Destinations != null)
                    return state;
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
    private string Serialize(DataModel data)
    {
        return JsonSerializer.Serialize(data);
    }
}
