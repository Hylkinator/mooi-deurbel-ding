﻿using ProjectApp.Views;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ProjectApp.Server
{
    class Connection
    {

        private IPAddress ipAddressServer;
        private int port;
        private IPEndPoint remoteEP;

        private Socket socket;


        public static string ErrorMessage = "";

        /// <summary>
        /// Returns a Connection object. You still have to manually invoke StartConnection() to establish a connection. 
        /// </summary>
        public Connection()
        {
            socket = null;
        }

        /// <summary>
        /// Attempts to establish a connection with the Arduino server.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when: ipAddress is valid, port is invalid, or a connection could not be made.</exception>
        /// <param name="ipAddress">The IP address of the server.</param>
        /// <param name="port">The port of the server.</param>
        public void StartConnection(string ipAddress, string port)
        {
            if (socket != null)
            {
                return;
            }

            if (CheckValidIpAddress(ipAddress) == false)
            {
                throw new ArgumentException("Invalid IP Address");
            }

            if (CheckValidPort(port) == false)
            {
                throw new ArgumentException("Invalid Port");
            }

            ipAddressServer = IPAddress.Parse(ipAddress);
            this.port = Convert.ToInt32(port);
            remoteEP = new IPEndPoint(ipAddressServer, this.port);

            try
            {
                socket = new Socket(ipAddressServer.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(remoteEP);
                ExecuteCommand("c");
            }
            catch
            {
                throw new ArgumentException("Could not connect to server");
            }
        }

        /// <summary>
        /// Closes the connection with the Arduino server.
        /// </summary>
        public void CloseConnection()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        /// <summary>
        /// Returns whether the socket is connected or not. 
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            if (socket == null)
                return false;

            else return socket.Connected;
        }

        /// <summary>
        /// Executes a command by sending a message to the server and returning the response from the server (with white spaces trimmed). Returns "error" if something went wrong. 
        /// </summary>
        /// <param name="command">The command to send to the server/</param>
        /// <returns></returns>
        public string ExecuteCommand(string command, bool receiveData = true)
        {

            // Assuming response is always 4 bytes
            byte[] buffer = new byte[4];
            int bytesReceived = 0;
            string result = "error";

            if (socket != null)
            {
                SendMessage(command);
                try
                {
                    if (receiveData)
                    {
                        bytesReceived = socket.Receive(buffer);

                        if (bytesReceived == 4)
                        {
                            // -1 to skip \n, Trim() to trim white spaces at the start since it's always 4 characters (though whitespaces later got removed so probably unnecessary now)
                            result = Encoding.ASCII.GetString(buffer, 0, bytesReceived - 1).Trim();
                        }
                    }
                    else
                    {
                        result = "Success";
                    }
                }
                catch
                {
                    result = "error";
                }
            }

            return result;
        }


        
        /*public async Task<bool> */ public bool SendSomething()
        {
            byte[] bytes = new byte[1024];
            AsyncCallback test = new AsyncCallback(ProcessClientInformation);

            object secondTest = new object();

            if (socket != null)
            {
                try
                {

                    socket.BeginReceive(bytes, 0, 0, SocketFlags.None, test, secondTest);

                    //SocketAsyncEventArgs asyncEventArgs = new SocketAsyncEventArgs();

                    //asyncEventArgs.SetBuffer(bytes, 0, 0);
                    //asyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Help);

                    //string data = null;
                    //bool isBusy = true;

                    //var test = socket.ReceiveAsync(asyncEventArgs);

                    /*		test	(null)	System.AsyncCallback


                    while (isBusy)
                    {
                        isBusy = socket.ReceiveAsync(asyncEventArgs);
                        
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                        if (data.IndexOf("BEL") > -1)
                            break;
                        
                    }
                    */

                    
                    //CloseConnection();

                    return true;
                }
                catch (Exception e)
                {
                    ErrorMessage = e.ToString();
                    return false;
                }
            }

            return false;
        }

        void ProcessClientInformation(IAsyncResult result)
        {
            int bytesRec = socket.EndReceive(result);

            byte[] bytes = new byte[1024];
            var data = Encoding.ASCII.GetString(bytes, 0, bytesRec);

            var test = data.ToString();
            ExecuteCommand("r", false);
        }

        // ===================================================
        // ================ PRIVATE FUNCTIONS ================
        // ===================================================

        /// <summary>
        /// Sends a message to the Arduino server, encoded in such a way that messages always end with ">".
        /// </summary>
        /// <param name="message">The message to send.</param>
        private void SendMessage(string message)
        {
            // Messages always end with ">"
            byte[] msgAsBytes = Encoding.ASCII.GetBytes(message + ">");
            socket.Send(msgAsBytes);
        }

        /// <summary>
        /// Returns whether an IP address is valid or not. Code from Sibbele Oosterhaven.
        /// </summary>
        /// <param name="ip">The IP address to check</param>
        /// <returns></returns>
        private bool CheckValidIpAddress(string ip)
        {
            if (ip == "")
                return false;

            //Check user input against regex
            Regex regex = new Regex("\\b((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\\.|$)){4}\\b");
            Match match = regex.Match(ip);
            return match.Success;
        }

        /// <summary>
        /// Returns whether a port is a valid port. Code from Sibbele Oosterhaven. 
        /// </summary>
        /// <param name="port">The port to check</param>
        /// <returns></returns>
        private bool CheckValidPort(string port)
        {
            //Check if a value is entered.
            if (port == "") 
                return false;

            Regex regex = new Regex("[0-9]+");
            Match match = regex.Match(port);

            if (match.Success)
            {
                int portAsInteger = Int32.Parse(port);

                //Check if port is in range.
                return ((portAsInteger >= 0) && (portAsInteger <= 65535));
            }
            else return false;
        }
    }
}
