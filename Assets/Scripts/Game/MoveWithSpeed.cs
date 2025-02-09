using UnityEngine;

public class MoveWithSpeed : MonoBehaviour
{
    [SerializeField] private float futureTime = 1.0f;

    Rigidbody2D rb;
    void Start()
    {
        rb = GetComponentInParent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = transform.right * rb.linearVelocity * futureTime;
    }
}
