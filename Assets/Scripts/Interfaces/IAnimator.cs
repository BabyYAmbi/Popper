using UnityEngine;

namespace PopperBurst
{
    public interface IAnimator
    {
        void PlayIdleAnimation();
        void PlayTapAnimation();
        void PlayBurstAnimation();
        void StopAllAnimations();
    }
}