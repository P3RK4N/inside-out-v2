using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using BNG;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

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
        White
    }

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
        Color.white
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

    Transform sprayDir;
    ParticleSystem ps;
    AudioSource sprayCanSound;
    Grabbable grab;

    // Start is called before the first frame update
    void Awake()
    {
        sprayDir = transform.Find("Dir");
        var emitter = Instantiate(sprayEmitter, sprayDir);
        ps = emitter.GetComponent<ParticleSystem>();
        grab = GetComponent<BNG.Grabbable>();
        sprayCanSound = GetComponent<AudioSource>();

        if(type == SprayType.xRed || type == SprayType.xGreen || type == SprayType.xBlue)
        {
            isCoat = true;
        }

        Gradient grad = new Gradient();
        grad.SetKeys
        (
            new GradientColorKey[]{ new GradientColorKey(Color.white, 0.0f), new GradientColorKey(colors[(int)type], 1.0f) },
            new GradientAlphaKey[]{ new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
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


                    StartCoroutine(isCoat ? delayedCoat(drawable, HitInfos[i].textureCoord) : delayedDraw(drawable, HitInfos[i].textureCoord));
                }
            }
        }
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
