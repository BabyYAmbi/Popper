using System.Collections.Generic;

namespace PopperBurst
{
    public struct ChainReactionEvent : IGameEvent
    {
        public int chainLength;
        public List<PopperController> burstPoppers;
    }
}