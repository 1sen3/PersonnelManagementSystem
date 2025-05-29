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
            PersonnelList = new ObservableCollection<Personnel>();
            LoadPersonnelsDataAsync();

            AddNewPersonnelCommand = new RelayCommand(ExecuteAddNewPersonnel);
        }
        public async Task LoadPersonnelsDataAsync()
        {
            try
            {
                IsLoading = true;
                var personnels = await DatabaseService.GetPersonnelListAsync();
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
                XamlRoot = App.MainWindow.Content.XamlRoot,
                IsPrimaryButtonEnabled = false
            };

            StackPanel contentPanel = new StackPanel { Spacing = 10 };

            // 选择员工
            ComboBox staffComboBox = new()
            {
                Header = "员工",
                PlaceholderText = "请选择变更的员工",
                Width = 250
            };
            var staffList = await DatabaseService.GetStaffIdAndNameAsync();
            foreach(var staff in staffList) staffComboBox.Items.Add(staff);
            contentPanel.Children.Add(staffComboBox);

            // 变更操作选择
            ComboBox operationBox = new ComboBox
            {
                Header = "变更操作",
                PlaceholderText = "请选择变更操作",
                Width = 250
            };
            var changes = await DatabaseService.GetChangeListAsync();
            foreach (string c in changes) operationBox.Items.Add(c);
            contentPanel.Children.Add(operationBox);

            // 创建动态表单区域
            StackPanel dynamicFormPanel = new StackPanel { Spacing = 10 };
            contentPanel.Children.Add(dynamicFormPanel);

            // 用于存储表单控件的引用
            ComboBox newJobBox = new()
            {
                Header = "新职务",
                PlaceholderText = "请选择新职务",
                Width = 250,
                Visibility = Microsoft.UI.Xaml.Visibility.Collapsed
            };
            var jobs = await DatabaseService.GetJobListAsync();
            foreach(var j in jobs) newJobBox.Items.Add(j);

            ComboBox newDepartmentBox = new()
            {
                Header = "新部门",
                PlaceholderText = "请选择新部门",
                Width = 250,
                Visibility = Microsoft.UI.Xaml.Visibility.Collapsed
            };
            var Depts = await DatabaseService.GetDepartmentListAsync();
            foreach (var d in Depts) newDepartmentBox.Items.Add(d);
            TextBox reasonTextBox = new()
            {
                Header = "原因",
                PlaceholderText = "请输入原因",
                Width = 250,
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left,
                Visibility = Microsoft.UI.Xaml.Visibility.Collapsed
            };

            dynamicFormPanel.Children.Add(newDepartmentBox);
            dynamicFormPanel.Children.Add(newJobBox);
            dynamicFormPanel.Children.Add(reasonTextBox);

            operationBox.SelectionChanged += async (sender, args) =>
            {
                string selected = operationBox.SelectedItem?.ToString();
                if(selected == "职位变动")
                {
                    newDepartmentBox.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    newJobBox.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    reasonTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                }
                else if(selected == "调岗")
                {
                    newDepartmentBox.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    newJobBox.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    reasonTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    newDepartmentBox.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    newJobBox.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    reasonTextBox.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                }
            };

            dialog.Content = contentPanel;

            void ValidateForm()
            {
                string operation = operationBox.SelectedItem?.ToString();
                bool isValid;
                if(operation == "职务变动")
                {
                    isValid = staffComboBox.SelectedItem != null && newJobBox.SelectedItem != null;
                }
                else if(operation == "调岗")
                {
                    isValid = staffComboBox.SelectedItem != null && newDepartmentBox.SelectedItem != null && newJobBox.SelectedItem != null && !string.IsNullOrEmpty(reasonTextBox.Text);
                }
                else
                {
                    isValid = staffComboBox.SelectedItem != null && !string.IsNullOrEmpty(reasonTextBox.Text);
                }
                dialog.IsPrimaryButtonEnabled = isValid;
            }

            staffComboBox.SelectionChanged += (s, e) => ValidateForm();
            newDepartmentBox.SelectionChanged += (s, e) => ValidateForm();
            newJobBox.SelectionChanged += (s, e) => ValidateForm();
            reasonTextBox.TextChanged += (s, e) => ValidateForm();

            ValidateForm();

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    string staffId = staffComboBox.SelectedItem?.ToString().Substring(0, 6);
                    string operation = operationBox.SelectedItem?.ToString();
                    string newDept = null;
                    string newJob = null;
                    string reason = null;

                    switch (operation)
                    {
                        case "职务变动":
                            newJob = newJobBox.SelectedItem.ToString();
                            break;
                        case "调岗":
                            newDept = newDepartmentBox.SelectedItem.ToString();
                            newJob = newJobBox.SelectedItem.ToString();
                            break;
                    }

                    if (operation != "职务变动")
                    {
                        reason = reasonTextBox.Text;
                    }

                    bool success = await DatabaseService.ChangeStaffAsync(staffId, operation, newDept, newJob, reason);

                    if (success)
                    {
                        ContentDialog successDialog = new ContentDialog
                        {
                            Title = "成功",
                            Content = "人事变更记录已添加",
                            CloseButtonText = "确定",
                            XamlRoot = App.MainWindow.Content.XamlRoot
                        };
                        await successDialog.ShowAsync();

                        await LoadPersonnelsDataAsync();
                    }
                }
                catch (Exception ex)
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "错误",
                        Content = $"添加人事变更记录失败: {ex.Message}",
                        CloseButtonText = "确定",
                        XamlRoot = App.MainWindow.Content.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }
    }
}
