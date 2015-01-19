using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkModule;
using System.Collections;

namespace GameNetwork
{
    public class ServerNetworkManager : NetworkManager
    {
        /// <summary>
        /// Atributes
        /// </summary>
        protected NetworkServer netServer;
        protected List<string> playerIPs;
        protected Dictionary<int, string> playerIPsTable;
        protected Dictionary<string, PlayerInfo> playerInfoTable;

        /// <summary>
        /// Events
        /// </summary>
        public event Action<PlayerState> PlayerStateReceived;
        public event Action<LevelState> LevelEndedReceived;
        public event Action NewPlayer;
        public event Action<string, PlayerInfo> PlayerDisconnected;
        public event Action<PlayerInfo> PlayerRecovered;
        
        public ServerNetworkManager()
            : base()
        {
        }

        public override void Init()
        {
            netServer = new NetworkServer();
            netServer.Initialize();
            netServer.ClientDisconnected += HandlePlayerDisconnected;
            playerIPs = new List<string>();
            playerIPsTable = new Dictionary<int, string>();
            playerInfoTable = new Dictionary<string, PlayerInfo>();
            messenger.Init();
        }
                
        public override void Close()
        {
            base.Close();
            netServer.Shutdown();
        }

        private void HandlePlayerDisconnected(string clientIP)
        {
            if (PlayerDisconnected != null)
            {
                PlayerInfo disconnectedPlayer = null;
                if (playerInfoTable.ContainsKey(clientIP))
                    disconnectedPlayer = playerInfoTable[clientIP];
                PlayerDisconnected(clientIP, disconnectedPlayer);
            }
        }        


        #region Send Methods

        /// <summary>
        /// Envía un mensaje a toda la red pra descubrir nuevos jugadores
        /// </summary>
        public void SendDiscovery()
        {
            Message msg = new Message((int)GameMessages.ServerDiscovery);
            messenger.SendBroadcastData(coder.encode(msg));
        }

        /// <summary>
        /// Envía la información a los jugadores la primera vez, asignandoles un ID de jugador a cada uno y guardandolos en una
        /// tabla ID-IP
        /// </summary>
        public void SendPlayerInfo(List<PlayerInfo> players)
        {
            int i = 0;
            foreach (PlayerInfo player in players)
            {
                Params p = new Params(ParamsType.NetworkObjectParams);
                p.addParam(player);
                Message msg = new Message((int)GameMessages.PlayerInfo, p);
                netServer.SendMessage(coder.encode(msg), new List<string>{playerIPs[i]}, true, true);
                playerIPsTable.Add(player.Id + player.GroupId*3, playerIPs[i]);
                playerInfoTable.Add(playerIPs[i], player );
                i++;
            }
        }

        public void SendGameStart()
        {
            Message msg = new Message((int)GameMessages.GameStart);
            netServer.BroadcastMessage(coder.encode(msg), true, true, false);
        }

        /// <summary>
        /// Envía un mensaje de juego terminado y el estado de los objetos
        /// </summary>
        public void SendGameState(List<ObjectState> states, List<MetaDataObjectState> metaStates)
        {
            Params p = new Params(ParamsType.NetworkObjectParams);
            foreach (ObjectState state in states)
            {
                p.addParam(state);
            }
            foreach (ObjectState state in metaStates)
            {
                p.addParam(state);
            }
            Message msg = new Message((int)GameMessages.GameState, p);
            netServer.BroadcastMessage(coder.encode(msg), true, true, false);
        }

        /// <summary>
        /// Envía mensaje de pasar al siguiente nivel y como parámetro la información de éste
        /// </summary>
        public void SendGoToLevel(LevelInfo info)
        {
            Params p = new Params(ParamsType.NetworkObjectParams);
            p.addParam(info);
            Message msg = new Message((int)GameMessages.GoToLevel, p);
            netServer.BroadcastMessage(coder.encode(msg), true, true, false);
        }

        public void SendRecoverToTraining(LevelInfo info, PlayerInfo pinfo)
        {
            if (playerIPsTable.ContainsKey(pinfo.Id + pinfo.GroupId * 3))
            {
                Params p = new Params(ParamsType.NetworkObjectParams);
                p.addParam(info);
                Message msg = new Message((int)GameMessages.RecoverToTraining, p);
                netServer.SendMessage(coder.encode(msg), new List<string> { playerIPsTable[pinfo.Id + pinfo.GroupId * 3] }, true, true);
            }
        }

