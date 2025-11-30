using UnityEngine;
using UnityEditor; // Required for the Editor-specific MenuItem

public class ProceduralScatterer : MonoBehaviour
{
    [Header("Placement Settings")]
    public LayerMask groundLayer; // Make sure your ground layer is selected
    public float scatterRadius = 50f;
    public float minimumDistance = 1f;

    [Header("Grass/Detail Settings")]
    public GameObject grassPrefab;
    [Range(10, 500)]
    public int grassCount = 100;
    public float grassScaleMin = 0.5f;
    public float grassScaleMax = 1.2f;

    [Header("Tree Settings")]
    public GameObject treePrefab;
    [Range(1, 50)]
    public int treeCount = 10;
    public float treeScaleMin = 0.8f;
    public float treeScaleMax = 1.5f;

    // --- EDITOR FUNCTIONALITY ---

    [ContextMenu("Scatter All")]
    public void ScatterAll()
    {
        ClearObjects(transform.Find("GrassContainer"));
        ClearObjects(transform.Find("TreeContainer"));

        ScatterGrass();
        ScatterTrees();

        Debug.Log("Procedural scattering complete!");
    }

    [ContextMenu("Clear All")]
    public void ClearAll()
    {
        ClearObjects(transform.Find("GrassContainer"));
        ClearObjects(transform.Find("TreeContainer"));

        Debug.Log("Procedurally scattered objects cleared.");
    }

    // --- SCATTERING LOGIC ---

    private void ScatterGrass()
    {
        if (grassPrefab == null) return;
        Transform container = GetOrCreateContainer("GrassContainer");

        float placementArea = scatterRadius * scatterRadius;

        for (int i = 0; i < grassCount; i++)
        {
            Vector3 randomPos = GetRandomPositionOnGround(placementArea, minimumDistance);
            if (randomPos != Vector3.zero)
            {
                // Instantiate the object
                GameObject newGrass = Instantiate(grassPrefab, randomPos, Quaternion.identity, container);

                // Randomize scale
                float scale = Random.Range(grassScaleMin, grassScaleMax);
                newGrass.transform.localScale = Vector3.one * scale;

                // Randomize rotation on the Y-axis
                newGrass.transform.Rotate(0, Random.Range(0f, 360f), 0);

                // Mark the object as part of the scene (Editor-only)
#if UNITY_EDITOR
                Undo.RegisterCreatedObjectUndo(newGrass, "Scatter Grass");
#endif
            }
        }
    }

    private void ScatterTrees()
    {
        if (treePrefab == null) return;
        Transform container = GetOrCreateContainer("TreeContainer");

        float placementArea = scatterRadius * scatterRadius;

        for (int i = 0; i < treeCount; i++)
        {
            Vector3 randomPos = GetRandomPositionOnGround(placementArea, minimumDistance * 5); // Trees need more space
            if (randomPos != Vector3.zero)
            {
                // Instantiate the object
                GameObject newTree = Instantiate(treePrefab, randomPos, Quaternion.identity, container);

                // Randomize scale
                float scale = Random.Range(treeScaleMin, treeScaleMax);
                newTree.transform.localScale = Vector3.one * scale;

                // Randomize rotation on the Y-axis
                newTree.transform.Rotate(0, Random.Range(0f, 360f), 0);

#if UNITY_EDITOR
                Undo.RegisterCreatedObjectUndo(newTree, "Scatter Tree");
#endif
            }
        }
    }

    // --- UTILITY FUNCTIONS ---

    private Vector3 GetRandomPositionOnGround(float maxRadiusSquared, float minDistance)
    {
        Vector3 point = Vector3.zero;

        // Safety loop to prevent infinite loop if placement is impossible
        for (int attempt = 0; attempt < 100; attempt++)
        {
            // 1. Generate a random point within a circle around the ground object
            float radius = Mathf.Sqrt(Random.Range(0f, maxRadiusSquared));
            float angle = Random.Range(0f, 360f);

            float x = transform.position.x + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float z = transform.position.z + radius * Mathf.Sin(angle * Mathf.Deg2Rad);

            point = new Vector3(x, transform.position.y + 100f, z); // Start high above the ground

            // 2. Raycast down to find the ground surface
            RaycastHit hit;
            if (Physics.Raycast(point, Vector3.down, out hit, 200f, groundLayer))
            {
                // Optional: Check distance to other placed objects (performance heavy)
                // For simplicity, this script relies on a general minimumDistance setting.

                return hit.point;
            }
        }
        return Vector3.zero; // Failed to find a suitable spot
    }

    private Transform GetOrCreateContainer(string name)
    {
        Transform container = transform.Find(name);
        if (container == null)
        {
            GameObject containerGO = new GameObject(name);
            container = containerGO.transform;
            container.SetParent(this.transform);
            container.localPosition = Vector3.zero;

#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(containerGO, "Create Scatter Container");
#endif
        }
        return container;
    }

    private void ClearObjects(Transform container)
    {
        if (container != null)
        {
            // Iterate backwards to avoid issues when destroying children
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(container.GetChild(i).gameObject);
            }
        }
    }
}