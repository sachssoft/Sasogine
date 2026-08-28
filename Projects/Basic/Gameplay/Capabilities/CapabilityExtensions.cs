using System;
using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Gameplay.Capabilities
{
    /// <summary>
    /// Provides extension methods for working with object capabilities.
    /// </summary>
    public static class CapabilityExtensions
    {
        // ---------------------------------------------------------------------
        // Move
        // ---------------------------------------------------------------------

        /// <summary>
        /// Determines whether the specified object can be moved.
        /// </summary>
        public static bool IsMovable(this IEngineObject obj)
        {
            return obj is IMovable movable && movable.AllowMove;
        }

        /// <summary>
        /// Determines whether the specified definition can be moved.
        /// </summary>
        public static bool IsMovable(this IDefinition definition)
        {
            return definition is IMovableDefinition movable;
        }

        /// <summary>
        /// Moves the specified object to the given position.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the object does not support movement or movement is not allowed.
        /// </exception>
        public static void Move(this IEngineObject obj, Vector2 position)
        {
            if (obj is not IMovable movable)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' does not support movement.");

            if (!movable.AllowMove)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' does not allow movement.");

            movable.Position = position;
        }

        /// <summary>
        /// Moves the specified definition to the given position.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the definition does not support movement or movement is not allowed.
        /// </exception>
        public static void Move(this IDefinition definition, Vector2 position)
        {
            if (definition is not IMovableDefinition movable)
                throw new InvalidOperationException(
                    $"Definition of type '{definition.GetType().Name}' does not support movement.");

            movable.Position = position;
        }

        /// <summary>
        /// Attempts to move the specified object to the given position.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the object was moved; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool TryMove(this IEngineObject obj, Vector2 position)
        {
            if (obj is not IMovable movable || !movable.AllowMove)
                return false;

            movable.Position = position;
            return true;
        }

        /// <summary>
        /// Attempts to move the specified definition to the given position.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the definition was moved; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool TryMove(this IDefinition definition, Vector2 position)
        {
            if (definition is not IMovableDefinition movable)
                return false;

            movable.Position = position;
            return true;
        }

        // ---------------------------------------------------------------------
        // Rotate
        // ---------------------------------------------------------------------

        /// <summary>
        /// Determines whether the specified object can be rotated.
        /// </summary>
        public static bool IsRotatable(this IEngineObject obj)
        {
            return obj is IRotatable rotatable && rotatable.AllowRotate;
        }

        /// <summary>
        /// Determines whether the specified definition can be rotated.
        /// </summary>
        public static bool IsRotatable(this IDefinition definition)
        {
            return definition is IRotatableDefinition rotatable;
        }

        /// <summary>
        /// Rotates the specified object to the given rotation.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the object does not support rotation or rotation is not allowed.
        /// </exception>
        public static void Rotate(this IEngineObject obj, float rotation)
        {
            if (obj is not IRotatable rotatable)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' does not support rotation.");

            if (!rotatable.AllowRotate)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' does not allow rotation.");

            rotatable.Rotation = rotation;
        }

        /// <summary>
        /// Rotates the specified definition to the given rotation.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the definition does not support rotation or rotation is not allowed.
        /// </exception>
        public static void Rotate(this IDefinition definition, float rotation)
        {
            if (definition is not IRotatableDefinition rotatable)
                throw new InvalidOperationException(
                    $"Definition of type '{definition.GetType().Name}' does not support rotation.");

            rotatable.Rotation = rotation;
        }

        /// <summary>
        /// Attempts to rotate the specified object to the given rotation.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the object was rotated; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool TryRotate(this IEngineObject obj, float rotation)
        {
            if (obj is not IRotatable rotatable || !rotatable.AllowRotate)
                return false;

            rotatable.Rotation = rotation;
            return true;
        }

        /// <summary>
        /// Attempts to rotate the specified definition to the given rotation.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the definition was rotated; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool TryRotate(this IDefinition definition, float rotation)
        {
            if (definition is not IRotatableDefinition rotatable)
                return false;

            rotatable.Rotation = rotation;
            return true;
        }

        // ---------------------------------------------------------------------
        // Enable
        // ---------------------------------------------------------------------

        /// <summary>
        /// Determines whether the specified object supports enabling and disabling.
        /// </summary>
        public static bool IsEnableable(this IEngineObject obj)
        {
            return obj is IEnableable;
        }

        /// <summary>
        /// Determines whether the specified definition supports enabling and disabling.
        /// </summary>
        public static bool IsEnableable(this IDefinition definition)
        {
            return definition is IEnableableDefinition;
        }

        /// <summary>
        /// Enables the specified object.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the object does not support enabling.
        /// </exception>
        public static void Enable(this IEngineObject obj)
        {
            if (obj is not IEnableable enableable)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' does not support enabling.");

            enableable.IsEnabled = true;
        }

        /// <summary>
        /// Enables the specified definition.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the definition does not support enabling.
        /// </exception>
        public static void Enable(this IDefinition definition)
        {
            if (definition is not IEnableableDefinition enableable)
                throw new InvalidOperationException(
                    $"Definition of type '{definition.GetType().Name}' does not support enabling.");

            enableable.IsEnabled = true;
        }

        /// <summary>
        /// Attempts to enable the specified object.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the object was enabled; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool TryEnable(this IEngineObject obj)
        {
            if (obj is not IEnableable enableable)
                return false;

            enableable.IsEnabled = true;
            return true;
        }

        /// <summary>
        /// Attempts to enable the specified definition.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the definition was enabled; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool TryEnable(this IDefinition definition)
        {
            if (definition is not IEnableableDefinition enableable)
                return false;

            enableable.IsEnabled = true;
            return true;
        }

        /// <summary>
        /// Disables the specified object.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the object does not support disabling.
        /// </exception>
        public static void Disable(this IEngineObject obj)
        {
            if (obj is not IEnableable enableable)
                throw new InvalidOperationException(
                    $"Object of type '{obj.GetType().Name}' does not support disabling.");

            enableable.IsEnabled = false;
        }

        /// <summary>
        /// Disables the specified definition.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the definition does not support disabling.
        /// </exception>
        public static void Disable(this IDefinition definition)
        {
            if (definition is not IEnableableDefinition enableable)
                throw new InvalidOperationException(
                    $"Definition of type '{definition.GetType().Name}' does not support disabling.");

            enableable.IsEnabled = false;
        }

        /// <summary>
        /// Attempts to disable the specified object.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the object was disabled; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool TryDisable(this IEngineObject obj)
        {
            if (obj is not IEnableable enableable)
                return false;

            enableable.IsEnabled = false;
            return true;
        }

        /// <summary>
        /// Attempts to disable the specified definition.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the definition was disabled; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool TryDisable(this IDefinition definition)
        {
            if (definition is not IEnableableDefinition enableable)
                return false;

            enableable.IsEnabled = false;
            return true;
        }
    }
}