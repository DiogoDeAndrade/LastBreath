using NaughtyAttributes;
using UnityEngine;

public class TestLocomotion : MonoBehaviour
{
    [SerializeField] 
    private Transform target;
    [SerializeField] 
    private bool      checkLOS;
    [SerializeField, ShowIf(nameof(checkLOS))] 
    private LayerMask layerMask;

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
            locomotion.MoveTo(target.position, checkLOS, layerMask);
        }
    }
}
