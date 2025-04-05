using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEditor;
using System;
using UC;

public class ChainLightning : ProximityAttack
{
    [SerializeField, Header("Chain Lightning")] 
    private int            maxHops = 3;
    [SerializeField] 
    private float          maxDistancePerHop = 200.0f;
    [SerializeField] 
    private float          moveSpeed = 50.0f;
    [SerializeField] 
    private float          fadeTime = 0.5f;
    [SerializeField] 
    private Hypertag       playerTag;
    [SerializeField] 
    private float          baseDamage = 5;
    [SerializeField] 
    private float          damagePerHop = 5;
    [SerializeField] 
    private float          freezeTimePerEnemy = 0.5f;
    [SerializeField] 
    private Hypertag       chainTag;
    [SerializeField] 
    private LineRenderer   lightningPrefab;
    [SerializeField]
    private AudioClip       lightningSnd;

    class TreeNode
    {
        public LineRenderer     lineRenderer;
        public float            startTime;
        public float            fadeTime;
        public Transform        transform;
        public HealthSystem     healthSystem;
        public List<TreeNode>   children;
    };

    TreeNode        currentTree;
    HashSet<Enemy>  enemyList;

    static List<LineRenderer> cacheLineRenderers;

    protected override void Update()
    {
        base.Update();

        if (currentTree == null) return;

        UpdateNode(null, currentTree, Time.deltaTime, 0);

        if (IsComplete(currentTree))
        {
            Clear();
        }
    }

    private void OnDestroy()
    {
        Clear();
    }

    void UpdateNode(TreeNode parentNode, TreeNode currentNode, float elapsedTime, int nHops)
    {
        if (parentNode != null)
        {
            Vector3 parentPos = parentNode.transform.position;
            Vector3 targetPos = (currentNode.transform) ? (currentNode.transform.position) : (parentPos);

            float speedAura = Submarine.GetAura(parentPos, -1);

            currentNode.startTime += elapsedTime * speedAura;

            Vector3 currentPos = Vector3.MoveTowards(parentPos, targetPos, moveSpeed * currentNode.startTime);

            if (Vector3.Distance(currentPos, targetPos) < 1e-3f)
            {
                currentNode.fadeTime += elapsedTime * speedAura;

                float tAlpha1 = 1.0f - Mathf.Clamp01(currentNode.fadeTime / fadeTime);
                float tAlpha2 = 1.0f - Mathf.Clamp01((currentNode.fadeTime - fadeTime) / fadeTime);

                currentNode.lineRenderer.startColor = new Color(1, 1, 1, tAlpha1);
                currentNode.lineRenderer.endColor = new Color(1, 1, 1, tAlpha2);

                UpdateChildren(currentNode, elapsedTime, nHops);

                if (currentNode.healthSystem)
                {
                    // Deal damage - if beyond range, no damage happens
                    if (Vector3.Distance(parentNode.transform.position, currentNode.healthSystem.transform.position) < maxDistancePerHop)
                    {
                        float damage = baseDamage + nHops * damagePerHop;
                        currentNode.healthSystem.DealDamage(HealthSystem.DamageType.Burst, damage, parentNode.transform.position, Vector3.zero, parentNode.transform.gameObject);

                        if (freezeTimePerEnemy > 0)
                        {
                            Submarine sub = currentNode.healthSystem.GetComponent<Submarine>();
                            if (sub)
                            {
                                sub.FreezeControl(freezeTimePerEnemy * nHops);
                                sub.EnableLightningEffect();
                            }
                        }
                    }

                    // Remove health system, since I don't want damage for multiple frames
                    currentNode.healthSystem = null;
                }
            }

            UpdateLineRenderer(currentNode.lineRenderer, parentPos, currentPos);
        }  
        else
        {
            UpdateChildren(currentNode, elapsedTime, nHops);
        }
    }

    bool IsComplete(TreeNode currentNode)
    {
        if (currentNode.lineRenderer != null)
        {
            if (currentNode.fadeTime < 2.0f * fadeTime) return false;
        }

        if (currentNode.transform == null) return true;

        if (currentNode.children != null)
        {
            foreach (var child in currentNode.children)
            {
                if (!IsComplete(child)) return false;
            }
        }

        return true;
    }

    private void UpdateChildren(TreeNode currentNode, float elapsedTime, int nHops)
    {
        if (currentNode.children != null)
        {
            foreach (var child in currentNode.children)
            {
                UpdateNode(currentNode, child, elapsedTime, nHops + 1);
            }
        }
    }

