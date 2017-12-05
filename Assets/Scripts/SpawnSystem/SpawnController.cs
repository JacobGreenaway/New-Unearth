using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpawnController : MonoBehaviour
{
    [SerializeField]
    private DepthController m_DepthController;
    [SerializeField]
    private GameObject m_SpawnObjectParent;
    [SerializeField]
    private SpawnData[] m_Spawnables;

    private Dictionary<SpawnData, List<GameObject>> m_SpawnedItems = new Dictionary<SpawnData, List<GameObject>>();

    private bool m_PlantsToggle = true;
    private bool m_AnimalsToggle = true;

    private void Awake()
    {
        for (int i = 0; i < m_Spawnables.Length; i++)
        {
            m_Spawnables[i].ShouldSpawnEvent += HandleShouldSpawnEvent;
            m_Spawnables[i].SpawnObjectDespawnedEvent += HandleSpawnObjectDespawnedEvent;
            m_SpawnedItems.Add(m_Spawnables[i], new List<GameObject>());
        }
    }

    private void Update()
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
            foreach(var kvp in m_SpawnedItems)
            {
                for (int i = kvp.Value.Count - 1; i >= 0; i--)
                {
                    kvp.Value[i].GetComponent<SpawnedObject>().Despawn();
                }
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
            Debug.LogError("Encountered unregistered SpawnData item : " + spawnData.Layer + " : " + spawnData.name);
        }
    }

    private void HandleShouldSpawnEvent(SpawnData spawnData)
    {
        if (m_SpawnedItems.ContainsKey(spawnData))
        {
            // If we aren't at our limit
            if (m_SpawnedItems[spawnData].Count < spawnData.SpawnCap)
            {
                var pos = m_DepthController.GetRandomPointOnLayer(spawnData.Layer);
                // If there is valid position to spawn at
                if (pos != m_DepthController.DefaultPos)
                {
                    var spawned = spawnData.Spawn();
                    spawned.transform.position = pos;
                    spawned.transform.rotation = Quaternion.Euler(90f, UnityEngine.Random.Range(0f, 360f), 0f);
                    spawned.transform.SetParent(m_SpawnObjectParent.transform);
                    m_SpawnedItems[spawnData].Add(spawned);
                    spawned.GetComponent<SpriteRenderer>().enabled = m_PlantsToggle;
                }
            }
        }
        else
        {
            Debug.LogError("Encountered unregistered SpawnData item : " + spawnData.Layer + " : " + spawnData.name);
        }
    }
}
