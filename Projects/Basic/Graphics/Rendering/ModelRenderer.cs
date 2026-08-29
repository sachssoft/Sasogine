using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Cameras;
using Sachssoft.Sasogine.Scenes;
using System;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    /// <summary>
    /// Provides rendering functionality for a <see cref="Model"/>.
    /// </summary>
    public sealed class ModelRenderer
    {
        private readonly Model _model;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelRenderer"/> class.
        /// </summary>
        /// <param name="model">The model to render.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="model"/> is <see langword="null"/>.
        /// </exception>
        public ModelRenderer(Model model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        /// <summary>
        /// Gets or sets the world transformation matrix of the model.
        /// </summary>
        public Matrix World { get; set; } = Matrix.Identity;

        /// <summary>
        /// Gets or sets an optional callback that is invoked before a shader is applied.
        /// </summary>
        public Action<IShader, ICamera, Matrix?>? ShaderSetupCallback { get; set; }

        /// <summary>
        /// Draws the model using the specified drawing context.
        /// </summary>
        /// <param name="context">The drawing context used to render the model.</param>
        /// <param name="transform">
        /// An optional transformation matrix applied in addition to <see cref="World"/>.
        /// </param>
        /// <param name="camera">
        /// The camera used for rendering. If <see langword="null"/>, the camera from
        /// <paramref name="context"/> is used.
        /// </param>
        /// <param name="customEffect">
        /// An optional custom shader used for rendering.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="context"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no camera is available for rendering.
        /// </exception>
        public void Draw(
            SceneDrawContext context,
            Matrix? transform = null,
            ICamera? camera = null,
            IShader? customEffect = null)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var cam = camera ?? context.ViewCamera
                ?? throw new InvalidOperationException("No camera available.");

            var graphics = context.GraphicsDevice;

            Matrix finalWorld = (transform ?? Matrix.Identity) * World;

            foreach (var mesh in _model.Meshes)
            {
                foreach (var effectBase in mesh.Effects)
                {
                    if (effectBase is IShader shader)
                    {
                        if (shader is IShaderTransform shaderTransform)
                        {
                            shaderTransform.Camera = cam;
                            shaderTransform.Transform = finalWorld;
                        }

                        ShaderSetupCallback?.Invoke(shader, cam, finalWorld);
                        shader.Apply();
                    }
                    else if (effectBase is BasicEffect basic)
                    {
                        basic.World = finalWorld;
                        basic.View = cam.View;
                        basic.Projection = cam.Projection;

                        ShaderSetupCallback?.Invoke(null, cam, finalWorld);
                        basic.CurrentTechnique.Passes[0].Apply();
                    }
                }

                mesh.Draw();
            }
        }
    }
}