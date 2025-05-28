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
            };

            StackPanel contentPanel = new StackPanel { Spacing = 10 };

            // 工号输入
            TextBox staffIdBox = new TextBox
            {
                Header = "工号",
                PlaceholderText = "请输入员工工号"
            };
            
            // 添加工号验证提示文本块
            TextBlock staffIdValidationMessage = new TextBlock
            {
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
                Visibility = Microsoft.UI.Xaml.Visibility.Collapsed,
                Margin = new Microsoft.UI.Xaml.Thickness(0, -5, 0, 5)
            };
            
            contentPanel.Children.Add(staffIdBox);
            contentPanel.Children.Add(staffIdValidationMessage);

            // 为工号输入框添加失去焦点事件处理
            staffIdBox.LostFocus += async (sender, args) =>
            {
                if (!string.IsNullOrEmpty(staffIdBox.Text))
                {
                    try
                    {
                        bool exists = await DatabaseService.CheckStaffExistAsync(staffIdBox.Text);
                        if (!exists)
                        {
                            staffIdValidationMessage.Text = "该工号不存在！";
                            staffIdValidationMessage.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                        }
                        else
                        {
                            staffIdValidationMessage.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        }
                    }
                    catch (Exception ex)
                    {
                        staffIdValidationMessage.Text = $"验证工号时出错: {ex.Message}";
                        staffIdValidationMessage.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    }
                }
            };

            // 变更操作选择
            ComboBox operationBox = new ComboBox
            {
                Header = "变更操作",
                PlaceholderText = "请选择变更操作"
            };
            var changes = await DatabaseService.GetChangeListAsync();
            foreach (string c in changes) operationBox.Items.Add(c);
            contentPanel.Children.Add(operationBox);

            // 创建动态表单区域
            StackPanel dynamicFormPanel = new StackPanel { Spacing = 10 };
            contentPanel.Children.Add(dynamicFormPanel);

            // 用于存储表单控件的引用
            ComboBox newJobBox = null;
            ComboBox newDepartmentBox = null;
            TextBox reasonTextBox = null;

            // 根据操作类型显示不同表单
            operationBox.SelectionChanged += async (sender, args) =>
            {
                dynamicFormPanel.Children.Clear();
                string selected = operationBox.SelectedItem?.ToString();

                if (string.IsNullOrEmpty(selected)) return;

                switch (selected)
                {
                    case "职务变动":
                        newJobBox = new ComboBox
                        {
                            Header = "新职务",
                            PlaceholderText = "请选择新职务"
                        };
                        var Jobs = await DatabaseService.GetJobListAsync();
                        foreach (string j in Jobs) newJobBox.Items.Add(j);
                        dynamicFormPanel.Children.Add(newJobBox);
                        break;
                    case "调岗":
                        newDepartmentBox = new ComboBox
                        {
                            Header = "新部门",
                            PlaceholderText = "请选择新部门"
                        };
                        var Departments = await DatabaseService.GetDepartmentListAsync();
                        foreach (string d in Departments) newDepartmentBox.Items.Add(d);

                        newJobBox = new ComboBox
                        {
                            Header = "新职务",
                            PlaceholderText = "请选择新职务"
                        };
                        Jobs = await DatabaseService.GetJobListAsync();
                        foreach (string j in Jobs) newJobBox.Items.Add(j);

                        dynamicFormPanel.Children.Add(newDepartmentBox);
                        dynamicFormPanel.Children.Add(newJobBox);
                        break;
                }

                if (selected != "职务变动")
                {
                    // 原因输入框
                    reasonTextBox = new TextBox
                    {
                        Header = "原因",
                        PlaceholderText = "原因"
                    };
                    dynamicFormPanel.Children.Add(reasonTextBox);
                }
            };

            dialog.Content = contentPanel;

            var result = await dialog.ShowAsync();

            // 处理确定按钮点击事件
            if (result == ContentDialogResult.Primary)
            {
                // 验证输入
                if (string.IsNullOrEmpty(staffIdBox.Text))
                {
                    // 显示错误提示
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "错误",
                        Content = "请输入员工工号",
                        CloseButtonText = "确定",
                        XamlRoot = App.MainWindow.Content.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                // 再次验证工号是否存在
                bool staffExists = await DatabaseService.CheckStaffExistAsync(staffIdBox.Text);
                if (!staffExists)
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "错误",
                        Content = "该工号不存在",
                        CloseButtonText = "确定",
                        XamlRoot = App.MainWindow.Content.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                string operation = operationBox.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(operation))
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "错误",
                        Content = "请选择变更操作",
                        CloseButtonText = "确定",
                        XamlRoot = App.MainWindow.Content.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                try
                {
                    string staffId = staffIdBox.Text;
                    if (staffId == null)
                    {
                        ContentDialog errorDialog = new ContentDialog
                        {
                            Title = "错误",
                            Content = "工号必须为数字",
                            CloseButtonText = "确定",
                            XamlRoot = App.MainWindow.Content.XamlRoot
                        };
                        await errorDialog.ShowAsync();
                        return;
                    }

                    string newDept = null;
                    string newJob = null;
                    string reason = null;

                    // 根据操作类型获取相应的表单数据
                    switch (operation)
                    {
                        case "职务变动":
                            if (newJobBox == null || newJobBox.SelectedItem == null)
                            {
                                ContentDialog errorDialog = new ContentDialog
                                {
                                    Title = "错误",
                                    Content = "请选择新职务",
                                    CloseButtonText = "确定",
                                    XamlRoot = App.MainWindow.Content.XamlRoot
                                };
                                await errorDialog.ShowAsync();
                                return;
                            }
                            newJob = newJobBox.SelectedItem.ToString();
                            break;
                        case "调岗":
                            if (newDepartmentBox == null || newDepartmentBox.SelectedItem == null)
                            {
                                ContentDialog errorDialog = new ContentDialog
                                {
                                    Title = "错误",
                                    Content = "请选择新部门",
                                    CloseButtonText = "确定",
                                    XamlRoot = App.MainWindow.Content.XamlRoot
                                };
                                await errorDialog.ShowAsync();
                                return;
                            }
                            if (newJobBox == null || newJobBox.SelectedItem == null)
                            {
                                ContentDialog errorDialog = new ContentDialog
                                {
                                    Title = "错误",
                                    Content = "请选择新职务",
                                    CloseButtonText = "确定",
                                    XamlRoot = App.MainWindow.Content.XamlRoot
                                };
                                await errorDialog.ShowAsync();
                                return;
                            }
                            newDept = newDepartmentBox.SelectedItem.ToString();
                            newJob = newJobBox.SelectedItem.ToString();
                            break;
                    }

                    // 获取原因（如果适用）
                    if (operation != "职务变动" && (reasonTextBox == null || string.IsNullOrEmpty(reasonTextBox.Text)))
                    {
                        ContentDialog errorDialog = new ContentDialog
                        {
                            Title = "错误",
                            Content = "请输入变更原因",
                            CloseButtonText = "确定",
                            XamlRoot = App.MainWindow.Content.XamlRoot
                        };
                        await errorDialog.ShowAsync();
                        return;
                    }

                    if (operation != "职务变动")
                    {
                        reason = reasonTextBox.Text;
                    }

                    // 调用 DatabaseService 的 ChangeStaffAsync 方法
                    bool success = await DatabaseService.ChangeStaffAsync(staffId, operation, newDept, newJob, reason);

                    if (success)
                    {
                        // 显示成功提示
                        ContentDialog successDialog = new ContentDialog
                        {
                            Title = "成功",
                            Content = "人事变更记录已添加",
                            CloseButtonText = "确定",
                            XamlRoot = App.MainWindow.Content.XamlRoot
                        };
                        await successDialog.ShowAsync();

                        // 重新加载变更记录数据
                        await LoadPersonnelsDataAsync();
                    }
                }
                catch (Exception ex)
                {
                    // 显示错误提示
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
