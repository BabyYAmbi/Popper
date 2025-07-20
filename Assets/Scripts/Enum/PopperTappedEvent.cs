namespace PopperBurst
{
    public struct PopperTappedEvent : IGameEvent
    {
        public PopperController popper;
        public PopperColor newColor;
    }
}