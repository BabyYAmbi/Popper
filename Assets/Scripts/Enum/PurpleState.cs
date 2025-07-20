using UnityEngine;

namespace PopperBurst
{
    public class PurpleState : IPopperState
    {
        public PopperColor GetColor() => PopperColor.Purple;
        public IPopperState GetNextState() => new BlueState();
        public bool CanBurst() => false;
        public Sprite GetSprite() => PopperSpriteManager.Instance?.GetSprite(PopperColor.Purple);
    }
}