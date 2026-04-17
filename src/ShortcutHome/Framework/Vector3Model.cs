using System.Text.Json.Serialization;
using UnityEngine;

namespace Pathoschild.TheLongDarkMods.ShortcutHome.Framework;

/// <summary>A JSON-serializable representation of a <see cref="Vector3"/> value.</summary>
internal struct Vector3Model
{
    /*********
    ** Accessors
    *********/
    /// <summary>The X position.</summary>
    public float X { get; }

    /// <summary>The Y position.</summary>
    public float Y { get; }

    /// <summary>The Z position.</summary>
    public float Z { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="x"><inheritdoc cref="X" path="/summary"/></param>
    /// <param name="y"><inheritdoc cref="Y" path="/summary"/></param>
    /// <param name="z"><inheritdoc cref="Z" path="/summary"/></param>
    [JsonConstructor]
    public Vector3Model(float x, float y, float z)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="vector">The vector to copy.</param>
    public Vector3Model(Vector3 vector)
        : this(vector.x, vector.y, vector.z) { }

    /// <summary>Get a Unity equivalent to this model.</summary>
    public Vector3 ToVector3()
    {
        return new Vector3(this.X, this.Y, this.Z);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"({this.X}, {this.Y}, {this.Z})";
    }
}
