using Common;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System;
using System.Linq;
using System.Xml.Linq;

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

        public void SetSampleLandareas()
        {
            // sample_mediapipe_pose
            string standardLandareaJson = "{\r\n\"肩幅\":[\r\n      \"11\",\r\n      \"12\"\r\n    ]\r\n}";
            string targetLandareaJson = "{\r\n    \"L腕\":[\"11\", \"13\", \"15\"],\r\n    \"R腕\":[\"12\", \"14\", \"16\"],\r\n    \"L手\":[\"15\", \"17\", \"19\", \"21\"],\r\n    \"R手\":[\"16\", \"18\", \"20\", \"22\"],\r\n    \"L脚\":[\"23\", \"25\", \"27\"],\r\n    \"R脚\":[\"24\", \"26\", \"28\"],\r\n    \"L足\":[\"27\", \"29\", \"31\"],\r\n    \"R足\":[\"28\", \"30\", \"32\"]\r\n}";
            List<SQLiteParameter> sql_params = new List<SQLiteParameter>() {
                new SQLiteParameter("@name", "sample_mediapipe_pose"),
                new SQLiteParameter("@landmark_type", "mediapipe_pose_landmarker"),
                new SQLiteParameter("@standard_landarea_json", standardLandareaJson),
                new SQLiteParameter("@target_landarea_json", targetLandareaJson),
            };
            db.ExecNonQuery($"INSERT INTO landarea (name, landmark_type, standard_landarea_json, target_landarea_json) VALUES (@name, @landmark_type, @standard_landarea_json, @target_landarea_json) ON CONFLICT (name) DO UPDATE SET name=@name, landmark_type = @landmark_type, standard_landarea_json=@standard_landarea_json, target_landarea_json=@target_landarea_json;", sql_params);

            // smple_mediapipe_face
            standardLandareaJson = "{\r\n  \"目鼻\": [\r\n    \"130\",\r\n    \"4\",\r\n    \"359\"\r\n  ]\r\n}";
            targetLandareaJson = "{\r\n  \"B唇\": [\r\n    \"14\",\r\n    \"317\",\r\n    \"402\",\r\n    \"318\",\r\n    \"324\",\r\n    \"308\",\r\n    \"292\",\r\n    \"306\",\r\n    \"375\",\r\n    \"321\",\r\n    \"405\",\r\n    \"314\",\r\n    \"17\",\r\n    \"84\",\r\n    \"181\",\r\n    \"91\",\r\n    \"146\",\r\n    \"76\",\r\n    \"62\",\r\n    \"78\",\r\n    \"95\",\r\n    \"88\",\r\n    \"178\",\r\n    \"87\"\r\n  ],\r\n  \"Lほうれい線\": [\r\n    \"358\",\r\n    \"327\",\r\n    \"391\",\r\n    \"322\",\r\n    \"410\",\r\n    \"287\",\r\n    \"434\",\r\n    \"427\",\r\n    \"425\",\r\n    \"266\"\r\n  ],\r\n  \"L下瞼1\": [\r\n    \"263\",\r\n    \"249\",\r\n    \"390\",\r\n    \"373\",\r\n    \"374\",\r\n    \"380\",\r\n    \"381\",\r\n    \"382\",\r\n    \"362\",\r\n    \"463\",\r\n    \"341\",\r\n    \"256\",\r\n    \"252\",\r\n    \"253\",\r\n    \"254\",\r\n    \"339\",\r\n    \"255\",\r\n    \"359\"\r\n  ],\r\n  \"L下瞼2\": [\r\n    \"265\",\r\n    \"340\",\r\n    \"346\",\r\n    \"347\",\r\n    \"348\",\r\n    \"349\",\r\n    \"350\",\r\n    \"357\",\r\n    \"453\",\r\n    \"452\",\r\n    \"451\",\r\n    \"450\",\r\n    \"449\",\r\n    \"448\",\r\n    \"261\",\r\n    \"446\",\r\n    \"342\",\r\n    \"353\"\r\n  ],\r\n  \"L上瞼\": [\r\n    \"362\",\r\n    \"398\",\r\n    \"384\",\r\n    \"385\",\r\n    \"386\",\r\n    \"387\",\r\n    \"388\",\r\n    \"466\",\r\n    \"263\",\r\n    \"359\",\r\n    \"467\",\r\n    \"260\",\r\n    \"259\",\r\n    \"257\",\r\n    \"258\",\r\n    \"286\",\r\n    \"414\",\r\n    \"463\"\r\n  ],\r\n  \"L眉毛\": [\r\n    \"336\",\r\n    \"296\",\r\n    \"334\",\r\n    \"293\",\r\n    \"300\",\r\n    \"276\",\r\n    \"283\",\r\n    \"282\",\r\n    \"295\",\r\n    \"285\"\r\n  ],\r\n  \"L鼻\": [\r\n    \"294\",\r\n    \"278\",\r\n    \"344\",\r\n    \"440\",\r\n    \"457\",\r\n    \"459\",\r\n    \"309\",\r\n    \"392\",\r\n    \"289\",\r\n    \"305\",\r\n    \"460\",\r\n    \"327\"\r\n  ],\r\n  \"L目\": [\r\n    \"474\",\r\n    \"475\",\r\n    \"476\",\r\n    \"477\"\r\n  ],\r\n  \"Rほうれい線\": [\r\n    \"36\",\r\n    \"129\",\r\n    \"98\",\r\n    \"165\",\r\n    \"92\",\r\n    \"186\",\r\n    \"57\",\r\n    \"214\",\r\n    \"207\",\r\n    \"205\"\r\n  ],\r\n  \"R下瞼1\": [\r\n    \"133\",\r\n    \"155\",\r\n    \"154\",\r\n    \"153\",\r\n    \"145\",\r\n    \"144\",\r\n    \"163\",\r\n    \"7\",\r\n    \"33\",\r\n    \"130\",\r\n    \"25\",\r\n    \"110\",\r\n    \"24\",\r\n    \"23\",\r\n    \"22\",\r\n    \"26\",\r\n    \"112\",\r\n    \"243\"\r\n  ],\r\n  \"R下瞼2\": [\r\n    \"113\",\r\n    \"226\",\r\n    \"31\",\r\n    \"228\",\r\n    \"229\",\r\n    \"230\",\r\n    \"231\",\r\n    \"232\",\r\n    \"233\",\r\n    \"128\",\r\n    \"121\",\r\n    \"120\",\r\n    \"119\",\r\n    \"118\",\r\n    \"117\",\r\n    \"111\",\r\n    \"35\",\r\n    \"124\"\r\n  ],\r\n  \"R上瞼\": [\r\n    \"33\",\r\n    \"246\",\r\n    \"161\",\r\n    \"160\",\r\n    \"159\",\r\n    \"158\",\r\n    \"157\",\r\n    \"173\",\r\n    \"133\",\r\n    \"243\",\r\n    \"190\",\r\n    \"56\",\r\n    \"28\",\r\n    \"27\",\r\n    \"29\",\r\n    \"30\",\r\n    \"247\",\r\n    \"130\"\r\n  ],\r\n  \"R眉毛\": [\r\n    \"70\",\r\n    \"63\",\r\n    \"105\",\r\n    \"66\",\r\n    \"107\",\r\n    \"55\",\r\n    \"65\",\r\n    \"52\",\r\n    \"53\",\r\n    \"46\"\r\n  ],\r\n  \"R鼻\": [\r\n    \"64\",\r\n    \"48\",\r\n    \"115\",\r\n    \"220\",\r\n    \"237\",\r\n    \"239\",\r\n    \"79\",\r\n    \"166\",\r\n    \"59\",\r\n    \"75\",\r\n    \"240\",\r\n    \"98\"\r\n  ],\r\n  \"R目\": [\r\n    \"469\",\r\n    \"470\",\r\n    \"471\",\r\n    \"472\"\r\n  ],\r\n  \"U唇\": [\r\n    \"0\",\r\n    \"267\",\r\n    \"269\",\r\n    \"270\",\r\n    \"409\",\r\n    \"306\",\r\n    \"292\",\r\n    \"308\",\r\n    \"415\",\r\n    \"310\",\r\n    \"311\",\r\n    \"312\",\r\n    \"13\",\r\n    \"82\",\r\n    \"81\",\r\n    \"80\",\r\n    \"191\",\r\n    \"78\",\r\n    \"62\",\r\n    \"76\",\r\n    \"185\",\r\n    \"40\",\r\n    \"39\",\r\n    \"37\"\r\n  ],\r\n  \"顎\": [\r\n    \"18\",\r\n    \"313\",\r\n    \"406\",\r\n    \"262\",\r\n    \"369\",\r\n    \"396\",\r\n    \"175\",\r\n    \"171\",\r\n    \"140\",\r\n    \"32\",\r\n    \"182\",\r\n    \"83\"\r\n  ]\r\n}";
            sql_params = new List<SQLiteParameter>() {
                new SQLiteParameter("@name", "sample_mediapipe_face"),
                new SQLiteParameter("@landmark_type", "mediapipe_face_mesh_landmarker"),
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
