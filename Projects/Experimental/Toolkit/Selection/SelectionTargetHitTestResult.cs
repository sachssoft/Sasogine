using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Represents the result of a hit test against selection targets.
    /// </summary>
    public sealed class SelectionTargetHitTestResult
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="SelectionTargetHitTestResult"/> class.
        /// </summary>
        /// <param name="targets">
        /// The selection targets hit by the test.
        /// </param>
        public SelectionTargetHitTestResult(
            IEnumerable<object> targets)
        {
            ArgumentNullException.ThrowIfNull(targets);

            Targets = targets.ToList();
        }

        /// <summary>
        /// Gets the selection targets hit by the test.
        /// </summary>
        public IReadOnlyList<object> Targets { get; }
    }
}