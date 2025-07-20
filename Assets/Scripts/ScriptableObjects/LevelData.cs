using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Levels/Level Data")]
public class LevelData : ScriptableObject
{
    public int targetScore;
    public int maxTaps;
    public int rows;
    public int cols;
    public PopperType[] gridData;
}

public enum PopperType
{
    Empty,
    Purple,
    Blue,
    Yellow
}