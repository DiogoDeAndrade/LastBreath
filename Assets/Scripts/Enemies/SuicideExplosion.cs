using UnityEngine;
using System.Collections;
using NaughtyAttributes;

public class SuicideExplosion : ProximityAttack
{
    [SerializeField, Header("Suicide Explosion")]
    private float        explosionDelay = 0.5f;
    [SerializeField]
    private GameObject   explosionPrefab;

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
