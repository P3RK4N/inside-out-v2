using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

using UnityEngine.Events;
using UnityEngine.EventSystems;
using Localization;

public class MainMenu : MonoBehaviour
{
    [SerializeField] AudioClip buttonClickSFX;
    [Space]
    [SerializeField] GameObject fader;
    [Space]
    [SerializeField] Button PlayButton;
    [SerializeField] Button QuizButton;
    [SerializeField] Button SettingsButton;
    [SerializeField] Button CreditsButton;
    [SerializeField] Button ExitButton;
    [SerializeField] Button BackButton;
    [Space]
    [SerializeField] TMP_Text Title;
    [SerializeField] TMP_Text Credits;
    [Space]
    [SerializeField] GameObject Volume; 
    // [SerializeField] GameObject Language;
    
    public static MainMenu Instance;

    void Awake()
    {
        if(Instance != null) Destroy(this);
        else Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        localize();

        PlayButton.onClick.AddListener      (() => { onAnyButtonClick(); StartCoroutine(onPlay());  });
        QuizButton.onClick.AddListener      (() => { onAnyButtonClick(); StartCoroutine(onQuiz());  });
        SettingsButton.onClick.AddListener  (() => { onAnyButtonClick(); onSettings();              });
        CreditsButton.onClick.AddListener   (() => { onAnyButtonClick(); onCredits();               });
        ExitButton.onClick.AddListener      (() => { onAnyButtonClick(); onExit();                  });
        BackButton.onClick.AddListener      (() => { onAnyButtonClick(); onBack();                  });


        Volume.GetComponentInChildren<Slider>().value = AudioListener.volume;
        Volume.GetComponentInChildren<Slider>().onValueChanged.AddListener((call) => AudioListener.volume = call);
        // Language.GetComponentInChildren<Button>().onClick.AddListener(() => { onAnyButtonClick(); onLang();});

        // PersistentData pd = Serializer.Load();
        // if(pd == null || !pd.storyCompleted) QuizButton.interactable = false;
        
        StartCoroutine(lateStart());
    }

    IEnumerator lateStart()
    {
        yield return new WaitForSeconds(1.0f);

        Fader.Instance.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
    }

    IEnumerator onPlay() 
    {
        float fadeIn = 0.0f;
        float fadeTime = 1.0f;

        Fader.Instance.gameObject.SetActive(true);
        while(fadeIn < fadeTime)
        {
            fadeIn += Time.deltaTime;
            FindObjectOfType<PlayerID>().GetComponent<AudioSource>().volume = fadeTime - fadeIn;
            Fader.Instance.setTransparency(fadeIn / fadeTime);
            yield return new WaitForEndOfFrame();
        }

        SceneManager.LoadScene("Sandbox");
    }

    IEnumerator onQuiz() 
    {
        float fadeIn = 0.0f;
        float fadeTime = 1.0f;

        Fader.Instance.gameObject.SetActive(true);
        while(fadeIn < fadeTime)
        {
            fadeIn += Time.deltaTime;
            FindObjectOfType<PlayerID>().GetComponent<AudioSource>().volume = fadeTime - fadeIn;
            Fader.Instance.setTransparency(fadeIn / fadeTime);
            yield return new WaitForEndOfFrame();
        }

        SceneManager.LoadScene("Quiz");
    }

    void onSettings() 
    {
        Title.text = Loc.loc(new string[]{ "Settings", "Postavke" });

        PlayButton.gameObject.SetActive(false);
        QuizButton.gameObject.SetActive(false);
        SettingsButton.gameObject.SetActive(false);
        CreditsButton.gameObject.SetActive(false);
        ExitButton.gameObject.SetActive(false);
        BackButton.gameObject.SetActive(true);
        Credits.gameObject.SetActive(false);
        Volume.SetActive(true);
        // Language.gameObject.SetActive(true);

    }

    void onCredits() 
    {
        Title.text = Loc.loc(new string[]{ "Credits", "Zasluge" });

        PlayButton.gameObject.SetActive(false);
        QuizButton.gameObject.SetActive(false);
        SettingsButton.gameObject.SetActive(false);
        CreditsButton.gameObject.SetActive(false);
        ExitButton.gameObject.SetActive(false);
        BackButton.gameObject.SetActive(true);
        Credits.gameObject.SetActive(true);
        Volume.SetActive(false);

    }

    void onBack() 
    {
        Title.text = "InsideOut";

        PlayButton.gameObject.SetActive(true);
        QuizButton.gameObject.SetActive(true);
        SettingsButton.gameObject.SetActive(true);
        CreditsButton.gameObject.SetActive(true);
        ExitButton.gameObject.SetActive(true);
        BackButton.gameObject.SetActive(false);
        Credits.gameObject.SetActive(false);
        Volume.SetActive(false);
        // Language.gameObject.SetActive(false);

    }

    void onExit() 
    {
        Application.Quit();
    }

    void onAnyButtonClick()
    {
        AudioSource.PlayClipAtPoint(buttonClickSFX, FindObjectOfType<PlayerID>().transform.position);
    }

    void onLang()
    {
        if(Settings.GameLanguage == Languages.English) 
        {
            Settings.GameLanguage = Languages.Hrvatski;
        }
        else if(Settings.GameLanguage == Languages.Hrvatski) 
        {
            Settings.GameLanguage = Languages.English;
        }

        localize();
    }

    void localize()
    {
        Title.text = 
            PlayButton.gameObject.activeInHierarchy ? 
                "InsideOut" : 
                Volume.gameObject.activeInHierarchy ? 
                    Loc.loc(new string[]{ "Settings", "Postavke" }) : 
                    Loc.loc(new string[]{ "Credits", "Zasluge" });

        Credits.text = Loc.loc(MainMenuTxt.CreditsText);
        Loc.locChild(Volume, MainMenuTxt.Volume);
        Loc.locChild(PlayButton, MainMenuTxt.Play);
        Loc.locChild(SettingsButton, MainMenuTxt.Settings);
        Loc.locChild(CreditsButton, MainMenuTxt.Credits);
        Loc.locChild(QuizButton, MainMenuTxt.Quiz);
        Loc.locChild(BackButton, MainMenuTxt.Back);
        Loc.locChild(ExitButton, MainMenuTxt.Exit);
        // Language.GetComponentInChildren<TMP_Text>().text = Loc.loc(MainMenuTxt.Language);
        // Loc.locChild(Language.GetComponentInChildren<Button>(), Loc.loc());
    }
}
