using System.Text.Json;
using Il2Cpp;
using MelonLoader;
using ModData;
using UnityEngine;

namespace Pathoschild.TheLongDarkMods.ShortcutHome.Framework;

/// <summary>Tracks the destination endpoint which the player last warped from, if any.</summary>
internal class DestinationManager
{
    /*********
    ** Public methods
    *********/
    /// <summary>Get the last saved destination, if any.</summary>
    public Destination? GetDestination()
    {
        string? rawData = this.CreateDataManager().Load();

        if (rawData is not null)
        {
            try
            {
                DataModel? state = JsonSerializer.Deserialize<DataModel>(rawData);
                if (state?.Scene != null)
                    return new Destination(state.Scene, new Vector3(state.X, state.Y, state.Z));
            }
            catch (JsonException ex)
            {
                MelonLogger.Error("Can't restore saved warp info.", ex);
            }
        }

        return null;
    }

    /// <summary>Set the player's current position as the tracked destination.</summary>
    public void SetDestination()
    {
        Transform player = GameManager.GetPlayerObject().transform;

        var destination = new Destination(GameManager.m_ActiveScene, player.position);

        this.CreateDataManager().Save(
            JsonSerializer.Serialize(
                new DataModel(destination)
            )
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
}
