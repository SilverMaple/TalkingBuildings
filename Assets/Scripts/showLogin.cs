using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class showLogin : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject showbtn;
    public GameObject showpanle;
    public GameObject hidepanle;
    public GameObject hidepanle2;
    public InputField inputFiled;
    bool isshow=false;
    Button btn;
    

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SendResetEmail()
    {
        if (inputFiled.text == null)//判定无效？？
        {
            
            showpanle.SetActive(false);
            return;
        }
        else
        {
           AppManager.Instance.RequestResetPassword(inputFiled.text, "Email");
           showpanle.SetActive(true);
        }
        
        //hidepanle.SetActive(false);
    }

    public void showpannel()
    {
        isshow = true;
        showpanle.SetActive(isshow);
    }

    public void hidepannel()
    {
        isshow = false ;
        hidepanle.SetActive(isshow);
        if (AppManager.Instance.InitialState)
        {
            AppManager.Instance.deactivateARCamera();//关闭
        }
    }

    public void hidepannel2()
    {
        isshow = false;
        hidepanle2.SetActive(isshow);
    }
    public void exit()
    {
        Debug.Log("It's working :)");
        Application.Quit();
    }
}
