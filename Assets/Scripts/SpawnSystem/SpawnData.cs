using System;
using System.Collections.Generic;
using UnityEngine;
using Layers = UnearthLayersController.Layers;

/// <summary>
/// Contains information for spawning animal and plant prefabs
/// </summary>
public class SpawnData : MonoBehaviour
{
    [SerializeField]
    private UnearthLayersController m_LayersController;
    // Prefab we want to spawn
    [SerializeField]
    private GameObject m_SpawnPrefab;
    // Layers that the object can spawn/live in
    [SerializeField]
    [EnumFlag]
    private Layers m_Layers;
    public Layers Layers { get { return m_Layers; } }
    // Maximum number allowed to be spawned at any one time.
    [SerializeField]
    private int m_SpawnCap;
    public int SpawnCap {  get { return m_SpawnCap; } }

    // How often will we try to spawn one?
    [SerializeField]
    private float m_SpawnFrequency = 5f;
    // How much do we want to vary on top of the spawn frequency?
    [SerializeField]
    private float m_SpawnVariance = 1f;

    // Spawn Timer
    private float m_Timer;
    // The next Time that we will attempt a spawn
    private float m_NextSpawnTime;

    /// <summary>
    /// Fired when a Spawn Timer is hit.
    /// </summary>
    public event Action<SpawnData> ShouldSpawnEvent;
    /// <summary>
    /// Fired when one of our spawned objects is despawned
    /// </summary>
    public event Action<SpawnData, GameObject> SpawnObjectDespawnedEvent;

    /// <summary>
    /// Pool of spawned items for re-use
    /// </summary>
    private List<GameObject> m_SpawnedPool = new List<GameObject>();

    /// <summary>
    /// Called by Unity on Object creation
    /// </summary>
    private void Awake()
    {
        m_Timer = 0f;
        CalcNextSpawnTime();
    }

    /// <summary>
    /// Calculates the next spawn attempt time, including random variance
    /// </summary>
    private void CalcNextSpawnTime()
    {
        m_NextSpawnTime = m_SpawnFrequency + UnityEngine.Random.Range(0f, m_SpawnVariance);
    }

    /// <summary>
    /// Called by Unity once per frame
    /// </summary>
    private void Update()
    {
        m_Timer += Time.deltaTime;

        if (m_Timer >= m_NextSpawnTime)
        {
            ShouldSpawnEvent?.Invoke(this);
            m_Timer = 0f;
            CalcNextSpawnTime();
        }
    }

    /// <summary>
    /// Spawn an instance of the prefab
    /// </summary>
    /// <returns>The spawned object</returns>
    public GameObject Spawn()
    {
        GameObject spawn = null;
        // If we have unused items in our spawn pool
        if (m_SpawnedPool.Count > 0)
        {
            // Grab the first item in the pool and reset it.
            spawn = m_SpawnedPool[0];
            spawn.SetActive(true);
            spawn.GetComponent<SpawnedObject>().Reset();
            m_SpawnedPool.RemoveAt(0);
        }
        else
        {
            // Instantiate new instance of prefab, and initialize
            spawn = Instantiate(m_SpawnPrefab);
            spawn.GetComponent<SpawnedObject>().Init(this, m_LayersController);
            spawn.GetComponent<SpawnedObject>().Reset();
        }
        return spawn;
    }

    /// <summary>
    /// Despawns the object and returns it to the pool for re-use
    /// </summary>
    /// <param name="go"></param>
    public void Despawn(GameObject go)
    {
        go.SetActive(false);
        m_SpawnedPool.Add(go);
        SpawnObjectDespawnedEvent?.Invoke(this, go);
    }
}