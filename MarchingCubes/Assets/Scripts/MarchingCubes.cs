using System.Collections.Generic;
using UnityEngine;

public static class MarchingCubes
{

    static Vector3 VertexInterp(float isoLevel, Vector3 p1, Vector3 p2, float valp1, float valp2)
    {
        // Ako je neki vrh tačno na površini, vrati ga direktno (štiti od 0/0)
        if (Mathf.Abs(isoLevel - valp1) < 0.00001f) return p1;
        if (Mathf.Abs(isoLevel - valp2) < 0.00001f) return p2;
        if (Mathf.Abs(valp1 - valp2) < 0.00001f) return p1;

        float mu = (isoLevel - valp1) / (valp2 - valp1);
        return p1 + mu * (p2 - p1);
    }
    
    public static void PolygoniseCube(
        Vector3[] cubePos,
        float[] cubeVal,
        float isoLevel,
        List<Vector3> outVerts,
        List<Vector3> outNormals)
    {
        int cubeIndex = 0;
        if (cubeVal[0] < isoLevel) cubeIndex |= 1;
        if (cubeVal[1] < isoLevel) cubeIndex |= 2;
        if (cubeVal[2] < isoLevel) cubeIndex |= 4;
        if (cubeVal[3] < isoLevel) cubeIndex |= 8;
        if (cubeVal[4] < isoLevel) cubeIndex |= 16;
        if (cubeVal[5] < isoLevel) cubeIndex |= 32;
        if (cubeVal[6] < isoLevel) cubeIndex |= 64;
        if (cubeVal[7] < isoLevel) cubeIndex |= 128;

        // Ako nema preseka sa površinom, nema ni trouglova.
        int edges = MarchingCubesTables.edgeTable[cubeIndex];
        if (edges == 0) return;

        // Izračunaj interpolisane presečne tačke po ivicama
        Vector3[] vertList = new Vector3[12];

        if ((edges & 1) != 0)    vertList[0]  = VertexInterp(isoLevel, cubePos[0], cubePos[1], cubeVal[0], cubeVal[1]);
        if ((edges & 2) != 0)    vertList[1]  = VertexInterp(isoLevel, cubePos[1], cubePos[2], cubeVal[1], cubeVal[2]);
        if ((edges & 4) != 0)    vertList[2]  = VertexInterp(isoLevel, cubePos[2], cubePos[3], cubeVal[2], cubeVal[3]);
        if ((edges & 8) != 0)    vertList[3]  = VertexInterp(isoLevel, cubePos[3], cubePos[0], cubeVal[3], cubeVal[0]);
        if ((edges & 16) != 0)   vertList[4]  = VertexInterp(isoLevel, cubePos[4], cubePos[5], cubeVal[4], cubeVal[5]);
        if ((edges & 32) != 0)   vertList[5]  = VertexInterp(isoLevel, cubePos[5], cubePos[6], cubeVal[5], cubeVal[6]);
        if ((edges & 64) != 0)   vertList[6]  = VertexInterp(isoLevel, cubePos[6], cubePos[7], cubeVal[6], cubeVal[7]);
        if ((edges & 128) != 0)  vertList[7]  = VertexInterp(isoLevel, cubePos[7], cubePos[4], cubeVal[7], cubeVal[4]);
        if ((edges & 256) != 0)  vertList[8]  = VertexInterp(isoLevel, cubePos[0], cubePos[4], cubeVal[0], cubeVal[4]);
        if ((edges & 512) != 0)  vertList[9]  = VertexInterp(isoLevel, cubePos[1], cubePos[5], cubeVal[1], cubeVal[5]);
        if ((edges & 1024) != 0) vertList[10] = VertexInterp(isoLevel, cubePos[2], cubePos[6], cubeVal[2], cubeVal[6]);
        if ((edges & 2048) != 0) vertList[11] = VertexInterp(isoLevel, cubePos[3], cubePos[7], cubeVal[3], cubeVal[7]);

        // Sastavi trouglove koristeći triTable
        int i = 0;
        while (MarchingCubesTables.triTable[cubeIndex, i] != -1)
        {
            Vector3 p0 = vertList[MarchingCubesTables.triTable[cubeIndex, i + 0]];
            Vector3 p1 = vertList[MarchingCubesTables.triTable[cubeIndex, i + 1]];
            Vector3 p2 = vertList[MarchingCubesTables.triTable[cubeIndex, i + 2]];

            // Površ normal (za difuzno osvetljenje)
            Vector3 n = Vector3.Normalize(Vector3.Cross(p1 - p0, p2 - p0));

            outVerts.Add(p0); outNormals.Add(n);
            outVerts.Add(p1); outNormals.Add(n);
            outVerts.Add(p2); outNormals.Add(n);

            i += 3;
        }
    }
}
