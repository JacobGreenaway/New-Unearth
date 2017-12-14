using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls spawning and despawning of plants and animals.
/// </summary>
public class UnearthSpawnController : MonoBehaviour
{
    [SerializeField]
    private UnearthDepthController m_DepthController;
    [SerializeField]
    private UnearthLayersController m_LayersController;

    // Parent object used for all spawned objects
    [SerializeField]
    private GameObject m_SpawnObjectParent;
    // World height to place all spawned items
    [SerializeField]
    private float m_SpawnHeight;
    // Array of SpawnData items used for each spawnable prefab.
    [SerializeField]
    private SpawnData[] m_Spawnables;

    /// <summary>
    /// Pool of spawned items.
    /// </summary>
    private Dictionary<SpawnData, List<GameObject>> m_SpawnedItems = new Dictionary<SpawnData, List<GameObject>>();

    // Should plants be showing?
    private bool m_PlantsToggle = true;
    // Should animals be showing?
    private bool m_AnimalsToggle = true;

    // Queue of spawn items, only perform one per frame to avoid performance spikes
    private Queue<SpawnData> m_SpawnQueue = new Queue<SpawnData>();

    /// <summary>
    /// Called by Unity once on Object creation.
    /// </summary>
    private void Awake()
    {
        // For each of the spawnables, hook onto their spawn and despawn events.
        for (int i = 0; i < m_Spawnables.Length; i++)
        {
            m_Spawnables[i].ShouldSpawnEvent += HandleShouldSpawnEvent;
            m_Spawnables[i].SpawnObjectDespawnedEvent += HandleSpawnObjectDespawnedEvent;
            m_SpawnedItems.Add(m_Spawnables[i], new List<GameObject>());
        }

        // Listen for Screen flipping events.
        SettingsController.Instance.Current.FlipHorizontalChanged += HandleHorizontalFlipped;
        SettingsController.Instance.Current.FlipVerticalChanged += HandleVerticalFlipped;
    }

    /// <summary>
    /// Called once per frame by Unity and all Update calls.
    /// </summary>
    private void LateUpdate()
    {
        // Toggle plants and animals input
        if (Input.GetButtonUp("Toggle plants"))
        {
            m_PlantsToggle = !m_PlantsToggle;
            EnablePlants(m_PlantsToggle);
        }

        if (Input.GetButtonUp("Toggle animals"))
        {
            m_AnimalsToggle = !m_AnimalsToggle;
            EnableAnimals(m_AnimalsToggle);
        }

        // Despawns all current animals and plants
        if (Input.GetButtonUp("Reset all"))
        {
            foreach (var kvp in m_SpawnedItems)
            {
                for (int i = kvp.Value.Count - 1; i >= 0; i--)
                {
                    kvp.Value[i].GetComponent<SpawnedObject>().Despawn();
                }
            }

            m_SpawnQueue.Clear();
        }

        // If there is anything in the spawn queue
        if (m_SpawnQueue.Count > 0)
        {
            var spawnData = m_SpawnQueue.Dequeue();
            // If we have the spawn data in the pool
            if (m_SpawnedItems.ContainsKey(spawnData))
            {
                // If we aren't at our limit
                if (m_SpawnedItems[spawnData].Count < spawnData.SpawnCap)
                {
                    // Find a random spawn position
                    var pos = m_LayersController.GetRandomPointOnLayers(spawnData.Layers);
                    // If there is valid position to spawn at
                    if (pos != m_LayersController.DefaultPos)
                    {
                        var spawned = spawnData.Spawn();
                        spawned.transform.position = pos + new Vector3(0f, m_SpawnHeight, 0f);
                        spawned.transform.rotation = Quaternion.Euler(90f, UnityEngine.Random.Range(0f, 360f), 0f);
                        spawned.transform.SetParent(m_SpawnObjectParent.transform);
                        m_SpawnedItems[spawnData].Add(spawned);
                        spawned.GetComponent<SpriteRenderer>().enabled = m_PlantsToggle;
                    }
                }
            }
            else
            {
                Debug.LogError("Encountered unregistered SpawnData item : " + spawnData.Layers + " : " + spawnData.name);
            }
        }
    }
    
    /// <summary>
    /// Toggles enabling/disabling of plants.  Simply enables their SpriteRenderer
    /// </summary>
    /// <param name="enable">Turn on or off</param>
    private void EnablePlants(bool enable)
    {
        foreach (var kvp in m_SpawnedItems)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                if (kvp.Value[i].GetComponent<Plant>() != null)
                {
                    kvp.Value[i].GetComponent<SpriteRenderer>().enabled = enable;
                }
            }
        }
    }

    /// <summary>
    /// Toggles enabling/disabling of animals.  Simply enables their SpriteRenderer
    /// </summary>
    /// <param name="enable">Turn on or off</param>
    private void EnableAnimals(bool enable)
    {
        foreach (var kvp in m_SpawnedItems)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                if (kvp.Value[i].GetComponent<Animal>() != null)
                {
                    kvp.Value[i].GetComponent<SpriteRenderer>().enabled = enable;
                }
            }
        }
    }

    /// <summary>
    /// Handle the screen flipping vertically
    /// </summary>
    /// <param name="flipped">Is the screen flipped?</param>
    private void HandleVerticalFlipped(bool flipped)
    {
        // For all spawned items
        foreach (var kvp in m_SpawnedItems)
        {
            // flip their z position
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                var pos = kvp.Value[i].transform.position;
                pos.z = -pos.z;
                kvp.Value[i].transform.position = pos;
            }
        }
    }

    /// <summary>
    /// Handle the screen flipping horizontally
    /// </summary>
    /// <param name="flipped">Is the screen flipped?</param>
    private void HandleHorizontalFlipped(bool flipped)
    {
        // For all spawned items
        foreach (var kvp in m_SpawnedItems)
        {
            // flip their x position
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                var pos = kvp.Value[i].transform.position;
                pos.x = -pos.x;
                kvp.Value[i].transform.position = pos;
            }
        }
    }

    /// <summary>
    /// Handle a SpawnData item saying it should spawn
    /// </summary>
    /// <param name="spawnData">The SpawnData item that wants to spawn</param>
    private void HandleShouldSpawnEvent(SpawnData spawnData)
    {
        // Put the item into the queue
        m_SpawnQueue.Enqueue(spawnData);
    }

    /// <summary>
    /// Handles an item despawning by putting it back into the pool
    /// </summary>
    /// <param name="spawnData">The SpawnData the Object belongs to</param>
    /// <param name="spawnedObject">The despawned object</param>
    private void HandleSpawnObjectDespawnedEvent(SpawnData spawnData, GameObject spawnedObject)
    {
        if (m_SpawnedItems.ContainsKey(spawnData))
        {
            if (!m_SpawnedItems[spawnData].Remove(spawnedObject))
            {
                Debug.LogError("Attempted to remove untracked spawned object.");
            }
        }
        else
        {
            Debug.LogError("Encountered unregistered SpawnData item : " + spawnData.Layers + " : " + spawnData.name);
        }
    }
}
