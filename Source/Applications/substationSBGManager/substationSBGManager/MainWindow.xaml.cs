﻿//******************************************************************************************************
//  MainWindow.xaml.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/22/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using GSF.Configuration;
using GSF.IO;
using GSF.Reflection;
using GSF.Security;
using GSF.TimeSeries.UI;
using GSF.TimeSeries.UI.DataModels;

namespace substationSBGManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ResizableWindow
    {
        #region [ Members ]

        // Fields
        private ObservableCollection<MenuDataItem> m_menuDataItems;
        private WindowsServiceClient m_windowsServiceClient;
        private AlarmMonitor m_alarmMonitor;
        private string m_defaultNodeID;

        #endregion

        #region [ Properties ]

        public ObservableCollection<MenuDataItem> MenuDataItems
        {
            get
            {
                return m_menuDataItems;
            }
        }

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates an instance of <see cref="MainWindow"/>.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            Title = ((App)Application.Current).Title;
            TextBoxTitle.Text = AssemblyInfo.EntryAssembly.Title;

            CommonFunctions.CurrentUser = Thread.CurrentPrincipal.Identity.Name;
            CommonFunctions.CurrentPrincipal = Thread.CurrentPrincipal as SecurityPrincipal;

            if (!string.IsNullOrEmpty(CommonFunctions.CurrentUser))
                Title += " - " + SecurityProviderCache.CurrentProvider.UserData.LoginID;

            ConfigurationFile configFile = ConfigurationFile.Current;
            CategorizedSettingsElementCollection configSettings = configFile.Settings["systemSettings"];

            if (configSettings["NodeID"] != null)
                m_defaultNodeID = configSettings["NodeID"].Value;

            CommonFunctions.SetRetryServiceConnection(true);
            CommonFunctions.ServiceConnectionRefreshed += CommonFunctions_ServiceConnectionRefreshed;
            CommonFunctions.CanGoForwardChanged += (sender, args) => ForwardButton.IsEnabled = CommonFunctions.CanGoForward;
            CommonFunctions.CanGoBackChanged += (sender, args) => BackButton.IsEnabled = CommonFunctions.CanGoBack;
        }

        #endregion

        #region [ Methods ]

        private void CommonFunctions_ServiceConnectionRefreshed(object sender, EventArgs e)
        {
            try
            {
                Dispatcher.Invoke((Action)delegate()
                {
                    if (ComboboxNode.SelectedItem == null)
                    {
                        ComboboxNode.ItemsSource = Node.GetLookupList(null);
                        if (ComboboxNode.Items.Count > 0)
                            ComboboxNode.SelectedIndex = 0;
                    }

                    if (ComboboxNode.SelectedItem != null)
                    {
                        KeyValuePair<Guid, string> currentNode = (KeyValuePair<Guid, string>)ComboboxNode.SelectedItem;

                        ComboboxNode.ItemsSource = Node.GetLookupList(null);
                        if (ComboboxNode.Items.Count > 0)
                        {
                            ComboboxNode.SelectedItem = currentNode;
                            if (ComboboxNode.SelectedItem == null)
                                ComboboxNode.SelectedIndex = 0;
                        }
                    }
                });
            }
            finally
            {
                ConnectToService();
            }
        }

        /// <summary>
        /// Method to handle window loaded event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Load Menu
            XmlRootAttribute xmlRootAttribute = new XmlRootAttribute("MenuDataItems");
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<MenuDataItem>), xmlRootAttribute);

            using (XmlReader reader = XmlReader.Create(FilePath.GetAbsolutePath("Menu.xml")))
            {
                m_menuDataItems = (ObservableCollection<MenuDataItem>)serializer.Deserialize(reader);
            }

            MenuMain.DataContext = m_menuDataItems;

            // Populate Node Dropdown
            Dictionary<Guid, string> nodesList = Node.GetLookupList(null);
            ComboboxNode.ItemsSource = nodesList;
            if (ComboboxNode.Items.Count > 0)
            {
                if (!string.IsNullOrEmpty(m_defaultNodeID) && nodesList.ContainsKey(new Guid(m_defaultNodeID)))
                {
                    foreach (KeyValuePair<Guid, string> item in nodesList)
                    {
                        if (item.Key.ToString().ToLower() == m_defaultNodeID.ToLower())
                        {
                            ComboboxNode.SelectedItem = item;
                            break;
                        }
                    }
                }
                else
                    ComboboxNode.SelectedIndex = 0;
            }

            // Create alarm monitor as singleton
            m_alarmMonitor = new AlarmMonitor(true);
            m_alarmMonitor.Start();

            IsolatedStorageManager.InitializeIsolatedStorage(false);
        }

        /// <summary>
        /// Handles windows closing event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                Properties.Settings.Default.Save();
                CommonFunctions.SetRetryServiceConnection(false);
                m_alarmMonitor.Dispose();
                Application.Current.Shutdown();
            }
            catch (NullReferenceException)
            {
                Application.Current.Shutdown();
                MessageBox.Show("Please Re-run the ConfigrationSetupUtility");
            }
            catch
            {
                // Do Nothing. Just let it shut down gracefully without crashing.
            }
        }

        /// <summary>
        /// Handles selectionchanged event on node selection combobox.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event argument.</param>
        private void ComboboxNode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if ((object)ComboboxNode.SelectedItem != null)
                ((App)Application.Current).NodeID = ((KeyValuePair<Guid, string>)ComboboxNode.SelectedItem).Key;

            m_menuDataItems[0].Command.Execute(null);
        }

        private void ConnectToService()
        {
            if ((object)m_windowsServiceClient != null && (object)m_windowsServiceClient.Helper != null && (object)m_windowsServiceClient.Helper.RemotingClient != null)
            {
                m_windowsServiceClient.Helper.RemotingClient.ConnectionEstablished -= RemotingClient_ConnectionEstablished;
                m_windowsServiceClient.Helper.RemotingClient.ConnectionTerminated -= RemotingClient_ConnectionTerminated;
            }

            m_windowsServiceClient = CommonFunctions.GetWindowsServiceClient();

            if ((object)m_windowsServiceClient != null)
            {
                m_windowsServiceClient.Helper.RemotingClient.ConnectionEstablished += RemotingClient_ConnectionEstablished;
                m_windowsServiceClient.Helper.RemotingClient.ConnectionTerminated += RemotingClient_ConnectionTerminated;

                if (m_windowsServiceClient.Helper.RemotingClient.CurrentState == GSF.Communication.ClientState.Connected)
                {
                    EllipseConnectionState.Dispatcher.BeginInvoke((Action)delegate()
                    {
                        EllipseConnectionState.Fill = Application.Current.Resources["GreenRadialGradientBrush"] as RadialGradientBrush;
                        ToolTipService.SetToolTip(EllipseConnectionState, "Connected to the service");
                    });
                }
                else
                {
                    EllipseConnectionState.Dispatcher.BeginInvoke((Action)delegate()
                    {
                        EllipseConnectionState.Fill = Application.Current.Resources["RedRadialGradientBrush"] as RadialGradientBrush;
                        ToolTipService.SetToolTip(EllipseConnectionState, "Disconnected from the service");
                    });
                }
            }
        }

        private void RemotingClient_ConnectionTerminated(object sender, EventArgs e)
        {
            EllipseConnectionState.Dispatcher.BeginInvoke((Action)delegate()
            {
                EllipseConnectionState.Fill = Application.Current.Resources["RedRadialGradientBrush"] as RadialGradientBrush;
                ToolTipService.SetToolTip(EllipseConnectionState, "Disconnected from the service");
            });
        }

        private void RemotingClient_ConnectionEstablished(object sender, EventArgs e)
        {
            EllipseConnectionState.Dispatcher.BeginInvoke((Action)delegate()
            {
                EllipseConnectionState.Fill = Application.Current.Resources["GreenRadialGradientBrush"] as RadialGradientBrush;
                ToolTipService.SetToolTip(EllipseConnectionState, "Connected to the service");
            });
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            CommonFunctions.GoBack();
        }

        private void ButtonForward_Click(object sender, RoutedEventArgs e)
        {
            CommonFunctions.GoForward();
        }

        private void ButtonLogo_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://www.gridprotectionalliance.org/");
        }

        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            bool useLocalHelp = false;

            try
            {
                // Check for internet connectivity.
                Dns.GetHostEntry("substationSBG.codeplex.com");

                // Launch the help page available on web.
                Process.Start("http://substationSBG.codeplex.com/wikipage?title=Manager%20Configuration");
            }
            catch
            {
                useLocalHelp = true;
            }

            if (useLocalHelp)
            {
                try
                {
                    // Launch the offline copy of the help page.
                    string helpfile = FilePath.GetAbsolutePath("substationSBGManagerHelp.mht");

                    if (File.Exists(helpfile))
                        Process.Start(helpfile);
                    else
                        MessageBox.Show("Local help file \"" + helpfile + "\" was not found", "substationSBG Manager Help", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to launch local help file." + Environment.NewLine + ex.Message, "substationSBG Manager Help", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }

        #endregion
    }
}
