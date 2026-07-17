using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Camera;
using Sachssoft.Sasogine.Scenes;
using System;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    public sealed class ModelRenderer
    {
        private readonly Model _model;

        public ModelRenderer(Model model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        /// <summary>
        /// Weltmatrix des Modells
        /// </summary>
        public Matrix World { get; set; } = Matrix.Identity;

        /// <summary>
        /// Optionaler Effekt-Override
        /// </summary>
        public Action<IShader, ICamera, Matrix?>? ShaderSetupCallback { get; set; }

        /// <summary>
        /// Zeichnet das Modell innerhalb eines RenderScopes
        /// </summary>
        public void Draw(SceneDrawContext context, Matrix? transform = null, ICamera? camera = null, IShader? customEffect = null)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var cam = camera ?? context.ViewCamera ?? throw new InvalidOperationException("No camera available.");
            var graphics = context.GraphicsDevice;

            // Berechne finale Weltmatrix
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

                            //effect.World = finalWorld;
                            //effect.View = cam.View;
                            //effect.Projection = cam.Projection;
                        }

                        ShaderSetupCallback?.Invoke(shader, cam, finalWorld);
                        shader.Apply();
                    }
                    else if (effectBase is BasicEffect basic)
                    {
                        basic.World = finalWorld;
                        basic.View = cam.View;
                        basic.Projection = cam.Projection;

                        ShaderSetupCallback?.Invoke(null, cam, finalWorld); // optional für BasicEffect
                        basic.CurrentTechnique.Passes[0].Apply();
                    }
                }

                mesh.Draw();
            }
        }
    }
}
