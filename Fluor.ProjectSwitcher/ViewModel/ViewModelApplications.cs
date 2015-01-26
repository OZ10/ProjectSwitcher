﻿using System.Collections.ObjectModel;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight;
using Fluor.ProjectSwitcher.Class;
using System.Linq;

namespace Fluor.ProjectSwitcher.ViewModel
{
    public class ViewModelApplications :ViewModelBase 
    {
        private ObservableCollection<SwitcherItem> applicationsCollection;
        public ObservableCollection<SwitcherItem> ApplicationsCollection
        {
            get
            {
                return applicationsCollection;
            }
            set
            {
                applicationsCollection = value;
                RaisePropertyChanged("ApplicationsCollection");
            }
        }

        private ObservableCollection<ListBox> activeApplicationCollection;
        public ObservableCollection<ListBox> ActiveApplicationCollection
        {
            get
            {
                return activeApplicationCollection;
            }
            set
            {
                activeApplicationCollection = value;
                RaisePropertyChanged("ActiveApplicationCollection");
            }
        }

        public ViewModelApplications()
        {
            Messenger.Default.Register<GenericMessage<TopApplication>>(this, UpdateApplicationsCollection);
            Messenger.Default.Register<GenericMessage<MenuItem>>(this, SelectApplications);
            Messenger.Default.Register<GenericMessage<TextBlock>>(this, DisplayContextMenus);
        }

        private void UpdateApplicationsCollection(GenericMessage<TopApplication> message)
        {
            //check = false;
            ApplicationsCollection = message.Content.SubItems;

            ActiveApplicationCollection = new ObservableCollection<ListBox>();

            foreach (TopApplication application in ApplicationsCollection)
            {
                ListBox lb = new ListBox();
                lb.Template = (ControlTemplate)System.Windows.Application.Current.Resources["ApplicationListTemplate"];
                lb.DataContext = application;

                ActiveApplicationCollection.Add(lb);
            }
        }

        public void DisplayContextMenus(GenericMessage<TextBlock> msg)
        {
            // Triggered by a right-click on an application.

            TextBlock tb = (TextBlock)msg.Content;

            SubApplication subApplication = (SubApplication)tb.DataContext;  //GetSelectedApplication(applicationName);

            //ApplicationContextMenus = new ObservableCollection<MenuItem>();

            // Send a message containing the project name to the main view model. The main view model returns the context
            // menu parameters as listed in the associations section
            Messenger.Default.Send(new NotificationMessageAction<string>(subApplication, subApplication.Name, (contextMenuParameters) =>
            {
                //subApplication.CreateContextMenus(contextMenuParameters);
            }));

            tb.ContextMenu.ItemsSource = subApplication.ContextMenuCollection;
        }

        public void SelectApplications(GenericMessage<MenuItem> msg)
        {
            SwitcherItem selectedApp = (SwitcherItem)msg.Content.DataContext;
            string selectedMenuItem = msg.Content.Header.ToString();

            foreach (TopApplication application in ApplicationsCollection.Where(sa => sa == selectedApp.ParentItem))
            {
                foreach (SubApplication subApp in application.SubItems)
                {
                    subApp.IsSelected = false;

                    switch (selectedMenuItem)
                    {
                        case "select all":
                            subApp.IsSelected = true;
                            break;

                        //case "select none":
                        //    subApp.IsSelected = false;
                        //    break;

                        case "select only this":
                            if (subApp == selectedApp)
                            {
                                subApp.IsSelected = true;
                            }
                            break;

                        case "select all except this":
                            if (subApp != selectedApp)
                            {
                                subApp.IsSelected = true;
                            }
                            break;

                        default:
                            break;
                    }
                    
                }
            }
        }
    }
}
