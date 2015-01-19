using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameNetwork;
using System.IO;
using System.Xml.Linq;

namespace GameUtils
{
    public struct ResultData
    {
        public string text;
        public bool success;
    }

    public class GroupManager
    {
        private string groupLevelClass = "Astronautas2D.Levels.TeamLevel, Astronautas2D";
        private string individualLevelClass = "Astronautas2D.Levels.SoloLevel, Astronautas2D";

        private string groupLabel = "Grupo";
        private string playerLabel = "Jugador";
        private string[] playerNames2 = new string[] { "Círculo",  "Triángulo", "Cuadrado" };
        private string[] playerNames = new string[] { "Círculo", "Cuadrado", "Triángulo" };

        private List<GroupInfo> allGroups;
        public List<GroupInfo> AllGroups { get { return allGroups; } }
        private Dictionary<int, PlayerInfo> allPlayers;
        public List<PlayerInfo> AllPlayers { get { return allPlayers.Values.ToList<PlayerInfo>(); } }

        public Dictionary<int, GameData> PlayersData { get; private set; }
        public Dictionary<int, GameData> GroupsData { get; private set; }

        private int currentGroup = 0;
        
        public int CurrentTraining {get; private set;}
        private int numTrainings = 0;
        public int CurrentLevel {get; private set;}
        private int numLevels = 0;

        private string resultsFileName = "";

        public GroupManager(int numTrainings, int numLevels)
        {
            allGroups = new List<GroupInfo>();
            allPlayers = new Dictionary<int, PlayerInfo>();
            PlayersData = new Dictionary<int, GameData>();
            GroupsData = new Dictionary<int, GameData>();
            resultsFileName = "resultados" + System.DateTime.Now.ToShortDateString() + ".txt";

            CurrentTraining = -1;
            CurrentLevel = 0;
            this.numTrainings = numTrainings;
            this.numLevels = numLevels;
        }

        public GroupManager(int numLevels) : this(0, numLevels)
        {
        }

        public GroupManager()
            : this(0, 0)
        {
        }

        public void Load(string gameFile)
        {
            this.numTrainings = 0;
            this.numLevels = 0;

            playerNames = playerNames2;

            XDocument xmlDoc = XDocument.Load(gameFile);

            XElement gameNode = xmlDoc.Elements("game").First();
            XElement levelRoot = gameNode.Elements("levels").First();
            IEnumerable<XElement> levelElements = levelRoot.Elements("level");


            foreach (XElement levelNode in levelElements)
            {
                string fileName = levelNode.Attribute("file").Value;
                string objectType = levelNode.Attribute("class").Value;
                if (objectType.Equals(individualLevelClass))
                    numTrainings++;
                else
                    numLevels++;
            }

        }

        public void NewGroup()
        {
            GroupInfo ginfo = new GroupInfo(currentGroup);
            allGroups.Add(ginfo);
            GroupsData.Add(currentGroup, new GameData());
            
            PlayerInfo pinfo = new PlayerInfo(3*currentGroup, currentGroup);
            allPlayers.Add(pinfo.Id,pinfo);
            PlayersData.Add(pinfo.Id, new GameData());
            pinfo = new PlayerInfo(3 * currentGroup + 1, currentGroup);
            allPlayers.Add(pinfo.Id, pinfo);
            PlayersData.Add(pinfo.Id, new GameData());
            pinfo = new PlayerInfo(3 * currentGroup + 2, currentGroup);
            allPlayers.Add(pinfo.Id, pinfo);
            PlayersData.Add(pinfo.Id, new GameData());
            
            currentGroup++;
        }


        public bool NextTraining()
        {
            if (CurrentTraining >= numTrainings-1)
                return false;

            foreach (KeyValuePair<int, GameData> playerPair in PlayersData)
            {
                playerPair.Value.NewLevel();
            }
            CurrentTraining++;
            return true;
        }


        public bool NextLevel()
        {
            if (CurrentLevel + CurrentTraining >= numLevels + numTrainings - 1)
                return false;
            
            foreach (KeyValuePair<int, GameData> groupPair in GroupsData)
            {
                groupPair.Value.NewLevel();
            }
            CurrentLevel++;
            return true;
        }

