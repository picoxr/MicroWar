using MicroWar;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshVisualizer : MonoBehaviour
{
    public Material navMeshVisualMaterial;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh navigationMesh;

    public Transform generatedMeshOffsetReference;

    public NavMeshSurface navMesh;

   private void Start()
    {
        meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
        meshFilter = this.gameObject.AddComponent<MeshFilter>();
        navigationMesh = new Mesh();

        meshRenderer.material = navMeshVisualMaterial;

        GameManager.Instance.EnvironmentManager.OnBattlegroundRepositioned += GenerateNavMeshVisual;
        this.gameObject.SetActive(false);
    }

    private void GenerateNavMeshVisual()
    {
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        navigationMesh.SetVertices(triangulation.vertices);
        navigationMesh.SetIndices(triangulation.indices, MeshTopology.Triangles, 0);
        meshFilter.mesh = navigationMesh;

        transform.SetPositionAndRotation(navMesh.navMeshData.position - navMesh.center / 2, generatedMeshOffsetReference.rotation);
        this.gameObject.SetActive(true);
    }



}
