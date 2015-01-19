using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkModule;
using System.Collections;
using System.IO;

namespace GameNetwork
{
    public class PlayerNetworkManager : NetworkManager
    {
        /// <summary>
        /// Atributes
        /// </summary>
        protected NetworkClient netClient;
        protected string serverIP;
        protected bool isServerDiscovered = false;
        public bool IsConnected { get { return netClient.IsConnected; } }
        protected int groupId;
        protected int playerId;
        protected string recoveryFile = "recovery.txt";

        /// <summary>
        /// Events
        /// </summary>
        public event Action<List<ObjectState>> GameStateReceived;
        public event Action<LevelState> LevelStateReceived;
        public event Action<LevelInfo> GoToLevelReceived;
        public event Action<PlayerInfo> PlayerInfoReceived;
        public event Action ServerConnected;
        public event Action GameStarted;
        public event Action ResetLevel;
        public event Action PauseGame;
        public event Action ResumeGame;
        public event Action<LevelInfo> RecoverToTraining;
        public event Action<LevelInfo> RecoverToMission;

        public PlayerNetworkManager()
            : base()
        {
        }

        public override void Init()
        {
            netClient = new NetworkClient();
            messenger.Init();
        }

        public override void Close()
        {
            base.Close();
            //File.Delete(recoveryFile);
        }

        public void SaveRecoveryInfo()
        {
            File.Delete(recoveryFile);
            using (StreamWriter sw = new StreamWriter(recoveryFile))
            {
                sw.WriteLine("RECOVERY");
                sw.WriteLine(serverIP);
                sw.WriteLine(playerId.ToString());
                sw.WriteLine(groupId.ToString());
            }
        }

        public PlayerInfo RecoverConnection()
        {
            PlayerInfo pinfo = null;
            if (File.Exists(recoveryFile))
            {
                using (StreamReader sr = new StreamReader(recoveryFile))
                {
                    sr.ReadLine();
                    serverIP = sr.ReadLine();
                    playerId = int.Parse(sr.ReadLine());
                    groupId = int.Parse(sr.ReadLine());
                    pinfo = new PlayerInfo(playerId, groupId);
                }
                netClient.Connect(serverIP);
            }
            return pinfo;
        }




        #region Send Methods

        public void SendPlayerConnected()
        {
            if (!netClient.IsConnected)
                return;

            Message msg = new Message((int)GameMessages.PlayerConnected);
            netClient.SendMessage(coder.encode(msg), true, true);
        }
        
        /// <summary>
        /// Envía mensaje con estado del jugador
        /// </summary>
        public void SendPlayerState(PlayerState state)
        {
            if (!netClient.IsConnected)
                return;

            Params p = new Params(ParamsType.NetworkObjectParams);
            p.addParam(state);
            Message msg = new Message((int)GameMessages.PlayerState, p);
            netClient.SendMessage(coder.encode(msg), true, true);
        }

        /// <summary>
        /// Envía mensaje de juego terminado
        /// </summary>
        public void SendLevelEnded(LevelState state)
        {
            if (!netClient.IsConnected)
                return;

            Params p = new Params(ParamsType.NetworkObjectParams);
            p.addParam(state);
            Message msg = new Message((int)GameMessages.LevelEnded, p);
            netClient.SendMessage(coder.encode(msg), true, true);
        }

        public void SendReconnect()
        {
            if (!netClient.IsConnected)
                return;

            PlayerInfo pinfo = new PlayerInfo(playerId, groupId);
            Params p = new Params(ParamsType.NetworkObjectParams);
            p.addParam(pinfo);
            Message msg = new Message((int)GameMessages.PlayerRecover, p);
            netClient.SendMessage(coder.encode(msg), true, true);
        }


        #endregion

        #region Message Reception

        /// <summary>
        /// Controla mensajes de conexión
        /// </summary>
        public void HandleConnectionMessages()
        {
            List<byte[]> messages = messenger.ReceiveData();
            foreach (byte[] msg in messages)
            {
                Message mMsg = coder.decode(msg);
                if (mMsg.MessageId == (int)GameMessages.ServerDiscovery && !isServerDiscovered)
                {
                    serverIP = messenger.RemoteIP.ToString();
                    netClient.Connect(serverIP); //TODO: Verificar, tras esto NO queda isConected = true;
                    //SendPlayerConnected();
                    OnServerConnected();
                    isServerDiscovered = true;
                }
            }
        }

