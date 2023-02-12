using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Yokumiyone.Exiftool;

namespace Yokumiyone
{
    internal class Ffmpeg
    {
        private string ffmpegPath
        {
            get
            {
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "3rd", "ffmpeg.exe");
            }
        }

        private string targetVideoPath;

        public Ffmpeg(string targetVideoPath)
        {
            this.targetVideoPath = targetVideoPath;
        }

        public void ExportPng(string start, string duration, string frameRate, string dstDir)
        {
            string fileName = Path.GetFileNameWithoutExtension(targetVideoPath);
            string dstBaseDir = Path.Combine(dstDir, fileName);
            if (Directory.Exists(dstBaseDir) == false)
            {
                Directory.CreateDirectory(dstBaseDir);
            }
            string dstPath = Path.Combine(dstBaseDir, "%06d.png");

            var proc = new Process();
            proc.StartInfo.FileName = this.ffmpegPath;
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
