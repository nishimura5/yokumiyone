using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using System.Windows;


namespace Common
{
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
                ExecNonQuery("CREATE TABLE \"scene_path\" (\r\n\t\"scene\"\tTEXT,\r\n\t\"path\"\tTEXT,\r\n\t\"status\"\tINTEGER DEFAULT 1,\r\n\tPRIMARY KEY(\"scene\",\"path\")\r\n)", sql_params);
                ExecNonQuery("CREATE TABLE \"video_prop\" (\r\n\t\"path\"\tTEXT,\r\n\t\"fps\"\tTEXT,\r\n\t\"duration\"\tTEXT,\r\n\t\"width\"\tINTEGER,\r\n\t\"height\"\tINTEGER,\r\n\t\"scene_cnt\"\tTEXT,\r\n\tPRIMARY KEY(\"path\")\r\n)", sql_params);
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
