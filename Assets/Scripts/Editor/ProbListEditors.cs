
using UnityEditor;
using UnityEngine;

namespace UC
{

    [CustomPropertyDrawer(typeof(ResourceDataProbList))]
    public class ResourceDataProbListDrawer : ProbListPropertyDrawer<ResourceData>
    {

    }


    [CustomPropertyDrawer(typeof(EnemyProbList))]
    public class EnemyProbListDrawer : ProbListPropertyDrawer<Enemy>
    {

    }

}