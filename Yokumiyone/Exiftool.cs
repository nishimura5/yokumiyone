using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Xml.Linq;

namespace Yokumiyone
{
    class Exiftool
    {
        private YokumiyoneXmp xmp = new();
        private string ExiftoolPath
        {
            get
            {
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "3rd", "exiftool.exe");
            }
        }

        // XMPファイル内のSceneとBagはこの中にある
        private static readonly XNamespace Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        private static readonly XNamespace Iptc4xmpCore = "http://iptc.org/std/Iptc4xmpCore/1.0/xmlns/";

        public List<SceneProp> SceneList = new();
        public Meta MetaData = new();

        public Exiftool(string targetVideoPath)
        {
            xmp.targetVideoPath = targetVideoPath;
        }

        public void GetMetaData()
        {
            var proc = new Process();
            proc.StartInfo.FileName = this.ExiftoolPath;
            proc.StartInfo.Arguments = $"\"{xmp.targetVideoPath}\" -charset filename=\"\" -s -Duration -ImageHeight -ImageWidth -Rotation -CompressorId -VideoFrameRate -AvgBitrate -j -b";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.Start();
            string rawResults = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            proc.Close();
            proc.Dispose();
            if (rawResults == "")
            {
                return;
            }
            Meta[]? metadata = JsonSerializer.Deserialize<Meta[]>(rawResults);
            if (metadata != null && metadata.Length == 1)
            {
                MetaData = metadata[0];
            }
        }

        public class Meta
        {
            private string? duration;
            private string? avgBitrate;
            public string? SourceFile { get; set; }
            public string Duration
            {
                get { return duration ?? "EE:EE:EE.EEE"; }
                set { duration = value; }
            }
            public int ImageHeight { get; set; }
            public int ImageWidth { get; set; }
            public int Rotation { get; set; }
            public float VideoFrameRate { get; set; }
            public string? CompressorID { get; set; }  // Exiftoolの戻りの都合でIdではなくIDとしている。
            public string AvgBitrate
            {
                get { return avgBitrate ?? "EEE"; }
                set { avgBitrate = value; }
            }
        }
        // 動画ファイルからメタデータを抽出
        public string ExtractMetaDataFile()
        {
            var proc = new Process();
            proc.StartInfo.FileName = this.ExiftoolPath;
            proc.StartInfo.Arguments = $"-tagsfromfile \"{xmp.targetVideoPath}\" \"{xmp.XmpPath}\" -charset filename=\"\" -overwrite_original_in_place -all:all -xmp:all -exif:all -composite:all -quicktime:all -iptc:all -gps:all -ee -api largefilesupport=1";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            proc.Start();
            string stderr = proc.StandardError.ReadToEnd();
            string[] stderrs = stderr.Split("\r\n");
            foreach (string msg in stderrs)
            {
                if (msg.StartsWith("Error: Nothing to write") == true)
                {
                    xmp.CreateBlankXmp();
                }
                else if (msg.StartsWith("Warning") == true)
                {
                    MessageBox.Show(msg, this.GetType().Name);
                }
            }

            proc.WaitForExit();
            proc.Close();
            proc.Dispose();
            return stderr;
        }

        public void LoadMetaDataFile()
        {
            if (File.Exists(xmp.XmpPath) == false)
            {
                xmp.CreateBlankXmp();
            }
            // AppdataLocalに保存されているxmpファイルを読み込んでParse
            XElement xmpElem = XElement.Load(xmp.XmpPath);
            IEnumerable<XElement> stream = from el in xmpElem.Descendants(Iptc4xmpCore + "Scene").Elements(Rdf + "Bag").Elements(Rdf + "li") select el;
            foreach (XElement el in stream)
            {
                var ScenePropCsv = el.Value.ToString().Split(",");
                if (ScenePropCsv.Length < 3)
                {
                    continue;
                }
                SceneList.Add(new SceneProp(ScenePropCsv[0], ScenePropCsv[1], ScenePropCsv[2]));
            }
        }

        public int CountNumOfScene()
        {
            if (File.Exists(xmp.XmpPath) == false)
            {
                return 0;
            }
            int cnt = 0;
            XElement xmpElem = XElement.Load(xmp.XmpPath);
            cnt = xmpElem.Descendants(Iptc4xmpCore + "Scene").Count();
            if (cnt > 0)
            {
                IEnumerable<XElement> elements = from el in xmpElem.Descendants(Iptc4xmpCore + "Scene").Elements(Rdf + "Bag").Elements(Rdf + "li") select el;
                cnt = elements.Count();
            }
            return cnt;
        }

        public void UpdateMetaDataFile()
        {
            XElement xmpElem = XElement.Load(xmp.XmpPath);

            // Sceneを探す
            int cnt = xmpElem.Descendants(Iptc4xmpCore + "Scene").Count();
            if (cnt > 0)
            {
                // Sceneがあるなら中身を消して上書き
                IEnumerable<XElement> elements = from el in xmpElem.Descendants(Iptc4xmpCore + "Scene").Elements(Rdf + "Bag").Elements(Rdf + "li") select el;
                elements.Remove();
            }
            else
            {
                // SceneがないならDescription/Scene/Bagを作成
                xmpElem.Descendants(Rdf + "RDF").First().AddFirst(
                    new XElement(Rdf + "Description",
                    new XAttribute(XNamespace.Xmlns + "Iptc4xmpCore", "http://iptc.org/std/Iptc4xmpCore/1.0/xmlns/"),
                    new XElement(Iptc4xmpCore + "Scene",
                    new XElement(Rdf + "Bag"))));
            }

            foreach (SceneProp scene in SceneList)
            {
                xmpElem.Descendants(Iptc4xmpCore + "Scene").Elements(Rdf + "Bag").First().Add(new XElement(Rdf + "li", scene.ScenePropCsv));
            }

            xmpElem.Save(xmp.XmpPath);
        }

        public void InsertMetaData()
        {
            var proc = new Process();
            proc.StartInfo.FileName = this.ExiftoolPath;
            proc.StartInfo.Arguments = $"-tagsfromfile \"{xmp.XmpPath}\" \"{xmp.targetVideoPath}\" -overwrite_original_in_place -api largefilesupport=1";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.Start();
            proc.WaitForExit();
            proc.Close();
            proc.Dispose();
        }
    }
}
