using System.Collections.Generic;
using UC;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class RLAgentLearn : Agent
{
    private record FoeProbe
    {
        public Vector2  toEnemy;
        public float    isSub;
        public float    isTorpedo;
        public float    isTracker;
    }
    private record ResourceProbe
    {
        public Vector2  toResource;
        public float    type;
    }

    [SerializeField]
    private int         envProbeCount = 16;
    [SerializeField]
    private LayerMask   envProbeLayers;
    [SerializeField]
    private List<ResourceData> allResourceData;

    const int MAX_FOES = 4;
    const int MAX_RESOURCES = 8;
    const float MAX_DISTANCE = 5000.0f;

    Submarine           submarine;
    Vector3             startPos;
    Quaternion          startRot; 
    float[]             envProbes;
    List<FoeProbe>      foeProbes;
    List<ResourceProbe> resourceProbes;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        submarine = GetComponent<Submarine>();

        BehaviorParameters behaviour = GetComponent<BehaviorParameters>();
        behaviour.TeamId = submarine.playerId;

        startPos = transform.position;
        startRot = transform.rotation;

        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
        EnvironmentReset();
    }

    private void EnvironmentReset()
    {
        envProbes = new float[envProbeCount];

        // Move back to start position
        transform.position = startPos;
        transform.rotation = startRot;        
    }

    void Update()
    {
        // Update environment probes
        float angleInc = Mathf.PI * 2.0f / (envProbeCount - 1);
        for (int i = 0; i < envProbeCount; i++)
        {
            float   angle = i * angleInc;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            envProbes[i] = UpdateEnvProbe(dir);
        }

        // Update foe probes (and sort them by distance, closest first)
        foeProbes = new();
        var subs = FindObjectsByType<Submarine>(FindObjectsSortMode.None);
        foreach (var sub in subs)
        {
            if ((sub != submarine) && (sub.isAlive))
            {
                foeProbes.Add(new FoeProbe
                {
                    toEnemy = (sub.transform.position - transform.position).xy(),
                    isSub = 1.0f,
                    isTorpedo = 0.0f,
                    isTracker = 0.0f
                });
            }
        }
        var torpedos = FindObjectsByType<Projectile>(FindObjectsSortMode.None);
        foreach (var torpedo in torpedos)
        {
            if (torpedo.IsHostile(submarine.playerId))
            {
                foeProbes.Add(new FoeProbe
                {
                    toEnemy = (torpedo.transform.position - transform.position).xy(),
                    isSub = 0.0f,
                    isTorpedo = 1.0f,
                    isTracker = torpedo.isTracker ? 1.0f : 0.0f
                });
            }
        }
        foeProbes.Sort((a,b) => a.toEnemy.magnitude.CompareTo(b.toEnemy.magnitude));
        
        // Update resource probes (and sort them by distance, closest first)
        resourceProbes = new();
        var resources = FindObjectsByType<Resource>(FindObjectsSortMode.None);
        foreach (var res in resources)
        {
            resourceProbes.Add(new ResourceProbe
            {
                toResource = (res.transform.position - transform.position).xy(),
                type = ToResId(res.data)
            });
        }
        resourceProbes.Sort((a, b) => a.toResource.magnitude.CompareTo(b.toResource.magnitude));

        RequestDecision();
    }

    float ToResId(ResourceData data)
    {
        return allResourceData.IndexOf(data) / (float)(allResourceData.Count - 1);
    }

    private float UpdateEnvProbe(Vector2 dir)
    {
        var hit = Physics2D.Raycast(transform.position, dir, float.MaxValue, envProbeLayers);
        if (hit.collider == null)
        {
            return MAX_DISTANCE;
        }
        else
        {
            return hit.distance;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Sub properties (5)
        sensor.AddObservation(submarine.subData.speed / 1000.0f);
        sensor.AddObservation(submarine.subData.acceleration / 1000.0f);
        sensor.AddObservation(submarine.subData.rotationSpeed / 5000.0f);
        sensor.AddObservation(submarine.subData.drag / 10.0f);
        sensor.AddObservation(submarine.subData.health / 1000.0f);

        // Physical state (5)
        sensor.AddObservation(submarine.currentVelocity / submarine.subData.speed);
        sensor.AddObservation(submarine.normalizedHealth);
        sensor.AddObservation(submarine.inputDir);

        // Scanners (envProbeCount + MAX_FOES * 5 + MAX_RESOURCES * 3 = 32 + 20 + 24 = 76)
        for (int i = 0; i < envProbeCount; i++)
        {
            sensor.AddObservation(envProbes[i] / MAX_DISTANCE); // Normalize to 1000 units
        }
        for (int i = 0; i < MAX_FOES; i++)
        {
            if (i < foeProbes.Count)
            {
                sensor.AddObservation(foeProbes[i].toEnemy.x / MAX_DISTANCE); // Normalize to 1000 units
                sensor.AddObservation(foeProbes[i].toEnemy.y / MAX_DISTANCE);
                sensor.AddObservation(foeProbes[i].isSub);
                sensor.AddObservation(foeProbes[i].isTorpedo);
                sensor.AddObservation(foeProbes[i].isTracker);
            }
            else
            {
                sensor.AddObservation(1.0f);
                sensor.AddObservation(1.0f);
                sensor.AddObservation(0.0f);
                sensor.AddObservation(0.0f);
                sensor.AddObservation(0.0f);
            }
        }
        for (int i = 0; i < MAX_RESOURCES; i++)
        {
            if (i < resourceProbes.Count)
            {
                sensor.AddObservation(resourceProbes[i].toResource.x / 5000.0f); // Normalize to 1000 units
                sensor.AddObservation(resourceProbes[i].toResource.y / 5000.0f);
                sensor.AddObservation(resourceProbes[i].type);
            }
            else
            {
                sensor.AddObservation(1.0f);
                sensor.AddObservation(1.0f);
                sensor.AddObservation(0.0f);
            }
        }   
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector2 moveDir = new Vector2(Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f), Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f));
        submarine.inputDir = moveDir.normalized;
    }

    private void OnDrawGizmos()
    {
        if (envProbes == null) return;
        if (envProbes.Length < envProbeCount) return;
        for (int i = 0; i < envProbeCount; i++)
        {
            float angleInc = Mathf.PI * 2.0f / (envProbeCount - 1);
            float angle = i * angleInc;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Gizmos.color = Color.green;
            Gizmos.color = Color.red; 
            Gizmos.DrawLine(transform.position, transform.position.xy() + dir * envProbes[i]);
        }

        // Draw foe probes
        if (foeProbes != null)
        {
            Gizmos.color = Color.yellow;
            int count = Mathf.Min(MAX_FOES, foeProbes.Count);
            for (int i = 0; i < count; i++)
            {
                var foe = foeProbes[i];
                Gizmos.DrawLine(transform.position, transform.position.xy() + foe.toEnemy);
            }
        }

        // Draw resource probes
        if (resourceProbes != null)
        {
            Gizmos.color = Color.green;
            int count = Mathf.Min(MAX_RESOURCES, resourceProbes.Count);
            for (int i = 0; i < count; i++)
            {
                var res = resourceProbes[i];
                Gizmos.DrawLine(transform.position, transform.position.xy() + res.toResource);
            }
        }
    }
}
