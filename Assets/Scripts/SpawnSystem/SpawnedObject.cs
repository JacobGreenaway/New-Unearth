using UnityEngine;

public class SpawnedObject : MonoBehaviour
{
    private SpawnData m_SpawnData;
    private UnearthLayersController m_LayersController;

    public void Init(SpawnData spawnData, UnearthLayersController layersController)
    {
        m_SpawnData = spawnData;
        m_LayersController = layersController;
    }

    public void Reset()
    {
        var plant = GetComponent<Plant>();
        if(plant != null)
        {
            plant.Reset();
        }
    }

    public bool CheckDepth()
    {
        return m_SpawnData.Layers.HasFlag(m_LayersController.GetLayerAtPosition(transform.position));
    }

    public void Despawn()
    {
        m_SpawnData.Despawn(gameObject);
    }
}
