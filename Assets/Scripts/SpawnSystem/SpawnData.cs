using System;
using System.Collections.Generic;
using UnityEngine;
using Layers = UnearthLayersController.Layers;

public class SpawnData : MonoBehaviour
{
    [SerializeField]
    private UnearthLayersController m_LayersController;
    [SerializeField]
    private GameObject m_SpawnPrefab;
    [SerializeField]
    [EnumFlag]
    private Layers m_Layers;
    public Layers Layers { get { return m_Layers; } }
    [SerializeField]
    private int m_SpawnCap;
    public int SpawnCap {  get { return m_SpawnCap; } }

    [SerializeField]
    private float m_SpawnFrequency = 5f;
    [SerializeField]
    private float m_SpawnVariance = 1f;

    private float m_Timer;
    private float m_NextSpawnTime;

    public event Action<SpawnData> ShouldSpawnEvent;
    public event Action<SpawnData, GameObject> SpawnObjectDespawnedEvent;

    private List<GameObject> m_SpawnedPool = new List<GameObject>();

    private void Awake()
    {
        m_Timer = 0f;
        CalcNextSpawnTime();
    }

    private void CalcNextSpawnTime()
    {
        m_NextSpawnTime = m_SpawnFrequency + UnityEngine.Random.Range(0f, m_SpawnVariance);
    }

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

    public GameObject Spawn()
    {
        GameObject spawn = null;
        if (m_SpawnedPool.Count > 0)
        {
            spawn = m_SpawnedPool[0];
            spawn.SetActive(true);
            spawn.GetComponent<SpawnedObject>().Reset();
            m_SpawnedPool.RemoveAt(0);
        }
        else
        {
            spawn = Instantiate(m_SpawnPrefab);
            spawn.GetComponent<SpawnedObject>().Init(this, m_LayersController);
        }
        return spawn;
    }

    public void Despawn(GameObject go)
    {
        go.SetActive(false);
        m_SpawnedPool.Add(go);
        if(SpawnObjectDespawnedEvent != null)
        {
            SpawnObjectDespawnedEvent(this, go);
        }
    }
}