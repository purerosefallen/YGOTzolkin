using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using YGOTzolkin.Utility;
using YGOTzolkin.YGOModel;

namespace YGOTzolkin.Service
{
    public class NetworkService
    {
        #region singleton
        private static NetworkService instance = null;
        private static readonly object lockObject = new object();
        private NetworkService()
        {
            InstantQueue = new BlockingCollection<byte[]>();
            DelayedQueue = new BlockingCollection<byte[]>();
        }
        public static NetworkService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new NetworkService();
                        }
                        return instance;
                    }
                }
                return instance;
            }
        }
        #endregion

        public static BlockingCollection<byte[]> InstantQueue { get; private set; }

        public static BlockingCollection<byte[]> DelayedQueue { get; private set; }

        public bool Connected { get; private set; }

        private Socket clientSocket;
        private NetworkStream stream;
        private Thread receiveThread;

        public bool JoinServer(string hostAddress, int port, string playerName, string roomCode = null)
        {
            if (Connected || hostAddress == null || playerName == null)
            {
                return false;
            }
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(hostAddress, port);
                stream = new NetworkStream(clientSocket);
                Connected = true;
            }
            catch (Exception e)
            {
                Connected = false;
                Debug.Log(e.Message + e.GetType() + e.StackTrace);
                return false;
            }
            MainGame.Instance.Duel.Start();
            receiveThread = new Thread(Receive);
            receiveThread.Start();
            if (playerName.Length > 20)
            {
                playerName = playerName.Substring(0, 20);
            }
            //playerinfo
            byte[] result = new byte[43];
            BinaryWriter writer = new BinaryWriter(new MemoryStream(result));
            writer.Write((ushort)41);
            writer.Write((byte)CToSMessage.PlayerInfo);
            writer.Write(ByteExtension.StringToUnicode(playerName, 40));
            Send(result);
            //joingame
            byte[] data = new byte[51];
            writer = new BinaryWriter(new MemoryStream(data));
            writer.Write((ushort)49);
            writer.Write((byte)CToSMessage.JoinGame);
            writer.Write((int)Config.YGOVersion);
            writer.Write((uint)0);
            if (roomCode == null)
            {
                roomCode = string.Empty;
            }
            writer.Write(ByteExtension.StringToUnicode(roomCode, 40));
            Send(data);
            return true;
        }

        private byte[] ReadStream(int length)
        {
            byte[] data = new byte[length];
            int size = 0;
            while (size < length)
            {
                int rl = stream.Read(data, size, length - size);
                size += rl;
                if (rl == 0)
                {
                    Connected = false;
                    break;
                }
            }
            return data;
        }

        private void Receive()
        {
            while (Connected)
            {
                try
                {
                    int length = BitConverter.ToUInt16(ReadStream(2), 0);
                    byte[] data = ReadStream(length);
                    if (data != null && data.Length > 0)
                    {
                        if (data[0] == (byte)SToCMessage.Chat)
                        {
                            InstantQueue.Add(data);
                        }
                        else
                        {
                            DelayedQueue.Add(data);
                        }
                    }
                    else
                    {
                        Connected = false;
                        break;
                    }
                }
                catch (Exception)
                {
                    Connected = false;
                    break;
                }
            }
        }

        public void Send(byte[] data)
        {
            try
            {
                clientSocket.Send(data);
            }
            catch (Exception e)
            {
                Debug.Log("-->send exception" + e.Message);
            }
        }

        public void Send(CToSMessage type)
        {
            Send(new byte[]
                 {
                    0x01,
                    0x00,
                    (byte)type,
                 });
        }

        public void Disconnect()
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Disconnect(true);
            Connected = false;
        }
    }
}
