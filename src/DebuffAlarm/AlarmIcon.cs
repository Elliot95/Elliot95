using Godot;

namespace DebuffAlarm;

/// <summary>
/// Screen-space indicator: a dim ring that fills in and pulses when
/// DebuffAlarmState says any player on the team currently has Vulnerable or
/// Weak. Drawn as its own CanvasLayer so it sits on top of the run UI without
/// depending on the exact scene path of the native top bar - see README for
/// how to nudge TopOffset/Diameter to line it up with your resolution/UI scale.
/// </summary>
public partial class AlarmIcon : CanvasLayer
{
    private const int Diameter = 28;
    private const int TopOffset = 12;

    private static readonly Color IdleColor = new(0.35f, 0.35f, 0.38f, 0.55f);
    private static readonly Color ActiveColor = new(0.95f, 0.25f, 0.2f, 1f);

    private Panel _dot;
    private StyleBoxFlat _style;
    private Tween _pulseTween;
    private bool _wasActive;

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
            CustomMinimumSize = new Vector2(Diameter, Diameter),
            AnchorLeft = 0.5f,
            AnchorRight = 0.5f,
            OffsetLeft = -Diameter / 2f,
            OffsetRight = Diameter / 2f,
            OffsetTop = TopOffset,
            OffsetBottom = TopOffset + Diameter,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            TooltipText = "Debuff Alarm: lit when a teammate currently has Vulnerable or Weak"
        };
        _dot.AddThemeStyleboxOverride("panel", _style);

        AddChild(_dot);
    }

    public override void _Process(double delta)
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
}
