using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;

public class TableTouchManager : MonoBehaviour
{
    private void Awake()
    {
        // Make sure we run AFTER MRUK loads the scene
        MRUK.Instance.SceneLoadedEvent.AddListener(OnSceneLoaded);
    }

    private void OnDestroy()
    {
        if (MRUK.Instance != null)
            MRUK.Instance.SceneLoadedEvent.RemoveListener(OnSceneLoaded);
    }

    private void OnSceneLoaded()
    {
        var room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            DebugTag.LogWarning(nameof(TableTouchManager), "No MRUK room found.");
            return;
        }

        SetupTableTouchZones(room);
    }

    private void SetupTableTouchZones(MRUKRoom room)
    {
        foreach (var anchor in room.Anchors)
        {
            // Only care about TABLE anchors
            if ((anchor.Label & MRUKAnchor.SceneLabels.TABLE) == 0)
                continue;

            CreateTouchZoneForTable(anchor);
        }
    }

    private void CreateTouchZoneForTable(MRUKAnchor tableAnchor)
    {
        GameObject zoneGO = new GameObject("TableTouchZone");
        zoneGO.transform.SetParent(tableAnchor.transform, false);

        
        zoneGO.AddComponent<TableTouchZone>();
        var collider = zoneGO.AddComponent<BoxCollider>();
        collider.isTrigger = true;

        var vbOpt = tableAnchor.VolumeBounds;
        if (vbOpt.HasValue)
        {
            var vb = vbOpt.Value;
            collider.center = vb.center;
            collider.size   = vb.size * 1.2f;
        }
        else
        {
            collider.center = tableAnchor.GetAnchorCenter();
            collider.size   = tableAnchor.GetAnchorSize() * 1.2f;
        }

        // Add a translucent cube to visualize
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(zoneGO.transform, false);
        cube.transform.localPosition = collider.center;
        cube.transform.localScale    = collider.size;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0f, 1f, 1f, 0.25f);        // cyan, 25 % alpha
        cube.GetComponent<MeshRenderer>().material = mat;
    }
}
