//------------------------------------------------------------------------------
// FunctionSettingsControl.cs
//------------------------------------------------------------------------------

namespace PostureMatching
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Markup;

    using Microsoft.Kinect;

    /// <summary>
    /// Interaction logic for FunctionSettings.xaml
    /// </summary>
    public partial class FunctionSettingsControl : UserControl, INotifyPropertyChanged
    {
        // The function settings
        private FunctionSettings functionSettings = new FunctionSettings();

        // The bones
        private List<JointType> boneList = new List<JointType>();

        public FunctionSettingsControl()
        {
            this.InitializeComponent();

            this.boneList.Add(JointType.Spine);
            this.boneList.Add(JointType.ShoulderCenter);
            this.boneList.Add(JointType.ShoulderLeft);
            this.boneList.Add(JointType.ShoulderRight);
            this.boneList.Add(JointType.Head);
            this.boneList.Add(JointType.ElbowLeft);
            this.boneList.Add(JointType.ElbowRight);
            this.boneList.Add(JointType.WristLeft);
            this.boneList.Add(JointType.WristRight);
            this.boneList.Add(JointType.HandLeft);
            this.boneList.Add(JointType.HandRight);
            this.boneList.Add(JointType.HipLeft);
            this.boneList.Add(JointType.HipRight);
            this.bonesCB.ItemsSource = this.boneList;
        }

        public FunctionSettings Settings
        {
            get
            {
                return this.functionSettings;
            }
        }

        /// <summary>
        /// Property change event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public bool NearMode
        {
            get
            {
                return this.functionSettings.NearMode;
            }

            set
            {
                this.functionSettings.NearMode = value;

                if (null != this.PropertyChanged)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("NearMode"));
                }
            }
        }

        public bool Checking
        {
            get
            {
                return this.functionSettings.Checking;
            }

            set
            {
                this.functionSettings.Checking = value;

                if (null != this.PropertyChanged)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Checking"));
                }
            }
        }

        public bool Mirror
        {
            get
            {
                return this.functionSettings.Mirror;
            }

            set
            {
                this.functionSettings.Mirror = value;

                if (null != this.PropertyChanged)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Mirror"));
                }
            }
        }

        public bool UseWeight
        {
            get
            {
                return this.functionSettings.UseWeight;
            }

            set
            {
                this.functionSettings.UseWeight = value;

                if (null != this.PropertyChanged)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("UseWeight"));
                }
            }
        }

        public PlayingStatus PStatus
        {
            get
            {
                return this.functionSettings.PStatus;
            }

            set
            {
                this.functionSettings.PStatus = value;

                if (null != this.PropertyChanged)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("PStatus"));
                }
            }
        }

        public JointType BoneJoint
        {
            get
            {
                return this.boneList[this.bonesCB.SelectedIndex];
            }

            set
            {
                this.bonesCB.SelectedIndex = this.boneList.FindIndex(bone => bone == value);

                if (null != this.PropertyChanged)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("BoneJoint"));
                }
            }
        }

        public string Status
        {
            get
            {
                return this.functionSettings.Status;
            }

            set
            {
                this.functionSettings.Status = value;

                if (null != this.PropertyChanged)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Status"));
                }
            }
        }

        private void PlayingChecked(object sender, RoutedEventArgs e)
        {
            if (true == this.none.IsChecked)
            {
                this.PStatus = PlayingStatus.None;
            }
            else if (true == this.recording.IsChecked)
            {
                this.PStatus = PlayingStatus.Recording;
            }
            else
            {
                this.PStatus = PlayingStatus.Playing;
            }
        }
    }
}