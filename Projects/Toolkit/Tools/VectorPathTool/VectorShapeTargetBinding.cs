//using Sachssoft.Sasogine.Common.Collections;
//using Sachssoft.Sasogine.Components.Tools.Vector;
//using System;

//namespace Sachssoft.Sasogine.Components.Tools;

///// <summary>
///// Maintains the vector shape associated with a vector path target.
///// </summary>
//public sealed class VectorShapeTargetBinding
//{
//    private VectorShape? _activeShape;

//    /// <summary>
//    /// Initializes a new instance of the <see cref="VectorShapeTargetBinding"/>
//    /// class using the specified target definition.
//    /// </summary>
//    /// <param name="definition">
//    /// The vector path target definition.
//    /// </param>
//    public VectorShapeTargetBinding(
//        IVectorPathTarget target)
//    {
//        ArgumentNullException.ThrowIfNull(target);

//        Definition = target.Definition;
//        _activeShape = new VectorShape(target.ActiveShape);
//    }

//    /// <summary>
//    /// Gets the vector path target definition associated with this binding.
//    /// </summary>
//    public IVectorPathTargetDefinition Definition { get; }

//    /// <summary>
//    /// Gets the active runtime vector shape.
//    /// </summary>
//    public VectorShape ActiveShape => _activeShape;
//}