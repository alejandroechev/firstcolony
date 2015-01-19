using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Collections;


namespace NetworkModule
{

    public class NetworkMessenger
    {
        private int m_nUdpCommPort = 13000;
                
        /// <summary>
        /// Socket UDP utilizado para envio
        /// </summary>
        private UdpSocket m_udpsockSendSocket;

        /// <summary>
        /// Socket UDP utilizado para recepcion
        /// </summary>
        private UdpSocket m_udpsockReceiveSocket;

        /// <summary>
        /// IP local
        /// </summary>
        private IPAddress m_ipLocalIP;
                
        /// <summary>
        /// Abstraccion del Ip propio
        /// </summary>
        public IPAddress MyIP { get { return m_ipLocalIP; } }

        private IPAddress m_ipRemoteIP;
        public IPAddress RemoteIP { get { return m_ipRemoteIP; } }
                
        /// <summary>
        /// Manejo de singleton de la clase
        /// </summary>
        private static NetworkMessenger Messenger = null;
        public static NetworkMessenger getInstance()
        {
            if (Messenger == null)
                Messenger = new NetworkMessenger();

            return Messenger;
        }
                
        /// <summary>
        /// Constructor
        /// </summary>
        private NetworkMessenger()
        {
        }

        /// <summary>
        /// Funcion de inicialicacion del singleton
        /// </summary>
        /// <param name="ipLocal">Ip Local asociado</param>
        /// <param name="ipNetwork">Ip de la red local</param>
        public void Init()
        {
            m_ipLocalIP = IPAddress.Any;
            m_udpsockSendSocket = new UdpSocket(UdpSocketType.Broadcast);
            m_udpsockReceiveSocket = new UdpSocket(UdpSocketType.Receive);
            m_udpsockReceiveSocket.setNonBlocking();
            m_udpsockReceiveSocket.bind(new IPEndPoint(m_ipLocalIP, m_nUdpCommPort));
        }

        public void Close()
        {
            m_udpsockSendSocket.close();
        }
           
        /// <summary>
        /// Envia datos mediante conexion udp
        /// </summary>
        /// <param name="bData"> datos a enviar</param>
        /// <param name="sIP">IP del destinatario</param>
        public void SendBroadcastData(byte[] bData)
        {
            m_udpsockSendSocket.sendBroadcast(bData, IPAddress.Broadcast, m_nUdpCommPort);
        }

        public List<byte[]> ReceiveData()
        {
            List<byte[]> messages = new List<byte[]>();
            if (m_udpsockReceiveSocket.Available > 0)
            {
                byte[] buffer = m_udpsockReceiveSocket.receive();
                if (buffer.Length > 0)
                {
                    messages.Add(buffer);
                    m_ipRemoteIP = m_udpsockReceiveSocket.RemoteIP;
                }
            }
            return messages;
        }       
    }

}
