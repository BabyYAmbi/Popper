using UnityEngine;

namespace PopperBurst
{
    public class BlueState : IPopperState
    {
        public PopperColor GetColor() => PopperColor.Blue;
        public IPopperState GetNextState() => new YellowState();
        public bool CanBurst() => false;
        public Sprite GetSprite() => PopperSpriteManager.Instance?.GetSprite(PopperColor.Blue);
    }
}