using System;
using UC;
using UnityEngine;

public class CurrentLineFX : MonoBehaviour
{
    [SerializeField] private float      tSpeed;
    [SerializeField] private float      fadeTime = 0.1f;
    [SerializeField] private float      alpha = 1.0f;

    private float                       currentT = 0.0f;
    private Vector3                     offset;
    private PathXY                      path;
    private BoxCollider2D               boxCollider;
    private Current                     parentCurrent;
    private TrailRenderer               trailRenderer;
    private float                       minT, maxT;
    private bool                        active;
    private Tweener.BaseInterpolator    interpolator;

    public bool isRunning => active || (trailRenderer.positionCount > 0);

    public void Init(float startT, float endT, Vector3 offset)
    {
        InitTrailRenderer();

        currentT = minT = startT;
        maxT = Mathf.Clamp01(Mathf.Max(minT + fadeTime * tSpeed, endT));
        currentT = minT = Mathf.Clamp01(Mathf.Min(minT, maxT - fadeTime * tSpeed));
        this.offset = offset;
        active = true;

        if (boxCollider)
        {
            transform.position = boxCollider.bounds.Random();
        }

        trailRenderer.FadeTo(alpha, 0.0f, fadeTime, "TrailFade");
    }

    void Start()
    {
        InitTrailRenderer();

        currentT = minT;
        UpdatePos();
    }

    // Update is called once per frame
    void Update()
    {
        trailRenderer.emitting = ((currentT > minT) && (currentT < maxT));

        currentT = Mathf.Clamp(currentT + tSpeed * Time.deltaTime, minT, maxT);

        UpdatePos();
        if (path != null)
        {
            if ((active) && (currentT == maxT))
            {
                trailRenderer.emitting = false;
                if ((interpolator == null) || (interpolator.isFinished))
                {
                    interpolator = trailRenderer.FadeTo(0.0f, 0.0f, fadeTime, "TrailFade").Done(
                        () =>
                        {
                            active = false;
                            interpolator = null;
                        });
                }
            }
        }
        else if (boxCollider)
        {
            if (active) 
            {
                if (!boxCollider.bounds.Contains(transform.position))
                {
                    if ((interpolator == null) || (interpolator.isFinished))
                    {
                        interpolator = trailRenderer.FadeTo(0.0f, 0.0f, fadeTime, "TrailFade").Done(
                            () =>
                            {
                                active = false;
                                interpolator = null;
                            });
                    }
                }
            }
        }
    }

    void UpdatePos()
    {
        if (path)
        {
            transform.position = path.EvaluateWorld(currentT).xy0() + offset;
        }
        else if ((boxCollider) && (active))
        {
            float maxSize = Mathf.Max(boxCollider.bounds.size.x, boxCollider.bounds.size.y);
            transform.position = transform.position.xy() + parentCurrent.direction * tSpeed * maxSize * Time.deltaTime;
        }
    }

    void InitTrailRenderer()
    {
        if (trailRenderer == null)
        {
            trailRenderer = GetComponent<TrailRenderer>();
        }
        path = GetComponentInParent<PathXY>();
        boxCollider = GetComponentInParent<BoxCollider2D>();
        parentCurrent = GetComponentInParent<Current>();
    }
}
