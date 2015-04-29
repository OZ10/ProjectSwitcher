using GalaSoft.MvvmLight;
using Ini;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using Fluor.ProjectSwitcher.Class;
using System.ComponentModel;
using System.Windows.Controls;
using System.Xml.Serialization;
using System.IO;

namespace Fluor.ProjectSwitcher.ViewModel
{
    public class MainViewModel : ViewModelBase
    {

        // TODO Allow associations -- in one xml row -- between mulitple projects and one application. Required for applications which don't require project specific setup
        //      i.e. notepad is just an exe. Defining a row per project is pointless and messy
        // TODO Create transitions between project & subproject (currently both are on the same tab)
        // TODO Add code to deal with applications with only one sub application to run - i.e. notepad
        // TODO Fix bug where selected sub apps will launch even if they are not currently displayed - i.e. even with SPEL selected, Drawing Manager will launch because it's selected by default

        private TopApplication _selectedApplication;

        public ProjectSwitcherSettings ProjectSwitcherSettings { get; set; }

        //public ObservableCollection<Project> ProjectsCollection { get; set; }
        //public ObservableCollection<SwitcherItem> ApplicationsCollection { get; set; }
        //public List<Association> Associations { get; set; }
        //public ObservableCollection<SwitcherItem> AssociatedApplicationCollection { get; set; }

        Project selectedTile;
        public Project SelectedTile
        {
            get
            {
                return selectedTile;
            }
            set
            {
                selectedTile = value;
                RaisePropertyChanged("SelectedProject");
            }
        }

        private ObservableCollection<MenuItem> contextMenus;
        public ObservableCollection<MenuItem> ContextMenus
        {
            get
            {
                return contextMenus;
            }
            set
            {
                contextMenus = value;
                RaisePropertyChanged("ContextMenus");
            }
        }

        bool isTileTabSelected;
        public bool IsTileTabSelected
        {
            get
            {
                return isTileTabSelected;
            }
            set
            {
                isTileTabSelected = value;
                RaisePropertyChanged("IsTileTabSelected");
            }
        }

        bool isApplicationsTabSelected;
        public bool IsApplicationsTabSelected
        {
            get
            {
                return isApplicationsTabSelected;
            }
            set
            {
                isApplicationsTabSelected = value;
                RaisePropertyChanged("IsApplicationsTabSelected");
            }
        }

        bool isAddNewTabSelected;
        public bool IsAddNewTabSelected
        {
            get
            {
                return isAddNewTabSelected;
            }
            set
            {
                isAddNewTabSelected = value;
                RaisePropertyChanged("IsAddNewTabSelected");
            }
        }

        private ObservableCollection<Button> breadcrumbCollection;
        public ObservableCollection<Button> BreadcrumbCollection
        {
            get
            {
                return breadcrumbCollection;
            }
            set
            {
                breadcrumbCollection = value;
                RaisePropertyChanged("BreadcrumbCollection");
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel 
        /// </summary>
        public MainViewModel()
        {
            IsTileTabSelected = true;
            BreadcrumbCollection = new ObservableCollection<Button>();

            Messenger.Default.Register<Message.MessageUpdateSelectedTile>(this, ChangeSelectedTile);
            Messenger.Default.Register<NotificationMessageAction<string>>(this, GetContextMenuParameters);
            Messenger.Default.Register<GenericMessage<TopApplication>>(this, UpdateApplicationsCollection);
            Messenger.Default.Register<GenericMessage<ObservableCollection<MenuItem>>>(this, UpdateSelectedProjectContextMenus);
            Messenger.Default.Register<GenericMessage<SwitcherItem>>(this, GoToTilesView);
            Messenger.Default.Register<Message.M_SimpleAction>(this, ChangeTab);
        }

        /// <summary>
        /// Setups the environment.
        /// </summary>
        public void SetupEnvironment()
        {
            try
            {
                ProjectSwitcherSettings = Deserialize();

                //_xmlSettings = XElement.Load("Fluor.ProjectSwitcher.Projects.xml");
                //Messenger.Default.Send<Message.M_LoadFromSettings>(new Message.M_LoadFromSettings(Message.M_LoadFromSettings.SettingLoadType.Project, _xmlSettings));
                //Messenger.Default.Send<Message.M_LoadFromSettings>(new Message.M_LoadFromSettings(Message.M_LoadFromSettings.SettingLoadType.Application, _xmlSettings));

                Messenger.Default.Send<ObservableCollection<Project>>(ProjectSwitcherSettings.Projects);
                Messenger.Default.Send<ObservableCollection<TopApplication>>(ProjectSwitcherSettings.Applications);

                //PopulateProjects();
                //PopulateAssociations();
                //PopulateApplications();
            }
            catch (Exception)
            {
                MessageBox.Show("Doh!");
                //Messenger.Default.Send<Message.MessageStatusUpdate>(new Message.MessageStatusUpdate(Visibility.Visible, "Doh!"));
            }
        }

        static public void Serialize(ProjectSwitcherSettings d)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ProjectSwitcherSettings));
            using (TextWriter writer = new StreamWriter(@"Fluor.ProjectSwitcher.Projects.xml"))
            {
                serializer.Serialize(writer, d);
            }
        }

