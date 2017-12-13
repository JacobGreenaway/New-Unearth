using System.Collections.Generic;
using UnityEngine;

public class UnearthSpawnController : MonoBehaviour
{
    [SerializeField]
    private UnearthDepthController m_DepthController;
    [SerializeField]
    private UnearthLayersController m_LayersController;
    [SerializeField]
    private GameObject m_SpawnObjectParent;
    [SerializeField]
    private float m_SpawnHeight;
    [SerializeField]
    private SpawnData[] m_Spawnables;


    private Dictionary<SpawnData, List<GameObject>> m_SpawnedItems = new Dictionary<SpawnData, List<GameObject>>();

    private bool m_PlantsToggle = true;
    private bool m_AnimalsToggle = true;

    private Queue<SpawnData> m_SpawnQueue = new Queue<SpawnData>();

    private void Awake()
    {
        for (int i = 0; i < m_Spawnables.Length; i++)
        {
            m_Spawnables[i].ShouldSpawnEvent += HandleShouldSpawnEvent;
            m_Spawnables[i].SpawnObjectDespawnedEvent += HandleSpawnObjectDespawnedEvent;
            m_SpawnedItems.Add(m_Spawnables[i], new List<GameObject>());
        }

        SettingsController.Instance.Current.FlipHorizontalChanged += HandleHorizontalFlipped;
        SettingsController.Instance.Current.FlipVerticalChanged += HandleVerticalFlipped;
    }

    private void LateUpdate()
    {
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

        if (m_SpawnQueue.Count > 0)
        {
            var spawnData = m_SpawnQueue.Dequeue();
            if (m_SpawnedItems.ContainsKey(spawnData))
            {
                // If we aren't at our limit
                if (m_SpawnedItems[spawnData].Count < spawnData.SpawnCap)
                {
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

    private void HandleVerticalFlipped(bool flipped)
    {
        foreach (var kvp in m_SpawnedItems)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                var pos = kvp.Value[i].transform.position;
                pos.z = -pos.z;
                kvp.Value[i].transform.position = pos;
            }
        }
    }

    private void HandleHorizontalFlipped(bool flipped)
    {
        foreach (var kvp in m_SpawnedItems)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                var pos = kvp.Value[i].transform.position;
                pos.x = -pos.x;
                kvp.Value[i].transform.position = pos;
            }
        }
    }

    private void HandleShouldSpawnEvent(SpawnData spawnData)
    {
        m_SpawnQueue.Enqueue(spawnData);
    }

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
