﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Fluor.ProjectSwitcher.Admin.View
{
    /// <summary>
    /// Interaction logic for ViewProjects.xaml
    /// </summary>
    public partial class ViewProjects : UserControl
    {
        public ViewProjects()
        {
            InitializeComponent();
        }

        private void tvProjects_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }
    }
}
