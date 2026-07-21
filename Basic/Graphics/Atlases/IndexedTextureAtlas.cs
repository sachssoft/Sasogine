using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Resources;

/// <summary>
/// Represents a texture atlas with an additional strongly typed mapping
/// between enum values and texture atlas frames.
///
/// This class extends <see cref="TextureAtlas"/> and allows accessing
/// atlas frames using enum keys instead of string frame names.
///
/// This provides type-safe and efficient access for rendering and animation
/// systems where atlas entries are known at compile time.
///
/// The mapping between an enum value and a frame is created by referencing
/// an existing frame name from the base <see cref="TextureAtlas"/>.
/// </summary>
/// <typeparam name="TEnumKey">
/// The enum type used as the key for texture atlas frame mappings.
/// </typeparam>
public class IndexedTextureAtlas<TEnumKey> : TextureAtlas
    where TEnumKey : struct, Enum
{
    /// <summary>
    /// Stores the mapping between enum keys and their associated
    /// texture atlas frames.
    ///
    /// This dictionary is used internally while public access is provided
    /// through the strongly typed indexer.
    /// </summary>
    private readonly Dictionary<TEnumKey, TextureAtlasFrame> _enumFrames = new();


    /// <summary>
    /// Adds an existing frame from the base texture atlas and associates it
    /// with the specified enum key.
    /// </summary>
    /// <param name="key">
    /// The enum key used to access the frame.
    /// </param>
    /// <param name="frameName">
    /// The name of the existing frame in the base texture atlas.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the specified frame name does not exist in the base atlas.
    /// </exception>
    public void AddFrame(
        TEnumKey key,
        string frameName)
    {
        if (base.Frames.TryGetValue(frameName, out var frame))
        {
            _enumFrames[key] = frame;
            return;
        }

        throw new InvalidOperationException(
            $"Frame '{frameName}' not found in base TextureAtlas.");
    }


    /// <summary>
    /// Gets the texture atlas frame associated with the specified enum key.
    /// </summary>
    /// <param name="key">
    /// The enum key identifying the frame.
    /// </param>
    /// <returns>
    /// The associated <see cref="TextureAtlasFrame"/>.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no frame is registered for the specified key.
    /// </exception>
    public TextureAtlasFrame this[TEnumKey key]
        => _enumFrames[key];


    /// <summary>
    /// Gets all enum-based frame mappings as a read-only dictionary.
    /// </summary>
    public IReadOnlyDictionary<TEnumKey, TextureAtlasFrame> EnumFrames
        => _enumFrames;


    /// <summary>
    /// Creates a cropped texture from the frame associated with the specified
    /// enum key.
    /// </summary>
    /// <param name="key">
    /// The enum key identifying the frame to crop.
    /// </param>
    /// <returns>
    /// A new <see cref="Texture2D"/> containing the cropped frame texture.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no frame is registered for the specified key.
    /// </exception>
    public Texture2D Crop(TEnumKey key)
    {
        return Crop(_enumFrames[key].Name);
    }


    /// <summary>
    /// Attempts to create a cropped texture from the frame associated with
    /// the specified enum key.
    /// </summary>
    /// <param name="key">
    /// The enum key identifying the frame to crop.
    /// </param>
    /// <param name="croppedTexture">
    /// Receives the cropped <see cref="Texture2D"/> when successful;
    /// otherwise <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if cropping succeeded; otherwise <c>false</c>.
    /// </returns>
    public bool TryCrop(
        TEnumKey key,
        [MaybeNullWhen(false)] out Texture2D croppedTexture)
    {
        return TryCrop(
            _enumFrames[key].Name,
            out croppedTexture);
    }
}