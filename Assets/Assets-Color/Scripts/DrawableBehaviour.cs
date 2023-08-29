using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using Unity.VisualScripting;

/// <summary>
/// Color indices
/// </summary>
public enum ColorIndex : uint
{
    Red     = 0,
    Green   = 1,
    Blue    = 2,
    Yellow  = 3,
    Cyan    = 4,
    Magenta = 5,
    White   = 6,
}

public class DrawableBehaviour : MonoBehaviour
{
    /// <summary>
    /// Number of texels per 1 meter
    /// </summary>
    [SerializeField] public int resolution = 100;

    /// <summary>
    /// Color which defines clear surface
    /// </summary>
    /// <returns></returns>
    [SerializeField] Color clearColor = new Color(0.03f, 0.03f, 0.03f, 1.0f);

    /// <summary>
    /// Shader which will draw on a texture
    /// </summary>
    public static ComputeShader drawShaderTemplate;
    [SerializeField] ComputeShader drawShader;

    /// <summary>
    /// Scale of compute shader noise
    /// </summary>
    [SerializeField] Vector2 noiseScale;

    /// <summary>
    /// Offset of compute shader noise
    /// </summary>
    [SerializeField] Vector2 noiseOffset;


    /// <summary>
    /// Factor of surface/plane size.
    /// Plane is by default 10x10 meters
    /// </summary>
    public static readonly float PLANE_FACTOR = 10.0f;

    /// <summary>
    /// Factor of sampler scaling based on local scale of drawable object.
    /// </summary>
    public static readonly float SCALE_FACTOR = PLANE_FACTOR / 2.0f;

    /// <summary>
    /// Margin of collider borders measured in meters. 
    /// This is used to properly register spraying on borders.
    /// </summary>
    public static readonly float COLLIDER_MARGIN = 0.5f;

    /// <summary>
    /// Number of colors used
    /// </summary>
    public static readonly int NumColors = 7;

    /// <summary>
    /// Colors used in textual format
    /// </summary>
    public static readonly string[] ColorStrings =
    {
        "Red",
        "Green",
        "Blue",
        "Yellow",
        "Cyan",
        "Magenta",
        "White"
    };

