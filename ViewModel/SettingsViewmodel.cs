using PhotoUploader.Common;
using PhotoUploader.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PhotoUploader.ViewModel
{
    public class SettingsViewmodel : ObservableObject
    {

        public Window parentWindow { get; set; }
        private Language _selectedLanguage { get; set; }

        private ICommand _questionCommand;


        private string SelectedLangugeFromConfig()
        {
            Configuration configuration =
            ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            string dlang = ConfigurationManager.AppSettings["defaultlanguage"];
            return dlang;
        }
        public SettingsViewmodel()
        {

            string dlang = SelectedLangugeFromConfig();

            IsChanged = false;
            IsContinue = true;

            LanguageList = new ObservableCollection<Language>();
            LanguageList.Add(new Language { Name = "English", LangID = 1, Abbrevation = "en" });
            LanguageList.Add(new Language { Name = "Dutch", LangID = 2, Abbrevation = "nl" });
            LanguageList.Add(new Language { Name = "German", LangID = 2, Abbrevation = "de" });


            SelectedLanguage = LanguageList.Where(k => k.Abbrevation == dlang).FirstOrDefault();
        }

        public Language SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {

                string dlang = SelectedLangugeFromConfig();
                if (value != null && dlang != value.Abbrevation)
                {
                    IsChanged = true;
                    IsContinue = false;
                }

                else
                {
                    IsChanged = false; IsContinue = true;
                }

                _selectedLanguage = value;

                OnPropertyChanged("SelectedLanguage");
               
            }
        }

        private ObservableCollection<Language> _languagelist { get; set; }
        public ObservableCollection<Language> LanguageList
        {
            get { return _languagelist; }
            set { _languagelist = value; OnPropertyChanged("LanguageList"); }

        }

        private bool _isChanged { get; set; }
        public bool IsChanged {
            get { return _isChanged; }
            set { _isChanged = value; OnPropertyChanged("IsChanged"); }
        }


        private bool _isContinue { get; set; }
        public bool IsContinue
        {
            get { return _isContinue; }
            set { _isContinue = value; OnPropertyChanged("IsContinue"); }
        }

        private void ChangeLanguage()
        {

            System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show("Would you like to change language?", "", MessageBoxButton.OK);
            if (result == MessageBoxResult.OK)
            {



                SetSetting("defaultlanguage", SelectedLanguage.Abbrevation);

                parentWindow.Close();
                Thread.Sleep(100);

                var folder = System.AppDomain.CurrentDomain.BaseDirectory;
                Process.Start(Path.Combine(folder, "PhotoUploader.exe"));
                return;
            }

        }


        private static void SetSetting(string key, string value)
        {
            Configuration configuration =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");


        }



        private ICommand _changeLanguageCommand { get; set; }
        public ICommand ChangeLanguageCommand
        {

            get
            {

                if (_changeLanguageCommand == null)
                    _changeLanguageCommand = new RelayCommand(param => ChangeLanguage());

                return _changeLanguageCommand;
            }
        }

        public ICommand ContinueCommand
        {

            get
            {

                if (_questionCommand == null)
                    _questionCommand = new RelayCommand(param => GetUploaderWindow());

                return _questionCommand;
            }
        }


        

        private void GetUploaderWindow()
        {
            UploaderView view = new UploaderView();
            view.Show();

            Thread.Sleep(100);

            parentWindow.Close();
        }

    }
}
