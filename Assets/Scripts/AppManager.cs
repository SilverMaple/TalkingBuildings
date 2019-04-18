using LeanCloud;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vuforia;

public struct ResultStatus
{
    public bool IsSuccess;
    public string Message;
}

public class AppManager : MonoBehaviour
{
    public static GameObject leanCloudObject;
    public static bool InitialState = false;
    public GameObject loginPanel;
    public Button loginButton;
    public Button logoutButton;
    public Button signupButton;
    public InputField accountInputField;
    public InputField passwordInputField;

    private GameObject mGeneratedObject;
    private GUIStyle mFontStyle;
    private GUIStyle mButtonStyle;
    private TextMesh mTextObject;

    // Singleton
    private static AppManager _instance;
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

    void Awake()
    {
        _instance = this;
        // 防止载入新场景时被销毁
        DontDestroyOnLoad(_instance.gameObject);
    }

    // Start is called before the first frame update
    public async void Start()
    {
        if (!InitialState)
        {
            AVObject.RegisterSubclass<Comment>();
            AVObject.RegisterSubclass<Place>();

            Debug.Log("Bind event for buttons");
            Button btn;
            btn = (Button)loginButton.GetComponent<Button>();
            btn.onClick.AddListener(async delegate () { await LoginAsync(); });
            btn = (Button)logoutButton.GetComponent<Button>();
            btn.onClick.AddListener(delegate () { Logout(); });
            btn = (Button)signupButton.GetComponent<Button>();
            btn.onClick.AddListener(async delegate () { await SignupAsync(); });

            //RequestResetPassword

            ResultStatus rs;
        }

        //rs = await SignupAsync("PhoneNumber");
        //rs = await LoginAsync("PhoneNumber");
        //List<string> tagList = new List<string> { "A", "B", "E" };
        //await SendPlaceComment("Place_1", "testCommentFromUser", 3.8, tagList);
        //BindPhoneNumber();
        //BindEmail();
        //Logout();
        //await testAsync();
    }

    /// <summary>
    /// 初始化AR场景
    /// </summary>
    public void InitARScene()
    {
        if (!InitialState)
        {
            Debug.Log("Create target for all image targets");
            var temp = SceneManager.GetSceneByName("AR").GetRootGameObjects();

            var imageTargets = GameObject.FindGameObjectWithTag("ImageTargetList").GetComponentsInChildren<Transform>();
            Text templateText = GameObject.FindGameObjectWithTag("TextTemplate").GetComponentInChildren<Text>();

            // 隐藏ARCamera
            //Camera worldCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponentInChildren<Camera>();
            //worldCamera.GetComponentInChildren<VuforiaBehaviour>().enabled = false;
            //GameObject.FindGameObjectWithTag("MainCamera").SetActive(false);
            // 保留imagetarget
            //DontDestroyOnLoad(imageTargets[0].gameObject);


            for (int i = 1; i < imageTargets.Length; i++)
            {
                // 替换imageTarget的脚本
                Destroy(imageTargets[i].GetComponent<DefaultTrackableEventHandler>());
                imageTargets[i].gameObject.AddComponent<MyTrackableEventHandler>();
                var canvasGameObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                canvasGameObject.transform.SetParent(imageTargets[i].transform);
                canvasGameObject.transform.localScale = new Vector3(.1f, .1f, .1f);
                canvasGameObject.transform.localPosition = new Vector3(0, 0, 0);
                Destroy(canvasGameObject.GetComponent<MeshRenderer>());
                canvasGameObject.AddComponent<Canvas>();
                Canvas canvas = canvasGameObject.GetComponent<Canvas>();

                var tmp = Text.Instantiate<Text>(templateText);
                if (imageTargets[i].name == "ImageTarget (31)")
                {
                    tmp.verticalOverflow = VerticalWrapMode.Overflow;
                    tmp.text = "<color=red>广州市满洲窗可以追溯到清代，当时广州是中国自由的通商港口，彩色玻璃流传进入此地。深受当时的富商、官员的青睐，他们把这些彩色玻璃用在自家的窗户装饰上，既能很好的采光又达到美观大方的效果。后来，清代末年，彩色玻璃大量传入广州。\n</color>" +
                        "<color=green>经过岁月的变迁，新材料的发明，满洲窗在不同的地方也有不同的表现。现在在广州西关的很多地方的装饰都运用到了满洲窗，并运用新的技术加上改良，也有少数采仍然采用传统的生产加工工艺。\n</color>" +
                        "<color=blue>\"满洲窗\"作为广州特定历史时期下的产物，从一个侧面反映了广州乃至我国近代以来传统建筑文化与国外建筑文化逐渐相互融合的一个过程。金碧辉煌、美轮美奂的\"满洲窗\"具有非常高的历史文化和艺术欣赏价值，是岭南非物质文化遗产的重要组成部分。</color>";
                }
                else
                {
                    tmp.text = imageTargets[i].name;
                }
                
                tmp.alignment = TextAnchor.MiddleCenter;
                float red = UnityEngine.Random.Range(0.0f, 1.0f);
                float green = UnityEngine.Random.Range(0.0f, 1.0f);
                float blue = UnityEngine.Random.Range(0.0f, 1.0f);
                tmp.color = new Color(red, green, blue);

                tmp.transform.SetParent(canvasGameObject.transform);
                tmp.transform.localPosition = new Vector3(0, 0, 0);
                tmp.transform.rotation = Quaternion.FromToRotation(new Vector3(0, 1, 0), new Vector3(0, 0, 1));
                tmp.transform.localScale = new Vector3(.1f, .1f, .1f);
                //Debug.Log(imageTargets[i].name);
            }

            Debug.Log("Init button event");
            var commentContext = GameObject.FindGameObjectWithTag("CommentContext");
            var button = commentContext.GetComponentInChildren<Button>();
            var inputField = commentContext.GetComponentInChildren<InputField>();
            button.onClick.AddListener(delegate ()
            {
               this.SendBarrage(inputField.text);
            });

            var snapshotButton = GameObject.FindGameObjectWithTag("SnapshotButton");
            snapshotButton.GetComponent<Button>().onClick.AddListener(delegate ()
            {
                GameObject c = GameObject.FindGameObjectWithTag("ARSceneCanvas");
                this.ShareBtnPress(c);
            });

            //InitialState = true;
            // 每次切换场景都需要初始化
        }
    }

