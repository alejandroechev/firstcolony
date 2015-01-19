using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;


namespace NetworkModule
{
    /// <summary>
    /// Tipos de sockets
    /// </summary>
    enum UdpSocketType
    {
        Unicast, Receive, Broadcast, Multicast,
    }

    /// <summary>
    /// Clase que encapsula el funcionamiento de un socket UDP
    /// </summary>
    class UdpSocket
    {
        /// <summary>
        /// Constantes
        /// </summary>
        private int UdpLimit = 512; 
        private int MaxUdpBuffer = 64 * 1024; // 64KB
		
        /// <summary>
        /// Socket UDP
        /// </summary>
        private Socket m_sockUdpSocket;

        /// <summary>
        /// Tipo de socket asociado al objeto
        /// </summary>
        private UdpSocketType m_udpstType;

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
        /// Disponibilidad del Socket
        /// </summary>
        public int Available { get { return m_sockUdpSocket.Available; } }

        /// <summary>
        /// Parametros para seteo de non-blocking socket
        /// </summary>
        private readonly int FIONBIO = unchecked((int)0x8004667E);    //Constante para definir la opcion de Blocking de un socket usando IOControl
        private readonly int WSAEWOULDBLOCK = 10035;  

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stType">Tipo de socket</param>
        public UdpSocket(UdpSocketType stType)
        {
            m_sockUdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            m_udpstType = stType;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stType">Tipo de socket</param>
        /// <param name="nUdpLimit">Limite de sub paquetes Udp</param>
        /// <param name="nMaxUdpBuffer">Maximo tamaño de un paquete Udp</param>
        public UdpSocket(UdpSocketType stType, int nUdpLimit, int nMaxUdpBuffer)
        {
            m_sockUdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            m_udpstType = stType;
            UdpLimit = nUdpLimit;
            MaxUdpBuffer = nMaxUdpBuffer;
        }
        
        /// <summary>
        /// Envia mensaje UDP al Ip y puerto entregados
        /// </summary>
        /// <param name="byData">Mensaje a enviar</param>
        /// <param name="adrIP">Ip del destinatario</param>
        /// <param name="nPort">Puero asociado</param>
        public void send(byte[] byData, IPAddress adrIP, int nPort)
        {
            
            if (byData.Length > MaxUdpBuffer)
                throw new CUdpSocketException("Data too large", UdpSocketErrors.General);
            
            m_iepRemoteEndPoint = new IPEndPoint(adrIP, nPort);
                        
            try
            {
                int nTotalBytesSent = 0;
                while (nTotalBytesSent < byData.Length)
                {
                    // Si lo que falta es menos que UdpLimit, mandamos ese tamaño
                    int nSize = UdpLimit;
                    if ((byData.Length - nTotalBytesSent) < UdpLimit)
                        nSize = (byData.Length - nTotalBytesSent);
                    
                    // Enviamos info desde offest dado y con size dado
                    int nBytesSent = m_sockUdpSocket.SendTo(byData, nTotalBytesSent, nSize, SocketFlags.None, m_iepRemoteEndPoint);
                                        
                    nTotalBytesSent += nBytesSent;

                }
            }
            catch(SocketException se)
            {
                throw new CUdpSocketException(se.Message, se.ErrorCode, UdpSocketErrors.Send);
            }
        }

        /// <summary>
        /// Envía un mensaje a toda la red
        /// </summary>
        /// <param name="byData">Mensaje a enviar</param>
        /// <param name="adrNetwork">Direccion de la red</param>
        /// <param name="nPort">Puerto asociado</param>
        public void sendBroadcast(byte[] byData, IPAddress adrNetwork, int nPort)
        {
            m_sockUdpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                        
            send(byData, adrNetwork, nPort);
        }

        /// <summary>
        /// Asocia el socket a un endpoint local
        /// </summary>
        /// <param name="localEndPoint">End point local</param>
        public void bind(IPEndPoint localEndPoint)
        {
            try
            {
                m_iepLocalEndPoint = localEndPoint;

                m_sockUdpSocket.Bind(localEndPoint);
            }
            catch (SocketException se)
            {
                throw new CUdpSocketException(se.Message, se.ErrorCode, UdpSocketErrors.Bind);
            }
        }

        /// <summary>
        /// Función que recibe un arreglo de bytes por UDP
        /// </summary>
        /// <returns>Data recibida</returns>
        public byte[] receive()
        {
            IPEndPoint ipepRemote = new IPEndPoint(IPAddress.Any, 0);
            EndPoint epRemoteSender = (EndPoint)ipepRemote;

            // Obtenemos informacion, bloqueando hasta recibirla
            byte[] byData = new Byte[MaxUdpBuffer];
            int nTotalBytes = 0;
            int nBytes = 0;

            try
            {
                do
                {
                    nBytes = m_sockUdpSocket.ReceiveFrom(byData, nTotalBytes, UdpLimit, SocketFlags.None, ref epRemoteSender);
                    
                    nTotalBytes += nBytes;
                    if (nTotalBytes + UdpLimit > MaxUdpBuffer)
                        break; // No exceder buffer
                } while (m_sockUdpSocket.Available != 0);
            }
            catch (SocketException se)
            {
                throw new CUdpSocketException(se.Message, se.ErrorCode, UdpSocketErrors.Receive);
            }

            m_iepRemoteEndPoint = (IPEndPoint)epRemoteSender;

            return NetworkUtils.copyBuffer(byData, 0, nTotalBytes);
        }

        /// <summary>
        /// Cierra el socket
        /// </summary>
        public void close()
        {
            try
            {
                if (m_sockUdpSocket != null)
                {
                    m_sockUdpSocket.Shutdown(SocketShutdown.Both);
                    m_sockUdpSocket.Close();
                }
            }
            catch (SocketException se)
            {
                throw new CUdpSocketException(se.Message, se.ErrorCode, UdpSocketErrors.Close);
            }
        }

        /// <summary>
        /// Setea el socket en modo no bloqueante
        /// </summary>
        public void setNonBlocking()
        {
            //Seteamos al socket en modo no bloqueo (no funciona la propiedad Blocking)
            byte[] invalue = new byte[1];
            invalue[0] = 1;

            try
            {
                m_sockUdpSocket.Blocking = false;                
            }
            catch (SocketException se)
            {
                throw new CUdpSocketException(se.Message, se.ErrorCode, UdpSocketErrors.Settings);
            }

           
        }

    }

