using PopperBurst;

public static class PopperStateFactory
{
    public static IPopperState CreateState(PopperColor color)
    {
        return color switch
        {
            PopperColor.Purple => new PurpleState(),
            PopperColor.Blue => new BlueState(),
          //  PopperColor.Green => new GreenState(),
            PopperColor.Yellow => new YellowState(),
          //  PopperColor.Red => new RedState(),
            _ => new PurpleState()
        };
    }
}