using UnityEngine;

public class SpawnedObject : MonoBehaviour
{
    private SpawnData m_SpawnData;
    private DepthController m_DepthController;

    public void Init(SpawnData spawnData, DepthController depthController)
    {
        m_SpawnData = spawnData;
        m_DepthController = depthController;
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
        return m_DepthController.GetLayerAtPosition(transform.position) == m_SpawnData.Layer;
    }

    public void Despawn()
    {
        m_SpawnData.Despawn(gameObject);
    }
}
