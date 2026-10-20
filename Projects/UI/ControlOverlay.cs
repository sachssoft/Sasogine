using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Controls;
internal sealed record ControlOverlay(Control Control, Bounds2 Placement, bool Modal, bool Popup, Control? Owner, Control? PreviousFocus, Action? Closed);
