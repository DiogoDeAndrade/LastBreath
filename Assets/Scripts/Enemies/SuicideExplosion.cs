using UnityEngine;
using System.Collections;
using UC;

public class SuicideExplosion : ProximityAttack
{
    [SerializeField, Header("Suicide Explosion")]
    private float        explosionDelay = 0.5f;
    [SerializeField]
    private GameObject   explosionPrefab;
    [SerializeField]
    private AudioClip   triggerSound;

    Animator        animator;
    Locomotion      locomotion;

    private void Start()
    {
        animator = GetComponent<Animator>();
        locomotion = GetComponent<SquidLocomotion>();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override bool Execute(HealthSystem target)
    {
        animator.SetTrigger("Explode");
        if (triggerSound) SoundManager.PlaySound(SoundType.PrimaryFX, triggerSound, 1.0f, 1.0f);
        locomotion.Stop();

        if (explosionPrefab)
        {
            StartCoroutine(ExplodeCR());
        }

        return true;
    }

    IEnumerator ExplodeCR()
    {
        yield return new WaitForSeconds(explosionDelay);

        Instantiate(explosionPrefab, transform.position, transform.rotation);

        Destroy(gameObject);
    }
}
