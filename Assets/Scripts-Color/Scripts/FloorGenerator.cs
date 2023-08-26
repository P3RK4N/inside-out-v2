using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

#endif

public class FloorGenerator : MonoBehaviour
{
    [SerializeField]
    CaveSpec f_CaveSpec;

    [SerializeField]
    public Material f_WallMaterial;

    Transform r_TF;

    List<GameObject> walls;

    void Awake()
    {
        Debug.Log("Awake Called!");

        r_TF = transform;

        createCave();
    }

    void Start()
    {
        Debug.Log("Start Called!");
    }

    public void resetCaveSpecs()
    {
        f_CaveSpec = new CaveSpec(true);
    }

    public void createCave()
    {
        Noise.s_PerlinSeed = f_CaveSpec.seed;

        {
            // Cave wall
            if(walls != null)
            {
                foreach(var wall in walls)
                {
                    Destroy(wall);
                }
            }

            walls = SquareMarcher.s_CreateCave(f_CaveSpec);

            foreach (var wall in walls)
            {
                wall.GetComponent<MeshRenderer>().material = f_WallMaterial;
                wall.AddComponent<DrawableBehaviour>();
            }
        }
    }
}

#if UNITY_EDITOR

[CustomEditor (typeof (FloorGenerator))]
public class FloorGeneratorEditor : Editor
{
    void resetCave()
    {
        FloorGenerator fg = (FloorGenerator)target;

        fg.resetCaveSpecs();
    }

    void refreshCave()
    {
        FloorGenerator fg = (FloorGenerator)target;

        fg.createCave();
    }

    public override void OnInspectorGUI() 
    {
        FloorGenerator fg = (FloorGenerator)target;

        if(GUILayout.Button("Reset Cave")) resetCave();

        if(Application.isPlaying)
        {
            if(GUILayout.Button("Refresh Cave")) refreshCave();
        }

        GUILayout.Space(20);

        GUILayout.Label("Settings");
        if(DrawDefaultInspector() && Application.isPlaying)
        {
            refreshCave();
        }
    }

}

#endif