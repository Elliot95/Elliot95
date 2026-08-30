using Godot;
using MegaCrit.Sts2.Core.Nodes;

namespace DebuffAlarm;

/// <summary>
/// Indicator dot: dim when idle, filled red and pulsing when
/// DebuffAlarmState says any player on the team currently has Vulnerable or
/// Weak. Docks itself just to the right of the native energy counter
/// (re-checked once a second, since that widget only exists during combat
/// and gets torn down/recreated between fights) so it reads as part of the
/// top bar rather than a random floating overlay. Falls back to a fixed
/// top-center position whenever no energy counter is present, e.g. on the map.
/// </summary>
public partial class AlarmIcon : CanvasLayer
{
    private const int Diameter = 28;
    private const int TopOffset = 12;
    private const float ReacquireIntervalSeconds = 1.0f;
    private const float GapFromEnergyCounter = 8f;

    private static readonly Color IdleColor = new(0.35f, 0.35f, 0.38f, 0.55f);
    private static readonly Color ActiveColor = new(0.95f, 0.25f, 0.2f, 1f);

    private Panel _dot;
    private StyleBoxFlat _style;
    private Tween _pulseTween;
    private bool _wasActive;
    private double _timeSinceSearch;
    private NEnergyCounter _dockedNear;

    public override void _Ready()
    {
        Layer = 100;

        _style = new StyleBoxFlat
        {
            BgColor = IdleColor,
            CornerRadiusTopLeft = Diameter / 2,
            CornerRadiusTopRight = Diameter / 2,
            CornerRadiusBottomLeft = Diameter / 2,
            CornerRadiusBottomRight = Diameter / 2
        };

        _dot = new Panel
        {
            Name = "Dot",
            TopLevel = true,
            CustomMinimumSize = new Vector2(Diameter, Diameter),
            Size = new Vector2(Diameter, Diameter),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            TooltipText = "Debuff Alarm: lit when a teammate currently has Vulnerable or Weak"
        };
        _dot.AddThemeStyleboxOverride("panel", _style);

        AddChild(_dot);
        UpdatePosition(force: true, delta: 0);
    }

    public override void _Process(double delta)
    {
        UpdateActiveState();
        UpdatePosition(force: false, delta: delta);
    }

    private void UpdateActiveState()
    {
        var active = DebuffAlarmState.HasAnyTrackedDebuff();
        if (active == _wasActive)
        {
            return;
        }

        _wasActive = active;
        _pulseTween?.Kill();

        if (active)
        {
            _style.BgColor = ActiveColor;
            _pulseTween = CreateTween().SetLoops();
            _pulseTween.TweenProperty(_dot, "modulate:a", 0.45, 0.5)
                .SetTrans(Tween.TransitionType.Sine);
            _pulseTween.TweenProperty(_dot, "modulate:a", 1.0, 0.5)
                .SetTrans(Tween.TransitionType.Sine);
        }
        else
        {
            _style.BgColor = IdleColor;
            _dot.Modulate = new Color(1, 1, 1, 1);
        }
    }

    private void UpdatePosition(bool force, double delta)
    {
        _timeSinceSearch += delta;

        if (force || _dockedNear == null || !IsInstanceValid(_dockedNear) || _timeSinceSearch >= ReacquireIntervalSeconds)
        {
            _timeSinceSearch = 0;
            var root = GetTree()?.Root;
            _dockedNear = root != null ? TopBarAnchor.Find(root) : null;
        }

        if (_dockedNear != null && IsInstanceValid(_dockedNear))
        {
            var counterSize = _dockedNear.Size;
            _dot.GlobalPosition = _dockedNear.GlobalPosition
                + new Vector2(counterSize.X + GapFromEnergyCounter, (counterSize.Y - Diameter) / 2f);
            return;
        }

        var viewportWidth = GetViewport()?.GetVisibleRect().Size.X ?? Diameter;
        _dot.GlobalPosition = new Vector2(viewportWidth / 2f - Diameter / 2f, TopOffset);
    }
}