    #region CUdpSocketException
    /// <summary>
    /// Tipos de errores de sockets
    /// </summary>
    enum UdpSocketErrors
    {
        General, Bind, Send, Receive, Close, Settings
    }

    /// <summary>
    /// Clase que maneja excepciones de socket TCP
    /// </summary>
    class CUdpSocketException : ApplicationException
    {
        /// <summary>
        /// Mensaje de excepcion
        /// </summary>
        private string m_stringExcMsg;
        public string Message { get { return m_stringExcMsg; } }

        /// <summary>
        /// Codigo de excepcion
        /// </summary>
        private int m_nExcCode;
        public int Code { get { return m_nExcCode; } }

        /// <summary>
        /// Tipo de error
        /// </summary>
        private UdpSocketErrors m_udpseError;
        public UdpSocketErrors Error { get { return m_udpseError; } }

        /// <summary>
        /// Constructor: 3 parametros
        /// </summary>
        /// <param name="strMessage">Mensaje de excpecion</param>
        /// <param name="nCode">Codigo de excpecion</param>
        /// <param name="tcpseError">Tipo de excpecion</param>
        public CUdpSocketException(string strMessage, int nCode, UdpSocketErrors udpseError)
        {
            m_stringExcMsg = strMessage;
            m_nExcCode = nCode;
            m_udpseError = udpseError;
        }

        /// <summary>
        /// Constructor: 2 parametros, setea el codigo en -1
        /// </summary>
        /// <param name="strMessage">Mensaje de excpecion</param>
        /// <param name="tcpseError">Tipo de excpecion</param>
        public CUdpSocketException(string strMessage, UdpSocketErrors udpseError)
        {
            m_stringExcMsg = strMessage;
            m_nExcCode = -1;
            m_udpseError = udpseError;
        }


    }
    #endregion
}