        public void IncreasePlayerIterations(int id)
        {
            PlayersData[id].IncreaseIterations();
        }

        public void IncreasePlayerTries(int id)
        {
            PlayersData[id].IncreaseTries();
        }

        public void ResetPlayerData(int id)
        {
            PlayersData[id].ResetData();
        }

        public void IncreaseGroupIterations(int id)
        {
            GroupsData[id].IncreaseIterations();
        }

        public void IncreaseGroupTries(int id)
        {
            GroupsData[id].IncreaseTries();
        }

        public List<ResultData> GetCurrentPlayerIterationsData()
        {
            List<ResultData> data = new List<ResultData>();
            foreach (KeyValuePair<int, GameData> playerData in PlayersData)
            {
                int group = allPlayers[playerData.Key].GroupId + 1;
                int playerPos = playerData.Key % 3;
                string s = groupLabel + " " + group + " " + playerNames[playerPos] + ": " + playerData.Value.CurrentLevelData.ToStringIterations();
                ResultData r = new ResultData();
                r.text = s;
                r.success = playerData.Value.CurrentLevelData.Iterations > 0;
                data.Add(r);
            }
            return data;
        }

        public List<ResultData> GetCurrentPlayerAllData()
        {
            List<ResultData> data = new List<ResultData>();
            foreach (KeyValuePair<int, GameData> playerData in PlayersData)
            {
                int group = allPlayers[playerData.Key].GroupId + 1;
                int playerPos = playerData.Key % 3;
                string s = groupLabel + " " + group + " " + playerNames[playerPos] + ": " + playerData.Value.CurrentLevelData.ToString() + "\n";
                ResultData r = new ResultData();
                r.text = s;
                r.success = playerData.Value.CurrentLevelData.Iterations > 0;
                data.Add(r);
            }
            return data;
        }

        public List<ResultData> GetCurrentGroupIterationsData()
        {
            List<ResultData> data = new List<ResultData>();
            foreach (KeyValuePair<int, GameData> groupData in GroupsData)
            {
                string s = groupLabel + (groupData.Key + 1) + ": " + groupData.Value.CurrentLevelData.ToStringIterations() + "\n";
                ResultData r = new ResultData();
                r.text = s;
                r.success = groupData.Value.CurrentLevelData.Iterations > 0;
                data.Add(r);
            }
            return data;
        }

        public List<ResultData> GetCurrentGroupAllData()
        {
            List<ResultData> data = new List<ResultData>();
            foreach (KeyValuePair<int, GameData> groupData in GroupsData)
            {
                string s = groupLabel + (groupData.Key + 1) + ": " + groupData.Value.CurrentLevelData.ToString() + "\n";
                ResultData r = new ResultData();
                r.text = s;
                r.success = groupData.Value.CurrentLevelData.Iterations > 0;
            }
            return data;
        }

        public void PrintData()
        {
            using(StreamWriter sw = new StreamWriter(resultsFileName))
            {
                foreach (KeyValuePair<int, GameData> playerData in PlayersData)
                {
                    int group = allPlayers[playerData.Key].GroupId + 1;
                    int playerPos = playerData.Key % 3;
                    string s = groupLabel + " " + group + " " + playerNames[playerPos];
                    sw.WriteLine(s);
                    sw.WriteLine(playerData.Value.PrintData());
                    sw.WriteLine("");
                }

                foreach (KeyValuePair<int, GameData> groupData in GroupsData)
                {
                    string s = groupLabel + (groupData.Key + 1);
                    sw.WriteLine(s);
                    sw.WriteLine(groupData.Value.PrintData());
                    sw.WriteLine("");
                }
            }
        }

        public void PrintDataIterations()
        {
            using (StreamWriter sw = new StreamWriter(resultsFileName))
            {
                foreach (KeyValuePair<int, GameData> playerData in PlayersData)
                {
                    sw.WriteLine(playerLabel + playerData.Key);
                    sw.WriteLine(playerData.Value.PrintDataIterations());
                    sw.WriteLine("");
                }

                foreach (KeyValuePair<int, GameData> groupData in GroupsData)
                {
                    sw.WriteLine(groupLabel + groupData.Key);
                    sw.WriteLine(groupData.Value.PrintDataIterations());
                    sw.WriteLine("");
                }
            }
        }
    }
}
