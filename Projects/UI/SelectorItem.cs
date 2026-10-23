using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// Explicit list data; no property binding or member discovery is performed.
/// </summary>
public sealed record SelectorItem(string Text, object? Value = null, bool IsEnabled = true, string? Icon = null);
