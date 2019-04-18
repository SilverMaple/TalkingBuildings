using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class showLogin : MonoBehaviour
{
    // Start is called before the first frame update
    public Button showbtn;
    public GameObject showpanle;
    public GameObject hidepanle;
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

    public void showpannel()
    {
        isshow = true;
        showpanle.SetActive(isshow);
        if (inputFiled == null)
        {
            return;
        }
        AppManager.Instance.RequestResetPassword(inputFiled.text, "Email");
        /*btn = loginbtn.GetComponent<Button>();
        btn.onClick.AddListener(delegate () {
            isshow = !isshow;
            loginpanle.SetActive(isshow);

        });*/
    }

    public void hidepannel()
    {
        isshow = false ;
        hidepanle.SetActive(isshow);
    }
}
