using Common;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System;
using System.Linq;

namespace Yokumiyone.tables
{
    internal class LandareaTable
    {
        private YokumiyoneDb db = new YokumiyoneDb();
        private JsonSerializerOptions op = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            };
        public string Name = "";
        public string LandmarkType = "";
        public Dictionary<string, List<string>> StandardLandarea = new();
        public Dictionary<string, List<string>> TargetLandarea = new();
        public string DbPath { get { return db.DbPath; } }

        public LandareaTable()
        {
            db.CreateDb();
        }

        public void SetLandarea(string name)
        {            
            string standardLandareaJson = JsonSerializer.Serialize(StandardLandarea, op);
            string targetLandareaJson = JsonSerializer.Serialize(TargetLandarea, op);

            List<SQLiteParameter> sql_params = new List<SQLiteParameter>() {
                new SQLiteParameter("@name", name),
                new SQLiteParameter("@landmark_type", LandmarkType),
                new SQLiteParameter("@standard_landarea_json", standardLandareaJson),
                new SQLiteParameter("@target_landarea_json", targetLandareaJson),
            };
            db.ExecNonQuery($"INSERT INTO landarea (name, landmark_type, standard_landarea_json, target_landarea_json) VALUES (@name, @landmark_type, @standard_landarea_json, @target_landarea_json) ON CONFLICT (name) DO UPDATE SET name=@name, landmark_type = @landmark_type, standard_landarea_json=@standard_landarea_json, target_landarea_json=@target_landarea_json;", sql_params);
        }

        public void GetLandarea(string name)
        {
            List<SQLiteParameter> sql_params = new List<SQLiteParameter>() {
                new SQLiteParameter("@name", name),
            };
            ExecReader($"SELECT * FROM landarea WHERE name == @name", sql_params);
        }

        public List<string> GetNames()
        {
            List<string> names = ExecReader($"SELECT name FROM landarea", "name");
            return names;
        }
        public List<string> GetLandmarkTypes()
        {
            List<string> landmarkTypes = ExecReader($"SELECT landmark_type FROM landarea", "landmark_type");
            IEnumerable<string> uniqueList = landmarkTypes.Distinct();
            return uniqueList.ToList();
        }
        public void Delete(string targetName)
        {
            List<SQLiteParameter> sql_params = new List<SQLiteParameter>() {
                new SQLiteParameter("@name", targetName),
            };
            db.ExecNonQuery($"DELETE FROM landarea WHERE name == @name", sql_params);
        }

        private void ExecReader(string stmt, List<SQLiteParameter> sql_params)
        {
            string standardLandareaJson = "";
            string targetLandareaJson = "";

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
                            Name = (string)reader["name"];
                            LandmarkType = (string)reader["landmark_type"];
                            standardLandareaJson = (string)reader["standard_landarea_json"];
                            targetLandareaJson = (string)reader["target_landarea_json"];
                        }
                    }
                }
                StandardLandarea = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(standardLandareaJson);
                TargetLandarea = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(targetLandareaJson);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, GetType().Name);
            }
        }
        private List<string> ExecReader(string stmt, string col)
        {
            List<string> retList = new();

            try
            {
                // 接続先を指定
                using (var conn = new SQLiteConnection($"Data Source={db.DbPath}"))
                using (var command = conn.CreateCommand())
                {
                    conn.Open();
                    command.CommandText = stmt;
                    using (var reader = command.ExecuteReader())
                    {
                        // 1行ずつデータを取得、最後に取得した値を返す
                        while (reader.Read())
                        {
                            retList.Add((string)reader[col]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, GetType().Name);
            }
            return retList;
        }
    }
}
