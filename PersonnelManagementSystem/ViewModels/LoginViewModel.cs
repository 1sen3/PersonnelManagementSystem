using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Windowing;
using PersonnelManagementSystem.Services;
using PersonnelManagementSystem.Views;

namespace PersonnelManagementSystem.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _id;
        private string _password;
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand LoginCommand { get; private set; }
        public string Id
        {
            get => _id;
            set
            {
                if(_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Password
        {
            get => _password;
            set
            {
                if(_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                }
            }
        }
        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLoginAsync);
        }
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public async void ExecuteLoginAsync()
        {
            App.CurrentUser = await DatabaseService.LoginAsync(Id, Password);
            if(App.CurrentUser != null)
            {
                if(App.CurrentUser.Authority == "管理员")
                {
                    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                    {
                        App.MainWindow.NavigatePage(new AdminPage());
                    });
                }
                else
                {
                    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                    {
                        App.MainWindow.NavigatePage(new StaffPage());
                    });
                }
            }
        }
    }
}
