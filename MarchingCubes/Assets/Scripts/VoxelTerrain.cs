using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VoxelTerrain : MonoBehaviour
{
    [Header("Grid / Isosurface")]
    [Min(4)] public int GridSizeX = 64;
    [Min(4)] public int GridSizeY = 32;
    [Min(4)] public int GridSizeZ = 64;
    [Min(0.1f)] public float VoxelSize = 0.5f;
    public float IsoLevel = 0.0f;

    [Header("Interaction")]
    public LayerMask TerrainLayer;

    [Header("Brush")]
    [Min(0.1f)] public float BrushRadius = 2.5f;
    public float BrushStrength = 0.75f;

    [Header("Flatten")]
    public float FlattenHeight = 0f;

    [Header("Random Terrain")]
    public int RandomSeed;
    [Range(0.01f, 2f)] public float NoiseFrequency = 0.08f;
    [Range(0f, 2f)] public float NoiseAmplitude = 0.8f;
    [Range(0, 6)] public int NoiseOctaves = 3;
    [Range(0f, 1f)] public float CaveBias = 0.5f;

    float[,,] density;
    Texture3D densityTex3D;
    Color[] densityPixels;
    Mesh mesh;
    MeshFilter mf;
    MeshCollider mcol;
    readonly Vector3[] cubePos = new Vector3[8];
    readonly float[] cubeVal = new float[8];
    float minY, maxY;

    void Awake()
    {
        mf = GetComponent<MeshFilter>();
        mcol = GetComponent<MeshCollider>();
        if (!mcol) mcol = gameObject.AddComponent<MeshCollider>();
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mf.sharedMesh = mesh;

        RegenerateAll();
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    public void RegenerateAll()
    {
        Debug.Log($"Regenerating terrain with size: {GridSizeX}x{GridSizeY}x{GridSizeZ}");
        AllocateDensity();
        AllocateTexture3D();
        GenerateRandomTerrain();
    }


    void AllocateDensity()
    {
        density = new float[GridSizeX + 1, GridSizeY + 1, GridSizeZ + 1];
    }

    void AllocateTexture3D()
    {
        if(densityTex3D) Destroy(densityTex3D);
        
        densityTex3D = new Texture3D(GridSizeX + 1, GridSizeY + 1, GridSizeZ + 1, TextureFormat.RFloat, false);
        densityTex3D.wrapMode = TextureWrapMode.Clamp;
        densityTex3D.filterMode = FilterMode.Bilinear;
        densityPixels = new Color[(GridSizeX + 1) * (GridSizeY + 1) * (GridSizeZ + 1)];
        Shader.SetGlobalTexture("_DensityTex3D", densityTex3D);
    }

    int Idx(int x, int y, int z)
    {
        int w = GridSizeX + 1;
        int h = GridSizeY + 1;
        return (y * (GridSizeZ + 1) + z) * w + x;
    }
    public void SyncDensityTexture3D()
    {
        if (densityPixels == null || !densityTex3D) return;
        int w = GridSizeX + 1, h = GridSizeY + 1, d = GridSizeZ + 1;
        for (int y = 0; y < h; y++)
        for (int z = 0; z < d; z++)
        for (int x = 0; x < w; x++)
        {
            float v = density[x, y, z];
            float mapped = Mathf.InverseLerp(-2f, 2f, v);
            densityPixels[Idx(x, y, z)] = new Color(mapped, 0, 0, 1);
        }
        densityTex3D.SetPixels(densityPixels);
        densityTex3D.Apply(false, false);
    }
    public void GenerateRandomTerrain()
    {
        Random.InitState(RandomSeed);
        int sx = GridSizeX + 1, sy = GridSizeY + 1, sz = GridSizeZ + 1;
        float worldH = GridSizeY * VoxelSize ;
        float baseH  = worldH * 0.5f;
        float amp    = Mathf.Max(0.1f, worldH * 0.25f);
        float falloffRadiusX = GridSizeX * 0.5f;
        float falloffRadiusZ = GridSizeZ * 0.5f;
        Vector2 center = new Vector2(GridSizeX * 0.5f, GridSizeZ * 0.5f);
        for (int x = 0; x < sx; x++)
        for (int z = 0; z < sz; z++)
        {
            float offsetX = RandomSeed * 0.1234f;
            float offsetZ = RandomSeed * 0.5678f;
            Vector2 p2 = new Vector2(x + offsetX, z + offsetZ) * (VoxelSize * NoiseFrequency);
            float n = 0f, a = 1f, f = 1f, norm = 0f;
            for (int o = 0; o < Mathf.Max(1, NoiseOctaves); o++)
            {
                n += Mathf.PerlinNoise(p2.x * f, p2.y * f) * a;
                norm += a;
                a *= 0.5f;
                f *= 2f;
            }
            n = (norm > 0f) ? n / norm : n;
            float dx = (x - center.x) / falloffRadiusX;
            float dz = (z - center.y) / falloffRadiusZ;
            float r  = Mathf.Sqrt(dx * dx + dz * dz);
            float falloff = Mathf.Clamp01(r);
            float edgeDrop = Mathf.Lerp(0f, amp * 0.5f, falloff);
            float H = baseH + (n * 2f - 1f) * amp - edgeDrop;
            for (int y = 0; y < sy; y++)
            {
                float worldY = transform.position.y + y * VoxelSize;
                density[x, y, z] = worldY - H;
            }
        }
        RebuildMesh();
        SyncDensityTexture3D();
    }
    public void RebuildMesh()
    {
        List<Vector3> verts = new List<Vector3>(200000);
        List<Vector3> norms = new List<Vector3>(200000);
        minY = float.PositiveInfinity;
        maxY = float.NegativeInfinity;
        for (int x = 0; x < GridSizeX; x++)
        for (int y = 0; y < GridSizeY; y++)
        for (int z = 0; z < GridSizeZ; z++)
        {
            FillCube(x, y, z, cubePos, cubeVal);
            MarchingCubes.PolygoniseCube(cubePos, cubeVal, IsoLevel, verts, norms);
        }
        UpdateMinMaxY(verts);
        Vector3[] vArr = verts.ToArray();
        Vector3[] nArr = norms.ToArray();
        Color[] cArr = new Color[vArr.Length];
        for (int i = 0; i < vArr.Length; i++) cArr[i] = ColorByHeight(vArr[i].y);
        int[] tris = new int[vArr.Length];
        for (int i = 0; i < tris.Length; i++) tris[i] = i;
        mesh.Clear();
        mesh.vertices = vArr;
        mesh.normals = nArr;
        mesh.colors = cArr;
        mesh.triangles = tris;
        mesh.RecalculateBounds();
        if (mcol)
        {
            mcol.sharedMesh = null;
            mcol.sharedMesh = mesh;
        }
    }
    void FillCube(int x, int y, int z, Vector3[] cpos, float[] cval)
    {
        Vector3 basePos = transform.position + new Vector3(x, y, z) * VoxelSize;
        cpos[0] = basePos + new Vector3(0, 0, 0) * VoxelSize;
        cpos[1] = basePos + new Vector3(1, 0, 0) * VoxelSize;
        cpos[2] = basePos + new Vector3(1, 0, 1) * VoxelSize;
        cpos[3] = basePos + new Vector3(0, 0, 1) * VoxelSize;
        cpos[4] = basePos + new Vector3(0, 1, 0) * VoxelSize;
        cpos[5] = basePos + new Vector3(1, 1, 0) * VoxelSize;
        cpos[6] = basePos + new Vector3(1, 1, 1) * VoxelSize;
        cpos[7] = basePos + new Vector3(0, 1, 1) * VoxelSize;
        cval[0] = density[x, y, z];
        cval[1] = density[x + 1, y, z];
        cval[2] = density[x + 1, y, z + 1];
        cval[3] = density[x, y, z + 1];
        cval[4] = density[x, y + 1, z];
        cval[5] = density[x + 1, y + 1, z];
        cval[6] = density[x + 1, y + 1, z + 1];
        cval[7] = density[x, y + 1, z + 1];
    }
    void UpdateMinMaxY(List<Vector3> verts)
    {
        if (verts.Count == 0) { minY = 0; maxY = 1; return; }
        minY = float.PositiveInfinity; maxY = float.NegativeInfinity;
        foreach (var v in verts)
        {
            if (v.y < minY) minY = v.y;
            if (v.y > maxY) maxY = v.y;
        }
        if (Mathf.Approximately(minY, maxY)) maxY = minY + 1f;
    }
    Color ColorByHeight(float y)
    {
        float t = Mathf.InverseLerp(minY, maxY, y);
        float hue = Mathf.Lerp(0f, 300f / 360f, t);
        return HSVUtil.HSV01(hue, 1f, 1f);
    }
    public void ApplyBrush(Vector3 worldCenter, float radius, float strength, bool add)
    {
        Vector3 local = (worldCenter - transform.position) / VoxelSize;
        int minX = Mathf.Clamp(Mathf.FloorToInt(local.x - radius / VoxelSize) - 1, 0, GridSizeX);
        int maxX = Mathf.Clamp(Mathf.CeilToInt (local.x + radius / VoxelSize) + 1, 0, GridSizeX);
        int minY = Mathf.Clamp(Mathf.FloorToInt(local.y - radius / VoxelSize) - 1, 0, GridSizeY);
        int maxY = Mathf.Clamp(Mathf.CeilToInt (local.y + radius / VoxelSize) + 1, 0, GridSizeY);
        int minZ = Mathf.Clamp(Mathf.FloorToInt(local.z - radius / VoxelSize) - 1, 0, GridSizeZ);
        int maxZ = Mathf.Clamp(Mathf.CeilToInt (local.z + radius / VoxelSize) + 1, 0, GridSizeZ);
        float r2 = (radius / VoxelSize) * (radius / VoxelSize);
        for (int x = minX; x <= maxX; x++)
        for (int y = minY; y <= maxY; y++)
        for (int z = minZ; z <= maxZ; z++)
        {
            Vector3 p = new Vector3(x, y, z);
            float d2 = (p - local).sqrMagnitude;
            if (d2 > r2) continue;
            float w = 1f - Mathf.Sqrt(d2 / r2);
            float delta = strength * w * (add ? -1f : 1f);
            density[x, y, z] += delta;
        }
        RebuildMesh();
        SyncDensityTexture3D();
    }
    public void FlattenAll()
    {
        int sx = GridSizeX + 1, sy = GridSizeY + 1, sz = GridSizeZ + 1;
        for (int x = 0; x < sx; x++)
        for (int y = 0; y < sy; y++)
        for (int z = 0; z < sz; z++)
        {
            float worldY = transform.position.y + y * VoxelSize;
            density[x, y, z] = worldY - FlattenHeight;
        }
        RebuildMesh();
        SyncDensityTexture3D();
    }
}