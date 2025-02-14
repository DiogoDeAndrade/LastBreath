using Mono.Cecil.Cil;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField, Header("Death")] 
    private LootList        lootList;
    [SerializeField]
    private Resource        resourcePrefab;
    [SerializeField] 
    private ParticleSystem  deathPS;
    [SerializeField] 
    private GameObject      explosionPrefab;

    SpriteRenderer  spriteRenderer;
    HealthSystem    healthSystem;
    SpriteEffect    spriteEffect;

    void Start()
    {
        healthSystem = GetComponent<HealthSystem>();
        spriteEffect = GetComponent<SpriteEffect>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        healthSystem.onHit += HealthSystem_onHit;
        healthSystem.onDead += HealthSystem_onDead;
    }

    private void HealthSystem_onHit(HealthSystem.DamageType damageType, float damage, Vector3 damagePosition, Vector3 hitDirection, GameObject damageSource)
    {
        spriteEffect.FlashInvert(0.1f);
    }

    private void HealthSystem_onDead(GameObject damageSource)
    {
        if (deathPS) deathPS.Play();
        if (explosionPrefab) Instantiate(explosionPrefab, transform.position, transform.rotation);
        spriteRenderer.FadeTo(new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.1f);

        if (lootList)
        {
            var item = lootList.Get();
            if (item != null)
            {
                var res = Instantiate(resourcePrefab, transform.position, transform.rotation);
                res.SetType(item);
                res.RandomThrow();
                res.duration = 20.0f;
            }
        }

        Destroy(gameObject, 1.0f);
    }
}
