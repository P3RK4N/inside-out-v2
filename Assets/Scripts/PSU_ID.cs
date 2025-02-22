using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSU_ID : MonoBehaviour
{
    [SerializeField] AudioClip powerOn;

    // Start is called before the first frame update
    void Start()
    {
        TransitionManager.AttachCallbackEvent.AddListener((comp) => onAttach(comp));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    static string[] PsuAttached = 
    {
        "Power Supply attached successfuly!",
        "Napajanje uspješno priključeno!"
    };

    void onAttach(PC.Component comp)
    {
        if(comp.name == "PSU") 
        {
            Debug.Log("PSU Attached!");
            AudioSource.PlayClipAtPoint(powerOn, comp.snapPositions[0]);
            PromptScript.instance.updatePrompt(Localization.Loc.loc(PsuAttached), 3.0f);
        }
    }
}
