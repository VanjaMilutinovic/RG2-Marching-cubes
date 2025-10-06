using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BrushEditor : MonoBehaviour
{
    [Header("Reference")]
    public VoxelTerrain terrain;

    [Header("Brush Settings")]
    public float minRadius = 1f;
    public float maxRadius = 50f;
    public float radiusStep = 1f;
    [Space]
    [Tooltip("For the camera-relative mode, this controls the Depth of the effect.")]
    public float minStrength = 1f;
    [Tooltip("For the camera-relative mode, this controls the Depth of the effect.")]
    public float maxStrength = 20f;
    public float strengthStep = 0.5f;
    [Space]
    [Tooltip("The 'power' of the brush applied at each step of the extrusion/digging.")]
    public float applicationPower = 1.5f;
    [Tooltip("How densely to apply the brush along the extrusion path. Lower value = smoother result.")]
    [Range(0.1f, 1f)] public float strokeStepMultiplier = 0.4f;

    [Header("Voxel Density Control")]
    public List<Vector3Int> DensityLevels = new List<Vector3Int>
    {
        new Vector3Int(32, 16, 32),
        new Vector3Int(64, 32, 64),
        new Vector3Int(128, 64, 128)
    };
    [Range(0, 10)] public int currentDensityIndex = 1;

    [Header("Controls")]
    public KeyCode radiusModifier = KeyCode.LeftControl;
    public KeyCode strengthModifier = KeyCode.LeftAlt;

    [Header("Visuals")]
    public bool drawGizmo = true;
    public bool showUIText = true;

    private Vector3 _gizmoPos;
    private bool _hasHit;

    void Start()
    {
        ApplyDensityLevel(currentDensityIndex, false); 
    }

    void Update()
    {
        if (!terrain) return;

        HandleBrushAdjustments();
        HandleTerrainInteraction();
        HandleTerrainCommands();
    }

    private void HandleBrushAdjustments()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            if (Input.GetKey(radiusModifier))
            {
                terrain.BrushRadius += scroll * radiusStep;
                terrain.BrushRadius = Mathf.Clamp(terrain.BrushRadius, minRadius, maxRadius);
            }
            else if (Input.GetKey(strengthModifier))
            {
                terrain.BrushStrength += scroll * strengthStep;
                terrain.BrushStrength = Mathf.Clamp(terrain.BrushStrength, minStrength, maxStrength);
            }
        }
    }
    
    private void HandleTerrainInteraction()
    {
        if (!Camera.main) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        // --- KLJUČNA IZMENA JE OVDE ---
        // Uklonili smo ", terrain.TerrainLayer" sa kraja poziva.
        // Sada će Raycast pogađati SVE objekte sa colliderom.
        if (Physics.Raycast(ray, out RaycastHit hit, 10000f))
        {
            _hasHit = true;
            _gizmoPos = hit.point;
            
            bool isMouseButtonHeld = Input.GetMouseButton(0) || Input.GetMouseButton(1);

            if (isMouseButtonHeld)
            {
                bool isAdding = Input.GetMouseButton(0);
                Vector3 direction = ray.direction;
                float depth = terrain.BrushStrength;
                
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    depth *= 2f;
                }

                float stepSize = terrain.BrushRadius * strokeStepMultiplier;
                if (stepSize <= 0) stepSize = 0.1f;

                for (float dist = 0; dist < depth; dist += stepSize)
                {
                    Vector3 applyPoint = hit.point + (isAdding ? -direction : direction) * dist;
                    terrain.ApplyBrush(applyPoint, terrain.BrushRadius, applicationPower * Time.deltaTime, isAdding);
                }
            }
        }
        else
        {
            _hasHit = false;
        }
    }
    
    #region Unchanged Code
    private void HandleTerrainCommands()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            terrain.RandomSeed = Random.Range(Int32.MinValue, Int32.MaxValue);
            terrain.GenerateRandomTerrain();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            terrain.FlattenAll();
        }
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            currentDensityIndex++;
            ApplyDensityLevel(currentDensityIndex);
        }
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            currentDensityIndex--;
            ApplyDensityLevel(currentDensityIndex);
        }
    }
    
    private void ApplyDensityLevel(int index, bool regenerate = true)
    {
        if (DensityLevels.Count == 0) return;
        currentDensityIndex = Mathf.Clamp(index, 0, DensityLevels.Count - 1);
        Vector3Int newSize = DensityLevels[currentDensityIndex];
        terrain.GridSizeX = newSize.x;
        terrain.GridSizeY = newSize.y;
        terrain.GridSizeZ = newSize.z;
        if (regenerate)
        {
            terrain.RegenerateAll();
        }
    }

    void OnDrawGizmos()
    {
        if (!drawGizmo || terrain == null || !_hasHit) return;
        Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.6f);
        Gizmos.DrawWireSphere(_gizmoPos, terrain.BrushRadius);
        
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            if (Camera.main != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 direction = ray.direction;
                bool isAdding = Input.GetMouseButton(0);
            
                Gizmos.color = isAdding ? Color.green : Color.red;
                Vector3 endPoint = _gizmoPos + (isAdding ? -direction : direction) * terrain.BrushStrength;
                Gizmos.DrawLine(_gizmoPos, endPoint);
                Gizmos.DrawWireCube(endPoint, Vector3.one * terrain.BrushRadius * 0.2f);
            }
        }
    }

    void OnGUI()
    {
        if (!showUIText || !terrain) return;
        GUI.backgroundColor = Color.black;
        GUI.Box(new Rect(10, 10, 250, 100), "Brush Controls");
        GUI.Label(new Rect(20, 40, 230, 20), $"Radius: {terrain.BrushRadius:F2} (Ctrl+Scroll)");
        GUI.Label(new Rect(20, 60, 230, 20), $"Depth/Strength: {terrain.BrushStrength:F2} (Alt+Scroll)");
        GUI.Label(new Rect(20, 80, 230, 20), $"Voxel Density: {terrain.GridSizeX}x{terrain.GridSizeY}x{terrain.GridSizeZ} (PgUp/PgDown)");
    }
    #endregion
}