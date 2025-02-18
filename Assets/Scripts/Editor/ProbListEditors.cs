
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ResourceDataProbList))]
public class ResourceDataProbListDrawer : ProbListPropertyDrawer<ResourceData>
{

}


[CustomPropertyDrawer(typeof(EnemyProbList))]
public class EnemyProbListDrawer : ProbListPropertyDrawer<Enemy>
{

}

