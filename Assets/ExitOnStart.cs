using System.Collections;
using System.Collections.Generic;
using BNG;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ExitOnStart : MonoBehaviour
{
    InputBridge input;

    // Start is called before the first frame update
    void Awake()
    {
        input = GetComponent<InputBridge>();
    }

    // Update is called once per frame
    void Update()
    {
        if
        (
            input.StartButtonDown ||
            Input.GetKeyDown(KeyCode.Escape) ||
            OVRInput.GetDown(OVRInput.Button.Start)
        )
        {
            SceneManager.LoadScene("Main Menu");
        }
    }
}
