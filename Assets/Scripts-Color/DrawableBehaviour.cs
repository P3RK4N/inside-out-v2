using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

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
    [SerializeField] int resolution = 100;

    /// <summary>
    /// Color which defines clear surface
    /// </summary>
    /// <returns></returns>
    [SerializeField] Color clearColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);

    /// <summary>
    /// Shader which will draw on a texture
    /// </summary>
    [SerializeField] ComputeShader drawShader;

    /// <summary>
    /// Factor of surface/plane size.
    /// Plane is by default 10x10 meters
    /// </summary>
    public static readonly float PLANE_FACTOR = 10.0f;

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
    public static ComputeBuffer ColorCountBuffer = null;

    Transform tf;
    MeshRenderer r;
    RenderTexture drawTexture;

    public static ColorIndex colorIndex;

    void Awake()
    {
        // Grab component references
        tf = transform;
        r = GetComponent<MeshRenderer>();

        // Create shared color count buffer
        if(ColorCountBuffer == null)
        {
            ColorCountBuffer = new ComputeBuffer(NumColors, sizeof(int));
            ColorCountBuffer.SetData(new int[]{ 0,0,0,0,0,0,0 });
        }

        // Check if there is too much chunks of surface to be painted | We can make it work but not needed.
        Assert.IsTrue
        (
            resolution * PLANE_FACTOR * tf.localScale.x <= 1000 && resolution * PLANE_FACTOR * tf.localScale.y <= 1000,
            $"Object {gameObject.name} is to large or it has too big resolution! Consider fragmenting it!"
        );

        // Texture init and assign to material
        drawTexture = new RenderTexture
        (
            (int)(tf.localScale.x * resolution * PLANE_FACTOR),
            (int)(tf.localScale.y * resolution * PLANE_FACTOR),
            1,
            UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat
        );
        drawTexture.enableRandomWrite = true;
        drawTexture.filterMode = FilterMode.Bilinear;
        drawTexture.Create();
        r.material.SetTexture("_BaseMap", drawTexture);

        // Compute shader init
        drawShader = Instantiate(drawShader);
        drawShader.SetTexture(0, "Result", drawTexture);
        drawShader.SetTexture(1, "Result", drawTexture);
        drawShader.SetTexture(2, "Result", drawTexture);
        drawShader.SetBuffer(0, "ColorCountBuffer", ColorCountBuffer);
        drawShader.SetBuffer(1, "ColorCountBuffer", ColorCountBuffer);
        drawShader.SetBuffer(2, "ColorCountBuffer", ColorCountBuffer);
        drawShader.SetInt("resolution", resolution);
        drawShader.SetFloats("scale", new float[]{ PLANE_FACTOR * tf.localScale.x, PLANE_FACTOR * tf.localScale.y });
        drawShader.SetFloats("clearColor", new float[]{ clearColor.r, clearColor.g, clearColor.b, clearColor.a });
        drawShader.Dispatch(1, 32, 32, 1);
    }

    /// <summary>
    /// Draws on a texture attached to this mesh
    /// </summary>
    /// <param name="uv">Coordinates of place where to draw</param>
    /// <param name="radius">Radius of drawing in meters.</param>
    public void Draw(Color c, Vector2 uv, float radius = 0.1f)
    {
        drawShader.SetFloat("radius", radius);
        drawShader.SetFloats("uv", new float[]{ uv.x, uv.y });
        drawShader.SetFloats("color", new float[]{ c.r, c.g, c.b, c.a });
        drawShader.Dispatch(0, 32, 32, 1);
    }

    /// <summary>
    /// Erases from a texture attached to this mesh
    /// </summary>
    /// <param name="uv">Coordinates of place where to draw</param>
    /// <param name="radius">Radius of drawing in meters</param>
    public void Erase(Vector2 uv, float radius = 0.1f)
    {
        drawShader.SetFloat("radius", radius);
        drawShader.SetFloats("uv", new float[]{ uv.x, uv.y });
        drawShader.Dispatch(2, 32, 32, 1);
    }
}
