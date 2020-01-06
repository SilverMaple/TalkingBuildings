using LeanCloud;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Vuforia;

public struct ResultStatus
{
    public bool IsSuccess;
    public string Message;
}

public class AppManager : MonoBehaviour
{
    #region PublicAttributes:ScriptMountObject
    public Camera ARCamera;
    public static GameObject leanCloudObject;
    public bool InitialState = false;

    // 账号管理界面
    public GameObject loginPanel;
    public GameObject userpanel;
    public GameObject userButton;
    public Button loginButton;
    public Button logoutButton;
    public Button signupButton;
    public InputField accountInputField;
    public InputField passwordInputField;
    public InputField emailInputField;

    // 个人中心界面
    public GameObject personalInformationObject;
    public GameObject mycomment;
    public GameObject myplace;

    // AR互动界面
    public GameObject arSceneCanvas;
    public Button barrageButton;
    public Button videoButton;
    public Button audioButton;
    public Button snapshotButton;
    public Button commentButton;
    public InputField barrageInputField;
    public Button sendBarrageButton;
    public bool showVideo = false;
    public bool showAudio = false;
    public bool showBarrage = true;
    public bool showDescription = false;

    public GameObject butterflyAnimation;
    #endregion

    #region PrivateAttributes
    // Singleton
    private static AppManager _instance;
    #endregion

    #region LifeCycle
    /// <summary>
    /// 单例模式实现
    /// </summary>
    public static AppManager Instance
    {
        get
        {
            if (_instance == null)
            {
                //leanCloudObject = GameObject.Find("LeanCloudObject");
                //DontDestroyOnLoad(leanCloudObject);
                return new AppManager();
            }
            return _instance;
        }
    }

    /// <summary>
    /// 初始化函数，游戏开始时自动调用
    /// </summary>
    void Awake()
    {
        _instance = this;
        // 防止载入新场景时被销毁
        DontDestroyOnLoad(_instance.gameObject);
    }

    // 设置自动对焦，效果不好【挂载脚本到ARCamera下】
    //VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
    //VuforiaARController.Instance.RegisterOnPauseCallback(OnPaused);

    //private void OnVuforiaStarted()
    //{
    //    CameraDevice.Instance.SetFocusMode(
    //        CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
    //}

    //private void OnPaused(bool paused)
    //{
    //    if (!paused)
    //    { // resumed
    //        // Set again autofocus mode when app is resumed
    //        CameraDevice.Instance.SetFocusMode(
    //            CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
    //    }
    //}

    /// <summary>
    /// 初始化函数，在Update函数前,Awake函数后调用
    /// </summary>
    public void Start()
    {
        if (!InitialState)
        {
            AVObject.RegisterSubclass<Comment>();//复制comment类中的方法
            AVObject.RegisterSubclass<Place>();

            Debug.Log("Bind event for buttons");
            Button btn;

            // 登陆注册界面
            btn = (Button)loginButton.GetComponent<Button>();
            btn.onClick.AddListener(async delegate () { await LoginAsync(); });//监听委托事件，响应LoginAsync事件
            btn = (Button)logoutButton.GetComponent<Button>();
            btn.onClick.AddListener(delegate () { Logout(); });
            btn = (Button)signupButton.GetComponent<Button>();
            btn.onClick.AddListener(async delegate () { await SignupAsync(); });

            // AR互动界面
            btn = (Button)barrageButton.GetComponent<Button>();
            btn.onClick.AddListener(delegate () { ChangeBarrageState(); });
            btn = (Button)commentButton.GetComponent<Button>();
            btn.onClick.AddListener(delegate () { ChangeDescriptionState(); });
            btn = (Button)videoButton.GetComponent<Button>();
            btn.onClick.AddListener(delegate () { ChangeVideoState(); });
            btn = (Button)audioButton.GetComponent<Button>();
            btn.onClick.AddListener(delegate () { ChangeAudioState(); });
            btn = (Button)sendBarrageButton.GetComponent<Button>();
            btn.onClick.AddListener(async delegate { await SendBarrage(); });
            //barrageInputField.onEndEdit.AddListener(async delegate { await SendBarrage(); });

            // 个人中心界面
            btn = (Button)mycomment.GetComponent<Button>();
            btn.onClick.AddListener(async delegate () { await GetMyComment(); });

            Debug.Log("End bind event for buttons");
        }
    }

    /// <summary>
    /// 刷新函数，每帧调用
    /// </summary>
    void Update()
    {

    }
    #endregion

    #region ARInteractiveInterface
    /// <summary>
    /// 改变识别物体后，弹幕显示状态
    /// </summary>
    private void ChangeBarrageState()
    {
        showBarrage = !showBarrage;
        Debug.Log("ShowBarrage: " + showBarrage);
        ShowAndroidToastMessage("ShowBarrage: " + showBarrage);
        barrageInputField.gameObject.SetActive(showBarrage);
        sendBarrageButton.gameObject.SetActive(showBarrage);
        var imageTargets = GameObject.FindGameObjectWithTag("ImageTargetList").GetComponentsInChildren<Transform>();
        for (int i = 1; i < imageTargets.Length; i++)
        {
            Transform canvasGameObject = imageTargets[i].Find("DisplayCanvas");
            if (canvasGameObject == null)
                continue;
            for (int j = 0; j < canvasGameObject.childCount; j++)
            {
                if (canvasGameObject.GetChild(j).name == "ImageTemplate(Clone)")
                    canvasGameObject.GetChild(j).gameObject.SetActive(showBarrage);
            }
        }
    }

