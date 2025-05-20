using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class GridObjectSpawner : MonoBehaviour
{
    [Header("Prefab References")]
    [SerializeField] private GameObject type1Prefab;
    [SerializeField] private GameObject type2Prefab;
    [SerializeField] private GameObject type3Prefab;
    // Add more prefab references as needed

    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector3 startPosition = Vector3.zero;

    [Header("Input Files")]
    [SerializeField] private TextAsset[] gridTextFiles;
    [SerializeField] private int currentFileIndex = 0;

    private Dictionary<char, GameObject> charToPrefab;
    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Awake()
    {
        // Initialize dictionary mapping characters to prefabs
        charToPrefab = new Dictionary<char, GameObject>
        {
            { '1', type1Prefab },
            { '2', type2Prefab },
            { '3', type3Prefab }
            // Add more character-to-prefab mappings as needed
        };
    }

    void Start()
    {
        // Load the first grid file on start
        if (gridTextFiles.Length > 0 && currentFileIndex < gridTextFiles.Length)
        {
            LoadAndCreateFromFile(currentFileIndex);
        }
    }

    // Load and process a specific grid text file by index
    public void LoadAndCreateFromFile(int fileIndex)
    {
        if (fileIndex < 0 || fileIndex >= gridTextFiles.Length)
        {
            Debug.LogError($"File index {fileIndex} is out of range. Available files: {gridTextFiles.Length}");
            return;
        }

        currentFileIndex = fileIndex;
        TextAsset textAsset = gridTextFiles[fileIndex];

        if (textAsset == null)
        {
            Debug.LogError($"Text file at index {fileIndex} is null");
            return;
        }

        CreateObjectsFromGrid(textAsset.text);
        //Debug.Log($"Loaded grid from file: {textAsset.name}");
    }

    public void CreateObjectsFromGrid(string input)
    {
        // Clear any previously spawned objects
        ClearSpawnedObjects();

        // Split input by new lines and validate
        string[] lines = input.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries)
                              .Select(line => line.Trim())
                              .ToArray();

        // Validate number of lines
        if (lines.Length != 6)
        {
            Debug.LogError($"Expected 6 lines, but got {lines.Length} in input:\n{input}");
            return;
        }

        // Process each line
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            // Process each character in the line (up to 3 columns)
            for (int j = 0; j < Mathf.Min(line.Length, 3); j++)
            {
                char c = line[j];

                // Skip '0' and spaces
                if (c == '0' || c == ' ')
                {
                    //Debug.Log($"Skipped position ({i},{j}) with character '{c}'");
                    continue;
                }

                // Try to instantiate object based on character
                if (charToPrefab.TryGetValue(c, out GameObject prefab))
                {
                    // Calculate position
                    Vector3 position = startPosition + new Vector3(j * cellSize - cellSize, 0, -i * cellSize);

                    // Instantiate the object
                    GameObject obj = Instantiate(prefab, this.transform.position + position, Quaternion.Euler(0, 0, 0), transform);
                    obj.name = $"Object_{c}_{i}_{j}";
                    spawnedObjects.Add(obj);

                    //Debug.Log($"Created object of type {c} at position ({i},{j})");
                }
                else
                {
                    Debug.LogWarning($"Unknown character '{c}' at position ({i},{j})");
                }
            }
        }
    }

    // Clear all spawned objects
    public void ClearSpawnedObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(obj);
                }
                else
                {
                    DestroyImmediate(obj);
                }
            }
        }
        spawnedObjects.Clear();
    }

    // For loading files at runtime from the Resources folder
    public void LoadGridFromResources(string resourcePath)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);
        if (textAsset != null)
        {
            CreateObjectsFromGrid(textAsset.text);
            //Debug.Log($"Loaded grid from Resources folder: {resourcePath}");
        }
        else
        {
            Debug.LogError($"Failed to load text asset from Resources: {resourcePath}");
        }
    }
}