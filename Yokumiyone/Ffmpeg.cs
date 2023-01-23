using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public void ExportPng(string start, string end)
        {
            var proc = new Process();
            proc.StartInfo.FileName = this.ffmpegPath;
            proc.StartInfo.Arguments = $"-i \"{targetVideoPath}\" -ss {start} -to {end} -r 10 -vcodec png \"./%06d.png\"";
            proc.StartInfo.CreateNoWindow = false;
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
        }
    }
}
