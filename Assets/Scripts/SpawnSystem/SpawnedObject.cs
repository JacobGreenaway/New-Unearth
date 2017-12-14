using UnityEngine;

/// <summary>
/// Tracks the parent SpawnData and returns when despawned
/// </summary>
public class SpawnedObject : MonoBehaviour
{
    // Parent SpawnData object
    private SpawnData m_SpawnData;
    private UnearthLayersController m_LayersController;

    /// <summary>
    /// Initialize the object
    /// </summary>
    /// <param name="spawnData">Parent SpawnData object</param>
    /// <param name="layersController">LayersController reference</param>
    public void Init(SpawnData spawnData, UnearthLayersController layersController)
    {
        m_SpawnData = spawnData;
        m_LayersController = layersController;
    }

    /// <summary>
    /// Resets the object to it's initial state
    /// </summary>
    public void Reset()
    {
        var plant = GetComponent<Plant>();
        if(plant != null)
        {
            plant.Reset();
        }
    }

    /// <summary>
    /// Check the Layer value at our current position
    /// </summary>
    /// <returns>True if in a valid layer, else false.</returns>
    public bool CheckDepth()
    {
        return m_SpawnData.Layers.HasFlag(m_LayersController.GetLayerAtPosition(transform.position));
    }

    /// <summary>
    /// Returns the object to it's SpawnData pool
    /// </summary>
    public void Despawn()
    {
        m_SpawnData.Despawn(gameObject);
    }
}
