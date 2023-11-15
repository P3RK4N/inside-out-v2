using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Localization;

public class Exit : MonoBehaviour
{
    bool exiting = false;

    // Start is called before the first frame update
    void Start()
    {
        localize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(!exiting && other.gameObject.GetComponentInParent<PlayerID>() != null) 
            StartCoroutine(ExitToMenu());
    }

    void localize()
    {
        Localization.Loc.locObj(transform.parent.Find("Door/Door_Wood_One/Title"), StoryTxt.ExitToMainMenu);
    }

    IEnumerator ExitToMenu()
    {
        exiting = true;
        float fadeIn = 0.0f;
        float fadeTime = 1.0f;

        var obj = GameObject.Find("Fader");
        obj.transform.GetChild(0).gameObject.SetActive(true);

        yield return new WaitForEndOfFrame();
        
        Fader f = obj.GetComponentInChildren<Fader>();

        while(fadeIn < fadeTime)
        {
            fadeIn += Time.deltaTime;
            f.setTransparency(fadeIn / fadeTime);
            yield return new WaitForEndOfFrame();
        }

        SceneManager.LoadScene("Main Menu");
    }
}
