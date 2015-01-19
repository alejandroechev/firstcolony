using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using Lidgren.Network;

namespace NetworkModule
{
    /// <summary>
    /// A callback/delegate function for client connection event
    /// </summary>
    /// <param name="clientIP">The IP address of the client that got connected</param>
    public delegate void HandleClientConnection(String clientIP);

    /// <summary>
    /// A callback/delegate function for client disconnection event
    /// </summary>
    /// <param name="clientIP">The IP address of the client that got disconnected</param>
    public delegate void HandleClientDisconnection(String clientIP);

    public class NetworkServer
    {
         #region Member Fields

        protected int portNumber;
        protected byte[] myIPAddress;
        protected bool enableEncryption;
        protected String appName;
        protected NetConfiguration netConfig;
        protected NetBuffer buffer;
        protected NetServer netServer;
        protected Dictionary<String, String> approveList;

        protected NetConnection prevSender;
        public IPAddress PrevSender { get { return prevSender.RemoteEndpoint.Address; } }
        public string PrevSenderString { get { return prevSender.RemoteEndpoint.ToString(); } }
        protected Dictionary<String, NetConnection> clients;

        #endregion

        #region Events

        public event HandleClientConnection ClientConnected;
        public event HandleClientDisconnection ClientDisconnected;

        #endregion

        #region Properties

        public int PortNumber
        {
            get { return portNumber; }
            set 
            {
                if (portNumber != value)
                {
                    Shutdown();
                    portNumber = value;
                    Initialize();
                }
            }
        }

        public byte[] MyIPAddress
        {
            get { return myIPAddress; }
        }

        public int NumConnectedClients
        {
            get { return clients.Count; }
        }

        public List<String> ClientIPAddresses
        {
            get
            {
                List<String> ipAddresses = new List<string>();
                foreach (String ipAddress in clients.Keys)
                    ipAddresses.Add(ipAddress);

                return ipAddresses;
            }
        }

        public bool EnableEncryption
        {
            get { return enableEncryption; }
            set { enableEncryption = value; }
        }

        #endregion 

        #region Constructors
        /// <summary>
        /// Creates a Lidgren network server with an application name and the port number
        /// to establish the connection.
        /// </summary>
        /// <param name="appName">An application name. Can be any names</param>
        /// <param name="portNumber">The port number to establish the connection</param>
        public NetworkServer(String appName, int portNumber)
        {
            this.portNumber = portNumber;
            this.appName = appName;
            enableEncryption = false;
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress addr = ipEntry.AddressList[0];
            myIPAddress = addr.GetAddressBytes();
            approveList = new Dictionary<string, string>();
            prevSender = null;
            clients = new Dictionary<string, NetConnection>();
        }

        public NetworkServer()
        {
            this.portNumber = 11000;
            this.appName = "Game";
            enableEncryption = false;
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress addr = ipEntry.AddressList[0];
            myIPAddress = addr.GetAddressBytes();
            approveList = new Dictionary<string, string>();
            prevSender = null;
            clients = new Dictionary<string, NetConnection>();
        }

        #endregion

        #region Public Methods

        public void Initialize()
        {
            // Create a net configuration
            netConfig = new NetConfiguration(appName);
            netConfig.MaxConnections = 32;
            netConfig.Port = portNumber;

            // enable encryption; this key was generated using the 'GenerateEncryptionKeys' application
            if (enableEncryption)
            {
                // No encryption mechanism for latest Lidgren library
            }

            // Create a server
            netServer = new NetServer(netConfig);
            netServer.SetMessageTypeEnabled(NetMessageType.ConnectionApproval, true);
            netServer.Start();

            buffer = netServer.CreateBuffer();
        }

