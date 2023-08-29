using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[DefaultExecutionOrder(1)]
public class SprayTask : MonoBehaviour
{
    SprayBehaviour[] sprays;
    Dictionary<SprayBehaviour, Vector3> spray2Position = new Dictionary<SprayBehaviour, Vector3>();

    [System.Serializable]
    struct Task
    {
        [SerializeField] public float[] amounts;
        [SerializeField] public SprayBehaviour.SprayType[] allowedTypes;
    }

    [SerializeField]
    List<Task> tasks = new List<Task>();
    int currentTask = -1;

    // Start is called before the first frame update
    void Awake()
    {
        // Validate tasks
        foreach(var task in tasks)
        {
            Assert.IsTrue(task.amounts.Length == DrawableBehaviour.NumColors, "Amounts count mismatch!");
        }

        sprays = GetComponentsInChildren<SprayBehaviour>();
        
        Debug.Log(sprays.Length);

        foreach(var spray in sprays)
        {
            spray2Position[spray] = spray.transform.position;
        }

        nextTask();
    }

    void nextTask()
    {
        currentTask++;

        if(currentTask == tasks.Count)
        {
            // TODO Change
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

            spray.gameObject.SetActive(false);
        }

        foreach(var allowedType in tasks[currentTask].allowedTypes)
        {
            foreach(var spray in sprays)
            {
                if(spray.type == allowedType)
                {
                    spray.gameObject.SetActive(true);
                    spray.transform.position = spray2Position[spray];
                    break;
                }
            }
        }
    }

    void checkTask()
    {
        if(currentTask < 0 || currentTask >= tasks.Count) return;

        bool completed = true;
        for(int i = 0; i < DrawableBehaviour.NumColors; i++)
        {
            if(DrawableBehaviour.GlobalColorCountBuffer[i] < tasks[currentTask].amounts[i])
            {
                completed = false;
                break;
            }
        }

        if(!completed) return;

        Debug.Log("Task completed!");
        nextTask();
    }

    void Update()
    {
        checkTask();
    }
}
