using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

[System.Serializable]
public struct CaveSpec
{
    [Header("General")]

    [SerializeField]
    public int seed;
    [SerializeField] [Range(1.0f, 1000.0f)]
    public float size;

    [Space(5)]
    [Header("Wall Settings")]

    [SerializeField]
    public Noise.PerlinSettings wallPerlinSettings;
    [SerializeField] [Range(0.0f, 1.0f)]
    public float perlinThreshold;
    [SerializeField]
    public int wallResolution;
    [SerializeField]
    public float wallHeight;

    public CaveSpec(bool parameterless)
    {
        seed = 1;
        size = 100;

        wallPerlinSettings = new Noise.PerlinSettings
        (
            Vector3.zero,
            14.34f,
            3,
            0.47f,
            0.79f
        );
        perlinThreshold = 0.45f;
        wallResolution = 100;
        wallHeight = 7;
    }
}

public static class SquareMarcher
{

    static Vector3 TL = new Vector3(-1.0f, 0.0f, 1.0f);
    static Vector3 TR = new Vector3(1.0f, 0.0f, 1.0f);
    static Vector3 BL = new Vector3(-1.0f, 0.0f, -1.0f);
    static Vector3 BR = new Vector3(1.0f, 0.0f, -1.0f);
    static Vector3 T  = new Vector3(0.0f, 0.0f, 1.0f);
    static Vector3 B  = new Vector3(0.0f, 0.0f, -1.0f);
    static Vector3 L  = new Vector3(-1.0f, 0.0f, 0.0f);
    static Vector3 R  = new Vector3(1.0f, 0.0f, 0.0f);

    static Vector3 dTL = new Vector3(-1.0f, -1.0f, 1.0f);
    static Vector3 dTR = new Vector3(1.0f, -1.0f, 1.0f);
    static Vector3 dBL = new Vector3(-1.0f, -1.0f, -1.0f);
    static Vector3 dBR = new Vector3(1.0f, -1.0f, -1.0f);
    static Vector3 dT  = new Vector3(0.0f, -1.0f, 1.0f);
    static Vector3 dB  = new Vector3(0.0f, -1.0f, -1.0f);
    static Vector3 dL  = new Vector3(-1.0f, -1.0f, 0.0f);
    static Vector3 dR  = new Vector3(1.0f, -1.0f, 0.0f);

    /*
    *   2----3
    *   |    |
    *   |    |
    *   0----1
    *
    *   z|__
    *     x
    */

    static readonly Vector3[][] s_Byte2WallVertices = new Vector3[][]
    {
        // 0b0000
        new Vector3[]
        {

        },
        
        // 0b0001
        new Vector3[]
        {
            B, L, dB,
            dB, L, dL,
        },

        // 0b0010
        new Vector3[]
        {
            R, B, dR,
            dR, B, dB,
        },

        // 0b0011
        new Vector3[]
        {
            R, L, dR,
            dR, L, dL,
        },

        // 0b0100
        new Vector3[]
        {
            L, T, dL,
            dL, T, dT,
        },

        // 0b0101
        new Vector3[]
        {
            B, T, dB,
            dB, T, dT,
        },

        // 0b0110
        new Vector3[]
        {
            R, B, dR,
            dR, B, dB,

            L, T, dL,
            dL, T, dT,
        },

        // 0b0111
        new Vector3[]
        {
            R, T, dR,
            dR, T, dT,
        },

        // 0b1000
        new Vector3[]
        {
            T, R, dT,
            dT, R, dR,
        },

        // 0b1001
        new Vector3[]
        {
            T, R, dT,
            dT, R, dR,

            B, L, dB,
            dB, L, dL,
        },

        // 0b1010
        new Vector3[]
        {
            T, B, dT,
            dT, B, dB,
        },

        // 0b1011
        new Vector3[]
        {
            T, L, dT,
            dT, L, dL,
        },

        // 0b1100
        new Vector3[]
        {
            L, R, dL,
            dL, R, dR,
        },

        // 0b1101
        new Vector3[]
        {
            B, R, dB,
            dB, R, dR,
        },

        // 0b1110
        new Vector3[]
        {
            L, B, dL,
            dL, B, dB,
        },

        // 0b1111
        new Vector3[]
        {

        },
    };

