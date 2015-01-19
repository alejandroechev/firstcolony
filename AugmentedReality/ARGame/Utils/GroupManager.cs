using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameNetwork;
using System.IO;

namespace ARGame.Utils
{
   
    public struct ResultData
    {
        public string text;
        public bool success;
    }

    public class LogData
    {
        public List<string> data = new List<string>();
    }

    public class LogInfo
    {
        public List<bool> isPlayer = new List<bool>();
        public List<int> playerCharge = new List<int>();
        public int crystalCharge = 0;

        public void AddPlayer()
        {
            isPlayer.Add(false);
            playerCharge.Add(2);
        }

        public bool Complete()
        {
            bool complete = true;
            foreach (bool b in isPlayer)
                complete &= b;
            return (complete);
        }

        public string LogEntry()
        {
            string s = "";
            foreach (int charge in playerCharge)
            {
                s += (charge == 2) ? "X" : (charge == 0) ? "N" : (charge == crystalCharge) ? "R" : "A";
                s += " ";
            }
            return s;
        }
    }
    

    public class GroupManager
    {
        private string groupLabel = "Grupo";
        private string playerLabel = "Jugador";
        

        private List<PlayerInfo> allPlayers;
        public List<PlayerInfo> AllPlayers { get { return allPlayers; } }
        private Dictionary<int, List<PlayerInfo>> playersByGroup;
        public Dictionary<int, List<PlayerInfo>> PlayersByGroup { get { return playersByGroup; } }

        public Dictionary<int, GameData> PlayersData { get; private set; }
        public Dictionary<int, GameData> GroupsData { get; private set; }
        public Dictionary<int, List<LogData>> GroupsLog { get; private set; }

        private int numPlayerGroup = 3;
        private int currentGroup = 0;
        private int currentPlayerId = 0;

        private string resultsFileName = "";
        
        public int CurrentTraining { get; private set; }
        public int CurrentLevel { get; private set; }
        
        private Dictionary<int, LogInfo> currentLogInfo = new Dictionary<int, LogInfo>();

        public GroupManager()
        {
            allPlayers = new List<PlayerInfo>();
            playersByGroup = new Dictionary<int, List<PlayerInfo>>();
            PlayersData = new Dictionary<int, GameData>();
            GroupsData = new Dictionary<int, GameData>();
            GroupsLog = new Dictionary<int, List<LogData>>();
            resultsFileName = "resultados" + System.DateTime.Now.ToShortDateString() + ".txt";
            CurrentTraining = -1;
            CurrentLevel = 0;
        }

        public void NewPlayer()
        {
            PlayerInfo pinfo = new PlayerInfo(currentPlayerId, currentGroup);
            allPlayers.Add(pinfo);
            if (!playersByGroup.ContainsKey(currentGroup))
            {
                playersByGroup.Add(currentGroup, new List<PlayerInfo>());
                GroupsData.Add(currentGroup, new GameData());
                //GroupsLog.Add(currentGroup, new List<LogData>());
                //currentLogInfo.Add(currentGroup, new LogInfo());
            }
            playersByGroup[currentGroup].Add(pinfo);
            //currentLogInfo[currentGroup].AddPlayer();
            PlayersData.Add(currentPlayerId + 3*currentGroup, new GameData());
            currentPlayerId++;
            if (currentPlayerId >= numPlayerGroup)
            {
                currentPlayerId = 0;
                currentGroup++;
            
            }
        }

        public int PlayersInGroup(int groupId)
        {
            if (PlayersByGroup.ContainsKey(groupId))
            {
                return PlayersByGroup[groupId].Count;
            }
            return -1;
        }


        public void NextTraining()
        {
            foreach (KeyValuePair<int, GameData> playerPair in PlayersData)
            {
                playerPair.Value.NewLevel();
            }
            CurrentTraining++;
        }


        public void NextLevel()
        {
            foreach (KeyValuePair<int, GameData> groupPair in GroupsData)
            {
                groupPair.Value.NewLevel();
            }
            foreach (KeyValuePair<int, List<LogData>> logPair in GroupsLog)
            {
                logPair.Value.Add(new LogData());
            }
            CurrentLevel++;
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

        public void LogPlayerState(int playerId, int groupId, float charge, bool active, float crystalCharge)
        {
            LogInfo currLog = currentLogInfo[groupId];
            currLog.isPlayer[playerId] = true;
            currLog.playerCharge[playerId] = active ? Math.Sign(charge) : 2;
            currLog.crystalCharge = Math.Sign(crystalCharge);
            if (currLog.Complete())
            {
                GroupsLog[groupId][CurrentLevel-1].data.Add(currLog.LogEntry());
                currentLogInfo[groupId] = new LogInfo();
            }

        }

        public List<ResultData> GetCurrentPlayerIterationsData()
        {
            List<ResultData> data = new List<ResultData>();
            foreach (KeyValuePair<int, GameData> playerData in PlayersData)
            {
                int group = allPlayers[playerData.Key].GroupId + 1;
                int playerPos = playerData.Key % 3;
                string s = playerLabel + " " + (playerPos+1) + ": " + playerData.Value.CurrentLevelData.ToStringIterations();
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
                string s = playerLabel + " " + (playerPos + 1) + ": " + playerData.Value.CurrentLevelData.ToString() + "\n";
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
            using (StreamWriter sw = new StreamWriter(resultsFileName))
            {
                foreach (KeyValuePair<int, GameData> playerData in PlayersData)
                {
                    int group = allPlayers[playerData.Key].GroupId + 1;
                    int playerPos = playerData.Key % 3;
                    string s = groupLabel + " " + group + " " + playerLabel + " " + (playerPos + 1);
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

        public void PrintLogs()
        {
            int i = 1;
            foreach (KeyValuePair<int, List<LogData>> logData in GroupsLog)
            {
                string logFileName = "log" + i + "_" + System.DateTime.Now.ToShortDateString() + ".txt";
                using (StreamWriter sw = new StreamWriter(logFileName))
                {
                    int j = 0;
                    foreach (LogData log in logData.Value)
                    {
                        sw.WriteLine("Mision " + j);
                        foreach (string s in log.data)
                        {
                            sw.WriteLine(s);
                        }
                        sw.WriteLine("");
                        j++;
                    }
                }
                i++;
            }
        }
    }
}
