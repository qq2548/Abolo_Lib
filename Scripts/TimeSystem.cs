using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    public class TimeSystem : AboloSingleton<TimeSystem>
    {
        public delegate void MyDele();
        public MyDele PerDay;
        public MyDele PerMonth;
        public MyDele PerYear;


        private int hour = 0;//0 ~ 23
        /// <summary>
        /// 小时
        /// </summary>
        public int Hour
        {
            get => hour;
            set => hour = value;//> 23? 0 : value;
        }
        private int date = 1;//1 ~ 30
        /// <summary>
        /// 日期
        /// </summary>
        public int Date
        {
            get => date;
            set => date = value;//> 30? 1: value;
        }
        private int month = 1;//1 ~ 12
        /// <summary>
        /// 月份
        /// </summary>
        public int Month
        {
            get => month;
            set => month = value;// > 12 ? 1 : value;
        }
        private int year = 1;
        /// <summary>
        ///年份
        /// </summary>
        public int Year
        {
            get => year;
            set => year = value;
        }
        private int dayTime = 0;//0 ~ 1 ，0夜晚 1白天
        /// <summary>
        /// 昼夜
        /// </summary>
        public int DayTime
        {
            get => dayTime;
            set => dayTime = value;
        }

        private int season = 0;//0 ~ 3，0春  1夏  2秋  3冬
        /// <summary>
        /// 四季
        /// </summary>
        public int Season
        {
            get => season;
            set => season = value;//> 3 ? 0 : value;
        }
        /// <summary>
        /// 游戏世界时间流逝速度
        /// </summary>
        public static int TimeScale = 10;

        private float _time = 0f;
        private float _timeUnit;
        public float TimeUnit
        {
            get => _timeUnit ;
            set => _timeUnit = value / TimeScale;
        }
        protected  void Awake()
        {

        }

        void Start()
        {
        
        }

        /// <summary>
        /// 暂停世界时间
        /// </summary>
        public void PauseGameTime()
        {
            TimeUnit = 0f;
        }
        /// <summary>
        /// 恢复世界时间
        /// </summary>
        public void ResumeGameTime()
        {
            TimeUnit = 1f;
        }

        public void NormalGameSpeed()
        {
            TimeScale = 1;
        }

        public void DoubleGameSpeed()
        {
            TimeScale = 2;
        }

        public void ThripleGameSpeed()
        {
            TimeScale = 3;
        }

        // Update is called once per frame
        void Update()
        {
            if (TimeUnit > 0)
            {
                _time += Time.deltaTime;
                if (_time > TimeUnit)
                {
                    _time = 0f;
                    Hour++;
                    if (Hour > 23)
                    {
                        Hour = 0;
                        Date++;
                        if (Date > 30)
                        {
                            Date = 1;
                            Month++;
                            if (Month > 12)
                            {
                                Month = 1;
                                Year++;
                                PerYear?.Invoke();
                            }
                            PerMonth?.Invoke();
                        }
                        PerDay?.Invoke();
                    }
                }
            }
        }
    }
}
