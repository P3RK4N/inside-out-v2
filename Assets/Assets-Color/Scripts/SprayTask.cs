using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

[DefaultExecutionOrder(1)]
public class SprayTask : MonoBehaviour
{
    [SerializeField] GameObject vanishFX;
    [SerializeField] AudioClip successSFX;

    SprayBehaviour[] sprays;
    Dictionary<SprayBehaviour, Vector3> spray2Position = new Dictionary<SprayBehaviour, Vector3>();

    [System.Serializable]
    struct Task
    {
        [SerializeField] public string name;
        [SerializeField] public float[] amounts;
        [SerializeField] public SprayBehaviour.SprayType[] allowedTypes;
    }

    [SerializeField]
    List<Task> tasks = new List<Task>();
    int currentTask = -1;

    TMP_Text taskBoard;

    // Start is called before the first frame update
    void Awake()
    {
        return;
        // Validate tasks
        foreach(var task in tasks)
        {
            Assert.IsTrue(task.amounts.Length == DrawableBehaviour.NumColors, "Amounts count mismatch!");
        }

        sprays = GetComponentsInChildren<SprayBehaviour>();
        taskBoard = GameObject.Find("Task").GetComponent<TMP_Text>();

        foreach(var spray in sprays)
        {
            spray2Position[spray] = spray.transform.position;
        }

        nextTask();
    }

    void nextTask()
    {
        currentTask++;

        foreach(var drawableSurface in FindObjectsOfType<DrawableBehaviour>())
        {
            drawableSurface.Clear();
        }

        if(currentTask == tasks.Count)
        {
            foreach (var spray in sprays)
            {
                resetSpray(spray);
            }
            taskBoard.text = "Kraj!";
            return;
        }

        foreach(var spray in sprays)
        {
            var grabber = spray.grab.GetPrimaryGrabber();
            if(grabber != null)
            {
                grabber.TryRelease();
            }
            
            Assert.IsTrue(!spray.grab.BeingHeld, "Grabber did not let go!");

            if(spray.gameObject.activeInHierarchy)
            {
                Instantiate(vanishFX, spray.transform.position, Quaternion.identity);
                spray.gameObject.SetActive(false);
            }
        }

        foreach(var allowedType in tasks[currentTask].allowedTypes)
        {
            foreach(var spray in sprays)
            {
                if(spray.type == allowedType)
                {
                    resetSpray(spray);
                    break;
                }
            }
        }
    }

    void checkTask()
    {
        if(currentTask < 0 || currentTask >= tasks.Count) return;

        bool completed = true;
        float res = DrawableBehaviour.Resolution;
        res *= res;

        for(int i = 0; i < DrawableBehaviour.NumColors; i++)
        {
            if(DrawableBehaviour.GlobalColorCountBuffer[i]/res < tasks[currentTask].amounts[i])
            {
                completed = false;
            }
        }

        if(!completed) return;

        AudioSource.PlayClipAtPoint(successSFX, Camera.main.transform.position);
        nextTask();
    }

    void resetSpray(SprayBehaviour spray)
    {
        spray.gameObject.SetActive(true);
        spray.rb.velocity = Vector3.zero;
        spray.rb.angularVelocity = Vector3.zero;

        spray.transform.position = spray2Position[spray];
    }

    void updateTaskUI()
    {
        if(currentTask < 0 || currentTask >= tasks.Count) return;

        string s = "";
        s += "Zadatak\n";
        s += tasks[currentTask].name + "\n";

        float res = DrawableBehaviour.Resolution;
        res *= res;

        for(int i = 0; i < DrawableBehaviour.NumColors; i++)
        {
            if(tasks[currentTask].amounts[i] == 0) continue;

            s += DrawableBehaviour.Boje[i] + ": " + $"{DrawableBehaviour.GlobalColorCountBuffer[i]/res:0.00}" + "/" + tasks[currentTask].amounts[i] + " m\xB2\n";
        }

        taskBoard.text = s;
    }

    void Update()
    {
        checkTask();
        updateTaskUI();
    }
}
