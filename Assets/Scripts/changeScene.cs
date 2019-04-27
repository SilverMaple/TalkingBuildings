using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vuforia;

public class changeScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Onclick()
    {
        //SceneManager.LoadScene("AR");
        AsyncOperation operation = SceneManager.LoadSceneAsync("AR");
        operation.completed += Operation_completed;
    }

    private void Operation_completed(AsyncOperation obj)
    {
        AppManager.Instance.InitARScene();
    }

    public void InitARScene()
    {
        AppManager.Instance.InitARScene();
    }

    public void Return()
    {
        SceneManager.LoadScene("ARv2");
    }

    public void exitGame()
    {
        Application.Quit();
    }
}
