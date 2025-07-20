using UnityEngine;

namespace PopperBurst
{
    public interface IPopperState
    {
        PopperColor GetColor();
        IPopperState GetNextState();
        bool CanBurst();
        Sprite GetSprite(); // New method for sprite

    }
}