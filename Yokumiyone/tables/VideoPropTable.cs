using Common;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.IO;

namespace Yokumiyone.tables
{
    internal class VideoPropTable
    {
        private YokumiyoneDb db = new YokumiyoneDb();

        private List<string> videoPathList = new List<string>();
        private List<VideoProp> videoPropList = new List<VideoProp>();

        public VideoPropTable()
        {
            db.CreateDb();
        }

        public List<string> VideoPathList { get { return videoPathList; } }
        public List<VideoProp> VideoPropList { get { return videoPropList; } }

        private List<VideoProp> ExecReader(string stmt, List<SQLiteParameter> sql_params)
        {
            List<VideoProp> ret = new List<VideoProp>();
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
                        // 1行ずつデータを取得
                        while (reader.Read())
                        {
                            int width = Convert.ToInt32((long)reader["width"]);
                            int height = Convert.ToInt32(reader["height"]);
                            int rotation = Convert.ToInt32(reader["rotation"]);
                            DateTime modified = DateTime.Parse((reader["modified"] == null) ? "2000/1/1 0:0:0" : reader["modified"].ToString());
                            ret.Add(new VideoProp(reader["path"].ToString(), reader["fps"].ToString(), reader["duration"].ToString(), width, height, rotation, modified, reader["scene_cnt"].ToString()));
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

        public void Insert(VideoProp prop)
        {
            List<SQLiteParameter> sql_params = new List<SQLiteParameter>() {
                new SQLiteParameter("@path", prop.FilePath),
                new SQLiteParameter("@fps", prop.FpsFormat),
                new SQLiteParameter("@duration", prop.DurationStr),
                new SQLiteParameter("@width", Convert.ToInt64(prop.Width)),
                new SQLiteParameter("@height", Convert.ToInt64(prop.Height)),
                new SQLiteParameter("@rotation", Convert.ToInt64(prop.Rotation)),
                new SQLiteParameter("@modified", prop.ModifiedDatetime.ToString()),
                new SQLiteParameter("@scene_cnt", prop.NumOfScene)
            };

            db.ExecNonQuery($"INSERT INTO video_prop (path, fps, duration, width, height, rotation, modified, scene_cnt) VALUES (@path, @fps, @duration, @width, @height, @rotation, @modified, @scene_cnt)", sql_params);
        }
        public void Delete(string filePath)
        {
            List<SQLiteParameter> sql_params = new List<SQLiteParameter>() {
                new SQLiteParameter("@path", filePath)
            };
            db.ExecNonQuery($"DELETE FROM video_prop WHERE path = @path", sql_params);
            videoPathList.Remove(filePath);
        }

        public void SelectFolder(string folderPath)
        {
            List<SQLiteParameter> sql_params = new List<SQLiteParameter>() {
                new SQLiteParameter("@folder_path", folderPath+"%")
            };
            videoPropList = ExecReader($"SELECT * FROM video_prop WHERE path LIKE @folder_path", sql_params);
            foreach (VideoProp prop in videoPropList)
            {
                videoPathList.Add(prop.FilePath);
            }
        }

        public void UpdateSceneCnt(string filePath, string newCnt)
        {
            DateTime writeTime = File.GetLastWriteTime(filePath);
            List<SQLiteParameter> sql_params = new List<SQLiteParameter>() {
                new SQLiteParameter("@path", filePath),
                new SQLiteParameter("@modified", writeTime),
                new SQLiteParameter("@scene_cnt", newCnt)
            };
            db.ExecNonQuery($"UPDATE video_prop SET scene_cnt = @scene_cnt, modified = @modified WHERE path = @path", sql_params);
        }
    }
}
