using System;
using System.Diagnostics;
using System.IO;
using Yokumiyone.tables;

namespace Yokumiyone
{
    internal class Ffmpeg
    {
        private string ffmpegPath;
        private readonly string targetVideoPath;
        private readonly string sceneTitle;
        private readonly string start;
        private readonly string duration;

        public Ffmpeg(string targetVideoPath, string sceneTitle, string start, string duration)
        {
            PreferencesTable preferencesTable = new();
            var prefString = preferencesTable.GetStringPreferences();
            ffmpegPath = prefString["ffmpegPath"];

            this.targetVideoPath = targetVideoPath;
            this.sceneTitle = sceneTitle;
            this.start = start;
            this.duration = duration;
        }

        public void ExportPng(string frameRate, string? dstDir)
        {
            string fileName = Path.GetFileNameWithoutExtension(targetVideoPath);
            string dstBaseDir = Path.Combine(dstDir, fileName + "_" + sceneTitle);
            if (Directory.Exists(dstBaseDir) == false)
            {
                Directory.CreateDirectory(dstBaseDir);
            }
            string dstPath = Path.Combine(dstBaseDir, "%06d.png");

            var proc = new Process();
            proc.StartInfo.FileName = ffmpegPath;
            proc.StartInfo.Arguments = $"-ss {start} -i \"{targetVideoPath}\" -t {duration} -r {frameRate} -vcodec png \"{dstPath}\"";
            proc.StartInfo.CreateNoWindow = false;
            proc.StartInfo.RedirectStandardOutput = true;
            //            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            proc.Start();
            string rawResults = proc.StandardOutput.ReadToEnd();
            //            string stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();
            proc.Close();
            proc.Dispose();
            if (rawResults == "")
            {
                return;
            }
        }
    }
}
