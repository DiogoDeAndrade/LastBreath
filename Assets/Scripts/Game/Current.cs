using NaughtyAttributes;
using UC;
using UnityEngine;
using System.Collections.Generic;

public class Current : MonoBehaviour
{
    [SerializeField]
    private int             lineCount = 5;
    [SerializeField, MinMaxSlider(0.5f, 10.0f)]
    private Vector2         startTime;
    [SerializeField]
    private float           range = 20.0f;

    [SerializeField] 
    private CurrentLineFX   lineFXPrefab;

    private List<CurrentLineFX> lines = new();

    void Start()
    {
        for (int i = 0; i < lineCount; i++)
        {
            Invoke(nameof(NewCurrentTrail), startTime.Random());
        }
    }

    void NewCurrentTrail()
    {
        var newTrail = Instantiate(lineFXPrefab, transform);

        InitTrail(newTrail);

        lines.Add(newTrail);
    }

    void InitTrail(CurrentLineFX trail)
    {
        float startT = Random.Range(0.0f, 1.0f);
        float endT= Random.Range(0.0f, 1.0f);
        trail.Init(Mathf.Min(startT, endT), Mathf.Max(startT, endT), Random.insideUnitCircle.xy0() * range);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var line in lines)
        {
            if (!line.isRunning)
            {
                InitTrail(line);
            }
        }
    }
}
