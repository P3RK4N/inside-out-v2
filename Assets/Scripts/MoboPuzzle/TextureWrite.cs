using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureWrite : MonoBehaviour
{
    public Renderer r;
    public Rotator rot;
    private Transform PlayerTransform;
    public ComputeShader tempShader;
    public List<Vector4> edgesList;
    public GameObject chip;

    static int texWidth =   200;
    static int texHeight =  200;

    RenderTexture weldTexture { get; set; }
    public RenderTexture getWeldTexture() { return weldTexture; }
    ComputeBuffer bitBuffer;
    public ComputeBuffer getBitBuffer() { return bitBuffer; }

    private float heatTime = 0.0f;
    private float coolTime = 0.0f;
    ComputeBuffer filterBuffer;
    ComputeBuffer edgesBuffer;

    public bool electricity = false;
    public bool needCleaning = false;
    public bool cleaning = false;
    public bool dirty = false;

    public static int successCount = 0;
    private void Awake() 
    {
        tempShader = (ComputeShader)Instantiate(tempShader);
        createTransistors();
        initEdgesBuffer();
    }

    static Color[] colors = new Color[]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        Color.cyan
    };

   private void createTransistors()
   {
        for(int i = 0; i < edgesList.Count; i++)
        {
            GameObject c;
            float fac = 5.0f;

            c = Instantiate(chip, transform);
            c.transform.localPosition = new Vector3(fac - 10*edgesList[i].x, 0.045f, fac - 10*edgesList[i].y);
            // c.transform.GetChild(0).GetComponent<Renderer>().material.color = colors[i];

            c = Instantiate(chip, transform);
            c.transform.localPosition = new Vector3(fac - 10*edgesList[i].z, 0.045f, fac - 10*edgesList[i].w);
            // c.transform.GetChild(0).GetComponent<Renderer>().material.color = colors[i];
        }
   }

   private void initEdgesBuffer()
   {
      int[] edges = new int[edgesList.Count * 4];

        for(int i = 0; i < edgesList.Count; i++)
        {
            edges[i*4 + 0] = (int)(edgesList[i].x * texWidth);
            edges[i*4 + 1] = (int)(edgesList[i].y * texHeight);
            edges[i*4 + 2] = (int)(edgesList[i].z * texWidth);
            edges[i*4 + 3] = (int)(edgesList[i].w * texHeight);
        }
        
        edgesBuffer = new ComputeBuffer(edgesList.Count * 4, sizeof(int));
        edgesBuffer.SetData(edges);

        tempShader.SetBuffer(0, "edges", edgesBuffer);
        tempShader.SetBuffer(1, "edges", edgesBuffer);
        tempShader.SetBuffer(2, "edges", edgesBuffer);
        tempShader.SetBuffer(3, "edges", edgesBuffer);
        tempShader.SetBuffer(4, "edges", edgesBuffer);
        tempShader.SetBuffer(5, "edges", edgesBuffer);
        tempShader.SetInt("edgesCount", edgesList.Count * 2);
   }

   // Start is called before the first frame update
   void Start()
    {
        //Find PlayerTransform
        PlayerTransform = GameObject.Find("CenterEyeAnchor").transform;

        weldTexture = new RenderTexture(texWidth, texHeight, 1, UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat);
        weldTexture.enableRandomWrite = true;
        weldTexture.filterMode = FilterMode.Bilinear;
        weldTexture.Create();

        //0 -> shortCircuit happened
        //0 -> BFS happened

        int[] empty = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        bitBuffer = new ComputeBuffer(10, sizeof(int));
        bitBuffer.SetData(empty);
        tempShader.SetBuffer(0, "bitBuffer", bitBuffer);
        tempShader.SetBuffer(1, "bitBuffer", bitBuffer);
        tempShader.SetBuffer(2, "bitBuffer", bitBuffer);
        tempShader.SetBuffer(3, "bitBuffer", bitBuffer);
        tempShader.SetBuffer(4, "bitBuffer", bitBuffer);
        tempShader.SetBuffer(5, "bitBuffer", bitBuffer);

        filterBuffer = new ComputeBuffer(BlowTorch.drawSize*BlowTorch.drawSize, sizeof(float));
        filterBuffer.SetData(BlowTorch.drawFilter);
        tempShader.SetBuffer(0, "drawFilter", filterBuffer);
        tempShader.SetBuffer(1, "drawFilter", filterBuffer);
        tempShader.SetBuffer(2, "drawFilter", filterBuffer);
        tempShader.SetBuffer(3, "drawFilter", filterBuffer);
        tempShader.SetBuffer(4, "drawFilter", filterBuffer);
        tempShader.SetBuffer(5, "drawFilter", filterBuffer);

        tempShader.SetTexture(0, "Result", weldTexture);
        tempShader.SetTexture(1, "Result", weldTexture);
        tempShader.SetTexture(2, "Result", weldTexture);
        tempShader.SetTexture(3, "Result", weldTexture);
        tempShader.SetTexture(4, "Result", weldTexture);
        tempShader.SetTexture(5, "Result", weldTexture);

        tempShader.SetInt("drawSize", BlowTorch.drawSize);
        tempShader.SetFloat("drawSpeed", BlowTorch.drawSpeed);
        tempShader.SetFloat("coolSpeed", BlowTorch.coolSpeed);
        tempShader.SetFloat("heatSpeed", BlowTorch.heatSpeed);
        tempShader.SetInt("texWidth", texWidth);
        tempShader.SetInt("texHeight", texHeight);

        tempShader.Dispatch(2, 8, 8, 1);
        tempShader.Dispatch(3, 8, 8, 1);

        r.material.SetTexture("_BaseMap", weldTexture);
        r.material.SetFloat("_TexelWidth", 1.0f / texWidth);
        r.material.SetFloat("_TexelHeight", 1.0f / texHeight);
        r.material.SetFloat("_ScaleFactor", transform.localScale.x);
    }


    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(transform.position, PlayerTransform.position);
        r.material.SetFloat("_PlayerDistance", dist);
        updateDeltaTime();

        if(dist < 6.0f) runTempShader();

        if(needCleaning) 
        {
            needCleaning = false;
            cleaning = true;
            StartCoroutine("clean");
        }
    }

   public IEnumerator runBFS()
   {
        electricity = true;
        while(true)
        {
            tempShader.Dispatch(4, 8, 8, 1);

            int[] data = new int[10]; 
            bitBuffer.GetData(data);

            if(data[0] == 1) 
            {
                data[0] = 0;
                bitBuffer.SetData(data);
                electricity = false;
                needCleaning = true;
                break;
            }
            if(data[1] == 1) { }
            else
            {
                electricity = false;
                break;
            }
            data[1] = 0;
            bitBuffer.SetData(data);
            yield return new WaitForSeconds(0.01f);
        }
        Debug.Log("BFS-ed");
        dirty = false;
        if(!needCleaning) validateResult();
   }

   private void OnDisable() 
   {
        if(edgesBuffer != null) edgesBuffer.Release();
        if(filterBuffer != null) filterBuffer.Release();
        if(bitBuffer != null) bitBuffer.Release();
    }

    void updateDeltaTime()
    {
        heatTime += Time.deltaTime;
        coolTime += Time.deltaTime;

        if(heatTime > 0.01f) 
        {
            tempShader.SetBool("heatStop", false);
            heatTime -= 0.01f;
        }
        else tempShader.SetBool("heatStop", true);

        if(coolTime > 0.03f)
        {
            tempShader.SetBool("coolStop", false);
            coolTime -= 0.03f;    
        }
        else tempShader.SetBool("coolStop", true);
    }

    void runTempShader()
    {
        tempShader.Dispatch(0, 8, 8, 1);

        tempShader.SetInt("beginWidth", -1000);
        tempShader.SetInt("beginHeight", -1000);
    }

    IEnumerator clean()
    {
        while(true)
        {
            tempShader.Dispatch(5,8,8,1);
            int[] data = new int[10];
            bitBuffer.GetData(data);
            if(data[2] == 1)
            {
                data[2] = 0;
                bitBuffer.SetData(data);
                yield return new WaitForSeconds(0.01f);
            }
            else
            {
                cleaning = false;
                break;
            }
        }
        Debug.Log("Cleaned!");
    }

    void validateResult()
    {
        Texture2D result = new Texture2D(texWidth, texHeight);
        RenderTexture.active = weldTexture;
        result.ReadPixels(new Rect(0, 0, texWidth, texHeight), 0, 0);

        Color[] texels = new Color[texWidth * texHeight];
        texels = result.GetPixels();
        var t = result.GetPixelData<Vector4>(0);

        float[,] vals = new float[texWidth, texHeight];

        for(int i = 0; i < texWidth; i++)
            for(int j = 0; j < texHeight; j++)
                vals[i,j] = texels[i*texHeight+j].b;

        HashSet<float> islands = new HashSet<float>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Vector2Int[] dirs = 
        {
            new Vector2Int(0,1),
            new Vector2Int(1,0),
            new Vector2Int(-1,0),
            new Vector2Int(0,-1)
        };

        for(int i = 0; i < texWidth; i++)
            for(int j = 0; j < texHeight; j++)
            {
                if(vals[i,j] == 0.0f || visited.Contains(new Vector2Int(i,j))) continue;

                //Insert check
                if(islands.Contains(vals[i,j]))
                {
                    needCleaning = true;
                    Debug.Log("Not connected!");
                    Debug.Log(String.Format("Failed at {0} {1}, repeated {2}", i, j, vals[i,j]));

                    return;
                };

                Stack<Vector2Int> stack = new Stack<Vector2Int>();
                islands.Add(vals[i,j]);
                stack.Push(new Vector2Int(i,j));
                visited.Add(new Vector2Int(i,j));

                while(stack.Count > 0)
                {
                    Vector2Int current = stack.Pop();
                    foreach (var dir in dirs)
                    {
                        Vector2Int next = current + dir;
                        if(next.x < 0 || next.x >= texWidth || next.y < 0 || next.y >= texHeight || vals[next.x,next.y] == 0.0f || visited.Contains(next)) continue;
                        stack.Push(next);
                        visited.Add(next);
                    }
                }
            }

        Debug.Log("Connected");
        onConnected();
    }

    static string[] WeldStation =
    {
        "/3 weld stations complete!",
        "/3 dovršene stanice za lemljenje!"
    };

    void onConnected()
    {
        electricity = true;
        rot.turnOn(150.0f);
        successCount++;
        if(successCount < 3)
        {
            PromptScript.instance.updatePrompt(successCount + Localization.Loc.loc(WeldStation), 3);
        }
        else
        {
            PromptScript.instance.updatePrompt(Localization.Loc.loc(Localization.StoryTxt.Completed), 3);
            TransitionManager.completePuzzle();
        }
    }
}
