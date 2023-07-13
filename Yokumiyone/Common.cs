using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Windows;


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
        public string DbPath
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
                using (var conn = new SQLiteConnection($"Data Source={DbPath}"))
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
            if (System.IO.File.Exists(DbPath) == false)
            {
                List<SQLiteParameter> sql_params = new();
                ExecNonQuery("CREATE TABLE \"scene_path\" (\"scene\" TEXT, \"path\" TEXT, \"status\" INTEGER DEFAULT 1, PRIMARY KEY(\"scene\",\"path\"))", sql_params);
                ExecNonQuery("CREATE TABLE \"video_prop\" (\"path\" TEXT, \"fps\" TEXT, \"duration\" TEXT, \"width\" INTEGER, \"height\"\tINTEGER, \"rotation\"\tINTEGER, \"compressor_id\" TEXT, \"modified\" TEXT,  \"scene_cnt\" TEXT, PRIMARY KEY(\"path\"))", sql_params);
                ExecNonQuery("CREATE TABLE \"preferences\" (\"key\" TEXT, \"value\" TEXT, PRIMARY KEY(\"key\"))", sql_params);
            }
        }
    }

    public class YokumiyoneXmp
    {
        public string targetVideoPath = "";
        private string XmpFolderPath
        {
            get
            {
                return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Yokumiyone", "xmp"); ;
            }
        }
        private void CreateXmpFolder()
        {
            if (Directory.Exists(XmpFolderPath) == false)
            {
                Directory.CreateDirectory(XmpFolderPath);
            }
        }
        public string XmpPath
        {
            get
            {
                if (targetVideoPath == "")
                {
                    return "";
                }
                CreateXmpFolder();
                return System.IO.Path.Combine(XmpFolderPath, System.IO.Path.GetFileNameWithoutExtension(targetVideoPath)) + ".xmp";
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
            File.WriteAllLines(XmpPath, lines);
        }
    }
}
