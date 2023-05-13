using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;


namespace Common
{
    public static class YokumiyoneFolder
    {
        public static void CreateLocalFolder()
        {
            string localFolderPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Yokumiyone");
            if (Directory.Exists(localFolderPath) == false)
            {
                Directory.CreateDirectory(localFolderPath);
            }
        }

    }
    public static class CopyHelper
    {
        /// <summary>
        /// DeepCopy
        /// </summary>
        public static T DeepCopy<T>(this T src)
        {
            ReadOnlySpan<byte> b = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes<T>(src);
            return System.Text.Json.JsonSerializer.Deserialize<T>(b);
        }
    }

    public class YokumiyoneDb
    {
        public string dbPath
        {
            get
            {
                YokumiyoneFolder.CreateLocalFolder();
                return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Yokumiyone", "yokumiyone.sqlite3");
            }
        }
        public void ExecNonQuery(string stmt, List<SQLiteParameter> sql_params)
        {
            try
            {
                // 接続先を指定
                using (var conn = new SQLiteConnection($"Data Source={dbPath}"))
                using (var command = conn.CreateCommand())
                {
                    conn.Open();
                    command.CommandText = stmt;
                    foreach (SQLiteParameter param in sql_params)
                    {
                        command.Parameters.Add(param);
                    }
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.GetType().Name);
            }
        }
        public void CreateDb()
        {
            if (System.IO.File.Exists(dbPath) == false)
            {
                List<SQLiteParameter> sql_params = new List<SQLiteParameter>();
                ExecNonQuery("CREATE TABLE \"scene_path\" (\"scene\" TEXT, \"path\" TEXT, \"status\" INTEGER DEFAULT 1, PRIMARY KEY(\"scene\",\"path\"))", sql_params);
                ExecNonQuery("CREATE TABLE \"video_prop\" (\"path\" TEXT, \"fps\" TEXT, \"duration\" TEXT, \"width\" INTEGER, \"height\"\tINTEGER, \"rotation\"\tINTEGER, \"modified\" TEXT,  \"scene_cnt\" TEXT, PRIMARY KEY(\"path\"))", sql_params);
                ExecNonQuery("CREATE TABLE \"preferences\" (\"key\" TEXT, \"value\" TEXT, PRIMARY KEY(\"key\"))", sql_params);
            }
        }
    }

    public class YokumiyoneXmp
    {
        public string targetVideoPath = "";
        private string xmpFolderPath
        {
            get
            {
                return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Yokumiyone", "xmp"); ;
            }
        }
        private void CreateXmpFolder()
        {
            if (Directory.Exists(xmpFolderPath) == false)
            {
                Directory.CreateDirectory(xmpFolderPath);
            }
        }
        public string xmpPath
        {
            get
            {
                if (targetVideoPath == "")
                {
                    return "";
                }
                CreateXmpFolder();
                return System.IO.Path.Combine(xmpFolderPath, System.IO.Path.GetFileNameWithoutExtension(targetVideoPath)) + ".xmp";
            }
        }
        public void CreateBlankXmp()
        {
            string[] lines =
            {
                "<?xpacket begin='﻿' id='W5M0MpCehiHzreSzNTczkc9d'?>",
                "<x:xmpmeta xmlns:x='adobe:ns:meta/' x:xmptk='Image::ExifTool 12.52'>",
                "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'>",
                "</rdf:RDF>",
                "</x:xmpmeta>",
                "<?xpacket end='w'?>"
            };
            File.WriteAllLines(xmpPath, lines);
        }
    }
}
