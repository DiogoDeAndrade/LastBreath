using UnityEngine;

public class Locomotion : MonoBehaviour
{
    protected Vector3 targetPosition;

    public void MoveTo(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }
}
