using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.Events;

public class TableTouchZone : MonoBehaviour
{
    public string[] ValidTouchTags = { "Hand" };

    [Tooltip("Offset above table surface to avoid clipping.")]
    public float yOffset = 0.05f;

    private void OnTriggerEnter(Collider other)
    {
        DebugTag.Log(nameof(TableTouchZone), $"{other.name} touched table anchor... tag={other.tag}");

        //if (!IsValidTouch(other))
        //    return;

        DebugTag.Log(nameof(TableTouchZone), $"{other.name} confirmed touch — spawning cube.");

        var thisCol = GetComponent<Collider>();
        Vector3 direction;
        float distance;

        if (Physics.ComputePenetration(
                other, other.transform.position, other.transform.rotation,
                thisCol, thisCol.transform.position, thisCol.transform.rotation,
                out direction, out distance))
        {
            Vector3 contactPoint = other.transform.position + direction * distance;
            SpawnCubeAtPoint(contactPoint);
            DebugTag.Log(nameof(TableTouchZone), $"Contact point: {contactPoint}");
        }
    }

    private void SpawnCubeAtPoint(Vector3 position)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = position;
        cube.transform.localScale = Vector3.one * 0.05f;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(Random.value, Random.value, Random.value, 1f);
        cube.GetComponent<MeshRenderer>().material = mat;

        Destroy(cube, 5f);
    }

    private bool IsValidTouch(Collider other)
    {
        foreach (var t in ValidTouchTags)
            if (!string.IsNullOrEmpty(t) && other.CompareTag(t))
                return true;

        return false;
    }

    private void SpawnCubeAtTouch(Collider other)
    {
        
        Vector3 spawnPos;

        // Try to place cube at actual contact point (if overlapping)
        Bounds overlap = other.bounds;
        spawnPos = overlap.center;

        // Adjust slightly above table so it’s visible
        spawnPos.y = transform.TransformPoint(Vector3.zero).y + yOffset;

        GameObject cube;
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = spawnPos;
        cube.transform.localScale = Vector3.one * 0.1f;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(Random.value, Random.value, Random.value, 1f);
        cube.GetComponent<MeshRenderer>().material = mat;

        Destroy(cube, 5f);
    }
}
