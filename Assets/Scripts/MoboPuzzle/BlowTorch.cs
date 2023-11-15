using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BNG;

public class BlowTorch : MonoBehaviour
{
    public ParticleSystem ps;
    public Grabbable g;

    AudioSource blowSound;

    public static float[,] drawFilter;
    public static float elipseFactor = 0.9f;
    public static int drawSize = 10;
    public static float drawSpeed = 0.02f;
    public static float coolSpeed = 0.002f;
    public static float heatSpeed = 0.015f;

    public int emission = 20;

    void Awake() 
    {
        initDrawFilter();    
    }

    // Start is called before the first frame update
    void Start()
    {
        ps.Stop();
        blowSound = GetComponent<AudioSource>();
    }

    void initDrawFilter()
    {
        drawFilter = new float[drawSize,drawSize];
        float peak = 0;

        for(int i = 0; i < drawSize; i++)
            for(int j = 0; j < drawSize; j++)
            {
                float halfDrawSize = drawSize/2.0f;
                float a = (i - halfDrawSize) * elipseFactor;
                float b = (j - halfDrawSize) * elipseFactor;
                float val = Mathf.Max(halfDrawSize*halfDrawSize*elipseFactor - a*a - b*b, 0.0f);
                drawFilter[i,j] = Mathf.Sqrt(val);
                peak = Mathf.Max(peak, drawFilter[i,j]);
            }

        for(int i = 0; i < drawSize; i++)
            for(int j = 0; j < drawSize; j++)
                drawFilter[i,j] /= peak;
    }

    // Update is called once per frame
    void Update()
    {
        updateLaser();
    }

    void updateLaser()
    {

        if
        (
            g.BeingHeld &&
            (
                InputBridge.Instance.RightTrigger > 0.5f || 
                Input.GetKey(KeyCode.Mouse0) ||
                OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)
            )
        )
        {
            if(!blowSound.isPlaying) blowSound.Play();
            else if(blowSound.time > 3.0f) blowSound.time = 1.0f;

            ps.Emit((int)(emission * Time.deltaTime * 144));
            RaycastHit hit;
            Ray ray = new Ray(transform.position, transform.TransformDirection(Vector3.right) * 2.0f);

            if(Physics.Raycast(ray, out hit) && hit.distance < 0.5f)
            {
                if(hit.transform.GetComponent<TextureWrite>() != null)
                {
                    TextureWrite tw = hit.transform.GetComponent<TextureWrite>();

                    tw.dirty = true;
                    ComputeShader weldShader = tw.tempShader;
                    RenderTexture weldTexture = tw.getWeldTexture();

                    int width = (int)   (hit.textureCoord.x * (float)weldTexture.width);
                    int height = (int)  (hit.textureCoord.y * (float)weldTexture.height);

                    weldShader.SetInt("beginWidth", width - drawSize / 2);
                    weldShader.SetInt("beginHeight", height - drawSize / 2);

                    weldShader.Dispatch(1, 16, 16, 1);
                }
            }
        }
        else
        {
            if(blowSound.isPlaying)
            {
                if(blowSound.time < 0.3f) blowSound.Stop();
                else if(blowSound.time < 13.3f) blowSound.time = 13.4f;
            }
        }
    }
}
