using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace NetworkModule
{
    public enum ParamsType
    {
        NoParams = 0,
        IntParams,
        StringParams,
        NetworkObjectParams,
    }

    public class MessageCodification
    {
        private Dictionary<ParamsType, IParamsCoder> m_dicParamsCoders;
        protected static readonly int HEADER_SIZE = 2; //bytes
        protected static readonly int PARAM_TYPE_SIZE = 1; //bytes

        protected static readonly int MESSAGE_ID_SIZE = 1; //bytes
        
        private static MessageCodification MessageCoder = null;
        public static MessageCodification getInstance()
        {
            if (MessageCoder == null)
                MessageCoder = new MessageCodification();
            return MessageCoder;
        }

        private MessageCodification()
        {
            m_dicParamsCoders = new Dictionary<ParamsType, IParamsCoder>();
            m_dicParamsCoders.Add(ParamsType.IntParams, new IntParamsCoder());
            m_dicParamsCoders.Add(ParamsType.StringParams, new StringParamsCoder());

        }

        public void addCoder(ParamsType nType, IParamsCoder iCoder)
        {
            m_dicParamsCoders.Add(nType, iCoder);
        }

        #region Decoding
        public Message decode(byte[] data)
        {
            int nMesssageId = NetworkUtils.decodeInt16(data, 0, MESSAGE_ID_SIZE);
            ParamsType pType = (ParamsType)NetworkUtils.decodeInt16(data, MESSAGE_ID_SIZE, PARAM_TYPE_SIZE);
                        
            if (pType == ParamsType.NoParams)
                return new Message(nMesssageId);
            if (!m_dicParamsCoders.ContainsKey(pType))
                throw new Exception("Coder: " + pType + " not implemented");
            byte[] baParams = new byte[data.Length - HEADER_SIZE];
            NetworkUtils.copyPartialByteArray(data, baParams, HEADER_SIZE, 0, baParams.Length);
            Params msgParams = m_dicParamsCoders[pType].decode(baParams);

            return new Message(nMesssageId, msgParams);
        }        

        #endregion

        #region Encoding
        public byte[] encode(Message msg)
        {
            byte[] data = null;
            if (msg.MsgParams.Type != ParamsType.NoParams)
            {
                if (!m_dicParamsCoders.ContainsKey(msg.MsgParams.Type))
                    throw new Exception("Coder: " + msg.MsgParams.Type + " not implemented");
                byte[] baParams = m_dicParamsCoders[msg.MsgParams.Type].encode(msg.MsgParams);
                data = new byte[baParams.Length + HEADER_SIZE];
                NetworkUtils.copyPartialByteArray(baParams, data, 0, HEADER_SIZE, baParams.Length);
            }
            else
                data = new byte[HEADER_SIZE];
            
            byte[] msgIdData = BitConverter.GetBytes((short)msg.MessageId);
            NetworkUtils.copyPartialByteArray(msgIdData, data, 0, 0, MESSAGE_ID_SIZE);
            
            byte[] paramTypeData = BitConverter.GetBytes((short)msg.MsgParams.Type);
            NetworkUtils.copyPartialByteArray(paramTypeData, data, 0, MESSAGE_ID_SIZE, PARAM_TYPE_SIZE);            

            return data;
        }
        #endregion       
        
    }

    public class Message 
    {
       private int m_nMessageId;
        public int MessageId { get { return m_nMessageId; } }

        private Params m_pParams;
        public Params MsgParams { get { return m_pParams; } }

        public Message(int nMessageId)
        {
            m_nMessageId = nMessageId;
            m_pParams = new Params(ParamsType.NoParams);
        }

        public Message(int nMessageId, Params pParams)
        {
            m_nMessageId = nMessageId;
            m_pParams = pParams;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Message))
                throw new Exception("Objeto comparado no es un mensaje");
            Message other = obj as Message;

            if (this.m_nMessageId != other.m_nMessageId)
                return false;
            if (this.m_pParams.Type != other.m_pParams.Type || this.m_pParams.ParamList.Count != other.m_pParams.ParamList.Count)
                return false;

            for (int i = 0; i < this.m_pParams.ParamList.Count; i++)
            {
                if (!this.m_pParams.ParamList[i].Equals(other.m_pParams.ParamList[i]))
                    return false;
            }

            return true;
        }
    }

    public class Params
    {
        private ParamsType m_nType;
        public ParamsType Type { get { return m_nType; } }

        private ArrayList m_alParams;
        public ArrayList ParamList { get { return m_alParams; } }

        public Params(ParamsType nType)
        {
            m_nType = nType;
            m_alParams = new ArrayList();
        }

        public Params() : this(ParamsType.IntParams)
        {

        }

        public void addParam(object oParam)
        {
            m_alParams.Add(oParam);
        }

    }

    public interface IParamsCoder
    {

        byte[] encode(Params param);
        Params decode(byte[] data);

    }

    class IntParamsCoder : IParamsCoder
    {
        private static readonly int INT_SIZE = 4;

        #region IParamsCoder Members

        public byte[] encode(Params param)
        {
            byte[] data = new byte[param.ParamList.Count * INT_SIZE];

            for (int i = 0; i < param.ParamList.Count; i++)
            {
                byte[] baParam = BitConverter.GetBytes((int)param.ParamList[i]);
                NetworkUtils.copyPartialByteArray(baParam, data, 0, INT_SIZE * i, INT_SIZE);
            }
            return data;
        }

        public Params decode(byte[] data)
        {
            Params p = new Params(ParamsType.IntParams);
            for (int i = 0; i < data.Length; i += INT_SIZE)
            {
                int n = BitConverter.ToInt32(data, i);
                p.addParam(n);
            }
            return p;
        }

        #endregion
    }

    class StringParamsCoder : IParamsCoder
    {
        private static char ParameterDelimiter = '#';
		
        #region IParamsCoder Members

        public byte[] encode(Params param)
        {
            string sParams = "";
            for (int i = 0; i < param.ParamList.Count-1; i++)
            {
                sParams += param.ParamList[i].ToString() + ParameterDelimiter;
            }
            sParams += param.ParamList[param.ParamList.Count - 1];
            byte[] data = System.Text.Encoding.UTF8.GetBytes(sParams);
            return data;
        }

        public Params decode(byte[] data)
        {
            Params p = new Params(ParamsType.StringParams);
            string sParams = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
            string[] sParamsArray = sParams.Split(ParameterDelimiter);
            for (int i = 0; i < sParamsArray.Length; i ++)
            {
                p.addParam(sParamsArray[i]);
            }
            return p;
        }

        #endregion
    }   

    
}