        public static ProjectSwitcherSettings Deserialize()
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(ProjectSwitcherSettings));
            TextReader reader = new StreamReader(@"Fluor.ProjectSwitcher.Projects.xml");
            object obj = deserializer.Deserialize(reader);
            reader.Close();
            return (ProjectSwitcherSettings)obj;
        }

        /// <summary>
        /// Closes any open applications and opens new ones.
        /// </summary>
        public async void CloseAndOpenApplications()
        {
            if (SelectedTile != null)
            {
                // Show status grid which overs all controls
                Messenger.Default.Send(new Message.MessageStatusUpdate(Visibility.Visible));

                // Only close open applcations if the selected project is not the active project
                if (SelectedTile.IsActiveProject != true)
                {
                    Messenger.Default.Send(new Message.MessageStatusUpdate(Visibility.Visible, "closing applications"));
                    bool closeResult = await CloseApplicationsAsync();
                }

                // Change the active project and setup any project specific settings
                //ChangeActiveProject();


                SetProjectSpecificSettings();


                // Update status window
                Messenger.Default.Send(new Message.MessageStatusUpdate(Visibility.Visible, "opening applications"));

                // Open new applications
                System.Threading.Thread.Sleep(1000);
                bool openResult = await OpenApplicationsAsync();

                // Hide the status grid
                Messenger.Default.Send(new Message.MessageStatusUpdate(Visibility.Hidden));
            }
        }

        public void SetProjectSpecificSettings()
        {
            Project selectedProject = SelectedTile as Project;

            // For all associations associated with the selected project
            //foreach (Association association in Associations.Where(ass => ass.ProjectName == SelectedTile.Name))
            //{
            // Associations have a 'parameters' field. This field contains project & application specific settings which need to be set.
            // This could be an ini file setting(s), registry setting(s) or something else (as long as code as been written to deal with it).

            // For each parameter, determine type and set
            //foreach (Parameter parameter in association.Parameters)
            if (selectedProject != null && _selectedApplication != null)
            {
                foreach (Parameter parameter in selectedProject.Associations.First<Association>(a => a.ApplicationName == _selectedApplication.Name).Parameters)
                {
                    // Determine type
                    if (parameter.Type == Parameter.ParameterTypeEnum.INI)
                    {
                        SetIni(parameter);
                    }
                    else if (parameter.Type == Parameter.ParameterTypeEnum.REG)
                    {
                        SetRegistry(parameter);
                    }
                }
            }

            //}
        }

        private void SetIni(Parameter parameter)
        {
            //TODO Check if ini file exists

            Ini.IniFile ini = new IniFile("");

            ini = new Ini.IniFile(parameter.Path);

            List<string> iniSettings = parameter.GetSettings();

            ini.IniWriteValue(iniSettings[0], iniSettings[1], iniSettings[2]);
        }

        private void SetRegistry(Parameter parameter)
        {
            List<string> regSettings = parameter.GetSettings();

            Registry.SetValue(regSettings[0], regSettings[1], regSettings[2]);
        }

        /// <summary>
        /// Closes the applications asynchronous.
        /// </summary>
        /// <returns></returns>
        public Task<bool> CloseApplicationsAsync()
        {
            return Task.Run<bool>(() => ClosingApplications());
        }

        /// <summary>
        /// Closes the applications.
        /// </summary>
        /// <returns></returns>
        private bool ClosingApplications()
        {
            // Check if the application is already open
            foreach (TopApplication application in SelectedTile.Applications)
            {
                CloseSubApplications(application);
            }
            return true;
        }

        private static void CloseSubApplications(TopApplication application)
        {
            foreach (SubApplication subApplication in application.SubItems)
            {
                Process[] procs = Process.GetProcessesByName(subApplication.Exe.Replace(".exe", ""));

                if (procs.Length > 0)
                {
                    for (var i = 0; i < procs.Length; i++)
                    {
                        procs[i].CloseMainWindow();
                    }
                }

                if (subApplication.SubItems.Any())
                {
                    CloseSubApplications(subApplication);
                }
            }
        }

        /// <summary>
        /// Opens the applications asynchronous.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        private Task<bool> OpenApplicationsAsync()
        {
            return Task.Run<bool>(() => OpeningApplications());
        }

        /// <summary>
        /// Opens the applications.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        private bool OpeningApplications()
        {
            try
            {
                int i = 0;
                foreach (TopApplication application in SelectedTile.Applications)
                {
                    foreach (SubApplication subApplication in application.SubItems)
                    {
                        if (subApplication.IsSelected == true && subApplication.Exe != "")
                        {
                            LaunchApplication(ref i, subApplication);
                        }

                        OpeningSubApplications(i, subApplication);
                    }
                    System.Threading.Thread.Sleep(2000 * i);
                }
                return true;
            }
            catch (Exception)
            {
                MessageBox.Show("Executable is missing from install directory.\n\nCheck the executable paths in the configuration XML file.", "Executable Missing", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
                //throw;
            }
        }

        private void OpeningSubApplications(int i, SubApplication application)
        {
            try
            {
                foreach (SubApplication subApplication in application.SubItems)
                {
                    if (subApplication.IsSelected == true)
                    {
                        LaunchApplication(ref i, subApplication);
                    }

                    if (subApplication.SubItems.Any())
                    {
                        OpeningSubApplications(i, subApplication);
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Executable is missing from install directory.\n\nCheck the executable paths in the configuration XML file.", "Executable Missing", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LaunchApplication(ref int i, SubApplication subApplication)
        {
            Process p;

            // Check if application is already open
            Process[] processName = Process.GetProcessesByName(subApplication.Exe.Replace(".exe", ""));

            try
            {
                if (processName.Length == 0)
                {
                    p = new Process();
                    p.StartInfo.FileName = subApplication.InstallPath + subApplication.Exe;
                    p.Start();
                    i += 1;
                }
            }
            catch (Win32Exception)
            {
                MessageBox.Show(subApplication.Name + " could not be started.\n\n- Check that application is installed correctly.\n- Check that the 'install path' in the configuration XML file is valid.", "Check Installation", MessageBoxButton.OK, MessageBoxImage.Error);
                //throw;
            }
        }

        /// <summary>
        /// Changes the selected tile.
        /// </summary>
        /// <param name="msg">Message containing the selected item.</param>
        private void ChangeSelectedTile(Message.MessageUpdateSelectedTile msg)
        {
            // Collect all the applications that are associated with the newly selected item

            SelectedTile = msg.SelectedTile;

            //if (msg.Sender is App | msg.Sender is ViewModelTiles)
            //{
            //    IsAddNewTabSelected = true;
            //    Messenger.Default.Send<Message.MessageUpdateSelectedTile>(new Message.MessageUpdateSelectedTile(SelectedTile, this));
            //}
            if (msg.Sender is ViewModelTiles)
            {
                //TODO Double message here so added this selected tab check. Must be a better way to do this
                if (IsAddNewTabSelected == false)
                {
                    // A tile has been selected so display the tile tab
                    IsTileTabSelected = true;

                    if (SelectedTile != null)
                    {
                        // Change the breadcrumb (title bar) to reflect the newly selected item
                        PopulateBreadCrumb(SelectedTile);

                        // Get all the associations associated with the selected item
                        Messenger.Default.Send<Message.M_GetAssociatedApplications>(new Message.M_GetAssociatedApplications(SelectedTile));

                        if (SelectedTile.Applications.Any())
                        {
                            if (SelectedTile.Applications.Count == 1)
                            {
                                // Only one application is associated with the selected item. Pass the application details to the applications tab for display.
                                TopApplication application = SelectedTile.Applications[0] as TopApplication;
                                Messenger.Default.Send<GenericMessage<TopApplication>>(new GenericMessage<TopApplication>(application));
                            }
                            else
                            {
                                // Send a message to the Tile view to display associated applications as tiles
                                Messenger.Default.Send<Message.M_SimpleAction>(new Message.M_SimpleAction(Message.M_SimpleAction.Action.DisplayApplicationsAsTiles));
                            }
                        }
                    }
                    else
                    {
                        // Selected tile was null (home button selected), reset the breadcrumb collection.
                        BreadcrumbCollection = new ObservableCollection<Button>();
                    }
                }
            }
        }

        /// <summary>
        /// Populates the breadcrumb in the title bar.
        /// </summary>
        /// <param name="switcherItem">The switcher item to be added.</param>
        private void PopulateBreadCrumb(SwitcherItem switcherItem)
        {
            Button btn = new Button();
            btn.Style = (Style)System.Windows.Application.Current.Resources["MetroBreadCrumbButtonStyle"];
            btn.DataContext = switcherItem;

            // Check if the breadcrumb collection already has items
            // Method: Add each item one by one to a new temp collection until the item which has been select (passed in) is found
            // Then overwrite the Breadcrumb collection with the temp collection - therefore any item after the selected item will be removed.
            if (BreadcrumbCollection.Any())
            {
                ObservableCollection<Button> tempCollection = new ObservableCollection<Button>();

                foreach (Button b in BreadcrumbCollection)
                {
                    if (b.DataContext != btn.DataContext)
                    {
                        tempCollection.Add(b);
                    }
                    else
                    {
                        break;
                    }
                }

                BreadcrumbCollection = tempCollection;
            }

            BreadcrumbCollection.Add(btn);
        }

        /// <summary>
        /// Gather & returns all context menus associated with the selected item.
        /// </summary>
        /// <param name="getContextMenuMessage">The get context menu message.</param>
        private void GetContextMenuParameters(NotificationMessageAction<string> msg)
        {
            // Message sent by either the projects or the applications view

            string contextMenus = "";

            Project project = msg.Sender as Project;
            //contextMenus = SetContextMenuValue(contextMenus, switcherItem.ContextMenus);

            if (project != null)
            {
                // Get all associtions associated with the selected item
                foreach (Association association in project.Associations) //.Where(ass => ass.ProjectName == msg.Notification))
                {
                    foreach (Class.ContextMenu contextMenu in association.ContextMenuCollection)
                    {
                        project.AddContextMenu(contextMenu);
                    }

                    //contextMenus = SetContextMenuValue(contextMenus, association.ContextMenus);
                }
            }

            // Return contextmenus to sender
            //TODO This no longer needs to be an Action message
            msg.Execute(contextMenus);
        }

        /// <summary>
        /// Combines context menu values (strings) into one string.
        /// </summary>
        /// <param name="contextMenus">Current context menus string.</param>
        /// <param name="value">Value to add.</param>
        /// <returns></returns>
        private string SetContextMenuValue(string contextMenus, string value)
        {
            if (value != "")
            {
                if (contextMenus != "")
                {
                    return contextMenus + ";" + value;
                }
                else
                {
                    return value;
                }
            }
            return contextMenus;
        }

        /// <summary>
        /// Opens the admin module.
        /// </summary>
        public void OpenAdminModule()
        {
            //Fluor.ProjectSwitcher.Admin.Class.Run adminModule = new Admin.Class.Run(ProjectsCollection, ApplicationsCollection, Associations);
        }

        private void UpdateApplicationsCollection(GenericMessage<TopApplication> message)
        {
            _selectedApplication = (TopApplication)message.Content;
            IsApplicationsTabSelected = true;
        }

        private void UpdateSelectedProjectContextMenus(GenericMessage<ObservableCollection<MenuItem>> msg)
        {
            ContextMenus = msg.Content;
        }

        private void GoToTilesView(GenericMessage<SwitcherItem> msg)
        {
            IsApplicationsTabSelected = false;
            IsAddNewTabSelected = false;
            isTileTabSelected = true;
        }

        public void SaveAndClose()
        {
            if (ProjectSwitcherSettings.HasBeenUpdated)
            {
                ProjectSwitcherSettings.HasBeenUpdated = false;

                //ProjectSwitcherSettings = new ProjectSwitcherSettings();

                // Send messages to get the Project & Application collections from their respective views
                // TODO I don't think these are required. Any changes to the collections should be reflected
                // in the ProjectSwitcherSettings collection. Needs testing.

                //var msg = new NotificationMessageAction<ObservableCollection<Project>>(this, "", (p) =>
                //{
                //    ObservableCollection<Project> projectCollection = p;
                //    ProjectSwitcherSettings.Projects = p;
                //});

                //Messenger.Default.Send(msg);

                //var msg2 = new NotificationMessageAction<ObservableCollection<TopApplication>>(this, "", (a) =>
                //{
                //    ObservableCollection<TopApplication> applicationsCollection = a;
                //    ProjectSwitcherSettings.Applications = a;
                //});

                //Messenger.Default.Send(msg2);

                Serialize(ProjectSwitcherSettings);
            }
        }

        private void ChangeTab(Message.M_SimpleAction msg)
        {
            switch (msg.SimpleAction)
            {
                case Fluor.ProjectSwitcher.Message.M_SimpleAction.Action.DisplayTilesTab:
                    IsTileTabSelected = true;
                    break;
                case Fluor.ProjectSwitcher.Message.M_SimpleAction.Action.DisplayApplicationsTab:
                    IsApplicationsTabSelected = true;
                    break;
                case Fluor.ProjectSwitcher.Message.M_SimpleAction.Action.DisplayAddNewTab:
                    IsAddNewTabSelected = true;
                    break;
                default:
                    break;
            }
        }
    }
}