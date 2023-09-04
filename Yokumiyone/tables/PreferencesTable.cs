using Common;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Documents;

namespace Yokumiyone.tables
{
    internal class PreferencesTable
    {
        private YokumiyoneDb db = new YokumiyoneDb();
        public PreferencesTable()
        {
            db.CreateDb();
        }

        public Dictionary<string,bool> GetBoolPreferences()
        {
            List<string> keys = new(){
                "enableSceneExport", "enableLandpackDialog"
            };
            List<SQLiteParameter> sql_params = new List<SQLiteParameter>() {
                new SQLiteParameter("@keyName1", keys[0]),
                new SQLiteParameter("@keyName2", keys[1]),
            };
            Dictionary<string, bool> retDict = new();
            var returnVal = ExecReader($"SELECT * FROM preferences WHERE key == @keyName1 OR key == @keyName2", sql_params);
            foreach (string key in keys)
            {
                if (returnVal.ContainsKey(key) == false)
                {
                    retDict.Add(key, false);
                    continue;
                }
                if (returnVal[key] == "True")
                {
                    retDict.Add(key, true);
                }else if (returnVal[key] == "False")
                {
                    retDict.Add(key, false);
                }
                else
                {
                    retDict.Add(key, false);
                }
            }
            return retDict;
        }

        public Dictionary<string, string> GetStringPreferences()
        {
            List<string> keys = new(){
                "ffmpegPath"
            };
            List<SQLiteParameter> sql_params = new List<SQLiteParameter>() {
                new SQLiteParameter("@keyName1", keys[0]),
            };
            Dictionary<string, string> retDict = new();
            var returnVal = ExecReader($"SELECT * FROM preferences WHERE key == @keyName1", sql_params);
            foreach (string key in keys)
            {
                if (returnVal.ContainsKey(key) == false)
                {
                    retDict.Add(key, "");
                }
                else
                {
                    retDict.Add(key, returnVal[key]);
                }                
            }
            return retDict;
        }

        public void SetEnableSceneExport(bool? enable)
        {
            Insert("enableSceneExport", enable.ToString());
        }

        public void SetEnableLandpackDialog(bool enable)
        {
            Insert("enableLandpackDialog", enable.ToString());
        }
        public void SetFfmpegPath(string ffmpegPath)
        {
            Insert("ffmpegPath", ffmpegPath);
        }

        private void Insert(string key, string value)
        {
            List<SQLiteParameter> sql_params = new List<SQLiteParameter>() {
                new SQLiteParameter("@key", key),
                new SQLiteParameter("@val", value),
            };
            db.ExecNonQuery($"INSERT INTO preferences (key, value) VALUES (@key, @val) ON CONFLICT (key) DO UPDATE SET value = @val;", sql_params);
        }

        private Dictionary<string,string> ExecReader(string stmt, List<SQLiteParameter> sql_params)
        {
            Dictionary<string,string> ret = new();
            try
            {
                // 接続先を指定
                using (var conn = new SQLiteConnection($"Data Source={db.DbPath}"))
                using (var command = conn.CreateCommand())
                {
                    conn.Open();
                    command.CommandText = stmt;
                    foreach (SQLiteParameter param in sql_params)
                    {
                        command.Parameters.Add(param);
                    }
                    using (var reader = command.ExecuteReader())
                    {
                        // 1行ずつデータを取得、最後に取得した値を返す
                        while (reader.Read())
                        {
                            ret.Add((string)reader["key"], (string)reader["value"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, GetType().Name);
            }
            return ret;
        }
    }
}
