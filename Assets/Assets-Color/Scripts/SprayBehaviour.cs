using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using BNG;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public static class ColorUtil
{
}


public class SprayBehaviour : MonoBehaviour
{
    public enum SprayType
    {
        Red,
        xRed,
        Green,
        xGreen,
        Blue,
        xBlue,
        Yellow,
        Magenta,
        Cyan,
        White,
        Black,
        xBlack
    }

    static string[] TypeToString = new string[]
    {
        "Red",
        "Red",
        "Green",
        "Green",
        "Blue",
        "Blue",
        "Yellow",
        "Magenta",
        "Cyan",
        "White",
    };

    static Color[] colors = new Color[]
    {
        Color.red,
        Color.gray,
        Color.green,
        Color.gray,
        Color.blue,
        Color.gray,
        new Color(1, 1, 0, 1),      // Yellow because default is retarded
        Color.magenta,
        Color.cyan,
        Color.white,
        Color.black,
        Color.gray
    };

    public SprayType type;
    public GameObject sprayEmitter;
    public float reach = 0.4f;
    public float radius = 0.2f;
    public float delay = 0.1f;

    public AudioClip sprayCanCollideSound;
    public float sprayCanCollideSoundMinImpact = 1.0f;

    static readonly float Height = 0.565f;
    bool isCoat = false;
    bool isBlack = false;

    Transform sprayDir;
    ParticleSystem ps;
    AudioSource sprayCanSound;
    Grabbable grab;
    TMP_Text sprayStat;
    Transform canvasJoint;

    // Start is called before the first frame update
    void Awake()
    {
        sprayDir = transform.Find("Dir");
        var emitter = Instantiate(sprayEmitter, sprayDir);
        ps = emitter.GetComponent<ParticleSystem>();
        grab = GetComponent<BNG.Grabbable>();
        sprayCanSound = GetComponent<AudioSource>();
        canvasJoint = transform.Find("CanvasJoint");
        sprayStat = transform.Find("CanvasJoint/Canvas/SprayStat").GetComponent<TMP_Text>();
        
        if(type == SprayType.xRed || type == SprayType.xGreen || type == SprayType.xBlue || type == SprayType.xBlack)
        {
            isCoat = true;
        }

        if(type == SprayType.Black || type == SprayType.xBlack)
        {
            isBlack = true;
        }

        Gradient grad = new Gradient();
        grad.SetKeys
        (
            new GradientColorKey[]
            {
                new GradientColorKey(colors[(int)type] * 0.25f, 0.0f),
                new GradientColorKey(colors[(int)type] * 0.10f, 1.0f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );

        var col = ps.colorOverLifetime;
        col.color = grad;
    }

    // Update is called once per frame

    static RaycastHit[] HitInfos = new RaycastHit[5];
    void Update()
    {
        if(ps.isStopped && grab.BeingHeld && Input.GetMouseButtonDown((int)MouseButton.Left))
        {
            ps.Play();
            sprayCanSound.Play();
        }
        else if(ps.isPlaying && (!grab.BeingHeld || Input.GetMouseButtonUp((int)MouseButton.Left)))
        {
            ps.Stop();
            sprayCanSound.Stop();
        }

        if(grab.BeingHeld)
        {
            updateSprayStat();
        }
        else
        {
            sprayStat.text = "";
        }

        if(ps.isPlaying)
        {
            // Draw with mouse
            if(Input.GetMouseButton((int)MouseButton.Left))
            {
                int hits = Physics.RaycastNonAlloc(sprayDir.position, sprayDir.right, HitInfos, reach, 1 << LayerMask.NameToLayer("Drawable"));

                for(int i = 0; i < hits; i++)
                {
                    var collider = HitInfos[i].collider;
                    var drawable = collider.GetComponentInParent<DrawableBehaviour>();
                    if(drawable == null) return;


                    if(type == SprayType.xBlack)
                    {
                        StartCoroutine(delayedUncoat(drawable, HitInfos[i].textureCoord));
                    }
                    else
                    {
                        StartCoroutine
                        (
                            isCoat ? 
                                delayedCoat(drawable, HitInfos[i].textureCoord) :
                                delayedDraw(drawable, HitInfos[i].textureCoord)
                        );
                    }
                }
            }
        }
    }

    void updateSprayStat()
    {
        // if(isBlack) return;
        Transform camera = Camera.main.transform;

        // Get direction from camera to canvas
        var cameraToCanvas = (- camera.position + canvasJoint.position);
        var right = Vector3.Cross(canvasJoint.parent.up, cameraToCanvas.normalized);

        // Rotate joint
        canvasJoint.LookAt(canvasJoint.position - right, canvasJoint.parent.up);

        float res = FindObjectOfType<DrawableBehaviour>().resolution;
        res *= res;

        string s = "";
        for(int i = 0; i < DrawableBehaviour.ColorStrings.Length; i++)
        {
            // if(TypeToString[(int)type] == DrawableBehaviour.ColorStrings[i])
            {
                s += $"{DrawableBehaviour.ColorStrings[i]}: {DrawableBehaviour.GlobalColorCountBuffer[i]/res:0.00}m\xB2\n";
                // break;
            }
        }
        sprayStat.text = s;
    }

    IEnumerator delayedDraw(DrawableBehaviour drawable, Vector2 texCoord)
    {
        yield return new WaitForSeconds(delay);
        drawable.Draw(colors[(int)type], texCoord, radius);
    }

    IEnumerator delayedCoat(DrawableBehaviour drawable, Vector2 texCoord)
    {
        yield return new WaitForSeconds(delay);
        drawable.Coat(colors[(int)type-1], texCoord, radius);
    }

    IEnumerator delayedUncoat(DrawableBehaviour drawable, Vector2 texCoord)
    {
        yield return new WaitForSeconds(delay);
        drawable.Uncoat(texCoord, radius);
    }

    void OnCollisionEnter(Collision other)
    {
        float power = other.impulse.sqrMagnitude;

        if(power < sprayCanCollideSoundMinImpact) return;

        var clampedPower = Mathf.Clamp(power, sprayCanCollideSoundMinImpact, 8);
        AudioSource.PlayClipAtPoint
        (
            sprayCanCollideSound, 
            transform.position, 
            (clampedPower-sprayCanCollideSoundMinImpact)/(8-sprayCanCollideSoundMinImpact)
        );
    }

}