    /// <summary>
    /// 改变识别物体后，描述显示状态
    /// </summary>
    private void ChangeDescriptionState()
    {
        showDescription = !showDescription;
        Debug.Log("showDescription: " + showDescription);
        ShowAndroidToastMessage("showDescription: " + showDescription);
        var imageTargets = GameObject.FindGameObjectWithTag("ImageTargetList").GetComponentsInChildren<Transform>();
        for (int i = 1; i < imageTargets.Length; i++)
        {
            Transform canvasGameObject = imageTargets[i].Find("DisplayCanvas");
            if (canvasGameObject == null)
                continue;
            for (int j = 0; j < canvasGameObject.childCount; j++)
            {
                if (canvasGameObject.GetChild(j).name == "DescriptionText(Clone)")
                    canvasGameObject.GetChild(j).gameObject.SetActive(showDescription);
            }
        }
    }

    /// <summary>
    /// 改变识别物体后，视频显示状态
    /// </summary>
    private void ChangeVideoState()
    {
        showVideo = !showVideo;
        Debug.Log("showVideo: " + showVideo);
        ShowAndroidToastMessage("showVideo: " + showVideo);
        var imageTargets = GameObject.FindGameObjectWithTag("ImageTargetList").GetComponentsInChildren<Transform>();
        for (int i = 1; i < imageTargets.Length; i++)
        {
            Transform canvasGameObject = imageTargets[i].Find("DisplayCanvas");
            if (canvasGameObject == null)
                continue;
            for (int j = 0; j < canvasGameObject.childCount; j++)
            {
                if (canvasGameObject.GetChild(j).name == "VideoTemplate(Clone)")
                    canvasGameObject.GetChild(j).gameObject.SetActive(showVideo);
            }
        }
    }

    /// <summary>
    /// 改变识别物体后，音频显示状态
    /// </summary>
    private void ChangeAudioState()
    {
        showAudio = !showAudio;
        Debug.Log("showAudio: " + showAudio);
        ShowAndroidToastMessage("showAudio: " + showAudio);
        var imageTargets = GameObject.FindGameObjectWithTag("ImageTargetList").GetComponentsInChildren<Transform>();
        for (int i = 1; i < imageTargets.Length; i++)
        {
            Transform canvasGameObject = imageTargets[i].Find("DisplayCanvas");
            if (canvasGameObject == null)
                continue;
            for (int j = 0; j < canvasGameObject.childCount; j++)
            {
                if (canvasGameObject.GetChild(j).name == "AudioTemplate(Clone)")
                    canvasGameObject.GetChild(j).gameObject.GetComponent<AudioSource>().enabled = showAudio;
            }
        }
    }

    /// <summary>
    /// 开启Vuforia ARCamera
    /// </summary>
    public void activateARCamera()
    {
        // 为了防止花屏，闪屏
        deactivateARCamera();
        ARCamera.GetComponent<VuforiaBehaviour>().enabled = true;
        // 设置ARCamera的World Center Mode 即可解决
        //if (TrackerManager.Instance.GetTracker<ObjectTracker>() != null)
        //{
        //    ObjectTracker objTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        //    objTracker.Start();
        //}
    }

    /// <summary>
    /// 关闭Vuforia ARCamera
    /// </summary>
    public void deactivateARCamera()
    {
        //if (TrackerManager.Instance.GetTracker<MarkerTracker>() != null)
        //{
        //    MarkerTracker marker = TrackerManager.Instance.GetTracker<MarkerTracker>();
        //    marker.Stop();
        //}

        //if (TrackerManager.Instance.GetTracker<TextTracker>() != null)
        //{
        //    TextTracker textTracker = TrackerManager.Instance.GetTracker<TextTracker>();
        //    textTracker.Stop();
        //}

        //if (TrackerManager.Instance.GetTracker<ObjectTracker>() != null)
        //{
        //    ObjectTracker objTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        //    objTracker.Stop();
        //}

        ARCamera.GetComponent<VuforiaBehaviour>().enabled = false;
    }

