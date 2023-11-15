using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public struct Question
{
    [SerializeField]
    public string text;
    [SerializeField]
    public List<string> answers;
    [SerializeField]
    public string solution;

    public Question(string text, List<string> answers, string solution)
    {
        this.text = text;
        this.answers = answers;
        this.solution = solution;
    }
}


public class Quiz : MonoBehaviour
{
    [SerializeField]
    string questionFile = "";

    [SerializeField]
    List<Question> Questions;

    [SerializeField]
    AudioClip correctSFX;
    
    [SerializeField]
    AudioClip incorrectSFX;

    Transform Answers;
    bool answerProcessing = false;
    bool quizBegan = false;
    int currentQuestion;
    int correct = 0;

    static string[] InsideOutQuiz =
    {
        "InsideOut - Quiz",
        "InsideOut - Kviz"
    };

    static string[] Intro =
    {
        "Welcome to the quiz!\nThere are {0} questions and you will have 15 seconds for each. Press start button when you are ready.",
        "Dobrodošli na kviz!\nPostoji {0} pitanja i za svako imate 15 sekundi. Pritisnite gumb za početak kada ste spremni."
    };

    static string[] StartQuiz =
    {
        "Start Quiz!",
        "Započni kviz!"
    };

    void Awake()
    {
        if(questionFile.Length > 0)
        {
            Questions = QuestionParser.Default();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Answers = transform.Find("Canvas/Question/Answers");
        transform.Find("Canvas/Title").GetComponent<TMP_Text>().text = Localization.Loc.loc(InsideOutQuiz);
        transform.Find("Canvas/Intro").GetComponent<TMP_Text>().text = string.Format(Localization.Loc.loc(Intro), Questions.Count);
        transform.Find("Canvas/StartButton").GetComponent<Button>().onClick.AddListener(() => beginQuiz());
        transform.Find("Canvas/StartButton").GetComponentInChildren<TMP_Text>().text = Localization.Loc.loc(StartQuiz);
        registerAnswerButtons();
    }

    void registerAnswerButtons()
    {
        for(int i = 0; i < Answers.childCount; i++)
        {
            int k = i;
            Answers.GetChild(i).GetComponent<Button>().onClick.AddListener(() => StartCoroutine(onAnswerClicked(k)));
        }
    }

    void beginQuiz()
    {
        if(quizBegan) return;
        quizBegan = true;

        transform.Find("Canvas/Intro").gameObject.SetActive(false);
        transform.Find("Canvas/StartButton").gameObject.SetActive(false);
        transform.Find("Canvas/Question").gameObject.SetActive(true);

        currentQuestion = 0;
        setQuestion();
    }

    void setQuestion()
    {
        StartCoroutine(startTimer());
        setPossibleAnswers(Questions[currentQuestion].answers);
    }

    IEnumerator startTimer()
    {
        int timeLeft = 15;

        while(timeLeft > 0 && !answerProcessing)
        {
            setQuestionTitle
            (
                string.Format("[{0}/{1}] ", currentQuestion+1, Questions.Count) + 
                Questions[currentQuestion].text + 
                string.Format(" {0}", timeLeft--)
            );
            yield return new WaitForSeconds(1.0f);
        }

        if(timeLeft == 0) StartCoroutine(onAnswerClicked(-1));
    }

    void setQuestionTitle(string title)
    {
        transform.Find("Canvas/Question/QuestTitle").GetComponent<TMP_Text>().text = title;
    }

    void setPossibleAnswers(List<string> answers)
    {
        if(answers.Count < 2 || answers.Count > 4) throw new System.Exception("Invalid amount of possible answers! Range [2,4]");
        for(int i = 0; i < 4; i++)
        {
            var obj = Answers.GetChild(i);
            if(i <= answers.Count)
            {
                obj.GetComponent<QuizButton>().setAnswer(answers[i]);
            }
            else
            {
                obj.gameObject.SetActive(false);
            }
        }
    }

    void showResult(int answer)
    {
        for(int i = 0; i < Questions[currentQuestion].answers.Count; i++)
        {
            var q = Answers.GetChild(i).GetComponent<QuizButton>();
            q.setBlockEvents(true);
            if(Questions[currentQuestion].answers[i] == Questions[currentQuestion].solution) q.mark(true);
            else if(i == answer) q.mark(false);
        }
    }

    static string[] QuizFinished =
    {
        "Quiz Finished!\nScore: ",
        "Kviz završen!\nRezultat: "
    };

    void showFinalResult()
    {
        transform.Find("Canvas/Question/QuestTitle").gameObject.SetActive(false);
        foreach (Transform tf in Answers) tf.gameObject.SetActive(false);

        var res = transform.Find("Canvas/Result");
        res.gameObject.SetActive(true);
        res.GetComponent<TMP_Text>().text = Localization.Loc.loc(QuizFinished) + correct;
    }

    IEnumerator onAnswerClicked(int answer)
    {
        if(!answerProcessing)
        {
            answerProcessing = true;

            if(answer >= 0 && Questions[currentQuestion].solution == Questions[currentQuestion].answers[answer])
            {
                correct++;
                AudioSource.PlayClipAtPoint(correctSFX, transform.position);
            }
            else
            {
                AudioSource.PlayClipAtPoint(incorrectSFX, transform.position);
            }

            showResult(answer);

            yield return new WaitForSeconds(4.0f);

            currentQuestion++;

            answerProcessing = false;

            if(currentQuestion == Questions.Count) showFinalResult();
            else setQuestion();
        } 
    }
}
