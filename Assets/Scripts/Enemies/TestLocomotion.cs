using UnityEngine;

public class TestLocomotion : MonoBehaviour
{
    [SerializeField] private Transform target;

    Locomotion locomotion;

    void Start()
    {
        locomotion = GetComponent<Locomotion>();
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToTarget = Vector3.Distance(target.position, transform.position);
        if (distanceToTarget > 10.0f)
        {
            locomotion.MoveTo(target.position);
        }
    }
}
