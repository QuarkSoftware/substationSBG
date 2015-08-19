﻿//******************************************************************************************************
//  Program.cs - Gbtc
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
//  05/04/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

namespace substationSBG
{
    class Program
    {
        static ServiceClient s_serviceClient;

        static void Main(string[] args)
        {
            // Enable console events.
            GSF.Console.Events.ConsoleClosing += OnConsoleClosing;
            GSF.Console.Events.EnableRaisingEvents();

            // Start the client component.
            s_serviceClient = new ServiceClient();
            s_serviceClient.Start(args);
            s_serviceClient.Dispose();

            System.Environment.Exit(0);
        }

        static void OnConsoleClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Dispose the client component.
            if (s_serviceClient != null)
                s_serviceClient.Dispose();
        }
    }
}
