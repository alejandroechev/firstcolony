using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Net;
using NetworkModule;
using System.Collections;

namespace GameNetwork
{
    /// <summary>
    /// Mensajes posible para enviar
    /// </summary>
    public enum GameMessages
    {
        ServerDiscovery = 0,
        PlayerConnected,

        PlayerInfo,
        GameStart,

        GameState,
        PlayerState,
        
        GoToLevel,
        LevelEnded,
        ResetLevelGroup,
        ResetLevelPlayer,
        PauseGame,
        ResumeGame,

        PlayerRecover,
        RecoverToTraining,
        RecoverToMission,
    }

    /// <summary>
    /// Clase abstracta, encargada de la conexión entre jugadores y servidor
    /// </summary>
    public abstract class NetworkManager
    {
        protected MessageCodification coder;
        protected NetworkMessenger messenger;
        protected NetworkObjectParamsCoder netObjParamCoder;
            
        public NetworkManager()
        {
            coder = MessageCodification.getInstance();
            messenger = NetworkMessenger.getInstance();
            netObjParamCoder = new NetworkObjectParamsCoder();
            
            netObjParamCoder.AddType(typeof(ObjectState));
            netObjParamCoder.AddType(typeof(MetaDataObjectState));
            netObjParamCoder.AddType(typeof(PlayerState));
            netObjParamCoder.AddType(typeof(PlayerInfo));
            netObjParamCoder.AddType(typeof(LevelInfo));
            netObjParamCoder.AddType(typeof(LevelState));
            coder.addCoder(ParamsType.NetworkObjectParams, netObjParamCoder);
        }

        public abstract void Init();

        public virtual void Close()
        {
            messenger.Close();
        }

        public void AddNetworkObjectType(Type type)
        {
            netObjParamCoder.AddType(type);
        }

        #region Message Reception
        public abstract void HandleNetworkMessages();
        #endregion


    }
}
