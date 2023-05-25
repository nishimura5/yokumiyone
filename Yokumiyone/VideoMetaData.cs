using Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Yokumiyone.tables;

namespace Yokumiyone
{
    class VideoMetaData
    {
        private string ffmpegPath = "";
        private string targetVideoPath = "";

        public ObservableCollection<SceneProp> ScenePropOc = new ObservableCollection<SceneProp>();
        private List<SceneProp> initScenePropList = new List<SceneProp>();

        public VideoMetaData()
        {
        }

        public void SetPath(string ffmpegPath)
        {
            this.ffmpegPath = ffmpegPath;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="targetVideoPath"></param>
        /// <returns></returns>
        public void LoadMetaDataFile(string targetVideoPath)
        {
            // xmpファイルを読み込み
            this.targetVideoPath = targetVideoPath;
            var exiftool = new Exiftool(targetVideoPath);
            exiftool.LoadMetaDataFile();

            this.ScenePropOc.Clear();
            this.ScenePropOc = new ObservableCollection<SceneProp>(exiftool.SceneList);
            initScenePropList = this.ScenePropOc.ToList().DeepCopy();
        }

        public async Task UpdateMetaData(ObservableCollection<SceneProp> scenePropListOc)
        {
            List<string> oldSceneTitleList = new List<string>();
            List<string> newSceneTitleList = new List<string>();

            // 最初に読み込んだ時と比べて新しかったら上書き
            List<string> oldSceneCsvList = new List<string>();
            List<string> newSceneCsvList = new List<string>();
            foreach (SceneProp oldScene in initScenePropList)
            {
                oldSceneCsvList.Add(oldScene.ScenePropCsv);
                if (oldSceneTitleList.Contains(oldScene.Title) == false)
                {
                    oldSceneTitleList.Add(oldScene.Title);
                }
            }
            foreach (SceneProp newScene in scenePropListOc)
            {
                newSceneCsvList.Add(newScene.ScenePropCsv);
                if (newSceneTitleList.Contains(newScene.Title) == false)
                {
                    newSceneTitleList.Add(newScene.Title);
                }
            }

            bool isEqual = Enumerable.SequenceEqual(oldSceneCsvList, newSceneCsvList);
            if (isEqual == true)
            {
                return;
            }

            ScenePathTable db = new ScenePathTable(targetVideoPath);

            var exiftool = new Exiftool(targetVideoPath);
            // 新しいsceneListで上書き
            exiftool.SceneList = scenePropListOc.ToList();
            exiftool.UpdateMetaDataFile();

            await Task.Run(() => exiftool.InsertMetaData());
        }
    }
}