    static readonly Vector3[][] s_Byte2TopVertices = new Vector3[][]
    {
        // 0b0000
        new Vector3[]
        {

        },
        
        // 0b0001
        new Vector3[]
        {
            L, B, BL
        },

        // 0b0010
        new Vector3[]
        {
            B, R, BR,
        },

        // 0b0011
        new Vector3[]
        {
            L, R, BL,
            BL, R, BR,
        },

        // 0b0100
        new Vector3[]
        {
            L, TL, T,
        },

        // 0b0101
        new Vector3[]
        {
            T, BL, TL,
            BL, T, B,
        },

        // 0b0110
        new Vector3[]
        {
            B, R, BR,
            L, TL, T,
        },

        // 0b0111
        new Vector3[]
        {
            T, BL, TL,
            R, BL, T,
            BL, R, BR,
        },

        // 0b1000
        new Vector3[]
        {
            T, TR, R,
        },

        // 0b1001
        new Vector3[]
        {
            T, TR, R,
            L, B, BL,
        },

        // 0b1010
        new Vector3[]
        {
            T, BR, B,
            BR, T, TR,
        },

        // 0b1011
        new Vector3[]
        {
            L, BR, BL,
            T, BR, L,
            BR, T, TR,
        },

        // 0b1100
        new Vector3[]
        {
            TL, R, L,
            R, TL, TR,
        },

        // 0b1101
        new Vector3[]
        {
            TL, B, BL,
            B, TL, R,
            TL, TR, R,
        },

        // 0b1110
        new Vector3[]
        {
            TL, TR, L,
            TR, B, L,
            TR, BR, B,
        },

        // 0b1111
        new Vector3[]
        {
            TL, TR, BL,
            TR, BR, BL,
        },
    };


    static readonly Vector3 m_OffsetVec0 = new Vector3(-0.5f, -0.5f, -0.5f);
    static Vector3 m_Offset = Vector3.zero;
    static Vector3[] m_TmpTriangle = new Vector3[3];

