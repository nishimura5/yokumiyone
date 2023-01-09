using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using Common;

namespace Yokumiyone
{
    internal class ScenePathTable
    {
        private YokumiyoneDb db = new YokumiyoneDb();
        private string path;

        public ScenePathTable()
        {
            db.CreateDb();
            this.path = "";
        }
        public ScenePathTable(string path)
        {
            this.path = path;
        }

        private List<string> ExecReader(string stmt, string path)
        {
            List<string> ret = new List<string>();
            try
            {
                // 接続先を指定
                using (var conn = new SQLiteConnection($"Data Source={db.dbPath}"))
                using (var command = conn.CreateCommand())
                {
                    conn.Open();
                    command.CommandText = stmt;
                    if (path != "")
                    {
                        command.Parameters.Add(new SQLiteParameter("@path", path));
                    }
                    using (var reader = command.ExecuteReader())
                    {
                        // 1行ずつデータを取得
                        while (reader.Read())
                        {
                            ret.Add((string)reader["scene"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.GetType().Name);
            }
            return ret;
        }

        // statusを0か1に更新する
        public void Update(List<string> srcSceneList)
        {
            List<string> aliveSceneList = new List<string>();
            foreach (string sceneTitle in srcSceneList)
            {
                aliveSceneList = aliveSceneList.Concat(sceneTitle.Split("|")).ToList();
            }
            aliveSceneList = aliveSceneList.Distinct().ToList();

            string insertStmt = "";
            if (aliveSceneList.Count > 0)
            {
                insertStmt = $"INSERT INTO scene_path(scene, path, status) VALUES ";
                List<string> stmtList = new List<string>();
                for (int idx = 0; idx < aliveSceneList.Count(); idx++)
                {
                    stmtList.Add($"(@scene{idx}, @path, 1)");
                }
                insertStmt += string.Join(",", stmtList);
                insertStmt += " ON CONFLICT (scene, path) DO UPDATE SET status = 1;";
            }

            string resetStmt = $"UPDATE scene_path SET status = 0 WHERE path = @path;";

            try
            {
                // 接続先を指定
                using (var conn = new SQLiteConnection($"Data Source={db.dbPath}"))
                using (var command = conn.CreateCommand())
                {
                    conn.Open();
                    // データを追加
                    command.CommandText = resetStmt + insertStmt;
                    command.Parameters.Add(new SQLiteParameter("@path", path));

                    for (int idx = 0; idx < aliveSceneList.Count(); idx++)
                    {
                        command.Parameters.Add(new SQLiteParameter($"@scene{idx}", aliveSceneList[idx]));
                    }
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.GetType().Name);
            }
        }

        public List<string> GetScenes()
        {
            List<string> SceneList = ExecReader($"SELECT scene FROM scene_path", "");
            SceneList = SceneList.Distinct().ToList();

            return SceneList;
        }

        // statusが0、つまり使われていないシーンを消す
        public void CleanRows()
        {
            List<SQLiteParameter> sql_params = new List<SQLiteParameter>();
            db.ExecNonQuery($"DELETE FROM scene_path WHERE status = 0", sql_params);
        }

        public void DeletePath(string filePath)
        {
            List<SQLiteParameter> sql_params = new List<SQLiteParameter>() {
                new SQLiteParameter("@path", filePath),
            };
            db.ExecNonQuery($"DELETE FROM scene_path WHERE path = @path", sql_params);
        }
    }
}
