﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Forms;

namespace ProjectApp.Server
{
    sealed class ArduinoHandler
    {
        // Singleton instance
        private static readonly ArduinoHandler HANDLER = new ArduinoHandler();

        /// <summary>Interval time in milliseconds to refresh the statuses.</summary>
        private const double refreshIntervalMilliseconds = 1000;

        /// <summary>Event that gets fired when the status Refreshes which the GUI can listen to.</summary>
        public event EventHandler StatusRefreshedEvent;

        /// <summary>
        /// Returns the static ArduinoHandler Singleton instance.
        /// </summary>
        public static ArduinoHandler Handler
        {
            get
            {
                return HANDLER;
            }
        }

        /// <summary>ArduinoStatus object that can be used as a DataBinding object for the user interface to automatically update and display data.</summary>
        private Connection connection;

        public ProgramStatus Status { get; private set; }

        /// <summary>Returns an ArduinoHandler object. Invoke StartConnection() to start a connection with the server.</summary>
        private ArduinoHandler()
        {
            connection = new Connection();
            Status = new ProgramStatus();
            OnStartup();
        }

        /// <summary>
        /// Attempts to establish a connection with the Arduino server and returns whether it went succesful or not.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when: ipAddress is valid, port is invalid, or a connection could not be made.</exception>
        /// <param name="ipAddress">The IP address of the server.</param>
        /// <param name="port">The port of the server.</param>
        /// <returns></returns>
        public bool StartConnection(string ipAddress, string port)
        {
            if (connection.IsConnected())
                return false;

            // Try to start connection
            try
            {
                // Start connection
                connection.StartConnection(ipAddress, port);

                // Initialize values
                OnStartup();

                // Set refresh timer
                Timer timer = new Timer(refreshIntervalMilliseconds);
                timer.Elapsed += (obj, args) => RefreshStatus(true);
                timer.Start();
            }

            // Something went wrong (like invalid IP or port)
            catch (Exception e)
            {
                Console.WriteLine("Could not connect because:");
                Console.WriteLine(e.Message);
                return false;
            }

            return connection.IsConnected();
        }

        /// <summary>
        /// Closes the connection with the Arduino server.
        /// </summary>
        public void CloseConnection()
        {
            connection.CloseConnection();
        }

        /// <summary>
        /// Refreshes the status on the main thread and fires off the StatusRefreshedEvent event. 
        /// </summary>
        /// <param name="checkPackage">If the package needs to be checked. Optional to keep UI more responsive since it's ran on the main thread.</param>
        public void RefreshStatus(bool checkPackage)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (IsConnected())
                {
                    IsConnected();
                    BoxStatus();
                    
                    if (checkPackage)
                        PackageStatus();
                }

                // Fire off the status refreshed event which GUI classes can listen to
                EventHandler handler = StatusRefreshedEvent;
                handler?.Invoke(this, EventArgs.Empty);

            });
        }

        /// <summary>
        /// Returns whether or not there is a connection with the Arduino going on. 
        /// </summary>
        public bool IsConnected()
        {
            bool connectstatus = connection.IsConnected();

            if (connectstatus)
                Status.ConnectionStatus = "Connected";
            else
                Status.ConnectionStatus = "Disconnected";

            return connectstatus;
        }

        /// <summary>
        /// On startup to set all the status texts
        /// </summary>
        public void OnStartup()
        {
            IsConnected();
            BoxStatus();
            PackageStatus();
        }

        /* =========================================================================
                                                      .___      
          ____  ____   _____   _____ _____    ____    __| _/______
        _/ ___\/  _ \ /     \ /     \\__  \  /    \  / __ |/  ___/
        \  \__(  <_> )  Y Y  \  Y Y  \/ __ \|   |  \/ /_/ |\___ \ 
         \___  >____/|__|_|  /__|_|  (____  /___|  /\____ /____  >
             \/            \/      \/     \/     \/      \/    \/ 

        Everything after this should be related to commands to send to the server.
        Make sure to provide very detailed comments of what the command is, and what the response should be,
        because the commands and responses should also be handled in the Arduino server code.
        The reason why commands each have their own methods is for safety and to make debugging easier. 
         */

        /* Command List - Put a list of commands here so it's easy to look up when coding the Arduino responses
         * Format: Method - Command - Expected Arduino response (NOT method return value)
         * 
         * Example:
         * LockPackageBox()     - "l"
         * UnlockPackageBox()   - "u"
         * BoxStatus()          - "s" - CLS (box is locked) or OPN (box is unlocked)
         * PackageStatus()      - "p" - "YES"(contains package) or NO (contains no package)
         */


        /// <summary>
        /// Sends a message to the arduino to close and lock the box
        /// NOTE: THIS METHOD BECAME UNUSED LATER
        /// </summary>
        //public void LockPackageBox()
        //{
        //    connection.ExecuteCommand("l", false);
        //    BoxStatus();
        //    RefreshStatus();
        //}

        /// <summary>
        /// Sends a message to the arduino to open/unlock the box.
        /// </summary>
        public void UnlockPackageBox()
        {
            connection.ExecuteCommand("u", false);
            RefreshStatus(false);
        }

        /// <summary>
        /// Updates the status of the box to "Locked" or "Unlocked".
        /// </summary>
        public void BoxStatus()
        {
            // "CLS" - box is closed and locked
            // "OPN" - box is unlocked
            // "Error" - if the response is unexpected

            string response = connection.ExecuteCommand("s");

            if (response == "CLS")
                Status.BoxStatus = "Closed";
            else if (response == "OPN")
                Status.BoxStatus = "Open";
            else 
                Status.BoxStatus = response;
        }

        /// <summary>
        /// Updates the package status to "Contains package" or "Empty". 
        /// </summary>
        public void PackageStatus()
        {
            // "YES" - There is a package
            // "NO" - There is no package

            string response = connection.ExecuteCommand("p");

            if (response == "YES")
            {
                Status.PackageStatus = "Contains package";
            }
            else if (response == "NO")
            {
                Status.PackageStatus = "Empty";
            }
            else
            {
                Status.PackageStatus = response;
            }
        }
    }
}
