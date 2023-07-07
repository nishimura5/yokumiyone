﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Yokumiyone
{
    public class LandmarkCalcJson
    {
        [JsonProperty("video_path")]
        public string? VideoPath { get; set; }
        [JsonProperty("fps")]
        public float Fps { get; set; }
        [JsonProperty("start_time")]
        public string? StartTimeStr { get; set; }
        [JsonProperty("end_time")]
        public string? EndTimeStr { get; set; }
        [JsonProperty("standard_landmarks")]
        public List<List<string>>? StandardLandmarks { get; set; }
        [JsonProperty("landmarks")]
        public List<List<string>>? Landmarks { get; set; }

    }
    public static class JsonExtensions
    {
        public static void SerializeToFile<T>(this T obj, string path)
        {
            try
            {
                using (var sw = new StreamWriter(path, false, System.Text.Encoding.UTF8))
                {
                    // JSON データにシリアライズ
                    var jsonData = JsonConvert.SerializeObject(obj, Formatting.Indented);

                    // JSON データをファイルに書き込み
                    sw.Write(jsonData);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"failed:{ex.Message}");
            }
        }
    }

}