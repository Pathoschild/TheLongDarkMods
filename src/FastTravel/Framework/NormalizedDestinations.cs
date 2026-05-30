using System;
using System.Collections.Generic;
using System.Linq;

namespace Pathoschild.TheLongDarkMods.FastTravel.Framework;

/// <summary>A normalized representation of a destination list.</summary>
internal class NormalizedDestinations
{
    /*********
    ** Fields
    *********/
    /// <summary>The mutable backing field for <see cref="FavoriteDestinations"/>.</summary>
    private readonly Dictionary<int, Destination> Favorites = [];

    /// <summary>The saved scenes, or <c>null</c> if it's not initialized yet.</summary>
    private HashSet<string>? SavedSceneNames;


    /*********
    ** Accessors
    *********/
    /// <summary>The player's favorite destinations, which are mapped to optional keybinds.</summary>
    public IReadOnlyDictionary<int, Destination> FavoriteDestinations => this.Favorites;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="destinations">The destination list to normalize.</param>
    public NormalizedDestinations(Dictionary<int, Destination> destinations)
    {
        this.SavedSceneNames = [];

        foreach ((int index, Destination destination) in destinations.OrderBy(p => p.Key))
        {
            this.Favorites[index] = destination;

            this.SavedSceneNames.Add(destination.Scene.Name);
        }
    }

    /// <summary>Save a destination to a favorite slot.</summary>
    /// <param name="index">The slot index to overwrite.</param>
    /// <param name="destination">The destination to save.</param>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is not within the favorites range.</exception>
    public void SaveFavorite(int index, Destination destination)
    {
        if (index is < 0 or >= ModConstants.MaxFavorites)
            throw new ArgumentOutOfRangeException(nameof(index));

        this.Favorites[index] = destination;
        this.SavedSceneNames = null;
    }

    /// <summary>Remove a destination.</summary>
    /// <param name="destination">The destination to remove.</param>
    public bool Remove(Destination destination)
    {
        foreach ((int index, Destination other) in this.Favorites)
        {
            if (object.ReferenceEquals(destination, other))
            {
                this.Favorites.Remove(index);
                return true;
            }
        }

        return false;
    }

    /// <summary>Get whether a scene contains any saved destinations.</summary>
    /// <param name="sceneName">The scene name to search.</param>
    public bool IsSavedScene(string sceneName)
    {
        if (this.SavedSceneNames is null)
        {
            this.SavedSceneNames = [];

            foreach (Destination destination in this.Favorites.Values)
                this.SavedSceneNames.Add(destination.Scene.Name);
        }

        return this.SavedSceneNames.Contains(sceneName);
    }
}
