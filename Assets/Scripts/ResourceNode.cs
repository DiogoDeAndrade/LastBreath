using NaughtyAttributes;
using UnityEngine;
using System.Collections.Generic;
using Mono.Cecil;
using System.Runtime.CompilerServices;

public class ResourceNode : MonoBehaviour
{
    [SerializeField] 
    private bool                unlimitedCharges = true;
    [SerializeField, HideIf(nameof(unlimitedCharges))] 
    private int                 nCharges = 4;
    [SerializeField]
    private float               chargeTime = 10.0f;
    [SerializeField]
    private ResourceDataProbList    allowedTypes;
    [SerializeField]
    private bool                randomizeOnEachSpawn;
    [SerializeField]
    private Resource            resourcePrefab;

    private float                       chargeTimer;
    private Resource                    resource;
    private ResourceData                type;
    private SpriteRenderer              spriteRenderer;
    private Tweener.BaseInterpolator    scaleEffect;

    void Start()
    {
        chargeTimer = 0;

        spriteRenderer = GetComponent<SpriteRenderer>();
        SelectType();
    }

    void SelectType()
    {
        type = allowedTypes.Get();
        spriteRenderer.sprite = type.nodeImage;
    }

    // Update is called once per frame
    void Update()
    {
        if (resource == null)
        {
            if ((unlimitedCharges) || (nCharges > 0))
            chargeTimer -= Time.deltaTime;
            if (chargeTimer <= 0.0f)
            {
                if (randomizeOnEachSpawn) SelectType();
                resource = Instantiate(resourcePrefab, transform.position, transform.rotation, transform);
                resource.SetType(type);
                chargeTimer = chargeTime;
                if (!unlimitedCharges) nCharges--;

                transform.ScaleTo(Vector3.one, 0.2f).EaseFunction(Ease.OutBack);
            }
            else
            {
                if ((transform.localScale.x == 1.0f) && ((scaleEffect == null) || (scaleEffect.isFinished)))
                {
                    scaleEffect = transform.ScaleTo(Vector3.zero, 0.2f);
                }
            }
        }
    }
}
