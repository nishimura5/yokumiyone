﻿using Reactive.Bindings;
using System;
using System.IO;

namespace Yokumiyone
{
    class VideoProp
    {

        public string FilePath { get; }
        public string FileName { get; }
        // FPS 数値
        public float Fps { get; }
        //FPS文字列
        public string FpsFormat { get; }
        public int Width { get; }
        public int Height { get; }
        public int Rotation { get; }
        public string WidthHeight { get; }
        public string DurationStr { get; }
        public TimeSpan Duration { get; }
        public DateTime ModifiedDatetime { get; }
        public string AvgBitrate { get; }

        private readonly ReactivePropertySlim<string> numOfScene = new("");
        public ReactivePropertySlim<string> NumOfScene
        {
            get { return numOfScene; }
        }

        private readonly ReactivePropertySlim<bool> isEnabled = new(true);
        public ReactivePropertySlim<bool> IsEnabled
        {
            get { return isEnabled; }
        }

        private readonly ReactivePropertySlim<string> videoStatus = new("");
        public ReactivePropertySlim<string> VideoStatus
        {
            get { return videoStatus; }
        }

        public VideoProp(string targetFilePath)
        {
            Exiftool exiftool = new(targetFilePath);
            exiftool.GetMetaData();
            string stderr = exiftool.ExtractMetaDataFile();

            FilePath = targetFilePath;
            FileName = Path.GetFileNameWithoutExtension(targetFilePath);
            FpsFormat = exiftool.MetaData.VideoFrameRate.ToString();
            Fps = exiftool.MetaData.VideoFrameRate;
            Width = exiftool.MetaData.ImageWidth;
            Height = exiftool.MetaData.ImageHeight;
            Rotation = exiftool.MetaData.Rotation;
            WidthHeight = $"{Width}x{Height}";
            DurationStr = exiftool.MetaData.Duration;
            NumOfScene.Value = exiftool.CountNumOfScene().ToString();
            isEnabled.Value = true;
            Duration = StringToTimeSpan(DurationStr);
            ModifiedDatetime = File.GetLastWriteTime(targetFilePath);
        }

        public VideoProp(string filePath, string fpsFormat, string durationStr, int width, int height, int rotation, DateTime modified, string scene_cnt)
        {
            FilePath = filePath;
            FileName = Path.GetFileNameWithoutExtension(filePath);
            FpsFormat = fpsFormat;
            Fps = (float)Convert.ToDouble(fpsFormat);
            Width = width;
            Height = height;
            Rotation = rotation;
            WidthHeight = $"{width}x{height}";
            DurationStr = durationStr;
            ModifiedDatetime = modified;
            NumOfScene.Value = scene_cnt.ToString();
            isEnabled.Value = true;
            Duration = StringToTimeSpan(DurationStr);
        }

        public void SetState(string state)
        {
            if (state == "selected")
            {
                videoStatus.Value = "選択中";
                isEnabled.Value = false;
            }
            else if (state == "unselected")
            {
                videoStatus.Value = "";
                isEnabled.Value = true;
            }
            else if (state == "updating")
            {
                videoStatus.Value = "更新中";
                isEnabled.Value = false;
            }
        }

        public void SetNumOfScene(int num)
        {
            numOfScene.Value = num.ToString();
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
    }
}