    async Task testAsync()
    {

        //var knife = new GameEquip();
        //var className = knife.ClassName;
        //Debug.Log(className);
        //knife.Name = "xiaodao";
        //knife.AttackValue = 1;
        //Debug.Log("1");
        //await knife.SaveAsync();
        //Debug.Log("2");

        //var query = new AVQuery<GameEquip>();
        //await query.FindAsync();

        //await knife.DeleteAsync();

        //AVQuery<AVObject> query = new AVQuery<AVObject>("GameEquip").WhereEqualTo("name", "短剑");

        //AVQuery<AVObject> query = new AVQuery<AVObject>("GameEquip");
        //AVObject gameEquip = await query.GetAsync("5c98dbbc67f356006212604d");
        //Debug.Log(gameEquip.ObjectId);

        //var equipBag = new AVObject("GameEquipBag");
        //equipBag["scale"] = 20;
        //equipBag["name"] = "装备背包";

        //var equip = new AVObject("GameEquip");
        //equip["name"] = "短剑";
        //equip["attackValue"] = 5;

        //equip["gameEquipBag"] = equipBag;
        //await equip.SaveAsync();

        // find all equipments that are in equip bag
        //var gameEquipBag = AVObject.CreateWithoutData("GameEquipBag", "5c98dbbcfe88c2006f6a512e");
        //var query = new AVQuery<AVObject>("GameEquip");
        //query = query.WhereEqualTo("gameEquipBag", gameEquipBag);
        //var equipments = (await query.FindAsync()).ToList();
        //equipments.ForEach((equip) =>
        //{
        //    var name = equip.Get<string>("name");
        //    Debug.Log(name);
        //});

        /*
         * key-value schema-free
         */
        //var equip = new AVObject("GameEquip");
        //equip["name"] = "sword";
        //equip["attackValue"] = 5;
        //equip["level"] = 1;
        //await equip.SaveAsync();
        //Debug.Log("Equip: " + equip.ObjectId);

        /*
         * Data type
         */
        //int testNumber = 2018;
        //float testFloat = 1.23f;
        //double testDouble = 3.2D;

        //bool testBool = true;
        //string testString = testNumber + " 年度音乐排行";
        //DateTime testDate = DateTime.Today;
        //byte[] testData = System.Text.Encoding.UTF8.GetBytes("短篇小说");

        //List<int> testNumbers = new List<int>();
        //testNumbers.Add(testNumber);
        //testNumbers.Add(123);
        //testNumbers.Add(456);

        //var testDictionary = new Dictionary<string, object>();
        //testDictionary.Add("number", testNumber);
        //testDictionary.Add("string", testString);

        //var testObject = new AVObject("DataTypes");
        //testObject["testInteger"] = testNumber;
        //testObject["testFloat"] = testFloat;
        //testObject["testDouble"] = testDouble;
        //testObject["testBoolean"] = testBool;
        //testObject["testDate"] = testDate;
        //testObject["testData"] = testData;
        //testObject["testArrayList"] = testNumbers;
        //testObject["testDictionary"] = testDictionary;
        //await testObject.SaveAsync();
        //Debug.Log("TestObject: " + testObject.ObjectId);
        //Debug.Log(testObject["testArrayList"]);

        //AVQuery<AVObject> query = new AVQuery<AVObject>("DataTypes");
        //AVObject ttt = await query.GetAsync(testObject.ObjectId);
        //Debug.Log(ttt.Keys);
        //Debug.Log(((List<Int32>)testObject["testArrayList"]).First());
        //Debug.Log(ttt["testArrayList"]);
        //List<System.Object> tt = (List<System.Object>)ttt["testArrayList"];
        //foreach(var sss in tt)
        //{
        //    Debug.Log((Int32)sss);
        //}
        //Debug.Log(testObject["testArrayList"]+" 222 ");

        /*
         * Get attribute
         */
        //var equipBag = new AVObject("GameEquipBag");
        //equipBag["scale"] = 20;
        //equipBag["name"] = "Equip Bag";
        //await equipBag.SaveAsync();

        //int bagScale = equipBag.Get<int>("scale");
        //string bagName = equipBag.Get<string>("name");
        //DateTime? createdAt = equipBag.CreatedAt;
        //DateTime? updatedAt = equipBag.UpdatedAt;

        //Debug.Log(String.Format("Equip Bag: {0}, {1}, {2}, {3}, {4}",
        //    bagScale, bagName, equipBag.ObjectId, createdAt, updatedAt));

        //if (equipBag.TryGetValue("name", out string tryBagName))
        //{
        //    Debug.Log(tryBagName);
        //}

        /*
         * Query object
         */
        //AVQuery<AVObject> query = new AVQuery<AVObject>("GameEquip");
        //AVObject equipment = await query.GetAsync("5c98cea312215f00728b7fc7");
        //Debug.Log("Query: " + equipment);
    }

