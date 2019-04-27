
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TextItem : MonoBehaviour
{
    /// <summary>
    /// text控件显示文字
    /// </summary>
    public string text = "";

    /// <summary>
    /// 当前脚本所在的text控件
    /// </summary>
    private Text currentText;

    /// <summary>
    /// 弹幕移动的速度
    /// </summary>
    public float speed;
    public float scaleFactor = .03f;

    void Start()
    {
        //初始化
        //currentText = GetComponent<Text>();
        currentText = GetComponentInChildren<Text>();

        //设置显示字体以及随机颜色
        //currentText.text = text;
        //currentText.color = Random.ColorHSV();

        //获取屏幕范围内的y坐标随机，这里没做屏幕适配，free aspect举列
        //float y = Random.Range(-200f, 220f);
        //transform.localPosition = new Vector3(550f, y, 0);
    }



    void Update()
    {
        if (speed != 0)
        {
            Transform t = this.gameObject.transform;
            float x =t.localPosition.x + speed * Time.deltaTime;
            t.localPosition = new Vector3(x, t.localPosition.y, t.localPosition.z);

            //出屏幕重置位置
            if (t.localPosition.x > Screen.width / 2 * scaleFactor)
            {
                t.localPosition = new Vector3(-Screen.width / 2 * scaleFactor, t.localPosition.y, t.localPosition.z);
                //Destroy(gameObject);
            }
        }
    }
}