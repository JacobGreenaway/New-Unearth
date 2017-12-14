using DG.Tweening;
using UnityEngine;

/// <summary>
/// Grows, hides and displays a plant sprite.
/// </summary>
public class Plant : MonoBehaviour
{
    private SpawnedObject spawnedObject;

    // Determine the max size of the object
    public float MaxSize;
    // Time taken to animate tweens
    [SerializeField]
    private float m_TweenTime = 0.25f;
    // Easing function used when growing
    [SerializeField]
    private Ease m_GrowEaseType = Ease.OutBounce;
    // Easing function used when shrinking
    [SerializeField]
    private Ease m_ShrinkEaseType = Ease.InBack;

    // Life
    [SerializeField]
    private float life = 100f;
    // How quickly the plant dies when not in a valid layer.
    private float decayRate = 10f;

    // Are we dead?
    private bool m_Died = false;
    
    /// <summary>
    /// Called once by Unity during startup
    /// </summary>
    void Start()
    {
        spawnedObject = GetComponent<SpawnedObject>();
    }

    /// <summary>
    /// Called by the SpawnedObject to reset when re-spawning the object.
    /// </summary>
    public void Reset()
    {
        life = 100f;
        m_Died = false;
        transform.localScale = Vector3.one * 0.00001f;
        transform.DOScale(MaxSize, m_TweenTime).SetEase(m_GrowEaseType);
    }

    // Update is called once per frame
    void Update()
    {
        if(m_Died)
        {
            return;
        }

        if (spawnedObject != null)
        {
            // If we aren't in a valid layer
            if (!spawnedObject.CheckDepth())
            {
                // Remove life
                life -= decayRate * Time.deltaTime;
            }
            else if (life < 100)
            {
                // Increase life
                life += decayRate * Time.deltaTime;
            }
            
            // If we've died
            if (life <= 0)
            {
                m_Died = true;
                // Scale down, then despawn
                transform.DOScale(0.00001f, m_TweenTime).SetEase(m_ShrinkEaseType).OnComplete(() =>
                {
                    spawnedObject.Despawn();
                });
            }
            // Clamp life
            life = Mathf.Clamp(life, 0f, 100f);
        }
    }
}
