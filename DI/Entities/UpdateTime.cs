namespace BB.Di
{
    public readonly struct UpdateTime
    {
        public readonly float _delta;

        public readonly float _unscaledDelta;

        public static implicit operator float(UpdateTime time)
            => time._delta;

        public UpdateTime(float delta, float unscaledDelta)
        {
            _unscaledDelta = unscaledDelta;
            _delta = delta;
        }
    }
    public enum UpdateType
    {
        Normal,
        Fixed,
        Late
    }
}