    /// <summary>
    /// 初始化AR场景
    /// </summary>
    public async Task InitARScene()
    {
        activateARCamera();
        if (!InitialState)
        {
            //activateARCamera();
            Debug.Log("Create target for all image targets");
            //var temp = SceneManager.GetSceneByName("AR").GetRootGameObjects();

            var imageTargets = GameObject.FindGameObjectWithTag("ImageTargetList").GetComponentsInChildren<Transform>();
            Text templateText = GameObject.FindGameObjectWithTag("TextTemplate").GetComponentInChildren<Text>();
            UnityEngine.UI.Image templateImage = GameObject.FindGameObjectWithTag("ImageTemplate").GetComponentInChildren<UnityEngine.UI.Image>();
            GameObject templateVideoObject = GameObject.FindGameObjectWithTag("VideoTemplate");
            GameObject templateAudioObject = GameObject.FindGameObjectWithTag("AudioTemplate");
            UnityEngine.UI.Image templateNimbusImage = GameObject.FindGameObjectWithTag("NimbusImageTemplate").GetComponentInChildren<UnityEngine.UI.Image>();

            float scaleFactor = 0.1f;
            for (int i = 1; i < imageTargets.Length; i++)
            {
                scaleFactor = 0.1f;
                // 替换imageTarget的脚本
                Destroy(imageTargets[i].GetComponent<DefaultTrackableEventHandler>());
                imageTargets[i].gameObject.AddComponent<MyTrackableEventHandler>();
                for (int j = 0; j < imageTargets[i].childCount; j++)
                {
                    Destroy(imageTargets[i].GetChild(j).gameObject);
                }
                var canvasGameObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                canvasGameObject.name = "DisplayCanvas";
                canvasGameObject.transform.SetParent(imageTargets[i].transform);
                canvasGameObject.transform.localScale = new Vector3(.1f, .1f, .1f);
                canvasGameObject.transform.localPosition = new Vector3(0, 0, 0);
                Destroy(canvasGameObject.GetComponent<MeshRenderer>());
                canvasGameObject.AddComponent<Canvas>();
                Canvas canvas = canvasGameObject.GetComponent<Canvas>();

                /*
                 * Nimbus Image Attach
                 * 
                 */
                UnityEngine.UI.Image nimbusImage = UnityEngine.UI.Image.Instantiate(templateNimbusImage);
                nimbusImage.transform.SetParent(canvasGameObject.transform);
                nimbusImage.transform.rotation = Quaternion.FromToRotation(new Vector3(0, 1, 0), new Vector3(0, 0, 1));
                nimbusImage.transform.localPosition = new Vector3(0, 0, 0);
                nimbusImage.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                nimbusImage.gameObject.SetActive(true);

                /*
                 * Description Text Attach
                 * 
                 */
                var descritionText = Text.Instantiate<Text>(templateText);
                descritionText.tag = "DescriptionText";
                if (imageTargets[i].name == "ImageTarget (31)")
                {
                    descritionText.verticalOverflow = VerticalWrapMode.Overflow;
                    descritionText.text = "<color=red>广州市满洲窗可以追溯到清代，当时广州是中国自由的通商港口，彩色玻璃流传进入此地。深受当时的富商、官员的青睐，他们把这些彩色玻璃用在自家的窗户装饰上，既能很好的采光又达到美观大方的效果。后来，清代末年，彩色玻璃大量传入广州。\n</color>" +
                        "<color=green>经过岁月的变迁，新材料的发明，满洲窗在不同的地方也有不同的表现。现在在广州西关的很多地方的装饰都运用到了满洲窗，并运用新的技术加上改良，也有少数采仍然采用传统的生产加工工艺。\n</color>" +
                        "<color=blue>\"满洲窗\"作为广州特定历史时期下的产物，从一个侧面反映了广州乃至我国近代以来传统建筑文化与国外建筑文化逐渐相互融合的一个过程。金碧辉煌、美轮美奂的\"满洲窗\"具有非常高的历史文化和艺术欣赏价值，是岭南非物质文化遗产的重要组成部分。</color>";
                }
                else
                {
                    descritionText.text = "Tag: " + imageTargets[i].name;
                }

                descritionText.alignment = TextAnchor.MiddleCenter;
                float red = UnityEngine.Random.Range(0.0f, 1.0f);
                float green = UnityEngine.Random.Range(0.0f, 1.0f);
                float blue = UnityEngine.Random.Range(0.0f, 1.0f);
                descritionText.color = new Color(red, green, blue);

                descritionText.transform.SetParent(canvasGameObject.transform);
                descritionText.transform.localPosition = new Vector3(0, 0, 0);
                descritionText.transform.up = imageTargets[i].up;
                descritionText.transform.rotation = Quaternion.FromToRotation(new Vector3(0, 1, 0), new Vector3(0, 0, 1));
                descritionText.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

                if (!showDescription)
                {
                    descritionText.gameObject.SetActive(false);
                }

                ///*
                // * Animation Attach【Implemented by MyTrackableEventHandler.cs】
                // * 
                // */
                //if (imageTargets[i].name == "ImageTarget (23)" || imageTargets[i].name == "ImageTarget (30)")
                //{
                //    GameObject butterflyTemplate = GameObject.FindGameObjectWithTag("ButterflyPrehab");
                //    GameObject butterflyObject = GameObject.Instantiate<GameObject>(butterflyTemplate);
                //    butterflyObject.transform.SetParent(canvasGameObject.transform);
                //    butterflyObject.transform.localPosition = new Vector3(0, 0.1f, 0);
                //    butterflyObject.transform.up = imageTargets[i].up;
                //    //butterflyObject.transform.rotation = Quaternion.FromToRotation(new Vector3(1, 0, 0), new Vector3(-1, 0, 0));
                //    //butterflyObject.transform.localScale = new Vector3(scaleFactor * 10, scaleFactor * 10, scaleFactor * 10);
                //}


                /*
                 * Video Attach
                 * 
                 */
                GameObject videoObject = GameObject.Instantiate<GameObject>(templateVideoObject);
                string videoPath = "Video/video";
                if (imageTargets[i].name == "ImageTarget (31)")
                    videoPath = "Video/video1";
                else if (imageTargets[i].name == "ImageTarget (38)")
                    videoPath = "Video/video2";
                else if (imageTargets[i].name == "ImageTarget (39)")
                    videoPath = "Video/video3";
                videoObject.GetComponent<VideoPlayer>().clip = Resources.Load<VideoClip>(videoPath);
                videoObject.GetComponent<MeshRenderer>().enabled = true;
                videoObject.GetComponent<MeshCollider>().enabled = true;
                //videoObject.GetComponent<VideoPlayer>().EnableAudioTrack(0, false);
                videoObject.transform.SetParent(canvasGameObject.transform);
                // 使视频在识别物前面
                videoObject.transform.localPosition = new Vector3(0, 0.1f, 0);
                videoObject.transform.up = imageTargets[i].up;
                videoObject.transform.rotation = Quaternion.FromToRotation(new Vector3(1, 0, 0), new Vector3(-1, 0, 0));
                videoObject.transform.localScale = new Vector3(scaleFactor * 10, scaleFactor * 10, scaleFactor * 10);
                videoObject.SetActive(showVideo);

                /*
                 * Audio Attach
                 * 
                 */
                GameObject audioObject = GameObject.Instantiate<GameObject>(templateAudioObject);
                string audioPath = "Audio/audio1";
                if (imageTargets[i].name == "ImageTarget (23)")
                    audioPath = "Audio/audio2";
                else if (imageTargets[i].name == "ImageTarget (31)")
                    audioPath = "Audio/audio2";
                else if (imageTargets[i].name == "ImageTarget (38)")
                    audioPath = "Audio/audio2";
                else if (imageTargets[i].name == "ImageTarget (30)")
                    audioPath = "Audio/audio2";
                else if (imageTargets[i].name == "ImageTarget (39)")
                    audioPath = "Audio/audio2";
                audioObject.GetComponent<AudioSource>().clip = Resources.Load<AudioClip>(audioPath);
                audioObject.transform.SetParent(canvasGameObject.transform);
                audioObject.transform.localPosition = new Vector3(0, 0, 0);
                audioObject.GetComponent<AudioSource>().enabled = showAudio;

                /*
                 * Barrage Text Attach
                 * 
                 */
                //List<string> imageTargetComments = await GetPlaceComment(imageTargets[i].name);

                //scaleFactor /= 5f;
                //foreach (var s in imageTargetComments)
                //{
                //    UnityEngine.UI.Image tmpImage = UnityEngine.UI.Image.Instantiate<UnityEngine.UI.Image>(templateImage);
                //    tmpImage.transform.SetParent(canvasGameObject.transform);

                //tmpImage.transform.rotation = canvasGameObject.GetChild(0).rotation;
                //    tmpImage.transform.localPosition = new Vector3(
                //        UnityEngine.Random.Range(-Screen.width / 2 * scaleFactor, Screen.width / 2 * scaleFactor),
                //        0,
                //        UnityEngine.Random.Range(-Screen.height / 2 * scaleFactor, Screen.height / 2 * scaleFactor)
                //    );
                //    tmpImage.transform.localScale = new Vector3(scaleFactor / 2, scaleFactor / 2, scaleFactor / 2);

                //    tmpImage.GetComponentInChildren<Text>().text = s;
                //    //tmpImage.GetComponentInChildren<Text>().transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                //    tmpImage.GetComponentInChildren<Text>().horizontalOverflow = HorizontalWrapMode.Overflow;

                //    tmpImage.gameObject.AddComponent<TextItem>();
                //    tmpImage.GetComponent<TextItem>().speed = UnityEngine.Random.Range(30f * scaleFactor, 60f * scaleFactor);
                //    tmpImage.GetComponent<TextItem>().enabled = true;

                //    tmpImage.gameObject.SetActive(showBarrage);
                //}
            }

            Debug.Log("End initialization");
            ShowAndroidToastMessage("End initialization");

            //InitialState = true;
            // 每次切换场景都需要初始化
        }
    }

