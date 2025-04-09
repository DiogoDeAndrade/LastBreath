using System;
using UC;
using UnityEngine;

public class CurrentLineFX : MonoBehaviour
{
    [SerializeField] private float      tSpeed;
    [SerializeField] private float      fadeTime = 0.1f;

    private float                       currentT = 0.0f;
    private Vector3                     offset;
    private PathXY                      path;
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

        trailRenderer.FadeTo(1.0f, 1.0f, fadeTime, "TrailFade");
    }

    void Start()
    {
        path = GetComponentInParent<PathXY>();

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

    void UpdatePos()
    {
        transform.position = path.EvaluateWorld(currentT).xy0() + offset;
    }

    void InitTrailRenderer()
    {
        if (trailRenderer == null)
        {
            trailRenderer = GetComponent<TrailRenderer>();
        }
    }
}