    // Update is called once per frame
    void Update()
    {

    }

    //void LoadFoucsType()
    //{
    //    //loadd focusType
    //    AVObject theSceneData = AVUser.CurrentUser.Get<AVObject>("SceneData");
    //    Task<AVObject> fetchTask = theSceneData.FetchIfNeededAsync();
    //    Debug.Log(theSceneData.Get<string>("typeName"));
    //}

    #region Login_Logout_Signup
    /// <summary>
    /// 检查用户登录状态
    /// </summary>
    /// <returns></returns>
    public async Task<bool> CheckUserStateAsync()
    {
        var user = AVUser.CurrentUser;
        var IsAuth = await user.IsAuthenticatedAsync();
        if (user == null || user.IsAnonymous || !IsAuth)
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

        var userName = "SilverMaple";
        var password = "password";
        var phoneNumber = "18826076293";
        var emailAddress = "1093387079@qq.com";

        AVUser user = null;
        ResultStatus result;
        try
        {
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
        //var userName = "SilverMaple";
        //var password = "password";
        var phoneNumber = "18826076293";

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
                Debug.Log(result.Message);
                loginPanel.SetActive(false);
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
        }
        else
        {
            Debug.Log("logout failed.");
        }
    }

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

    #endregion

    #region PersonalInformation
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
        var emailAddress = "1093387079@qq.com";
        var phoneNumber = "18826076293";

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

    public void SendBarrage(string comment)
    {
        
    }

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

        AVQuery<Place> query = new AVQuery<Place>("Place").WhereEqualTo("placeName", placeName);

        Task<IEnumerable<Place>> t = query.FindAsync();
        // 使用toList()可以获取，GetEnumerator()返回null
        Place place = (await t).ToList().First();
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

    #endregion

    #region Place
    /// <summary>
    /// 更新地点平均评分
    /// </summary>
    /// <param name="place"></param>
    public async void UpdatePlaceAvgRating(Place place)
    {
        List<Comment> comments = (await new AVQuery<Comment>()
            .WhereEqualTo("place", place).FindAsync()).ToList();
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
    private bool isProcessing = false;
    private bool isFocus = false;
    /// <summary>
    /// Button按钮事件
    /// </summary>
    public void ShareBtnPress(GameObject canvasShareObj)
    {
        if (!isProcessing)
        {
            canvasShareObj.SetActive(false);
            StartCoroutine(ShareScreenshot(canvasShareObj));
        }
    }



    IEnumerator ShareScreenshot(GameObject canvasShareObj)
    {
        isProcessing = true;
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot("screenshot.png", 2);
        string destination = Path.Combine(Application.persistentDataPath, "screenshot.png");
        yield return new WaitForSeconds(0.3f); //WaitForSecondsRealtime(0.3f);
        if (!Application.isEditor)
        {
            AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
            intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
            AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
            AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "file://" + destination);
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"),
            uriObject);
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"),
            "Guess where am I?");
            intentObject.Call<AndroidJavaObject>("setType", "image/jpeg");
            AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser",
            intentObject, "Share your new photo");
            currentActivity.Call("startActivity", chooser);
            yield return new WaitForSeconds(1f); //WaitForSecondsRealtime(1f);
        }
        yield return new WaitUntil(() => isFocus);
        canvasShareObj.SetActive(true);
        isProcessing = false;
    }
    private void OnApplicationFocus(bool focus)
    {
        isFocus = focus;
    }

    #endregion
}