    /// <summary>
    /// 发送弹幕显示
    /// </summary>
    public async Task SendBarrage()
    {
        string comment = barrageInputField.text;
        Debug.Log("Send barrage: " + comment);
        ShowAndroidToastMessage("Send barrage: " + comment);
        // editor 状态没有摄像头将无法获取
        IEnumerable<TrackableBehaviour> trackerIEnumerable = TrackerManager.Instance.GetStateManager().GetActiveTrackableBehaviours();
        Debug.Log(trackerIEnumerable.Count());

        if (trackerIEnumerable.Count() > 0)
        {
            TrackableBehaviour currentTracker = trackerIEnumerable.First();
            Debug.Log(currentTracker.name); // ImageTarget(31)
            Debug.Log(currentTracker.TrackableName); // 33
            ShowAndroidToastMessage(currentTracker.name + " " + currentTracker.TrackableName);

            string barrageText = barrageInputField.text.Trim();
            if (barrageText != "")
            {
                ResultStatus rs = await SendPlaceComment(currentTracker.name, barrageText, 5, new List<string>());
                if (rs.IsSuccess == false)
                {
                    ShowAndroidToastMessage(rs.Message);
                    return;
                }
                else
                {
                    ShowAndroidToastMessage("Send success!");
                }
                Transform canvasGameObject = currentTracker.gameObject.transform.Find("DisplayCanvas");
                UnityEngine.UI.Image templateImage = GameObject.FindGameObjectWithTag("ImageTemplate").GetComponentInChildren<UnityEngine.UI.Image>();
                UnityEngine.UI.Image tmpImage = UnityEngine.UI.Image.Instantiate<UnityEngine.UI.Image>(templateImage);
                tmpImage.transform.SetParent(canvasGameObject.transform);


                float scaleFactor = 0.1f / 5f;
                tmpImage.transform.up = currentTracker.gameObject.transform.up;

                tmpImage.transform.rotation = canvasGameObject.GetChild(0).rotation;
                //tmpImage.transform.rotation = Quaternion.FromToRotation(new Vector3(0, 0, -1), new Vector3(0, 0, 1));

                for (int i = 0; i < canvasGameObject.childCount; i++)
                {
                    Console.WriteLine(canvasGameObject.GetChild(i).name, canvasGameObject.GetChild(i).rotation);
                }
                Console.WriteLine(tmpImage.transform.rotation);
                tmpImage.transform.localPosition = new Vector3(
                    //UnityEngine.Random.Range(-Screen.width / 2 * scaleFactor, Screen.width / 2 * scaleFactor),
                    -Screen.height / 2 * scaleFactor,
                    0,
                    UnityEngine.Random.Range(-Screen.height / 2 * scaleFactor, Screen.height / 2 * scaleFactor)
                );
                tmpImage.transform.localScale = new Vector3(scaleFactor / 2, scaleFactor / 2, scaleFactor / 2);

                tmpImage.GetComponentInChildren<Text>().color = Color.red;
                tmpImage.GetComponentInChildren<Text>().text = barrageText;
                tmpImage.GetComponentInChildren<Text>().horizontalOverflow = HorizontalWrapMode.Overflow;

                tmpImage.gameObject.AddComponent<TextItem>();
                tmpImage.GetComponent<TextItem>().speed = UnityEngine.Random.Range(30f * scaleFactor, 60f * scaleFactor);
                tmpImage.GetComponent<TextItem>().enabled = true;

                tmpImage.gameObject.SetActive(showBarrage);
            }
        }
        else
        {
            // Begin Test
            string barrageText = barrageInputField.text.Trim();
            if (barrageText != "")
            {
                //Transform canvasGameObject = currentTracker.gameObject.transform.Find("DisplayCanvas");
                var imageTargets = GameObject.FindGameObjectWithTag("ImageTargetList").GetComponentsInChildren<Transform>();
                GameObject aaa = imageTargets[1].gameObject;
                Transform canvasGameObject = imageTargets[2];
                //Transform canvasGameObject = aaa.transform.Find("DisplayCanvas");
                UnityEngine.UI.Image templateImage = GameObject.FindGameObjectWithTag("ImageTemplate").GetComponentInChildren<UnityEngine.UI.Image>();
                UnityEngine.UI.Image tmpImage = UnityEngine.UI.Image.Instantiate<UnityEngine.UI.Image>(templateImage);
                tmpImage.transform.SetParent(canvasGameObject.transform);

                float scaleFactor = 0.1f / 5f;
                tmpImage.transform.up = aaa.transform.up;
                tmpImage.transform.rotation = canvasGameObject.GetChild(0).rotation;
                //tmpImage.transform.rotation = Quaternion.FromToRotation(new Vector3(0, 0, -1), new Vector3(0, 0, 1));
                tmpImage.transform.localPosition = new Vector3(
                    //UnityEngine.Random.Range(-Screen.width / 2 * scaleFactor, Screen.width / 2 * scaleFactor),
                    -Screen.height / 2 * scaleFactor,
                    0,
                    UnityEngine.Random.Range(-Screen.height / 2 * scaleFactor, Screen.height / 2 * scaleFactor)
                );
                tmpImage.transform.localScale = new Vector3(scaleFactor / 2, scaleFactor / 2, scaleFactor / 2);

                tmpImage.GetComponentInChildren<Text>().color = Color.red;
                tmpImage.GetComponentInChildren<Text>().text = barrageText;
                tmpImage.GetComponentInChildren<Text>().horizontalOverflow = HorizontalWrapMode.Overflow;

                tmpImage.gameObject.AddComponent<TextItem>();
                tmpImage.GetComponent<TextItem>().speed = UnityEngine.Random.Range(30f * scaleFactor, 60f * scaleFactor);
                tmpImage.GetComponent<TextItem>().enabled = true;

                tmpImage.gameObject.SetActive(showBarrage);
            }
            // End Test

            Debug.Log("No target found.");
        }
        // clear text
        barrageInputField.text = "";
    }

