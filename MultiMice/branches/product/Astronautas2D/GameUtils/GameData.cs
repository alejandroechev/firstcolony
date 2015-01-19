using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GameUtils
{
    public class GameData
    {
        private List<LevelData> levelDataList;
        public LevelData CurrentLevelData { get; private set; }

        public GameData()
        {
            levelDataList = new List<LevelData>();
        }

        public void NewLevel()
        {
            LevelData data = new LevelData();
            levelDataList.Add(data);
            CurrentLevelData = data;
        }

        public void IncreaseTries()
        {
            if (CurrentLevelData != null)
                CurrentLevelData.Tries++;
        }

        public void IncreaseIterations()
        {
            if (CurrentLevelData != null)
                CurrentLevelData.Iterations++;
        }

        public void EndLevel()
        {
            if (CurrentLevelData != null)
                CurrentLevelData.IsCompleted = true;
        }

        public void ResetData()
        {
            if (CurrentLevelData != null)
                CurrentLevelData.ResetData();
        }

        public string PrintData()
        {
            string data = "";
            int i = 1;
            foreach (LevelData levelData in levelDataList)
            {
                data += "Nivel" + i + ": " + levelData.ToString() + "\r\n";
                i++;
            }

            return data;
        }

        public string PrintDataIterations()
        {
            string data = "";
            int i = 1;
            foreach (LevelData levelData in levelDataList)
            {
                data += "Nivel" + i + ": " + levelData.ToStringIterations() + "\r\n";
                i++;
            }
            return data;
        }
    }
}
