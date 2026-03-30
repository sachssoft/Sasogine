using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Sachssoft.Sasogine.UI.ImGui
{
    /// <summary>
    /// ImGui renderer for use with XNA-likes (FNA & MonoGame)
    /// </summary>
    // Quelle: https://github.com/ImGuiNET/ImGui.NET/tree/master/src/ImGui.NET.SampleProgram.XNA
    public class ImGuiRenderer
    {
        private Game _game;

        // Graphics
        private GraphicsDevice _graphicsDevice;

        private BasicEffect _effect;
        private RasterizerState _rasterizerState;

        private byte[] _vertexData;
        private VertexBuffer _vertexBuffer;
        private int _vertexBufferSize;

        private byte[] _indexData;
        private IndexBuffer _indexBuffer;
        private int _indexBufferSize;

        // Textures
        private Dictionary<IntPtr, Texture2D> _loadedTextures;

        private int _textureId;
        private IntPtr? _fontTextureId;

        // Input
        private int _scrollWheelValue;
        private int _horizontalScrollWheelValue;
        private readonly float WHEEL_DELTA = 120;
        private Keys[] _allKeys = Enum.GetValues<Keys>();

        public ImGuiRenderer(Game game)
        {
            var context = ImGuiNET.ImGui.CreateContext();
            ImGuiNET.ImGui.SetCurrentContext(context);

            _game = game ?? throw new ArgumentNullException(nameof(game));
            _graphicsDevice = game.GraphicsDevice;

            _loadedTextures = new Dictionary<IntPtr, Texture2D>();

            _rasterizerState = new RasterizerState()
            {
                CullMode = CullMode.None,
                DepthBias = 0,
                FillMode = FillMode.Solid,
                MultiSampleAntiAlias = false,
                ScissorTestEnable = true,
                SlopeScaleDepthBias = 0
            };

            SetupInput();
        }

        #region ImGuiRenderer

        /// <summary>
        /// Creates a texture and loads the font data from ImGui. Should be called when the <see cref="GraphicsDevice" /> is initialized but before any rendering is done
        /// </summary>
        public virtual unsafe void RebuildFontAtlas()
        {
            // Get font texture from ImGui
            var io = ImGuiNET.ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out int width, out int height, out int bytesPerPixel);

            // Copy the data to a managed array
            var pixels = new byte[width * height * bytesPerPixel];
            unsafe { Marshal.Copy(new IntPtr(pixelData), pixels, 0, pixels.Length); }

            // Create and register the texture as an XNA texture
            var tex2d = new Texture2D(_graphicsDevice, width, height, false, SurfaceFormat.Color);
            tex2d.SetData(pixels);

            // Should a texture already have been build previously, unbind it first so it can be deallocated
            if (_fontTextureId.HasValue) UnbindTexture(_fontTextureId.Value);

            // Bind the new texture to an ImGui-friendly id
            _fontTextureId = BindTexture(tex2d);

            // Let ImGui know where to find the texture
            io.Fonts.SetTexID(_fontTextureId.Value);
            io.Fonts.ClearTexData(); // Clears CPU side texture data
        }

        /// <summary>
        /// Creates a pointer to a texture, which can be passed through ImGui calls such as <see cref="Sasogine.ImGui.Image" />. That pointer is then used by ImGui to let us know what texture to draw
        /// </summary>
        public virtual IntPtr BindTexture(Texture2D texture)
        {
            var id = new IntPtr(_textureId++);

            _loadedTextures.Add(id, texture);

            return id;
        }

        /// <summary>
        /// Removes a previously created texture pointer, releasing its reference and allowing it to be deallocated
        /// </summary>
        public virtual void UnbindTexture(IntPtr textureId)
        {
            _loadedTextures.Remove(textureId);
        }

        /// <summary>
        /// Sets up ImGui for a new frame, should be called at frame start
        /// </summary>
        public virtual void BeforeLayout(GameTime gameTime)
        {
            ImGuiNET.ImGui.GetIO().DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            UpdateInput();

            ImGuiNET.ImGui.NewFrame();
        }

        /// <summary>
        /// Asks ImGui for the generated geometry data and sends it to the graphics pipeline, should be called after the UI is drawn using ImGui.** calls
        /// </summary>
        public virtual void AfterLayout()
        {
            ImGuiNET.ImGui.Render();

            unsafe { RenderDrawData(ImGuiNET.ImGui.GetDrawData()); }
        }

        #endregion ImGuiRenderer

        #region Setup & Update

        /// <summary>
        /// Setup key input event handler.
        /// </summary>
        protected virtual void SetupInput()
        {
            var io = ImGuiNET.ImGui.GetIO();

            // MonoGame-specific //////////////////////
            _game.Window.TextInput += (s, a) =>
            {
                if (a.Character == '\t') return;
                io.AddInputCharacter(a.Character);
            };

            ///////////////////////////////////////////

            // FNA-specific ///////////////////////////
            //TextInputEXT.TextInput += c =>
            //{
            //    if (c == '\t') return;

            //    ImGui.GetIO().AddInputCharacter(c);
            //};
            ///////////////////////////////////////////
        }

        /// <summary>
        /// Updates the <see cref="Effect" /> to the current matrices and texture
        /// </summary>
        protected virtual Effect UpdateEffect(Texture2D texture)
        {
            _effect = _effect ?? new BasicEffect(_graphicsDevice);

            var io = ImGuiNET.ImGui.GetIO();

            _effect.World = Matrix.Identity;
            _effect.View = Matrix.Identity;
            _effect.Projection = Matrix.CreateOrthographicOffCenter(0f, io.DisplaySize.X, io.DisplaySize.Y, 0f, -1f, 1f);
            _effect.TextureEnabled = true;
            _effect.Texture = texture;
            _effect.VertexColorEnabled = true;

            return _effect;
        }

        /// <summary>
        /// Sends XNA input state to ImGui
        /// </summary>
        // Neu
        protected virtual void UpdateInput()
        {
            if (!_game.IsActive) return;

            var io = ImGuiNET.ImGui.GetIO();

            // --- Maus Handling (bereits vorhanden) ---
            var mouse = Mouse.GetState();
            io.AddMousePosEvent(mouse.X, mouse.Y);
            io.AddMouseButtonEvent(0, mouse.LeftButton == ButtonState.Pressed);
            io.AddMouseButtonEvent(1, mouse.RightButton == ButtonState.Pressed);
            io.AddMouseButtonEvent(2, mouse.MiddleButton == ButtonState.Pressed);

            // --- Touch Handling ---
            var touchState = TouchPanel.GetState();
            if (touchState.Count > 0)
            {
                var touch = touchState[0]; // Nur erster Finger
                io.AddMousePosEvent(touch.Position.X, touch.Position.Y);

                // Edge-trigger: Nur einmal pro Tap
                bool pressed = touch.State == TouchLocationState.Pressed || touch.State == TouchLocationState.Moved;
                io.AddMouseButtonEvent(0, pressed); // Linksklick ersetzen
            }

            // --- ScrollWheel + Keyboard (bestehend) ---
            io.AddMouseWheelEvent(
                (mouse.HorizontalScrollWheelValue - _horizontalScrollWheelValue) / WHEEL_DELTA,
                (mouse.ScrollWheelValue - _scrollWheelValue) / WHEEL_DELTA
            );
            _scrollWheelValue = mouse.ScrollWheelValue;
            _horizontalScrollWheelValue = mouse.HorizontalScrollWheelValue;

            foreach (var key in _allKeys)
            {
                if (TryMapKeys(key, out ImGuiNET.ImGuiKey imguikey))
                {
                    io.AddKeyEvent(imguikey, Keyboard.GetState().IsKeyDown(key));
                }
            }

            io.DisplaySize = new System.Numerics.Vector2(
                _graphicsDevice.PresentationParameters.BackBufferWidth,
                _graphicsDevice.PresentationParameters.BackBufferHeight
            );
            io.DisplayFramebufferScale = new System.Numerics.Vector2(1f, 1f);
        }

        // Original Alt
        //protected virtual void UpdateInput()
        //{
        //    if (!_game.IsActive) return;

        //    var io = ImGuiNET.ImGui.GetIO();

        //    var mouse = Mouse.GetState();
        //    var keyboard = Keyboard.GetState();
        //    io.AddMousePosEvent(mouse.X, mouse.Y);
        //    io.AddMouseButtonEvent(0, mouse.LeftButton == ButtonState.Pressed);
        //    io.AddMouseButtonEvent(1, mouse.RightButton == ButtonState.Pressed);
        //    io.AddMouseButtonEvent(2, mouse.MiddleButton == ButtonState.Pressed);
        //    io.AddMouseButtonEvent(3, mouse.XButton1 == ButtonState.Pressed);
        //    io.AddMouseButtonEvent(4, mouse.XButton2 == ButtonState.Pressed);

        //    io.AddMouseWheelEvent(
        //        (mouse.HorizontalScrollWheelValue - _horizontalScrollWheelValue) / WHEEL_DELTA,
        //        (mouse.ScrollWheelValue - _scrollWheelValue) / WHEEL_DELTA);
        //    _scrollWheelValue = mouse.ScrollWheelValue;
        //    _horizontalScrollWheelValue = mouse.HorizontalScrollWheelValue;

        //    foreach (var key in _allKeys)
        //    {
        //        if (TryMapKeys(key, out ImGuiNET.ImGuiKey imguikey))
        //        {
        //            io.AddKeyEvent(imguikey, keyboard.IsKeyDown(key));
        //        }
        //    }

        //    io.DisplaySize = new System.Numerics.Vector2(_graphicsDevice.PresentationParameters.BackBufferWidth, _graphicsDevice.PresentationParameters.BackBufferHeight);
        //    io.DisplayFramebufferScale = new System.Numerics.Vector2(1f, 1f);
        //}

        private bool TryMapKeys(Keys key, out ImGuiNET.ImGuiKey imguikey)
        {
            //Special case not handed in the switch...
            //If the actual key we put in is "None", return none and true. 
            //otherwise, return none and false.
            if (key == Keys.None)
            {
                imguikey = ImGuiNET.ImGuiKey.None;
                return true;
            }

            imguikey = key switch
            {
                Keys.Back => ImGuiNET.ImGuiKey.Backspace,
                Keys.Tab => ImGuiNET.ImGuiKey.Tab,
                Keys.Enter => ImGuiNET.ImGuiKey.Enter,
                Keys.CapsLock => ImGuiNET.ImGuiKey.CapsLock,
                Keys.Escape => ImGuiNET.ImGuiKey.Escape,
                Keys.Space => ImGuiNET.ImGuiKey.Space,
                Keys.PageUp => ImGuiNET.ImGuiKey.PageUp,
                Keys.PageDown => ImGuiNET.ImGuiKey.PageDown,
                Keys.End => ImGuiNET.ImGuiKey.End,
                Keys.Home => ImGuiNET.ImGuiKey.Home,
                Keys.Left => ImGuiNET.ImGuiKey.LeftArrow,
                Keys.Right => ImGuiNET.ImGuiKey.RightArrow,
                Keys.Up => ImGuiNET.ImGuiKey.UpArrow,
                Keys.Down => ImGuiNET.ImGuiKey.DownArrow,
                Keys.PrintScreen => ImGuiNET.ImGuiKey.PrintScreen,
                Keys.Insert => ImGuiNET.ImGuiKey.Insert,
                Keys.Delete => ImGuiNET.ImGuiKey.Delete,
                >= Keys.D0 and <= Keys.D9 => ImGuiNET.ImGuiKey._0 + (key - Keys.D0),
                >= Keys.A and <= Keys.Z => ImGuiNET.ImGuiKey.A + (key - Keys.A),
                >= Keys.NumPad0 and <= Keys.NumPad9 => ImGuiNET.ImGuiKey.Keypad0 + (key - Keys.NumPad0),
                Keys.Multiply => ImGuiNET.ImGuiKey.KeypadMultiply,
                Keys.Add => ImGuiNET.ImGuiKey.KeypadAdd,
                Keys.Subtract => ImGuiNET.ImGuiKey.KeypadSubtract,
                Keys.Decimal => ImGuiNET.ImGuiKey.KeypadDecimal,
                Keys.Divide => ImGuiNET.ImGuiKey.KeypadDivide,
                >= Keys.F1 and <= Keys.F24 => ImGuiNET.ImGuiKey.F1 + (key - Keys.F1),
                Keys.NumLock => ImGuiNET.ImGuiKey.NumLock,
                Keys.Scroll => ImGuiNET.ImGuiKey.ScrollLock,
                Keys.LeftShift => ImGuiNET.ImGuiKey.ModShift,
                Keys.LeftControl => ImGuiNET.ImGuiKey.ModCtrl,
                Keys.LeftAlt => ImGuiNET.ImGuiKey.ModAlt,
                Keys.OemSemicolon => ImGuiNET.ImGuiKey.Semicolon,
                Keys.OemPlus => ImGuiNET.ImGuiKey.Equal,
                Keys.OemComma => ImGuiNET.ImGuiKey.Comma,
                Keys.OemMinus => ImGuiNET.ImGuiKey.Minus,
                Keys.OemPeriod => ImGuiNET.ImGuiKey.Period,
                Keys.OemQuestion => ImGuiNET.ImGuiKey.Slash,
                Keys.OemTilde => ImGuiNET.ImGuiKey.GraveAccent,
                Keys.OemOpenBrackets => ImGuiNET.ImGuiKey.LeftBracket,
                Keys.OemCloseBrackets => ImGuiNET.ImGuiKey.RightBracket,
                Keys.OemPipe => ImGuiNET.ImGuiKey.Backslash,
                Keys.OemQuotes => ImGuiNET.ImGuiKey.Apostrophe,
                Keys.BrowserBack => ImGuiNET.ImGuiKey.AppBack,
                Keys.BrowserForward => ImGuiNET.ImGuiKey.AppForward,
                _ => ImGuiNET.ImGuiKey.None,
            };

            return imguikey != ImGuiNET.ImGuiKey.None;
        }

        #endregion Setup & Update

        #region Internals

        /// <summary>
        /// Gets the geometry as set up by ImGui and sends it to the graphics device
        /// </summary>
        private void RenderDrawData(ImGuiNET.ImDrawDataPtr drawData)
        {
            // Setup render state: alpha-blending enabled, no face culling, no depth testing, scissor enabled, vertex/texcoord/color pointers
            var lastViewport = _graphicsDevice.Viewport;
            var lastScissorBox = _graphicsDevice.ScissorRectangle;
            var lastRasterizer = _graphicsDevice.RasterizerState;
            var lastDepthStencil = _graphicsDevice.DepthStencilState;
            var lastBlendFactor = _graphicsDevice.BlendFactor;
            var lastBlendState = _graphicsDevice.BlendState;

            _graphicsDevice.BlendFactor = Color.White;
            _graphicsDevice.BlendState = BlendState.NonPremultiplied;
            _graphicsDevice.RasterizerState = _rasterizerState;
            _graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            // Handle cases of screen coordinates != from framebuffer coordinates (e.g. retina displays)
            drawData.ScaleClipRects(ImGuiNET.ImGui.GetIO().DisplayFramebufferScale);

            // Setup projection
            _graphicsDevice.Viewport = new Viewport(0, 0, _graphicsDevice.PresentationParameters.BackBufferWidth, _graphicsDevice.PresentationParameters.BackBufferHeight);

            UpdateBuffers(drawData);

            RenderCommandLists(drawData);

            // Restore modified state
            _graphicsDevice.Viewport = lastViewport;
            _graphicsDevice.ScissorRectangle = lastScissorBox;
            _graphicsDevice.RasterizerState = lastRasterizer;
            _graphicsDevice.DepthStencilState = lastDepthStencil;
            _graphicsDevice.BlendState = lastBlendState;
            _graphicsDevice.BlendFactor = lastBlendFactor;
        }

        private unsafe void UpdateBuffers(ImGuiNET.ImDrawDataPtr drawData)
        {
            if (drawData.TotalVtxCount == 0)
            {
                return;
            }

            // Expand buffers if we need more room
            if (drawData.TotalVtxCount > _vertexBufferSize)
            {
                _vertexBuffer?.Dispose();

                _vertexBufferSize = (int)(drawData.TotalVtxCount * 1.5f);
                _vertexBuffer = new VertexBuffer(_graphicsDevice, DrawVertDeclaration.Declaration, _vertexBufferSize, BufferUsage.None);
                _vertexData = new byte[_vertexBufferSize * DrawVertDeclaration.Size];
            }

            if (drawData.TotalIdxCount > _indexBufferSize)
            {
                _indexBuffer?.Dispose();

                _indexBufferSize = (int)(drawData.TotalIdxCount * 1.5f);
                _indexBuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.SixteenBits, _indexBufferSize, BufferUsage.None);
                _indexData = new byte[_indexBufferSize * sizeof(ushort)];
            }

            // Copy ImGui's vertices and indices to a set of managed byte arrays
            int vtxOffset = 0;
            int idxOffset = 0;

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImGuiNET.ImDrawListPtr cmdList = drawData.CmdLists[n];

                fixed (void* vtxDstPtr = &_vertexData[vtxOffset * DrawVertDeclaration.Size])
                fixed (void* idxDstPtr = &_indexData[idxOffset * sizeof(ushort)])
                {
                    Buffer.MemoryCopy((void*)cmdList.VtxBuffer.Data, vtxDstPtr, _vertexData.Length, cmdList.VtxBuffer.Size * DrawVertDeclaration.Size);
                    Buffer.MemoryCopy((void*)cmdList.IdxBuffer.Data, idxDstPtr, _indexData.Length, cmdList.IdxBuffer.Size * sizeof(ushort));
                }

                vtxOffset += cmdList.VtxBuffer.Size;
                idxOffset += cmdList.IdxBuffer.Size;
            }

            // Copy the managed byte arrays to the gpu vertex- and index buffers
            _vertexBuffer.SetData(_vertexData, 0, drawData.TotalVtxCount * DrawVertDeclaration.Size);
            _indexBuffer.SetData(_indexData, 0, drawData.TotalIdxCount * sizeof(ushort));
        }

        private unsafe void RenderCommandLists(ImGuiNET.ImDrawDataPtr drawData)
        {
            _graphicsDevice.SetVertexBuffer(_vertexBuffer);
            _graphicsDevice.Indices = _indexBuffer;

            int vtxOffset = 0;
            int idxOffset = 0;

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImGuiNET.ImDrawListPtr cmdList = drawData.CmdLists[n];

                for (int cmdi = 0; cmdi < cmdList.CmdBuffer.Size; cmdi++)
                {
                    ImGuiNET.ImDrawCmdPtr drawCmd = cmdList.CmdBuffer[cmdi];

                    if (drawCmd.ElemCount == 0)
                    {
                        continue;
                    }

                    if (!_loadedTextures.ContainsKey(drawCmd.TextureId))
                    {
                        throw new InvalidOperationException($"Could not find a texture with id '{drawCmd.TextureId}', please check your bindings");
                    }

                    _graphicsDevice.ScissorRectangle = new Rectangle(
                        (int)drawCmd.ClipRect.X,
                        (int)drawCmd.ClipRect.Y,
                        (int)(drawCmd.ClipRect.Z - drawCmd.ClipRect.X),
                        (int)(drawCmd.ClipRect.W - drawCmd.ClipRect.Y)
                    );

                    var effect = UpdateEffect(_loadedTextures[drawCmd.TextureId]);

                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

#pragma warning disable CS0618 // // FNA does not expose an alternative method.
                        _graphicsDevice.DrawIndexedPrimitives(
                            primitiveType: PrimitiveType.TriangleList,
                            baseVertex: (int)drawCmd.VtxOffset + vtxOffset,
                            minVertexIndex: 0,
                            numVertices: cmdList.VtxBuffer.Size,
                            startIndex: (int)drawCmd.IdxOffset + idxOffset,
                            primitiveCount: (int)drawCmd.ElemCount / 3
                        );
#pragma warning restore CS0618
                    }
                }

                vtxOffset += cmdList.VtxBuffer.Size;
                idxOffset += cmdList.IdxBuffer.Size;
            }
        }

        #endregion Internals
    }
}