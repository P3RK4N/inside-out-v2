using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

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

    public static readonly float PLANE_FACTOR = 10.0f;

    Transform tf;
    MeshRenderer r;
    RenderTexture drawTexture;

    void Awake()
    {
        tf = transform;
        r = GetComponent<MeshRenderer>();

        Assert.IsTrue
        (
            resolution * PLANE_FACTOR * tf.localScale.x <= 1000 && resolution * PLANE_FACTOR * tf.localScale.y <= 1000,
            $"Object {gameObject.name} is to large or it has too big resolution! Consider fragmenting it!"
        );

        // Texture and Material init
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
        drawShader = (ComputeShader)Instantiate(drawShader);
        drawShader.SetInt("resolution", resolution);
        drawShader.SetTexture(0, "Result", drawTexture);
        drawShader.SetTexture(1, "Result", drawTexture);
        drawShader.SetFloats("scale", new float[]{ PLANE_FACTOR * tf.localScale.x, PLANE_FACTOR * tf.localScale.y });
        drawShader.SetFloats("clearColor", new float[]{ clearColor.r, clearColor.g, clearColor.b, clearColor.a });
        drawShader.Dispatch(1, 32, 32, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton((int)MouseButton.Left))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray.origin, ray.direction, out RaycastHit info, 20.0f))
            {
                var collider = info.collider;

                if(collider.GetComponent<DrawableBehaviour>() == null) return;

                // DebugExtension.DebugWireSphere(info.point, Color.red, 1, 0.5f);
                Draw(Color.green, info.textureCoord, 0.5f);
            }
        }
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
}
