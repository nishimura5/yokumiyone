﻿using System;

namespace Yokumiyone
{
    [Serializable]
    public class SceneProp
    {
        private TimeSpan startTime;
        private TimeSpan endTime;
        private string? title;
        public TimeSpan StartTime { get { return startTime; } }
        public TimeSpan EndTime { get { return endTime; } }
        public string StartTimeStr
        {
            get
            {
                return TimeSpanToString(startTime);
            }
            set
            {
                startTime = StringToTimeSpan(value);
            }
        }
        public string EndTimeStr
        {
            get
            {
                return TimeSpanToString(endTime);
            }
            set
            {
                endTime = StringToTimeSpan(value);
            }
        }
        public string Title
        {
            get
            {
                return title ?? "ERROR";
            }
            set
            {
                title = value ?? "ERROR";
            }
        }

        public string TitleView
        {
            get
            {
                string[] titles = Title.Split("|");
                return string.Join(" | ", titles);
            }
        }

        public string ScenePropCsv
        {
            get { return $"{StartTimeStr},{EndTimeStr},{Title}"; }
            private set { ScenePropCsv = value; }
        }
        public string SceneDuration
        {
            get { return CalcDuration(); }
            private set { SceneDuration = value; }
        }

        public SceneProp() { }

        public SceneProp(string start, string end, string title)
        {
            StartTimeStr = start;
            EndTimeStr = end;
            Title = title;
        }

        private string CalcDuration()
        {
            TimeSpan diff = this.endTime - this.startTime;
            // H:mm:ss の文字列を戻す
            if (diff.TotalSeconds < 0)
            {
                return string.Concat("-", diff.ToString()[1..9]);
            }
            return diff.ToString()[..8];
        }

        public TimeSpan CalcMidTime()
        {
            TimeSpan diff = (this.endTime + this.startTime) / 2;
            return diff;
        }

        private TimeSpan StringToTimeSpan(string hmmss)
        {
            TimeSpan dst;
            bool res = TimeSpan.TryParseExact(hmmss, @"h\:mm\:ss\.fff", System.Globalization.CultureInfo.InvariantCulture, out dst);
            if (res == false)
            {
                res = TimeSpan.TryParseExact(hmmss, @"h\:mm\:ss", System.Globalization.CultureInfo.InvariantCulture, out dst);
                if (res == false)
                {
                    dst = TimeSpan.Parse("0:0");
                }
            }
            return dst;
        }

        private string TimeSpanToString(TimeSpan timeSpan)
        {
            string dst = timeSpan.ToString();
            if (dst.Length < 12)
            {
                dst = dst[..8];
            }
            else
            {
                dst = dst[..12];
            }
            return dst;
        }
    }
}
