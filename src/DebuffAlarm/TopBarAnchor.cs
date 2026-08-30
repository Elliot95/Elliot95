using Godot;
using MegaCrit.Sts2.Core.Nodes;

namespace DebuffAlarm;

/// <summary>
/// Finds the live native energy-counter widget in the scene tree so AlarmIcon
/// can dock next to it instead of floating at a guessed pixel offset.
/// NEnergyCounter is a confirmed real type (BaseLib-StS2's
/// NEnergyCounterFactory : NodeFactory&lt;NEnergyCounter&gt;), but its
/// namespace here is inferred from sibling native types (NRun, NOverlayStack,
/// ...) rather than confirmed directly - fix the `using` above if this
/// doesn't compile against your install.
/// </summary>
public static class TopBarAnchor
{
    public static NEnergyCounter Find(Node root)
    {
        if (root is NEnergyCounter counter)
        {
            return counter;
        }

        foreach (var child in root.GetChildren())
        {
            var found = Find(child);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
