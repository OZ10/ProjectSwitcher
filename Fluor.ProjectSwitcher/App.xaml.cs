﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;

namespace Fluor.ProjectSwitcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void lblProject_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Label l = (Label)sender;
            Messenger.Default.Send<NotificationMessage>(new NotificationMessage(l.Content.ToString()));
            //vm.DisplayContextMenus(l.Content.ToString());
        }
    }
}
