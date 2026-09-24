using Sachssoft.Engine.Assets.Audio;
using Sachssoft.Engine.Assets.Data;
using Sachssoft.Engine.Assets.Graphics;
using Sachssoft.Engine;

namespace Sachssoft.Engine.Assets;

/// <summary>
/// Provides a registry for creating assets from asset definitions.
/// </summary>
/// <remarks>
/// Built-in asset types are registered automatically by the registry.
/// Indexed frame set assets use integer indices and can therefore be registered
/// as built-in assets without requiring an application-specific enum type.
/// Strongly typed enum access can be applied separately at runtime.
/// </remarks>
public sealed class DefinitionAssetRegistry :
    DefinitionObjectRegistry<IAssetDefinition, IAsset>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefinitionAssetRegistry"/>
    /// class and registers the built-in asset types.
    /// </summary>
    public DefinitionAssetRegistry()
    {
        // Audio
        RegisterBuiltIn<MusicAssetDefinition, MusicAsset>(
            static definition => new MusicAsset(definition));

        RegisterBuiltIn<SoundAssetDefinition, SoundAsset>(
            static definition => new SoundAsset(definition));

        // Data
        RegisterBuiltIn<IndexedFrameSetDefinition, IndexedFrameSetAsset>(
            static definition => new IndexedFrameSetAsset(definition));

        RegisterBuiltIn<KeyedFrameSetAssetDefinition, KeyedFrameSetAsset>(
            static definition => new KeyedFrameSetAsset(definition));

        // Graphics
        RegisterBuiltIn<FontAssetDefinition, FontAsset>(
            static definition => new FontAsset(definition));

        RegisterBuiltIn<ModelAssetDefinition, ModelAsset>(
            static definition => new ModelAsset(definition));

        RegisterBuiltIn<ShaderAssetDefinition, ShaderAsset>(
            static definition => new ShaderAsset(definition));

        RegisterBuiltIn<Texture2DAssetDefinition, Texture2DAsset>(
            static definition => new Texture2DAsset(definition));

        RegisterBuiltIn<TextureGridAssetDefinition, TextureGridAsset>(
            static definition => new TextureGridAsset(definition));
    }
}