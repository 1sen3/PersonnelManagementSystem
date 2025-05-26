using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using PersonnelManagementSystem.Models;
using PersonnelManagementSystem.Services;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;

namespace PersonnelManagementSystem.ViewModels
{
    public class ChangeStaffViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private ObservableCollection<Personnel> _personnelList;
        private bool _isLoading;
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand AddNewPersonnelCommand { get; private set; }
        public ObservableCollection<Personnel> PersonnelList
        {
            get => _personnelList;
            set
            {
                if(_personnelList != value)
                {
                    _personnelList = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if(_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }
        public ChangeStaffViewModel()
        {
            _databaseService = new DatabaseService();
            PersonnelList = new ObservableCollection<Personnel>();
            LoadPersonnelsDataAsync();

            AddNewPersonnelCommand = new RelayCommand(ExecuteAddNewPersonnel);
        }
        public async Task LoadPersonnelsDataAsync()
        {
            try
            {
                IsLoading = true;
                var personnels = await _databaseService.GetPersonnelListAsync();
                PersonnelList.Clear();
                foreach (var Personnel in personnels) PersonnelList.Add(Personnel);
            }
            catch(System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载变更记录数据出错: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private async void ExecuteAddNewPersonnel()
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "人事变更",
                PrimaryButtonText = "确定",
                SecondaryButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            StackPanel contentPanel = new StackPanel { Spacing = 10 };

            // 工号输入
            TextBox staffIdBox = new TextBox
            {
                Header = "工号",
                PlaceholderText = "请输入员工工号"
            };
            contentPanel.Children.Add(staffIdBox);

            // 变更操作选择
            ComboBox operationBox = new ComboBox
            {
                Header = "变更操作",
                PlaceholderText = "请选择变更操作"
            };
            var changes = await _databaseService.GetChangeListAsync();
            foreach (string c in changes) operationBox.Items.Add(c);
            contentPanel.Children.Add(operationBox);

            // 创建动态表单区域
            StackPanel dynamicFormPanel = new StackPanel { Spacing = 10 };
            contentPanel.Children.Add(dynamicFormPanel);

            // 根据操作类型显示不同表单
            operationBox.SelectionChanged += async (sender, args) =>
            {
                dynamicFormPanel.Children.Clear();
                string selected = operationBox.SelectedItem?.ToString();

                if (string.IsNullOrEmpty(selected)) return;

                switch (selected)
                {
                    case "职务变动":
                        ComboBox newJobBox = new ComboBox
                        {
                            Header = "新职务",
                            PlaceholderText = "请选择新职务"
                        };
                        var Jobs = await _databaseService.GetJobListAsync();
                        foreach (string j in Jobs) newJobBox.Items.Add(j);
                        dynamicFormPanel.Children.Add(newJobBox);
                        break;
                    case "调岗":
                        ComboBox newDepartmentBox = new ComboBox
                        {
                            Header = "新部门",
                            PlaceholderText = "请选择新部门"
                        };
                        var Departments = await _databaseService.GetDepartmentListAsync();
                        foreach (string d in Departments) newDepartmentBox.Items.Add(d);

                        newJobBox = new ComboBox
                        {
                            Header = "新职务",
                            PlaceholderText = "请选择新职务"
                        };
                        Jobs = await _databaseService.GetJobListAsync();
                        foreach (string j in Jobs) newJobBox.Items.Add(j);

                        dynamicFormPanel.Children.Add(newDepartmentBox);
                        dynamicFormPanel.Children.Add(newJobBox);
                        break;
                }
            };

            dialog.Content = contentPanel;

            var result = await dialog.ShowAsync();
        }
    }
}