    public static List<GameObject> s_CreateCave(CaveSpec cs)
    {
        List<Vector3> wallVertices = new List<Vector3>();
        List<Vector3> topVertices = new List<Vector3>();

        float step = cs.size / cs.wallResolution;
        float halfSize = cs.size / 2.0f;
        float halfStep = step / 2.0f;
        float half3Step = halfStep * 3.0f;

        Vector2 mid = new Vector2(cs.size / 2, cs.size / 2);
        const float A = 0.001f;
        const float B = 1.2f;
        const float Power = 2;

        Func<float, float, int> filter = (x,y) => 
        {
            return Noise.samplePerlinNoise2x1(x, y, cs.wallPerlinSettings) /* * (-Mathf.Abs(Mathf.Pow(Vector2.Distance(mid, new Vector2(x,y)), Power) * A) + B) */ > cs.perlinThreshold ? 1 : 0;
        };

        for(float x = -halfSize + halfStep; x <= halfSize; x += step)
            for(float y = -halfSize + halfStep; y <= halfSize; y += step)
            {
                int index = 0;

                index += filter(x - halfStep, y - halfStep) << 0;
                index += filter(x + halfStep, y - halfStep) << 1;
                index += filter(x - halfStep, y + halfStep) << 2;
                index += filter(x + halfStep, y + halfStep) << 3;

                m_Offset.x = x;
                m_Offset.z = y;

                // Top - Debug Vertices
                for(int i = 0; i < s_Byte2TopVertices[index].Length; i += 3)
                {
                    // Vertex transforms
                    m_TmpTriangle[0] = s_Byte2TopVertices[index][i] * halfStep + m_Offset;
                    m_TmpTriangle[1] = s_Byte2TopVertices[index][i+1] * halfStep + m_Offset;
                    m_TmpTriangle[2] = s_Byte2TopVertices[index][i+2] * halfStep + m_Offset;

                    // Appending to a caveTop mesh
                    topVertices.Add(m_TmpTriangle[0]);
                    topVertices.Add(m_TmpTriangle[1]);
                    topVertices.Add(m_TmpTriangle[2]);
                }

                // Wall vertices
                for(int wallSegment = 0; wallSegment < 1; wallSegment++)
                {
                    float up = cs.wallHeight * (wallSegment + 1);
                    float down = cs.wallHeight * wallSegment;

                    for(int i = 0; i < s_Byte2WallVertices[index].Length; i += 3)
                    {
                        // Vertex transforms
                        m_TmpTriangle[0] = s_Byte2WallVertices[index][i] * halfStep + m_Offset;
                        m_TmpTriangle[1] = s_Byte2WallVertices[index][i+1] * halfStep + m_Offset;
                        m_TmpTriangle[2] = s_Byte2WallVertices[index][i+2] * halfStep + m_Offset;

                        // Vertices without noise and added function
                        m_TmpTriangle[0].y = m_TmpTriangle[0].y == 0.0f ? up : down;
                        m_TmpTriangle[1].y = m_TmpTriangle[1].y == 0.0f ? up : down;
                        m_TmpTriangle[2].y = m_TmpTriangle[2].y == 0.0f ? up : down;

                        // Appending to a cave wall
                        wallVertices.Add(m_TmpTriangle[0]);
                        wallVertices.Add(m_TmpTriangle[1]);
                        wallVertices.Add(m_TmpTriangle[2]);
                    }
                }
            }

        // Creating planes
        List<GameObject> planes = new List<GameObject>();

        //DEBUG
        {
            if(top != null) GameObject.Destroy(top);

            int[] topIndices = new int[topVertices.Count];
            Vector2[] topUVs = new Vector2[topVertices.Count];

            for(int i = 0; i < topVertices.Count; i++) 
            {
                topIndices[i] = i;
                topUVs[i].x = topVertices[i].x;
                topUVs[i].y = topVertices[i].z;
            }

            Mesh caveTop = new Mesh();
            caveTop.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            caveTop.SetVertices(topVertices);
            caveTop.SetIndices(topIndices, MeshTopology.Triangles, 0);
            caveTop.SetUVs(0, topUVs);
            caveTop.RecalculateBounds();
            caveTop.RecalculateNormals();

            top = new GameObject("Debug Top");
            top.transform.position = new Vector3(0.0f, 2f, 0.0f);
            var topFilter = top.AddComponent<MeshFilter>();
            var topRenderer = top.AddComponent<MeshRenderer>();
            topFilter.mesh = caveTop;
        }

        for(int i = 0; i < wallVertices.Count; i += 6)
        {
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);

            //0   1
            //2   5

            // Position
            Vector3 pos = (wallVertices[i+0] + wallVertices[i+1] + wallVertices[i+2] + wallVertices[i+5]) / 4;

            // Rotation
            Vector3 right =  wallVertices[i+0] - wallVertices[i+1];
            Vector3 up = wallVertices[i+0] - wallVertices[i+2];
            right.Normalize();
            up.Normalize();
            Vector3 forward = Vector3.Cross(right, up);

            var rot = Quaternion.LookRotation(up, -forward);

            // Scale
            Vector3 scale = new Vector3
            (
                Vector3.Distance(wallVertices[i+0], wallVertices[i+1]) / 10,
                1,
                Vector3.Distance(wallVertices[i+0], wallVertices[i+2]) / 10
            );

            plane.transform.position = pos;
            plane.transform.rotation = rot;
            plane.transform.localScale = scale;

            planes.Add(plane);
        }

        return planes;
    }

    static GameObject top = null;
}