        /// <summary>
        /// Maneja mensajes recibidos
        /// </summary>
        public override void HandleNetworkMessages()
        {
            List<byte[]> messages = netClient.ReceiveMessage();
            foreach (byte[] msg in messages)
            {
                Message mMsg = coder.decode(msg);
                if (mMsg.MessageId == (int)GameMessages.PlayerInfo)
                {
                    PlayerInfo pinfo = (PlayerInfo)mMsg.MsgParams.ParamList[0];
                    playerId = pinfo.Id;
                    groupId = pinfo.GroupId;

                    OnPlayerInfoReceived(pinfo);
                }
                else if (mMsg.MessageId == (int)GameMessages.GameState)
                {
                    List<ObjectState> states = new List<ObjectState>();
                    foreach (ObjectState state in mMsg.MsgParams.ParamList)
                        if (state.GroupId == groupId)
                        {
                            states.Add(state);
                        }

                    OnGameStateReceived(states);
                }
                else if (mMsg.MessageId == (int)GameMessages.LevelEnded)
                {
                    LevelState level = (LevelState)mMsg.MsgParams.ParamList[0];
                    if (level.GroupId == groupId)
                        OnLevelStateReceived(level);

                }
                else if (mMsg.MessageId == (int)GameMessages.GoToLevel)
                {
                    LevelInfo level = (LevelInfo)mMsg.MsgParams.ParamList[0];
                    OnGoToLevelReceived(level);

                }
                else if (mMsg.MessageId == (int)GameMessages.ResetLevelPlayer)
                {
                    int msgPlayerId = (int)mMsg.MsgParams.ParamList[0];
                    int msgGroupId = (int)mMsg.MsgParams.ParamList[1];
                    if (msgPlayerId == playerId && msgGroupId == groupId)
                        OnResetLevel();

                }
                else if (mMsg.MessageId == (int)GameMessages.ResetLevelGroup)
                {
                    int msgGroupId = (int)mMsg.MsgParams.ParamList[0];
                    if (msgGroupId == groupId)
                        OnResetLevel();

                }
                else if (mMsg.MessageId == (int)GameMessages.PauseGame)
                {
                    OnPauseGame();
                }
                else if (mMsg.MessageId == (int)GameMessages.ResumeGame)
                {
                    OnResumeGame();
                }
                else if (mMsg.MessageId == (int)GameMessages.GameStart)
                {
                    OnGameStarted();
                }
                else if (mMsg.MessageId == (int)GameMessages.RecoverToTraining)
                {
                    LevelInfo level = (LevelInfo)mMsg.MsgParams.ParamList[0];
                    OnRecoverToTraining(level);
                }
                else if (mMsg.MessageId == (int)GameMessages.RecoverToMission)
                {
                    LevelInfo level = (LevelInfo)mMsg.MsgParams.ParamList[0];
                    OnRecoverToMission(level);
                }
            }
        }

        #endregion

        #region Protected & Event Methods
        protected virtual void OnServerConnected()
        {
            if (ServerConnected != null)
                ServerConnected();
        }

        protected virtual void OnGameStateReceived(List<ObjectState> states)
        {
            if (GameStateReceived != null)
                GameStateReceived(states);
        }

        protected virtual void OnLevelStateReceived(LevelState state)
        {
            if (LevelStateReceived != null)
                LevelStateReceived(state);
        }

        protected virtual void OnGoToLevelReceived(LevelInfo level)
        {
            if (GoToLevelReceived != null)
                GoToLevelReceived(level);
        }


        protected virtual void OnResetLevel()
        {
            if (ResetLevel != null)
                ResetLevel();
        }

        protected virtual void OnPauseGame()
        {
            if (PauseGame != null)
                PauseGame();
        }

        protected virtual void OnResumeGame()
        {
            if (ResumeGame != null)
                ResumeGame();
        }

        protected virtual void OnPlayerInfoReceived(PlayerInfo pinfo)
        {
            if (PlayerInfoReceived != null)
                PlayerInfoReceived(pinfo);
        }

        protected virtual void OnGameStarted()
        {
            if (GameStarted != null)
                GameStarted();
        }

        protected virtual void OnRecoverToTraining(LevelInfo level)
        {
            if (RecoverToTraining != null)
                RecoverToTraining(level);
        }

        protected virtual void OnRecoverToMission(LevelInfo level)
        {
            if (RecoverToMission != null)
                RecoverToMission(level);
        }
        #endregion
    }
}
