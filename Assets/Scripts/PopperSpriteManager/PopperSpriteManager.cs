using PopperBurst;
using System.Collections.Generic;
using UnityEngine;

public class PopperSpriteManager : MonoBehaviour
{
    public static PopperSpriteManager Instance { get; private set; }

    [Header("Popper Sprites")]
    [SerializeField] private Sprite purpleSprite;
    [SerializeField] private Sprite blueSprite;
    [SerializeField] private Sprite yellowSprite;

    // Dictionary for fast sprite lookup
    private Dictionary<PopperColor, Sprite> spriteMap;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSpriteMap();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSpriteMap()
    {
        spriteMap = new Dictionary<PopperColor, Sprite>
        {
            { PopperColor.Purple, purpleSprite },
            { PopperColor.Blue, blueSprite },
           // { PopperColor.Green, greenSprite },
            { PopperColor.Yellow, yellowSprite },
          //  { PopperColor.Red, redSprite }
        };
    }

    public Sprite GetSprite(PopperColor color)
    {
        if (spriteMap != null && spriteMap.ContainsKey(color))
            return spriteMap[color];

        Debug.LogWarning($"No sprite found for color: {color}");
        return null;
    }

    // Validation method to check if all sprites are assigned
    [ContextMenu("Validate Sprites")]
    public void ValidateSprites()
    {
        bool allAssigned = true;

        if (!purpleSprite) { Debug.LogError("Purple sprite not assigned!"); allAssigned = false; }
        if (!blueSprite) { Debug.LogError("Blue sprite not assigned!"); allAssigned = false; }
        if (!yellowSprite) { Debug.LogError("Yellow sprite not assigned!"); allAssigned = false; }

        if (allAssigned)
            Debug.Log("✅ All popper sprites are assigned!");
    }
}