﻿using Fluor.ProjectSwitcher.Class;
using GalaSoft.MvvmLight.Messaging;
using System.Windows;
using System.Windows.Controls;

namespace Fluor.ProjectSwitcher.View
{
    /// <summary>
    /// Interaction logic for ProjectsUC.xaml
    /// </summary>
    public partial class ViewTiles : UserControl
    {
        public ViewModel.ViewModelTiles vm
        {
            get
            {
                return (ViewModel.ViewModelTiles)DataContext;
            }
        }

        public ViewTiles()
        {
            InitializeComponent();
        }

        private void btnAddNew_Click(object sender, RoutedEventArgs e)
        {
            vm.AddNewTile();
        }
    }
}
