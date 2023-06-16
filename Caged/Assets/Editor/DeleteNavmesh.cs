using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

public class DeleteNavmesh : MonoBehaviour
{
        [MenuItem("Debug/Force Cleanup NavMesh")]
        public static void ForceCleanupNavMesh()
        {
            if (Application.isPlaying)
                return;
 
            NavMesh.RemoveAllNavMeshData();
        }
}