    #endregion

    #region AccountManagementInterface
    /// <summary>
    /// 检查用户登录状态
    /// </summary>
    /// <returns></returns>
    public async Task<bool> CheckUserStateAsync()
    {
        var user = AVUser.CurrentUser;
        if (user == null)
        {
            return false;
        }
        var IsAuth = await user.IsAuthenticatedAsync();
        if (user.IsAnonymous || !IsAuth)
        {

            Debug.Log(String.Format("User==null:{0} Anonymous: {1} Authenticated:{2}",
                user == null, user.IsAnonymous, !IsAuth));
            return false;
        }
        Debug.Log(String.Format("Check success"));
        return true;
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="signupType">
    ///     "UserName": 用户名+密码
    ///     "PhoneNumber": 手机+验证码
    /// </param>
    public async Task<ResultStatus> SignupAsync(string signupType = "UserName")
    {
        Debug.Log("Signup Begin: ");
        //var userName = "SilverMaple";
        //var password = "password";
        //var phoneNumber = "18826076293";
        //var emailAddress = "1093387079@qq.com";

        AVUser user = null;
        ResultStatus result;
        try
        {
            var userName = accountInputField.text;
            var password = passwordInputField.text;
            var phoneNumber = "";
            var emailAddress = emailInputField.text;

            switch (signupType)
            {
                // 手机验证码注册
                case "PhoneNumber":
                    Debug.Log("Signup by phone");
                    // 发送短信验证码
                    await AVCloud.RequestSMSCodeAsync(phoneNumber);
                    var verificationCode = "171888";
                    // Task<AVUser>.Result -> AVUser
                    // 需要ContinueWith或使用await否则unity会卡死
                    Task<AVUser> t = AVUser.SignUpByMobilePhoneAsync(phoneNumber, verificationCode);
                    user = await t;
                    if (t.IsFaulted || t.IsCanceled)
                    {
                        result.IsSuccess = false;
                        result.Message = String.Format("Signup failed: {0} {1}",
                            t.Exception.Message, t.Exception.InnerExceptions[0].Message);
                        Debug.Log(result.Message);
                    }
                    else
                    {
                        user.Username = phoneNumber;
                        await user.SaveAsync();
                        result.IsSuccess = true;
                        result.Message = String.Format("Signup: {0} at {1}", user.Username, user.UpdatedAt);
                        Debug.Log(result.Message);
                    }
                    break;
                // 用户名密码注册
                case "UserName":
                    Debug.Log("Signup by username");
                    user = new AVUser();
                    user.Username = userName;
                    user.Password = password;
                    user.Email = emailAddress;
                    Task t1 = user.SignUpAsync();
                    await t1;
                    if (t1.IsFaulted || t1.IsCanceled)
                    {
                        // 展示注册错误信息
                        result.IsSuccess = false;
                        result.Message = String.Format("Signup failed: {0} {1}",
                            t1.Exception.Message, t1.Exception.InnerExceptions[0].Message);
                        Debug.Log(result.Message);
                        ShowAndroidToastMessage(result.Message);
                    }
                    else
                    {
                        result.IsSuccess = true;
                        result.Message = String.Format("Signup: {0} at {1}", user.Username, user.UpdatedAt);
                        Debug.Log(result.Message);
                        ShowAndroidToastMessage(result.Message);
                    }
                    break;
                default:
                    result.Message = "Error signup type";
                    result.IsSuccess = false;
                    ShowAndroidToastMessage(result.Message);
                    break;

            }
        }
        catch (Exception e)
        {
            result.IsSuccess = false;
            result.Message = String.Format("Exception: Signup failed: {0} {1}",
                e.Message, e.StackTrace);
            Debug.Log(result.Message);
            ShowAndroidToastMessage(result.Message);
        }
        return result;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="loginType">
    ///     "UserName": 用户名+密码
    ///     "PhoneNumber": 手机+密码
    ///     "verificationCode": 手机+验证码
    /// </param>
    public async Task<ResultStatus> LoginAsync(string loginType = "UserName")
    {
        Debug.Log("Login Begin: ");
        //获取输入框的值
        var phoneNumber = "";
        var userName = accountInputField.text;
        var password = passwordInputField.text;

        Task<AVUser> t = null;
        AVUser user = null;
        ResultStatus result;

        try
        {
            switch (loginType)
            {
                case "UserName":
                    t = AVUser.LogInAsync(userName, password);
                    user = await t;
                    break;
                case "PhoneNumber":
                    t = AVUser.LogInByMobilePhoneNumberAsync(phoneNumber, password);
                    user = await t;
                    break;
                case "VerificationCode":
                    await AVCloud.RequestSMSCodeAsync(phoneNumber);
                    var verficationCode = "171888";
                    t = AVUser.SignUpOrLoginByMobilePhoneAsync(phoneNumber, verficationCode);
                    user = await t;
                    break;
                default:
                    break;
            }
            if (!t.IsFaulted && !t.IsCanceled)
            {
                result.Message = String.Format("Login: {0} at {1}", user.Username, user.UpdatedAt);
                result.IsSuccess = true;
                //获取到子孙节点
                var nickNameText = personalInformationObject.GetComponentsInChildren<Text>().Where<Text>(text => text.name == "nickNameText").First();
                nickNameText.text = user.Username;

                Debug.Log(result.Message);
                loginPanel.SetActive(false);
                userButton.SetActive(false);//个人登录的按钮入口
                ShowAndroidToastMessage(result.Message);

                return result;
            }
            else
            {
                result.Message = String.Format("Login failed: {0} {1}",
                    t.Exception.Message, t.Exception.InnerExceptions[0].Message);
                result.IsSuccess = false;
                Debug.Log(result.Message);

                ShowAndroidToastMessage(result.Message);
                return result;
            }
        }
        catch (Exception e)
        {
            // One or more errors occurs: 可能是短时间验证码请求次数超出限制
            result.IsSuccess = false;
            result.Message = String.Format("Exception: Login failed: {0} {1}",
                e.Message, e.StackTrace);
            Debug.Log(result.Message);
            return result;
        }
    }

    /// <summary>
    /// 用户登出
    /// </summary>
    public void Logout()
    {
        AVUser.LogOut();
        var user = AVUser.CurrentUser;
        if (user == null)
        {
            Debug.Log("Logout success.");
            loginPanel.SetActive(true);
            userpanel.SetActive(false);
            userButton.SetActive(true);
            var nickNameText = personalInformationObject.GetComponentsInChildren<Text>().Where<Text>(text => text.name == "nickNameText").First();
            nickNameText.text = "";
        }
        else
        {
            Debug.Log("logout failed.");
        }
    }
    #endregion

    #region PersonalInformationInterface
    /// <summary>
    /// 绑定手机号码
    /// </summary>
    public void BindPhoneNumber()
    {
        var phoneNumber = "18826076293";
        AVCloud.RequestSMSCodeAsync(phoneNumber);
        var verificationCode = "171888";

        AVUser.VerifyMobilePhoneAsync(verificationCode).ContinueWith(t =>
        {
            if (t.IsFaulted || t.IsCanceled)
            {
                var error = t.Exception.Message;
                // 展示绑定错误信息
                Debug.Log(t.Exception.InnerExceptions[0].Message);
                Debug.Log(String.Format("Bind phone number failed: {0}", error));
            }
            else
            {
                var currentUser = AVUser.CurrentUser;
                //currentUser["mobilePhoneNumber"] = phoneNumber;
                currentUser.MobilePhoneNumber = phoneNumber;
                currentUser.SaveAsync();
            }
        });
    }

    /// <summary>
    /// 绑定邮箱（自动发邮件点击链接验证）
    /// </summary>
    public void BindEmail()
    {
        var emailAddress = "1093387079@qq.com";
        AVUser.RequestEmailVerifyAsync(emailAddress);

        var currentUser = AVUser.CurrentUser;
        currentUser.Email = emailAddress;
        currentUser.SaveAsync();
    }

    /// <summary>
    /// 获取验证码
    /// </summary>
    /// <param name="target"></param>
    /// <param name="type"></param>
    public void RequestVerificationCode(string target, string type = "PhoneNumber")
    {
        if (type == "PhoneNumber")
        {
            AVUser.RequestMobilePhoneVerifyAsync(target);
        }
        else if (type == "Email")
        {
            AVUser.RequestMobilePhoneVerifyAsync(target);
        }
        else
        {
            Debug.Log("Error verify type.");
        }
    }

    /// <summary>
    /// 重置密码请求
    /// </summary>
    /// <param name="resetType">
    ///     PhoneNumber: 发送短信验证码
    ///     Email: 发送重置密码邮件
    /// </param>
    public void RequestResetPassword(string target, string resetType = "Email")
    {
        Debug.Log("Reset Password Begin: ");
        //var emailAddress = emailInputField.text;
        var emailAddress = target;
        var phoneNumber = target;
        try
        {
            switch (resetType)
            {
                case "PhoneNumber":
                    AVUser.RequestPasswordResetBySmsCode(phoneNumber);
                    break;
                case "Email":
                    AVUser.RequestPasswordResetAsync(emailAddress);
                    break;
                default:
                    Debug.Log("Error type for password reset.");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.StackTrace);
            Debug.Log(e.Message);
            ShowAndroidToastMessage(e.Message);
        }
    }

    /// <summary>
    /// 重置密码：手机号+验证码
    /// </summary>
    /// <param name="password"></param>
    /// <param name="smsCode"></param>
    /// <returns></returns>
    public async Task ResetPasswordBySmsCodeAsync(string password, string smsCode)
    {
        await AVUser.ResetPasswordBySmsCodeAsync(password, smsCode);
    }

    //TODO: 是否上传头像
    public bool uploadAvatar(string imagepath, string sex, string age)
    {
        return false;
        //bool isOK = true;
        //byte[] data = GetPictureData(imagepath);//传入本地的地址获取序列
        //string imageUrl;
        //AVFile file = new AVFile("ASTest.jpg", data, new Dictionary<string, object>()
        //    {
        //        {"author",""}
        //     });
        //file.SaveAsync().ContinueWith(t =>
        //{
        //    Debug.Log(t.IsFaulted);
        //    if (!t.IsFaulted)
        //    {
        //        print("File_id:" + file.ObjectId);
        //    }
        //    else
        //    {
        //        isOK = false;
        //        print("失败打印");
        //        Debug.Log(t.Exception.Message);
        //        Debug.LogException(t.Exception);
        //    }
        //    imageUrl = file.Url.ToString();
        //    print("url=" + imageUrl);
        //    //更新个人信息(sex, age, imageUrl);//在这里调用更新方法
        //});
        //if (isOK)
        //    return true;
        //else return false;
    }

    #endregion

    #region Comments
    /// <summary>
    /// 地点评论
    /// </summary>
    /// <param name="placeName"></param>
    /// <param name="content"></param>
    /// <param name="rating"></param>
    /// <param name="tagList"></param>
    /// <returns></returns>
    public async Task<ResultStatus> SendPlaceComment(string placeName, string content, double rating, List<string> tagList)
    {
        ResultStatus result;

        AVQuery<Place> query = new AVQuery<Place>("Place").WhereEqualTo("placeName", placeName);//获得地点
        Task<IEnumerable<Place>> t = query.FindAsync();//查找
        // 使用toList()可以获取，GetEnumerator()返回null
        Place place = (await t).ToList().First();//开启另一个协程，获取第一个地点的评论
        if (t.IsCanceled || t.IsFaulted)
        {
            result.IsSuccess = false;
            result.Message = String.Format("Send place comment failed: {0} {1}",
                t.Exception.Message, t.Exception.InnerExceptions[0].Message);
            Debug.Log(result.Message);
        }
        else
        {
            bool flag = await CheckUserStateAsync();
            if (!flag) // 用户未登录
            {
                result.IsSuccess = false;
                result.Message = String.Format("Send place comment failed: {0}",
                    "User not login");
                Debug.Log(result.Message);
            }
            else if (place == null) // 地点不存在
            {
                result.IsSuccess = false;
                result.Message = String.Format("Send place comment failed: {0}",
                    "");
                Debug.Log(result.Message);
            }
            else
            {
                var comment = new Comment();
                comment.Content = content;
                comment.Rating = rating;
                comment.User = AVUser.CurrentUser;
                //comment.User = user;
                comment.Place = place;
                comment.TagList = tagList;
                await comment.SaveAsync();
                result.IsSuccess = true;
                result.Message = String.Format("Send place comment success: {0} {1} -- {2}",
                    placeName, comment, AVUser.CurrentUser.Username);
                Debug.Log(result.Message);

                UpdatePlaceAvgRating(place);
            }
        }
        return result;
    }

    /// <summary>
    /// 获取当前地点所有评论
    /// </summary>
    /// <param name="placeName"></param>
    public async Task<List<string>> GetPlaceComment(string placeName)
    {
        List<string> result = new List<string>();

        AVQuery<Place> query = new AVQuery<Place>("Place").WhereEqualTo("placeName", placeName);

        Task<IEnumerable<Place>> t = query.FindAsync();
        // 使用toList()可以获取，GetEnumerator()返回null
        Place place = (await t).ToList().First();
        if (t.IsCanceled || t.IsFaulted)
        {
            string message = String.Format("Get place comment failed: {0} {1}",
                t.Exception.Message, t.Exception.InnerExceptions[0].Message);
            Debug.Log(message);
        }
        else
        {
            bool flag = await CheckUserStateAsync();
            if (!flag) // 用户未登录
            {
                string message = String.Format("Get place comment failed: {0}",
                    "User not login");
                Debug.Log(message);
            }
            else if (place == null) // 地点不存在
            {
                string message = String.Format("Get place comment failed: {0}",
                    "");
                Debug.Log(message);
            }
            else
            {
                List<Comment> comments = (await new AVQuery<Comment>().WhereEqualTo("place", place).FindAsync()).ToList();
                HashSet<string> tagList = new HashSet<string> { };
                Debug.Log(comments.Count());
                foreach (Comment c in comments)
                {
                    result.Add(c.Content);
                }
                string message = String.Format("Get place comment success: {0} -- {1}",
                    placeName, comments.Count());
                Debug.Log(message);
            }
        }
        return result;
    }

    /// <summary>
    /// 获取当前用户所有评论
    /// </summary>
    public async Task<List<string>> GetMyComment()
    {
        List<string> result = new List<string>();

        if (!await CheckUserStateAsync())
        {
            return null;
        }

        var user = AVUser.CurrentUser;
        AVQuery<Comment> query = new AVQuery<Comment>("Comment").WhereEqualTo("user", user);//请求条件，用户名相同
        Debug.Log("getMyPlaceComment");

        Task<IEnumerable<Comment>> t = query.FindAsync();//找到匹配项
        // 使用toList()可以获取，GetEnumerator()返回null
        //Place myPlace = (await t).ToList();
        List<Comment> myComment = (await t).ToList();

        // AVQuery<Place> queryPlace = new AVQuery<Place>("Place");
        //Task<IEnumerable<Place>> p = queryPlace.FindAsync();//找到匹配项
        // 使用toList()可以获取所有地点的评论
        //List<Place> myPlace = (await p).ToList();


        if (t.IsCanceled || t.IsFaulted)
        {
            string message = String.Format("Get my comment failed: {0} {1}",
                t.Exception.Message, t.Exception.InnerExceptions[0].Message);
            Debug.Log(message);
        }
        else
        {
            bool flag = await CheckUserStateAsync();
            if (!flag) // 用户未登录
            {
                string message = String.Format("Get place comment failed: {0}",
                    "User not login");
                Debug.Log(message);
            }
            /*else if (user == null) // 地点不存在
            {
                string message = String.Format("Get place comment failed: {0}",
                    "");
                Debug.Log(message);
            }*/
            else
            {
                //List<Comment> comments = (await new AVQuery<Comment>().WhereEqualTo("place", place).FindAsync()).ToList();//地点相同的comment
                //HashSet<string> tagList = new HashSet<string> { };//创建合集和交集taglist
                Debug.Log(myComment.Count());
                //每一项内容添加进



                /*foreach (Comment c in myComment)
                {
                    foreach (Place p2 in myPlace)
                    {
                        if(p2.PlaceName==c.Place.PlaceName)

                    }
                    {

                        result.Add(p.PlaceName);
                    }

                    result.Add(c.Content);
                }*/


                string message = String.Format("Get place comment success: {0}", myComment.Count());
                Debug.Log(message);
            }
        }
        return result;
    }
    #endregion

    #region Place
    /// <summary>
    /// 更新地点平均评分
    /// </summary>
    /// <param name="place"></param>
    public async void UpdatePlaceAvgRating(Place place)
    {
        List<Comment> comments = (await new AVQuery<Comment>().WhereEqualTo("place", place).FindAsync()).ToList();
        double ratingSum = 0;
        //List<string> tagList = new List<string>{};
        HashSet<string> tagList = new HashSet<string> { };
        Debug.Log(comments.Count());
        foreach (Comment c in comments)
        {
            ratingSum += c.Rating;
            List<System.Object> objList = (List<System.Object>)c["tagList"];
            foreach (var ob in objList)
            {
                tagList.Add((string)ob);
            }
        }
        place.RatingSum = comments.Count();
        place.AvgRating = Math.Round((double)ratingSum / place.RatingSum, 2);
        place.TagList = tagList.ToList();
        place.TagList.Sort();
        try
        {
            await place.SaveAsync();
            string message = String.Format("Update place success: {0} {1} {2}",
                place.PlaceName, place.AvgRating, place.RatingSum);
            Debug.Log(message);
        }
        catch (Exception e)
        {
            string message = String.Format("Update place failed: {0}",
                e.Message);
            Debug.Log(message);
        }
    }

    #endregion

    #region Rank
    /// <summary>
    /// 获取用户打卡排行
    /// </summary>
    /// <returns></returns>
    //public async Task<ResultStatus> GetUserRank()
    //{
    //    ResultStatus result;
    //    AVQuery<AVObject> query = new AVQuery<AVObject>("UserScore")
    //        .WhereNotEqualTo("AllScore", "")
    //        .OrderBy("AllScore")
    //        .OrderByDescending("AllScore");//排序
    //    query.FindAsync().ContinueWith(t =>
    //    {
    //        if (t.IsCanceled)
    //        {
    //            print("失败了");
    //        }
    //        else
    //        {
    //            var players = t.Result;
    //            IEnumerator<AVObject> enumerator = players.GetEnumerator();
    //            int i = 0;
    //            List<string> myList = new List<string>();
    //            var user = new AVObject("UserScore");
    //            while (enumerator.MoveNext())
    //            {
    //                i++;
    //                var golden_player = enumerator.Current;//
    //                Debug.Log("玩家得分" + golden_player["userName"] + " :" + golden_player["AllScore"]);
    //                myList.Add(golden_player["userName"] + " :" + golden_player["AllScore"]);
    //                if (golden_player["userName"].Equals(this.UserName))
    //                {
    //                    user = golden_player;//获取到了自己的分数
    //                }
    //            }
    //            Debug.Log("总玩家数：" + i);
    //            //// enumerator.MoveNext();
    //            //for (i = 1; i <= 20; i++)
    //            //{
    //            //    text = GameObject.Find("主界面/Scroll View(Clone)/Viewport/Content/Text" + i).GetComponent<Text>();
    //            //    text.text = myList[i-1];
    //            //}
    //        }
    //    });
    //    result.Message = "";
    //    result.IsSuccess = false;
    //    return result;
    //}

    #endregion

    #region Share
    /// <summary>
    /// Implemented by ScreenshotSocialShare.cs
    /// </summary>
    #endregion

    #region Public Interface

    /// <summary>
    /// 调用安卓toast message
    /// </summary>
    private void ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }

    /// <summary>
    /// 获取当前Android activity
    /// </summary>
    private static AndroidJavaObject currentActivity
    {
        get
        {
            return new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        }
    }
    #endregion



}
