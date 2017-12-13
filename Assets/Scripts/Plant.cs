using DG.Tweening;
using UnityEngine;

public class Plant : MonoBehaviour
{
    private Spawnable spawnable;
    private SpawnedObject spawnedObject;

    //Determin the max size of the object
    public float MaxSize;
    [SerializeField]
    private float m_TweenTime = 0.25f;
    [SerializeField]
    private Ease m_GrowEaseType = Ease.OutBounce;
    [SerializeField]
    private Ease m_ShrinkEaseType = Ease.InBack;

    //Life
    [SerializeField]
    private float life = 100f;
    private float decayRate = 10f;

    private bool m_Died = false;
    
    // Use this for initialization
    void Start()
    {
        spawnable = this.GetComponent<Spawnable>();
        spawnedObject = GetComponent<SpawnedObject>();
    }

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
        if (spawnable != null)
        {
            if (!spawnable.CheckTerrain())
            {
                life -= decayRate * Time.deltaTime;
            }
            else if (life < 100)
            {
                life += decayRate * Time.deltaTime;
            }

            if (life < 0)
            {
                //Debug.Log("kill plant");
                spawnable.Die();
            }
        }
        if (spawnedObject != null)
        {
            if (!spawnedObject.CheckDepth())
            {
                life -= decayRate * Time.deltaTime;
            }
            else if (life < 100)
            {
                life += decayRate * Time.deltaTime;
            }
            life = Mathf.Clamp(life, 0f, 100f);

            if (life <= 0)
            {
                m_Died = true;
                transform.DOScale(0.00001f, m_TweenTime).SetEase(m_ShrinkEaseType).OnComplete(() =>
                {
                    spawnedObject.Despawn();
                });
            }
        }
    }
}