        public void BroadcastMessage(byte[] msg, bool reliable, bool inOrder, bool excludeSender)
        {
            // Test if any connections have been made to this machine, then send data
            if (clients.Count > 0)
            {
                // create new message to send to all clients
                NetBuffer buf = netServer.CreateBuffer();
                buf.Write(msg);

                //Log.Write("Sending message: " + msg.ToString(), Log.LogLevel.Log);
                //Console.WriteLine("Sending message: " + ByteHelper.ConvertToString(msg));

                NetChannel channel = NetChannel.Unreliable;
                if (reliable)
                {
                    if (inOrder)
                        channel = NetChannel.ReliableInOrder1;
                    else
                        channel = NetChannel.ReliableUnordered;
                }
                else if (inOrder)
                    channel = NetChannel.UnreliableInOrder1;

                // broadcast the message in order
                if (excludeSender && (prevSender != null))
                {
                    // if there is only one connection, then the sender to be excluded is
                    // the only connection the server has, so there is no point to broadcast
                    if (clients.Count > 1)
                        netServer.SendToAll(buf, channel, prevSender);
                }
                else
                    netServer.SendToAll(buf, channel);
            }
        }

        public void SendMessage(byte[] msg, List<string> ipAddresses, bool reliable, bool inOrder)
        {
            // Test if any connections have been made to this machine, then send data
            if (clients.Count > 0)
            {
                // create new message to send to all clients
                NetBuffer buf = netServer.CreateBuffer();
                buf.Write(msg);

                //Log.Write("Sending message: " + msg.ToString(), Log.LogLevel.Log);
                //Console.WriteLine("Sending message: " + ByteHelper.ConvertToString(msg));

                NetChannel channel = NetChannel.Unreliable;
                if (reliable)
                {
                    if (inOrder)
                        channel = NetChannel.ReliableInOrder1;
                    else
                        channel = NetChannel.ReliableUnordered;
                }
                else if (inOrder)
                    channel = NetChannel.UnreliableInOrder1;

                List<NetConnection> recipients = new List<NetConnection>();
                foreach (String ipAddress in ipAddresses)
                    if (clients.ContainsKey(ipAddress))
                        recipients.Add(clients[ipAddress]);

                if (recipients.Count > 0)
                    netServer.SendMessage(buf, recipients, channel);
            }
        }

        public List<byte[]> ReceiveMessage()
        {
            List<byte[]> messages = new List<byte[]>();

            NetMessageType type;
            NetConnection sender;

            // read a packet if available
            while (netServer.ReadMessage(buffer, out type, out sender))
            {
                switch (type)
                {
                    case NetMessageType.ConnectionApproval:
                        if (!approveList.ContainsKey(sender.RemoteEndpoint.ToString()))
                        {
                            sender.Approve();
                            if(!approveList.ContainsKey(sender.RemoteEndpoint.ToString()))
                                approveList.Add(sender.RemoteEndpoint.ToString(), "");
                        }
                        break;
                    case NetMessageType.StatusChanged:
                        if (sender.Status == NetConnectionStatus.Connected)
                        {
                            byte[] data = ByteHelper.ConvertToByte("NewConnectionEstablished");
                            byte[] size = BitConverter.GetBytes((short)data.Length);
                            messages.Add(ByteHelper.ConcatenateBytes(size, data));
                            if(!clients.ContainsKey(sender.RemoteEndpoint.ToString()))
                            {
                                clients.Add(sender.RemoteEndpoint.ToString(), sender);
                                approveList.Remove(sender.RemoteEndpoint.ToString());
                                if (sender != null)
                                    prevSender = clients[sender.RemoteEndpoint.ToString()];
                                if (ClientConnected != null)
                                    ClientConnected(sender.RemoteEndpoint.ToString());
                            }
                        }
                        else if (sender.Status == NetConnectionStatus.Disconnected)
                        {
                            clients.Remove(sender.RemoteEndpoint.ToString());
                            if (ClientDisconnected != null)
                                ClientDisconnected(sender.RemoteEndpoint.ToString());
                        }

                        break;
                    case NetMessageType.Data:
                        messages.Add(buffer.ToArray());
                        if (sender != null)
                            prevSender = clients[sender.RemoteEndpoint.ToString()];
                        break;
                }
            }

            return messages;
        }

        public void Shutdown()
        {
            // shutdown; sends disconnect to all connected clients with this reason string
            netServer.Shutdown("Application exiting");
        }

        #endregion
    }
}