    [Button("Test Trigger")]
    private void Execute()
    {
        HealthSystem    target = null;
        float           minDist = float.MaxValue;

        var objects = HypertaggedObject.GetInRadius<HealthSystem>(playerTag, transform.position, maxDistancePerHop);
        foreach (var obj in objects)
        {
            float d = Vector3.Distance(transform.position, obj.transform.position);
            if (d < minDist)
            {
                minDist = d;
                target = obj;
            }
        }

        Execute(target);
    }

    protected override bool Execute(HealthSystem target)
    {
        if (currentTree != null) return false;

        currentTree = new();
        currentTree.transform = transform;

        Build(currentTree, maxHops, target, new HashSet<Enemy>());

        if (lightningSnd)
        {
            SoundManager.PlaySound(SoundType.PrimaryFX, lightningSnd, 0.75f, UnityEngine.Random.Range(0.75f, 1.25f));
        }

        return true;
    }

    void Build(TreeNode parent, int remainingHops, HealthSystem target, HashSet<Enemy> alreadyChecked)
    {
        if (remainingHops == 0) return;

        var objects = HypertaggedObject.GetInRadius<Enemy>(chainTag, parent.transform.position, maxDistancePerHop);
        foreach (var obj in objects)
        {
            if (alreadyChecked.Contains(obj)) continue;

            TreeNode newNode = new()
            {
                lineRenderer = GetLineRenderer(parent.transform.position),
                startTime = 0.0f,
                fadeTime = 0.0f,
                transform = obj.transform
            };

            alreadyChecked.Add(obj);

            if (parent.children == null) parent.children = new();
            parent.children.Add(newNode);
        }
        
        if (parent.children != null)
        {
            foreach (var obj in parent.children)
            {
                Build(obj, remainingHops - 1, target, alreadyChecked);
            }
        }

        // Add target after building the rest of the chain
        if (target)
        {
            if (Vector3.Distance(target.transform.position, parent.transform.position) < maxDistancePerHop)
            {
                TreeNode newNode = new()
                {
                    lineRenderer = GetLineRenderer(parent.transform.position),
                    startTime = 0.0f,
                    fadeTime = 0.0f,
                    healthSystem = target,
                    transform = target.transform
                };

                if (parent.children == null) parent.children = new();
                parent.children.Add(newNode);
            }
        }
    }

    private LineRenderer GetLineRenderer(Vector3 startPoint)
    {
        if (cacheLineRenderers == null) return CreateLineRenderer(startPoint);

        cacheLineRenderers.RemoveAll((lr) => lr == null);
        if (cacheLineRenderers.Count == 0) return CreateLineRenderer(startPoint);

        var lineRenderer = cacheLineRenderers.PopFirst();
        lineRenderer.gameObject.SetActive(true);
        lineRenderer.enabled = false;
        UpdateLineRenderer(lineRenderer, startPoint, startPoint);

        return lineRenderer;
    }

    private LineRenderer CreateLineRenderer(Vector3 p)
    {
        var lineRenderer = Instantiate(lightningPrefab);

        // Not an error, the lightning starts on p1 and moves towards p2
        UpdateLineRenderer(lineRenderer, p, p);

        return lineRenderer;
    }

    private void UpdateLineRenderer(LineRenderer lineRenderer, Vector3 p1, Vector3 p2)
    {
        // Setup margin because of rounding endpoints
        if (lineRenderer.numCapVertices > 0)
        {
            Vector3 delta = (p2 - p1);
            if (delta.sqrMagnitude > 0) delta = delta.normalized * lineRenderer.widthCurve.Evaluate(0) * lineRenderer.widthMultiplier * 0.5f;

            lineRenderer.SetPosition(0, p1 + delta);
            lineRenderer.SetPosition(1, p2 - delta);
        }
        else
        {
            // No caps
            lineRenderer.SetPosition(0, p1);
            lineRenderer.SetPosition(1, p2);
        }

        lineRenderer.enabled = (p1 != p2);    
    }

    private void DestroyLineRenderer(LineRenderer lineRenderer)
    {
        if (cacheLineRenderers == null) cacheLineRenderers = new();

        lineRenderer.gameObject.SetActive(false);
        cacheLineRenderers.Add(lineRenderer);
    }

    [Button("Clear")]
    private void Clear()
    {
        Clear(currentTree);
        currentTree = null;
    }

    private void Clear(TreeNode treeNode)
    {
        if (treeNode == null) return;

        if (treeNode.lineRenderer)
        {
            DestroyLineRenderer(treeNode.lineRenderer);
        }

        if (treeNode.children != null)
        {
            foreach (var child in treeNode.children)
            {
                Clear(child);
            }
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, maxDistancePerHop);
        DebugHelpers.DrawTextAt(transform.position + Vector3.left * maxDistancePerHop, Vector3.up * 20.0f, 10, Gizmos.color, "Hop Distance");
    }
}
