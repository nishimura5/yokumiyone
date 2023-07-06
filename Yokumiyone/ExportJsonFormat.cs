using Newtonsoft.Json;
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
        public string? videoPath { get; set; }
        [JsonProperty("fps")]
        public float fps { get; set; }
        [JsonProperty("start_time")]
        public string? startTimeStr { get; set; }
        [JsonProperty("end_time")]
        public string? endTimeStr { get; set; }
        [JsonProperty("landmarks")]
        public List<List<string>> landmarks { get; set; }

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
