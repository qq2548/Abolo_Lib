using UnityEngine;
using UnityEngine.UI;

namespace AboloLib
{
    public class ShowFPS_OnGUI : MonoBehaviour
    {

        public float fpsMeasuringDelta = 2.0f;

        private float timePassed;
        private int m_FrameCount = 0;
        private float m_FPS = 0.0f;

        //显示FPS的数字
        public Text text;

        private void Start()
        {
            Application.targetFrameRate = 60;
            timePassed = 0.0f;
        }

        private void Update()
        {
            m_FrameCount = m_FrameCount + 1;
            timePassed = timePassed + Time.deltaTime;

            if (timePassed > fpsMeasuringDelta)
            {
                m_FPS = m_FrameCount / timePassed;

                timePassed = 0.0f;
                m_FrameCount = 0;
                text.text = Mathf.FloorToInt(m_FPS).ToString();
            }
        }

        //private void OnGUI() //这个方法暂时不用
        //{
        //    GUIStyle bb = new GUIStyle();
        //    bb.normal.background = null;    //这是设置背景填充的
        //    bb.normal.textColor = new Color(1.0f, 0.5f, 0.0f);   //设置字体颜色的
        //    bb.fontSize = 40;       //当然，这是字体大小

        //    //居中显示FPS
        //    GUI.Label(new Rect((Screen.width / 2) - 40, 250, 200, 200), "FPS: " + Mathf.FloorToInt(m_FPS), bb);
        //}
    }
}