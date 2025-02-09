
using UnityEngine;

public class Debries : MonoBehaviour
{
    [SerializeField] private float      speed;
    [SerializeField] private Transform  explosionCenter;
    [SerializeField] private float      timeToSleep;

    float       sleepTimer;
    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = (transform.position - explosionCenter.position).normalized * speed;
        rb.AddTorque(Random.Range(-360.0f, 360.0f), ForceMode2D.Impulse);

        transform.SetParent(null, true);        
    }

    private void Update()
    {
        var v = rb.linearVelocity;
        if (v.magnitude < 1e-3)
        {
            if (sleepTimer > 0)
            {
                sleepTimer -= Time.deltaTime;
                if (sleepTimer <= 0)
                {
                    SpriteRenderer sr = GetComponent<SpriteRenderer>();
                    sr.FadeTo(new Color(1.0f, 1.0f, 1.0f, 0.0f), 1.0f).Done(() => Destroy(gameObject));
                }
            }
        }
        else
        {
            sleepTimer = timeToSleep;
        }
    }
}