    /// <summary>
    /// Colors used in vector format
    /// </summary>
    public static readonly Color[] Colors =
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        Color.cyan,
        Color.magenta,
        Color.white,
    };

    /// <summary>
    /// Buffer which will hold atomic counters of amount of cm^2
    /// 
    /// Contains:
    ///     int CountR;
    ///     int CountG;
    ///     int CountB;
    ///     int CountRG;
    ///     int CountGB;
    ///     int CountRB;
    ///     int CountRGB;
    /// 
    /// </summary>
    public static int[] GlobalColorCountBuffer = new int[7] { 0, 0, 0, 0, 0, 0, 0 };
    ComputeBuffer LocalColorCountBuffer = null;

    Transform tf;
    MeshRenderer r;
    RenderTexture drawTexture;
    RenderTexture coatTexture;

    public static ColorIndex colorIndex;

    Vector2 colliderScale;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void LoadStaticAssets()
    {
        drawShaderTemplate = Resources.Load<ComputeShader>("ComputeDraw");
    }

    void OnValidate()
    {
        if(LocalColorCountBuffer == null) return;
        randomizeColor();
    }

    void Awake()
    {
        // Grab component references
        tf = transform;
        r = GetComponent<MeshRenderer>();

        // Create local color count buffer
        LocalColorCountBuffer = new ComputeBuffer(NumColors, sizeof(int));
        LocalColorCountBuffer.SetData(new int[]{ 0, 0, 0, 0, 0, 0, 0 });

        // Check if there is too much chunks of surface to be painted | We can make it work but not needed.
        Assert.IsTrue
        (
            resolution * PLANE_FACTOR * tf.localScale.x <= 1000 && resolution * PLANE_FACTOR * tf.localScale.z <= 1000,
            $"Object {gameObject.name} is to large or it has too big resolution! Consider fragmenting it!"
        );

        // Draw Texture init and assign to material
        drawTexture = new RenderTexture
        (
            (int)(tf.localScale.x * resolution * PLANE_FACTOR),
            (int)(tf.localScale.z * resolution * PLANE_FACTOR),
            1,
            UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat
        );
        drawTexture.enableRandomWrite = true;
        drawTexture.filterMode = FilterMode.Bilinear;
        drawTexture.Create();

        // Coat Texture init and assign to material
        coatTexture = new RenderTexture
        (
            (int)(tf.localScale.x * resolution * PLANE_FACTOR),
            (int)(tf.localScale.z * resolution * PLANE_FACTOR),
            1,
            UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat
        );
        coatTexture.enableRandomWrite = true;
        coatTexture.filterMode = FilterMode.Bilinear;
        coatTexture.Create();
    
        // Material
        r.material.SetTexture("_BaseMap", drawTexture);
        r.material.SetTexture("_CoatMap", coatTexture);
        r.material.SetColor("_ClearColor", clearColor);
        r.material.SetVector("_Scale", new Vector2(tf.localScale.x, tf.localScale.y) * SCALE_FACTOR);

        // Compute shader init
        drawShader = Instantiate(drawShaderTemplate);

        drawShader.SetTexture(0, "DrawTexture", drawTexture);
        drawShader.SetTexture(1, "DrawTexture", drawTexture);
        drawShader.SetTexture(2, "DrawTexture", drawTexture);
        drawShader.SetTexture(3, "DrawTexture", drawTexture);
        drawShader.SetTexture(4, "DrawTexture", drawTexture);
        drawShader.SetTexture(5, "DrawTexture", drawTexture);

        drawShader.SetTexture(0, "CoatTexture", coatTexture);
        drawShader.SetTexture(1, "CoatTexture", coatTexture);
        drawShader.SetTexture(2, "CoatTexture", coatTexture);
        drawShader.SetTexture(3, "CoatTexture", coatTexture);
        drawShader.SetTexture(4, "CoatTexture", coatTexture);
        drawShader.SetTexture(5, "CoatTexture", coatTexture);

        drawShader.SetBuffer(0, "ColorCountBuffer", LocalColorCountBuffer);
        drawShader.SetBuffer(1, "ColorCountBuffer", LocalColorCountBuffer);
        drawShader.SetBuffer(2, "ColorCountBuffer", LocalColorCountBuffer);
        drawShader.SetBuffer(3, "ColorCountBuffer", LocalColorCountBuffer);
        drawShader.SetBuffer(4, "ColorCountBuffer", LocalColorCountBuffer);
        drawShader.SetBuffer(5, "ColorCountBuffer", LocalColorCountBuffer);

        drawShader.SetInt("resolution", resolution);
        drawShader.SetFloats("scale", new float[]{ PLANE_FACTOR * tf.localScale.x, PLANE_FACTOR * tf.localScale.z });

        drawShader.Dispatch(1, 32, 32, 1);

        // Reshape SprayColor collider
        float surfaceX = PLANE_FACTOR * tf.localScale.x;
        float surfaceY = PLANE_FACTOR * tf.localScale.z;

        float colliderX = surfaceX + COLLIDER_MARGIN;
        float colliderY = surfaceY + COLLIDER_MARGIN;

        colliderScale = new Vector2(colliderX / surfaceX, colliderY / surfaceY);

        tf.Find("Collider").localScale = new Vector3(colliderScale.x, 1, colliderScale.y);


        // Noise related
        noiseOffset.x = tf.position.x;
        noiseOffset.y = tf.position.z;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            randomizeColor();
        }

        if(Input.GetKeyDown(KeyCode.G))
        {
            GenerateLevel();
        }
    }

    void OnDestroy()
    {
        if(LocalColorCountBuffer != null)
        {
            LocalColorCountBuffer.Dispose();
            LocalColorCountBuffer = null;
        }
    }

    /// <summary>
    /// Transforms UV from collider to draw surface space.
    /// </summary>
    /// <param name="colliderUV"></param>
    void calculateSprayUV(ref Vector2 uv)
    {
        uv -= new Vector2(0.5f, 0.5f);
        uv *= colliderScale;
        uv += new Vector2(0.5f, 0.5f);
    }

    /// <summary>
    /// Draws on a color texture attached to this mesh
    /// </summary>
    /// <param name="uv">Coordinates of place where to draw</param>
    /// <param name="radius">Radius of drawing in meters.</param>
    public void Draw(Color c, Vector2 uv, float radius = 0.1f)
    {
        calculateSprayUV(ref uv);

        drawShader.SetFloat("radius", radius);
        drawShader.SetFloats("uv", new float[]{ uv.x, uv.y });
        drawShader.SetFloats("color", new float[]{ c.r, c.g, c.b, c.a });
        drawShader.Dispatch(0, 32, 32, 1);

        Count();
    }

    /// <summary>
    /// Puts coat on a coat texture attached to this mesh
    /// </summary>
    /// <param name="uv">Coordinates of place where to draw</param>
    /// <param name="radius">Radius of drawing in meters.</param>
    public void Coat(Color c, Vector2 uv, float radius = 0.1f)
    {
        calculateSprayUV(ref uv);

        drawShader.SetFloat("radius", radius);
        drawShader.SetFloats("uv", new float[]{ uv.x, uv.y });
        drawShader.SetFloats("color", new float[]{ c.r, c.g, c.b, c.a });
        drawShader.Dispatch(4, 32, 32, 1);

        Count();
    }

    /// <summary>
    /// Removes coat from a coat texture attached to this mesh
    /// </summary>
    /// <param name="uv"></param>
    /// <param name="radius"></param>
    public void Uncoat(Vector2 uv, float radius = 0.1f)
    {
        calculateSprayUV(ref uv);

        drawShader.SetFloat("radius", radius);
        drawShader.SetFloats("uv", new float[]{ uv.x, uv.y });
        drawShader.Dispatch(2, 32, 32, 1);

        Count();
    }

    /// <summary>
    /// Erases from a texture attached to this mesh
    /// </summary>
    /// <param name="uv">Coordinates of place where to draw</param>
    /// <param name="radius">Radius of drawing in meters</param>
    public void Erase(Vector2 uv, float radius = 0.1f)
    {
        uv -= new Vector2(0.25f, 0.25f);
        uv *= 2.0f;

        drawShader.SetFloat("radius", radius);
        drawShader.SetFloats("uv", new float[]{ uv.x, uv.y });
        drawShader.Dispatch(2, 32, 32, 1);

        Count();
    }


    /// <summary>
    /// Randomly colors DrawTexture using noise
    /// </summary>
    int _noiseSeed = 0;
    void randomizeColor()
    {
        if(_noiseSeed == 0)
        {
            _noiseSeed = Random.Range(0, 100000);
            drawShader.SetFloat("noiseSeed", _noiseSeed);
        }

        drawShader.SetFloats("noiseScale", new float[]{ noiseScale.x, noiseScale.y });
        drawShader.SetFloats("noiseOffset", new float[]{ noiseOffset.x, noiseOffset.y });
        drawShader.Dispatch(3, 32, 32, 1);

        Count();
    }


    /// <summary>
    /// Recounts the colors in attached DrawTexture
    /// </summary>
    int[] _emptyArray = new int[] { 0, 0, 0, 0, 0, 0, 0 };
    void Count()
    {
        // Get current count
        int[] localColorCount = new int[NumColors];
        LocalColorCountBuffer.GetData(localColorCount);

        // Remove current count
        for(int i = 0; i < NumColors; i++)
        {
            GlobalColorCountBuffer[i] -= localColorCount[i];
        }

        // Reset and recount
        LocalColorCountBuffer.SetData(_emptyArray);
        drawShader.Dispatch(5, 32, 32, 1);

        // Add new count
        LocalColorCountBuffer.GetData(localColorCount);
        for(int i = 0; i < NumColors; i++)
        {
            GlobalColorCountBuffer[i] += localColorCount[i];
        }

    }

    void GenerateLevel(int seed = -1)
    {
        const int r = 1;
        const int g = 2;
        const int b = 4;
        const int y = r+g;
        const int m = r+b;
        const int c = b+g;
        const int w = r+g+b;
        const int _r = -r;
        const int _g = -g;
        const int _b = -b;

        List<int> options = new List<int>
        {
            r,g,b,y,m,c,w,_r,_g,_b
        };

        if(seed == -1)
        {
            seed = Random.Range(0, 2^32);
        }

        var rng = new System.Random(seed);

        System.Func<IEnumerable<int>, int> getNumChoices = (x) =>
        {
            var cols = x.ToList();
            cols.Sort((l,r) => r - l);
            string s = "";
            foreach(var col in cols) s += col + " ";
            Debug.Log(s);

            int numChoices = 0;

            HashSet<int> c = new HashSet<int>();

            if(cols[0] > 0) c.Add(cols[0]);
            if(cols[1] > 0) c.Add(cols[1]);
            if(cols[2] > 0) c.Add(cols[2]);

            if(c.Count == 1)
            {
                if((cols[0] & -cols[1]) != 0) c.Add(cols[0] + cols[1]);
                if((cols[0] & -cols[2]) != 0) c.Add(cols[0] + cols[2]);
                if((cols[0] & (-cols[1]-cols[2])) != 0) c.Add(cols[0] + cols[1] + cols[2]);
            }
            else if(c.Count == 2)
            {
                c.Add(cols[0] | cols[1]);
                foreach(var col in c.ToArray())
                {
                    if((col & -cols[2]) != 0) c.Add(col + cols[2]);
                }
            }
            else if(c.Count == 3)
            {
                c.Add(cols[0] | cols[1]);
                c.Add(cols[0] | cols[2]);
                c.Add(cols[1] | cols[2]);
                c.Add(cols[0] | cols[1] | cols[2]);
            }

            c.Remove(0);
            string s1 = "";
            foreach(var col in c) s1 += col + " ";
            Debug.Log(s1);
            Debug.Log(c.Count);
            return c.Count;
        };

        IEnumerable sample;
        int numChoices = 0;

        for(int i = 0; i < 10 || numChoices < 3; i++)
        {
            var currSample = options.OrderBy(x => rng.Next()).Take(3);
            int currNumChoices = getNumChoices(currSample);
            if(currNumChoices > numChoices)
            {
                numChoices = currNumChoices;
                sample = currSample;
            }
        }
    }
}




/*
    WALL PROCGEN:

    - 3 colors should be generated

    #1

    - if primary color gets selected:
        - give w|y|m|c spray
        - make wall juicy with suptractive color
        - if W then give another suptractive color

    - if secondary color gets selected_
        - give r|g|b|w spray
        - if not W, make wall juicy with undesired suptractive color
        - if W, make wall juicy with desired suptractive


    TOP DOWN?
    Needs to paint 

    + SPRAY
    - SUBTRACTIVE SPRAY
    ~ SUBTRACTIVE WALL

    RED:
        +YELLOW
            ~GREEN
            -GREEN
        +SMAGENTA
            ~BLUE
            -BLUE
        +WHITE
            ~BLUE
                ~GREEN
                -GREEN
            -BLUE
                ~GREEN
                -GREEN

    BOTTOM UP?
    Gets the spray

    1-2 Normal, 1-2 subtractive, exactly 3 sprays

    HEURISTICS:
        - if 2 are suptractive then normal is secondary


*/