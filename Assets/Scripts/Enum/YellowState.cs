using UnityEngine;

namespace PopperBurst
{
    public class YellowState : IPopperState
    {
        public PopperColor GetColor() => PopperColor.Yellow;
        public IPopperState GetNextState() => new YellowState();
        public bool CanBurst() => true;
        public Sprite GetSprite() => PopperSpriteManager.Instance?.GetSprite(PopperColor.Yellow);
    }
}