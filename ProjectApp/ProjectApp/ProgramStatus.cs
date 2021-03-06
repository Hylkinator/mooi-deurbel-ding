﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ProjectApp
{
    public class ProgramStatus : INotifyPropertyChanged
    {
        private string connectionStatus = "Disconnected";
        public string ConnectionStatus
        {
            get { return connectionStatus; }
            set
            {
                connectionStatus = value;
                OnPropertyChanged(nameof(ConnectionStatus));
            }
        }

        private string boxStatus = "Unknown";
        public string BoxStatus
        {
            get { return boxStatus; }
            set
            {
                boxStatus = value;
                OnPropertyChanged(nameof(BoxStatus));
            }
        }

        private string packageStatus = "Unknown";
        public string PackageStatus
        {
            get { return packageStatus; }
            set
            {
                packageStatus = value;
                OnPropertyChanged(nameof(PackageStatus));
            }
        }

        // The stuff below is needed so the GUI gets notified when the variables change so it gets automatically updated

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
