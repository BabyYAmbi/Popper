// Add this script to an empty GameObject for automatic grid setup
using UnityEngine;

public class PopperGridSpawner : MonoBehaviour
{
    public GameObject popperPrefab;
    public int gridWidth = 5;
    public int gridHeight = 5;
    public float spacing = 2f;

    void Start()
    {
        SpawnGrid();
    }

    void SpawnGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 position = new Vector3(x * spacing, y * spacing, 0);
                Instantiate(popperPrefab, position, Quaternion.identity);
            }
        }
    }
}