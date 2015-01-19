using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetworkModule
{
    #region CTcpSocket 
    /// <summary>
    /// Tipos de sockets
    /// </summary>
    public enum TcpSocketType
    {
        Send, Receive, Listen
    }

    /// <summary>
    /// Clase que encapsula el funcionamiento de un socket TCP
    /// </summary>
    class TcpSocket
    {
        /// <summary>
        /// Objeto socket 
        /// </summary>
        private Socket m_sockTCPSocket;

        
        /// <summary>
        /// Host local asociado a este socket
        /// </summary>
        private IPEndPoint m_iepLocalEndPoint;

        /// <summary>
        /// Host remoto asociado a este socket
        /// </summary>
        private IPEndPoint m_iepRemoteEndPoint;

        public IPAddress RemoteIP { get { return m_iepRemoteEndPoint.Address; } }

        /// <summary>
        /// Número de bytes enviados en ultima conexion
        /// </summary>
        private int m_nLastDataSize;

        /// <summary>
        /// Maximo tamaño de la cola de conexiones simultaneas que se pueden escuchar
        /// </summary>
        private readonly int m_nMaxListenQueueSize = 100;

        /// <summary>
        /// Tamaño maximo de mensaje TCP 64K
        /// </summary>
        private readonly int m_nMaxBufferSize = 64 * 1024;

        /// <summary>
        /// Tiempo de timeouts
        /// </summary>
        private readonly int m_nSendTimeOut = 500;
               
        /// <summary>
        /// Constructor: crea un nuevo socket
        /// </summary>
        public TcpSocket()
        {
            m_sockTCPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //m_sockTCPSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, m_nSendTimeOut);

        }

        /// <summary>
        /// Constructor: se asocia al socket recibido
        /// </summary>
        /// <param name="sIP">Socket a asociarse</param>
        public TcpSocket(Socket sockTCP, IPEndPoint endPoint)
        {
            m_sockTCPSocket = sockTCP;
            m_iepRemoteEndPoint = endPoint;
        }

        /// <summary>
        /// Retorna verdadero si el socket esta conectado
        /// </summary>
        /// <returns>Verdadero si hay conexion</returns>
        public bool isConnected()
        {
            return m_sockTCPSocket.Connected;
        }

        /// <summary>
        /// Conecta el socket al IP y puerto dados
        /// </summary>
        /// <param name="adrIP">IP remoto</param>
        /// <param name="nPort">Puerto remoto</param>
        public bool connect(IPAddress adrIP, int nPort)
        {
            m_iepRemoteEndPoint = new IPEndPoint(adrIP, nPort);
            m_sockTCPSocket.Connect(m_iepRemoteEndPoint);
            return true;
            
        }        

        private ManualResetEvent m_mrWaitObject = new ManualResetEvent(false);
        private bool m_bConnectionSuccess = true;
        
        public bool connect(IPAddress adrIP, int nPort, int timeoutMSec)
        {
            m_iepRemoteEndPoint = new IPEndPoint(adrIP, nPort);            
            m_mrWaitObject.Reset();
            IAsyncResult iar = m_sockTCPSocket.BeginConnect(m_iepRemoteEndPoint, new AsyncCallback(connectCallBack), m_sockTCPSocket);
            if (m_mrWaitObject.WaitOne(timeoutMSec, false))
                return m_bConnectionSuccess;
            else
                return false;
        }

        private void connectCallBack(IAsyncResult ar)
        {
            try
            {
                Socket client = ar.AsyncState as Socket;
                if (client != null)
                {
                    client.EndConnect(ar);
                }                
            }
            catch (Exception)
            {
                m_bConnectionSuccess = false;
            }
            finally
            {
                m_mrWaitObject.Set();
            }
        } 

        /// <summary>
        /// Envia data mediante conexion TCP
        /// </summary>
        /// <param name="bData">Data a enviar</param>
        public void send(byte[] bData)
        {            
            m_nLastDataSize = m_sockTCPSocket.Send(bData, 0, bData.Length, SocketFlags.None);
            
            if (m_nLastDataSize < bData.Length)
                throw new CTCPSocketException("Full message not sent", TcpSocketErrors.Send);

        }

        /// <summary>
        /// Setea a un socket para que escuche
        /// </summary>
        /// <param name="iepLocalEndPoint">End point local</param>
        public void listen(IPEndPoint iepLocalEndPoint)
        {
            m_iepLocalEndPoint = iepLocalEndPoint;
            m_sockTCPSocket.Bind(m_iepLocalEndPoint);
            m_sockTCPSocket.Listen(m_nMaxListenQueueSize);
        }

        /// <summary>
        /// Accepta una conexion creando un Socket de recepcion
        /// </summary>
        /// <returns>Socket TCP de recepcion</returns>
        public TcpSocket accept()
        {

            Socket sockTcp = null;
                        
            if (m_sockTCPSocket.LocalEndPoint == null)
                throw new CTCPSocketException("Socket not bound", TcpSocketErrors.Accept);
            
            sockTcp = m_sockTCPSocket.Accept();
            m_iepRemoteEndPoint = (IPEndPoint)sockTcp.RemoteEndPoint;
            
            return new TcpSocket(sockTcp, m_iepRemoteEndPoint);
        }

        /// <summary>
        /// Recibe data de la conexion tcp
        /// </summary>
        /// <returns>Arreglo de bytes con data recibida</returns>
        public byte[] receive()
        {
            byte[] buffer = new byte[m_nMaxBufferSize];
                        
            m_nLastDataSize = m_sockTCPSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                
            return NetworkUtils.copyBuffer(buffer, 0, m_nLastDataSize);
            
        }

        
        /// <summary>
        /// Cierra el socket
        /// </summary>
        public void close()
        {
            if (m_sockTCPSocket != null)
            {
                m_sockTCPSocket.Shutdown(SocketShutdown.Both);
                m_sockTCPSocket.Close();
            }          
        }
                
    }

    #endregion

    #region CTcpSocketException
    /// <summary>
    /// Tipos de errores de sockets
    /// </summary>
    enum TcpSocketErrors
    {
        General, Connect, Listen, Accept, Send, Receive, Close,
    }

    /// <summary>
    /// Clase que maneja excepciones de socket TCP
    /// </summary>
    class CTCPSocketException : ApplicationException
    {
        /// <summary>
        /// Mensaje de excepcion
        /// </summary>
        private string  m_stringExcMsg;
        
        /// <summary>
        /// Codigo de excepcion
        /// </summary>
        private int m_nExcCode;
        
        /// <summary>
        /// Tipo de error
        /// </summary>
        private TcpSocketErrors m_tcpseError;

        public string Message { get { return m_stringExcMsg; } }

        public int Code { get { return m_nExcCode; } }

        public TcpSocketErrors Error { get { return m_tcpseError; } }
        
        /// <summary>
		/// Constructor: 3 parametros
		/// </summary>
		/// <param name="strMessage">Mensaje de excpecion</param>
        /// <param name="nCode">Codigo de excpecion</param>
        /// <param name="tcpseError">Tipo de excpecion</param>
        public CTCPSocketException(string strMessage, int nCode, TcpSocketErrors tcpseError)
		{
            m_stringExcMsg = strMessage;
            m_nExcCode = nCode;
            m_tcpseError = tcpseError;
		}

        /// <summary>
        /// Constructor: 2 parametros, setea el codigo en -1
        /// </summary>
        /// <param name="strMessage">Mensaje de excpecion</param>
        /// <param name="tcpseError">Tipo de excpecion</param>
        public CTCPSocketException(string strMessage, TcpSocketErrors tcpseError)
        {
            m_stringExcMsg = strMessage;
            m_nExcCode = -1;
            m_tcpseError = tcpseError;
        }


    }
    #endregion
}
