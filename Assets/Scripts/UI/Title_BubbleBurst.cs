using NaughtyAttributes;
using UnityEngine;

public class Title_BubbleBurst : MonoBehaviour
{
    [SerializeField, MinMaxSlider(1.0f, 20.0f)]     private Vector2 cooldown;
    [SerializeField, MinMaxSlider(-300.0f, 300.0f)] private Vector2 xCoord;


    float timer = 0.0f;
    ParticleSystem ps;

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0.0f)
        {
            transform.position = new Vector3(xCoord.Random(), transform.position.y, transform.position.z);
            ps.Play();

            timer = cooldown.Random();
        }
    }
}