        public void SendRecoverToMission(LevelInfo info, PlayerInfo pinfo)
        {
            if (playerIPsTable.ContainsKey(pinfo.Id + pinfo.GroupId * 3))
            {
                Params p = new Params(ParamsType.NetworkObjectParams);
                p.addParam(info);
                Message msg = new Message((int)GameMessages.RecoverToMission, p);
                netServer.SendMessage(coder.encode(msg), new List<string> { playerIPsTable[pinfo.Id + pinfo.GroupId * 3] }, true, true);
            }
        }

        public void SendResetLevelPlayer(int player, int group)
        {
            Params p = new Params(ParamsType.IntParams);
            p.addParam(player);
            p.addParam(group);
            Message msg = new Message((int)GameMessages.ResetLevelPlayer, p);
            netServer.BroadcastMessage(coder.encode(msg), true, true, false);
        }

        public void SendResetLevelGroup(int group)
        {
            Params p = new Params(ParamsType.IntParams);
            p.addParam(group);
            Message msg = new Message((int)GameMessages.ResetLevelGroup, p);
            netServer.BroadcastMessage(coder.encode(msg), true, true, false);
        }

        public void SendPauseGame()
        {
            Message msg = new Message((int)GameMessages.PauseGame);
            netServer.BroadcastMessage(coder.encode(msg), true, true, false);
        }

        public void SendResumeGame()
        {
            Message msg = new Message((int)GameMessages.ResumeGame);
            netServer.BroadcastMessage(coder.encode(msg), true, true, false);
        }

        /// <summary>
        /// Envía mensaje de nivel terminado y como parámetro el estado de éste
        /// </summary>
        public void SendLevelEnded(LevelState state)
        {
            Params p = new Params(ParamsType.NetworkObjectParams);
            p.addParam(state);
            Message msg = new Message((int)GameMessages.LevelEnded, p);
            netServer.BroadcastMessage(coder.encode(msg), true, true, false);
        }

       
        #endregion

        #region Message Reception


        /// <summary>
        /// Maneja mensajes recibidos
        /// </summary>
        public override void HandleNetworkMessages()
        {
            List<byte[]> messages = netServer.ReceiveMessage();
            foreach (byte[] msg in messages)
            {
                Message mMsg = coder.decode(msg);
                ///Server Messages
                if (mMsg.MessageId == (int)GameMessages.PlayerConnected)
                {
                    if (!playerIPs.Contains(netServer.PrevSenderString))
                    {
                        playerIPs.Add(netServer.PrevSenderString);
                        OnNewPlayer();
                    }
                }
                else if (mMsg.MessageId == (int)GameMessages.PlayerState)
                {
                    OnPlayerStateReceived((PlayerState)mMsg.MsgParams.ParamList[0]);
                }
                else if (mMsg.MessageId == (int)GameMessages.LevelEnded)
                {
                    OnLevelEndedReceived((LevelState)mMsg.MsgParams.ParamList[0]);
                }
                else if (mMsg.MessageId == (int)GameMessages.PlayerRecover)
                {
                    PlayerInfo pinfo = (PlayerInfo)mMsg.MsgParams.ParamList[0];
                    if (playerIPsTable.ContainsKey(pinfo.Id + pinfo.GroupId * 3))
                    {
                        playerIPsTable.Remove(pinfo.Id + pinfo.GroupId * 3);
                        playerIPsTable.Add(pinfo.Id + pinfo.GroupId * 3, netServer.PrevSenderString);
                    }
                    OnPlayerRecovered(pinfo);
                }
             
            }
        }
        #endregion

        #region Protected & Event Methods

        protected virtual void OnPlayerStateReceived(PlayerState state)
        {
            if (PlayerStateReceived != null)
                PlayerStateReceived(state);
        }

        protected virtual void OnLevelEndedReceived(LevelState state)
        {
            if (LevelEndedReceived != null)
                LevelEndedReceived(state);
        }

        protected virtual void OnNewPlayer()
        {
            if (NewPlayer != null)
                NewPlayer();
        }

        protected virtual void OnPlayerRecovered(PlayerInfo pinfo)
        {
            if (PlayerRecovered != null)
                PlayerRecovered(pinfo);
        }

        #endregion

        #region Getters

        public NetworkServer NetServer { get { return netServer; } }
        public List<string> PlayerIPs { get { return playerIPs; } }
        public Dictionary<int, string> PlayerIPsTable { get { return playerIPsTable; } }

        #endregion
    }
}
