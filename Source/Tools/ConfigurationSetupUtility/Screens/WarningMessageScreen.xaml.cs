﻿//******************************************************************************************************
//  WarningMessageScreen.xaml.cs - Gbtc
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
//  09/07/2010 - Stephen C. Wills
//       Generated original version of source code.
//  09/14/2010 - J. Ritchie Carroll
//       Added help button that just browses to codeplex site for now...
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Diagnostics;

namespace ConfigurationSetupUtility.Screens
{
    /// <summary>
    /// Interaction logic for WarningMessageScreen.xaml
    /// </summary>
    public partial class WarningMessageScreen : UserControl, IScreen
    {

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="WarningMessageScreen"/> class.
        /// </summary>
        public WarningMessageScreen()
        {
            InitializeComponent();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the screen to be displayed when the user clicks the "Next" button.
        /// </summary>
        public IScreen NextScreen { get; set; }

        /// <summary>
        /// Gets a boolean indicating whether the user can advance to
        /// the next screen from the current screen.
        /// </summary>
        public bool CanGoForward
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a boolean indicating whether the user can return to
        /// the previous screen from the current screen.
        /// </summary>
        public bool CanGoBack
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a boolean indicating whether the user can cancel the
        /// setup process from the current screen.
        /// </summary>
        public bool CanCancel
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a boolean indicating whether the user input is valid on the current page.
        /// </summary>
        public bool UserInputIsValid
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Collection shared among screens that represents the state of the setup.
        /// </summary>
        public Dictionary<string, object> State { get; set; }

        /// <summary>
        /// Allows the screen to update the navigation buttons after a change is made
        /// that would affect the user's ability to navigate to other screens.
        /// </summary>
        public Action UpdateNavigation { get; set; }

        #endregion

        #region [ Methods ]

        private void hyperlinkHelp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start("http://substationSBG.codeplex.com/documentation/");
        }

        #endregion
    }
}
