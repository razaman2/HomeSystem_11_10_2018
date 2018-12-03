using System;
using Crestron.SimplSharp;
using System.Collections.Generic;
using Crestron.SimplSharp.CrestronSockets;
using System.Text.RegularExpressions;
using System.Text;
using Crestron.SimplSharpPro.CrestronThread;

namespace HomeSystem_11_10_2018
{
    public class Socket
    {
        TCPServer server = new TCPServer(new IPEndPoint(IPAddress.Any, 3333), 4096, EthernetAdapterType.EthernetLANAdapter, 10);

        ControlSystem system;

        Dictionary<uint, TCPServer> clients = new Dictionary<uint, TCPServer>();

        public Socket(ControlSystem system)
        {
            this.system = system;
            server.SocketStatusChange += new TCPServerSocketStatusChangeEventHandler(server_SocketStatusChange);
            StartSubscriptions();
            server.WaitForConnectionAsync(AcceptCallback, null);
        }

        void server_SocketStatusChange(TCPServer server, uint clientIndex, SocketStatus status)
        {
            ErrorLog.Notice("Server Socket Status Is {0} - {1}", server.ServerSocketStatus, server.State);

            if ((server.ServerSocketStatus == SocketStatus.SOCKET_STATUS_SOCKET_NOT_EXIST) || (server.ServerSocketStatus == SocketStatus.SOCKET_STATUS_NO_CONNECT))
            {
                server.DisconnectAll();
                clients.Clear();
            }
        }

        public void AcceptCallback(TCPServer server, uint clientIndex, object obj)
        {
            try
            {
                if ((server.ServerSocketStatus == SocketStatus.SOCKET_STATUS_CONNECTED) && (clientIndex != 0))
                {
                    clients.Add(clientIndex, server);
                    server.ReceiveDataAsync(clientIndex, ReceiveCallback, null);
                    byte[] data = server.GetIncomingDataBufferForSpecificClient(clientIndex);
                    string message = "HTTP/1.1 101 Switching Protocols\r\n" + "Connection: Upgrade\r\n" + "Upgrade: websocket\r\n" + "Sec-WebSocket-Accept: " + Convert.ToBase64String(Crestron.SimplSharp.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(new System.Text.RegularExpressions.Regex("Sec-WebSocket-Key: (.*)").Match(Encoding.ASCII.GetString(data, 0, data.Length)).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11\r\n"))) + "\r\n";
                    //NotifyClients("Client " + clientIndex + " Was Successfully Connected To Server");
                    ErrorLog.Notice("Client Handshake {0}", message);
                    NotifyClients(message);
                }
                else if (server.State == ServerState.SERVER_NOT_LISTENING)
                {
                    server.WaitForConnectionAsync(AcceptCallback, null);
                }
                else if (server.State == ServerState.SERVER_LISTENING)
                {
                    ErrorLog.Notice("Server Is Currently Listening");
                }
            }
            catch (Exception ex)
            {
                ErrorLog.Notice("Something Went Wrong While Trying To Connect Client To Server\r\n{0}", ex.Message);
            }

            server.WaitForConnectionAsync(AcceptCallback, null);
        }

        public void StartSubscriptions()
        {
            try
            {
                system.TempControl.TempEvents += new EventHandler<TempEventArgs>(TempControl_TempEvents);
                system.GarageControl.GarageStateChange += new EventHandler<GarageEventArgs>(GarageControl_GarageStateChange);
            }
            catch (Exception ex)
            {
                ErrorLog.Notice("Something Went Wrong When Trying To Start Subscriptions\r\n{0}", ex.Message);
            }
        }

        public void SendCallback(TCPServer server, uint clientIndex, int bytesSent, object obj)
        {
            ErrorLog.Notice("{0} bytes were sent to client {1}", bytesSent, clientIndex);
        }

        public void ReceiveCallback(TCPServer server, uint clientIndex, int bytesReceived, object obj)
        {
            new Thread((o) => 
            {
                string message = Encoding.ASCII.GetString(server.GetIncomingDataBufferForSpecificClient(clientIndex), 0, bytesReceived);

                ErrorLog.Notice("server received {0} bytes from client\r\n{1}", bytesReceived, message);

                if (Regex.IsMatch(message, "^temp:ambient", RegexOptions.IgnoreCase))
                {
                    Match temp = Regex.Match(message, @"(?<=temp:ambient:)\d+", RegexOptions.IgnoreCase);
                    system.TempControl.ambient(int.Parse(temp.Value));
                }
                if (Regex.IsMatch(message, "^drop:client", RegexOptions.IgnoreCase))
                {
                    Match client = Regex.Match(message, @"(?<=drop:client:)\d+", RegexOptions.IgnoreCase);
                    server.Disconnect(uint.Parse(client.Value));
                }

                /*else if (Regex.IsMatch(message, "^temp:", RegexOptions.IgnoreCase))
                {
                    string action = Regex.Match(message, @"(?<=temp:)\w+", RegexOptions.IgnoreCase).Value;
                    system.TempControl.GetType().GetMethod(action).Invoke(system.TempControl, null);
                }*/
                /*else if (Regex.IsMatch(message, "^garage:", RegexOptions.IgnoreCase))
                {
                    string action = Regex.Match(message, @"(?<=garage:)\w+", RegexOptions.IgnoreCase).Value;
                    action = Regex.Replace(action, "open|close", "toggle");
                    system.GarageControl.GetType().GetMethod(action).Invoke(system.GarageControl, null);
                }*/

                switch (message)
                {
                    case "garage:open":
                    case "garage:close":
                        system.GarageControl.toggle();
                        break;
                    case "garage:release":
                        system.GarageControl.release();
                        break;
                    case "temp:raise":
                        system.TempControl.raise();
                        break;
                    case "temp:lower":
                        system.TempControl.lower();
                        break;
                    case "temp:on":
                        system.TempControl.on();
                        break;
                    case "temp:off":
                        system.TempControl.off();
                        break;
                }

                server.ReceiveDataAsync(clientIndex, ReceiveCallback, null);
                return clientIndex;
            }, null);
        }

        void TempControl_TempEvents(object sender, TempEventArgs e)
        {
            NotifyClients("Ambient: " + e.Ambient + ", Setpoint: " + e.Setpoint + ", Status: " + e.Status.ToString());
        }

        void GarageControl_GarageStateChange(object sender, GarageEventArgs e)
        {
            NotifyClients("Garage: " + e.Status.ToString());
        }

        public void NotifyClients(string data)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            try
            {
                new Thread((o) =>
                {
                    foreach (var client in clients)
                    {
                        if (server.ClientConnected(client.Key))
                        {
                            server.SendDataAsync(client.Key, buffer, buffer.Length, null);
                        }
                        else
                        {
                            ErrorLog.Notice("Client {0} Disconnection Status: {1}", client.Key, server.Disconnect(client.Key));
                            clients.Remove(client.Key);
                        }
                    }
                    return clients;
                }, null);
            }
            catch (Exception ex)
            {
                ErrorLog.Notice("Something Went Wrong While Trying To Send Data To Client\r\n{1}", ex.Message);
            }
        }
    }
}