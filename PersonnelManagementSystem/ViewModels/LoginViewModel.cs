using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using PersonnelManagementSystem.Services;
using PersonnelManagementSystem.Views;

namespace PersonnelManagementSystem.ViewModels
{
    [ObservableObject]
    public partial class LoginViewModel
    {
        [ObservableProperty]
        private string _id;
        [ObservableProperty]
        private string _password;
        public LoginViewModel() {}
        [RelayCommand]
        public async void Login()
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
            else
            {
                await new ContentDialog
                {
                    Title = "登陆失败",
                    Content = "请在检查工号或密码后重试",
                    PrimaryButtonText = "好",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = App.MainWindow.Content.XamlRoot,
                }.ShowAsync();
            }
        }
        [RelayCommand]
        public async void ForgetPassword()
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "忘记密码",
                    PrimaryButtonText = "确定",
                    SecondaryButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = App.MainWindow.Content.XamlRoot,
                    IsPrimaryButtonEnabled = false
                };

                var stackPanel = new StackPanel { Spacing = 10 };

                // 工号输入框
                var IDTextBox = new TextBox
                {
                    Header = "工号",
                    PlaceholderText = "请输入工号",
                    Width = 300
                };

                // 验证方式选择框
                var VerifComboBox = new ComboBox
                {
                    Header = "验证方式",
                    Width = 300
                };
                VerifComboBox.Items.Add("手机号");
                VerifComboBox.Items.Add("邮箱");
                VerifComboBox.SelectedIndex = 0;

                // 验证信息输入框
                var VerifInfoTextBox = new TextBox
                {
                    Header = "手机号",
                    PlaceholderText = "请输入手机号",
                    Width = 300
                };

                // 新密码输入框
                var NewPasswordBox = new PasswordBox
                {
                    Header = "新密码",
                    PlaceholderText = "请输入新密码",
                    Width = 300
                };

                stackPanel.Children.Add(IDTextBox);
                stackPanel.Children.Add(VerifComboBox);
                stackPanel.Children.Add(VerifInfoTextBox);
                stackPanel.Children.Add(NewPasswordBox);

                dialog.Content = stackPanel;

                // 根据验证方式修改验证信息输入框
                VerifComboBox.SelectionChanged += (s, e) =>
                {
                    string selected = VerifComboBox.SelectedItem.ToString();
                    if (selected == "手机号")
                    {
                        VerifInfoTextBox.Header = "手机号";
                        VerifInfoTextBox.PlaceholderText = "请输入手机号";
                    }
                    else
                    {
                        VerifInfoTextBox.Header = "邮箱";
                        VerifInfoTextBox.PlaceholderText = "请输入邮箱地址";
                    }
                };

                void ValidateForm()
                {
                    bool isValid = !string.IsNullOrWhiteSpace(IDTextBox.Text) && !string.IsNullOrWhiteSpace(VerifInfoTextBox.Text) && !string.IsNullOrWhiteSpace(NewPasswordBox.Password);
                    dialog.IsPrimaryButtonEnabled = isValid;
                }

                ValidateForm();

                IDTextBox.TextChanged += (s, e) => ValidateForm();
                VerifInfoTextBox.TextChanged += (s, e) => ValidateForm();
                NewPasswordBox.PasswordChanged += (s, e) => ValidateForm();

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    string id = IDTextBox.Text;
                    string newPassword = NewPasswordBox.Password;
                    string verifInfo = VerifInfoTextBox.Text;

                    bool isSuccessed = await DatabaseService.EditPasswordAsync(id, newPassword, verifInfo, VerifComboBox.SelectedItem.ToString());

                    if (isSuccessed)
                    {
                        await new ContentDialog
                        {
                            Title = "修改成功",
                            Content = "成功修改密码",
                            PrimaryButtonText = "确定",
                            DefaultButton = ContentDialogButton.Primary,
                            XamlRoot = App.MainWindow.Content.XamlRoot
                        }.ShowAsync();
                    }
                    else
                    {
                        await new ContentDialog
                        {
                            Title = "修改失败",
                            Content = "请检查输入的信息并重试",
                            PrimaryButtonText = "确定",
                            DefaultButton = ContentDialogButton.Primary,
                            XamlRoot = App.MainWindow.Content.XamlRoot
                        }.ShowAsync();
                    }
                }
            }
            catch(Exception ex)
            {
                // 记录或显示错误
                Debug.WriteLine($"Error in ExecuteEditStaffInfoAsync: {ex.Message}");
                // 或者显示一个错误对话框
                await new ContentDialog
                {
                    XamlRoot = App.MainWindow.Content.XamlRoot,
                    Title = "错误",
                    Content = $"操作失败: {ex.Message}",
                    PrimaryButtonText = "确定"
                }.ShowAsync();
            }
        }
    }
}
