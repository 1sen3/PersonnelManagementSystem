using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using PersonnelManagementSystem.Models;
using PersonnelManagementSystem.Services;
using Windows.Globalization;

namespace PersonnelManagementSystem.ViewModels
{
    public class StaffListViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Staff> _staffList;
        private bool _isLoading;
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand AddNewStaffCommand { get;private set; }
        public ICommand EditStaffInfoCommand { get; private set; }
        public ObservableCollection<Staff> StaffList
        {
            get => _staffList;
            set
            {
                if(_staffList != value)
                {
                    _staffList = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }
        public StaffListViewModel()
        {
            StaffList = [];
            LoadStaffDataAsync();

            AddNewStaffCommand = new RelayCommand(ExecuteAddNewStaffAsync);
            EditStaffInfoCommand = new RelayCommand(ExecuteEditStaffInfoAsync);
        }
        public async Task LoadStaffDataAsync()
        {
            try
            {
                IsLoading = true;
                var staffs = await DatabaseService.GetStaffListAsync();
                StaffList.Clear();
                foreach (var staff in staffs) StaffList.Add(staff);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载员工数据出错: {ex.Message}");
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
        public async void ExecuteAddNewStaffAsync()
        {
            ContentDialog dialog = new()
            {
                XamlRoot = App.MainWindow.Content.XamlRoot,
                Title = "添加新员工",
                PrimaryButtonText = "确定",
                SecondaryButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                IsPrimaryButtonEnabled = false
            };

            var stackPanel = new StackPanel { Spacing = 10,Orientation = Orientation.Horizontal };

            var stackPanel1 = new StackPanel { Spacing = 10 };
            var stackPanel2 = new StackPanel { Spacing = 10 };
            stackPanel.Children.Add(stackPanel1);
            stackPanel.Children.Add(stackPanel2);

            // 姓名输入框
            var nameTextBox = new TextBox
            {
                Header = "姓名",
                PlaceholderText = "请输入姓名",
                Width = 200
            };

            // 性别选择框
            var genderComboBox = new ComboBox
            {
                Header = "性别",
                Width = 200,
                PlaceholderText = "请选择性别"
            };
            genderComboBox.Items.Add("男");
            genderComboBox.Items.Add("女");


            // 出生日期选择器
            var BirthdayCalenderDatePicker = new CalendarDatePicker
            {
                Header = "出生日期",
                PlaceholderText = "请选择出生日期",
                Width = 200,
                Language = "zh-CN"
            };

            // 部门选择器
            var DeptComboBox = new ComboBox
            {
                Header = "部门",
                PlaceholderText = "请选择部门",
                Width = 200
            };
            var depts = await DatabaseService.GetDepartmentListAsync();
            foreach (string d in depts) DeptComboBox.Items.Add(d);

            // 职位选择器
            var JobComboBox = new ComboBox
            {
                Header = "职位",
                PlaceholderText = "请选择职位",
                Width = 200
            };
            var jobs = await DatabaseService.GetJobListAsync();
            foreach (string j in jobs) JobComboBox.Items.Add(j);

            stackPanel1.Children.Add(nameTextBox);
            stackPanel1.Children.Add(genderComboBox);
            stackPanel1.Children.Add(BirthdayCalenderDatePicker);
            stackPanel1.Children.Add(DeptComboBox);
            stackPanel1.Children.Add(JobComboBox);

            // 教育程度选择器
            var EduComboBox = new ComboBox
            {
                Header = "受教育程度",
                PlaceholderText = "请选择受教育程度",
                Width = 200
            };
            var edus = await DatabaseService.GetEduListAsync();
            foreach (string e in edus) EduComboBox.Items.Add(e);

            // 专业技能输入框
            var SpecialtyTextBox = new TextBox
            {
                Header = "专业技能",
                PlaceholderText = "请输入专业技能",
                Width = 200
            };

            // 住址输入框
            var AddressTextBox = new TextBox
            {
                Header = "住址",
                PlaceholderText = "请输入住址",
                Width = 200
            };

            // 电话输入框
            var TelTextBox = new TextBox
            {
                Header = "电话",
                PlaceholderText = "请输入电话号码",
                Width = 200
            };

            // 邮箱输入框
            var EmailTextBox = new TextBox
            {
                Header = "邮箱",
                PlaceholderText = "请输入邮箱",
                Width = 200
            };

            stackPanel2.Children.Add(EduComboBox);
            stackPanel2.Children.Add(SpecialtyTextBox);
            stackPanel2.Children.Add(AddressTextBox);
            stackPanel2.Children.Add(TelTextBox);
            stackPanel2.Children.Add(EmailTextBox);

            dialog.Content = stackPanel;

            // 检查表单是否填满
            void ValidateForm()
            {
                bool isValid = !string.IsNullOrEmpty(nameTextBox.Text) && genderComboBox.SelectedItem != null && BirthdayCalenderDatePicker.Date != null
                               && DeptComboBox.SelectedItem != null && JobComboBox.SelectedItem != null && EduComboBox.SelectedItem != null && !string.IsNullOrWhiteSpace(SpecialtyTextBox.Text)
                               && !string.IsNullOrWhiteSpace(AddressTextBox.Text) && !string.IsNullOrWhiteSpace(TelTextBox.Text) && !string.IsNullOrWhiteSpace(EmailTextBox.Text);
                dialog.IsPrimaryButtonEnabled = isValid;
            }

            nameTextBox.TextChanged += (s, e) => ValidateForm();
            genderComboBox.SelectionChanged += (s, e) => ValidateForm();
            BirthdayCalenderDatePicker.DateChanged += (s, e) => ValidateForm();
            DeptComboBox.SelectionChanged += (s, e) => ValidateForm();
            JobComboBox.SelectionChanged += (s, e) => ValidateForm();
            EduComboBox.SelectionChanged += (s, e) => ValidateForm();
            SpecialtyTextBox.TextChanged += (s, e) => ValidateForm();
            AddressTextBox.TextChanged += (s, e) => ValidateForm();
            TelTextBox.TextChanged += (s, e) => ValidateForm();
            EmailTextBox.TextChanged += (s, e) => ValidateForm();

            ValidateForm();

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Staff NewStaff = new Staff
                {
                    Name = nameTextBox.Text,
                    Sex = genderComboBox.SelectedItem?.ToString(),
                    Birthday = BirthdayCalenderDatePicker.Date.Value.ToString("yyyy-MM-dd"),
                    Department = DeptComboBox.SelectedItem?.ToString(),
                    Job = JobComboBox.SelectedItem?.ToString(),
                    Education = EduComboBox.SelectedItem?.ToString(),
                    Specialty = SpecialtyTextBox.Text,
                    Address = AddressTextBox.Text,
                    Telephone = TelTextBox.Text,
                    Email = EmailTextBox.Text
                };

                await DatabaseService.AddNewStaffAsync(NewStaff);

                await new ContentDialog
                {
                    Title = "录入成功",
                    Content = "成功录入新员工的信息。",
                    PrimaryButtonText = "好",
                    XamlRoot = App.MainWindow.Content.XamlRoot
                }.ShowAsync();

                await LoadStaffDataAsync();
            }
        }
        public async void ExecuteEditStaffInfoAsync()
        {
            try
            {
                ContentDialog dialog = new()
                {
                    XamlRoot = App.MainWindow.Content.XamlRoot,
                    Title = "请先选择要修改信息的员工",
                    PrimaryButtonText = "确定",
                    SecondaryButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary,
                    IsPrimaryButtonEnabled = false
                };

                // 员工选择器
                var staffComboBox = new ComboBox
                {
                    Header = "员工",
                    PlaceholderText = "请选择员工",
                    Width = 200
                };
                var Staffs = await DatabaseService.GetStaffIdAndNameAsync();
                foreach (var s in Staffs) staffComboBox.Items.Add(s);

                dialog.Content = staffComboBox;

                void ValidateStaffChosen()
                {
                    bool isValid = staffComboBox.SelectedItem != null;
                    dialog.IsPrimaryButtonEnabled = isValid;
                }

                staffComboBox.SelectionChanged += (s, e) => ValidateStaffChosen();

                ValidateStaffChosen();

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    string ID = staffComboBox.SelectedItem?.ToString().Substring(0, 6);
                    var StaffInfo = await DatabaseService.GetStaffInfoByIDAsync(ID);
                    var NextDialog = new ContentDialog
                    {
                        XamlRoot = App.MainWindow.Content.XamlRoot,
                        Title = "修改员工信息",
                        PrimaryButtonText = "确定",
                        SecondaryButtonText = "取消",
                        DefaultButton = ContentDialogButton.Primary,
                        IsPrimaryButtonEnabled = false
                    };

                    var stackPanel = new StackPanel { Spacing = 10, Orientation = Orientation.Horizontal };
                    var stackPanel1 = new StackPanel { Spacing = 10 };
                    var stackPanel2 = new StackPanel { Spacing = 10 };
                    stackPanel.Children.Add(stackPanel1);
                    stackPanel.Children.Add(stackPanel2);

                    // 姓名输入框
                    var NameTextBox = new TextBox
                    {
                        Header = "姓名",
                        Text = StaffInfo.Name,
                        Width = 200
                    };

                    // 性别选择框
                    var GenderComboBox = new ComboBox
                    {
                        Header = "性别",
                        Width = 200
                    };
                    GenderComboBox.Items.Add("男");
                    GenderComboBox.Items.Add("女");
                    GenderComboBox.SelectedItem = StaffInfo.Sex;

                    // 生日日期选择器
                    var BirthdayDatePicker = new CalendarDatePicker
                    {
                        Header = "出生日期",
                        Width = 200
                    };
                    BirthdayDatePicker.Date = DateTime.ParseExact(StaffInfo.Birthday, "yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture);

                    // 教育水平选择器
                    var EduComboBox = new ComboBox
                    {
                        Header = "教育水平",
                        Width = 200
                    };
                    var edus = await DatabaseService.GetEduListAsync();
                    foreach (var e in edus) EduComboBox.Items.Add(e);
                    EduComboBox.SelectedItem = StaffInfo.Education;

                    stackPanel1.Children.Add(NameTextBox);
                    stackPanel1.Children.Add(GenderComboBox);
                    stackPanel1.Children.Add(BirthdayDatePicker);
                    stackPanel1.Children.Add(EduComboBox);

                    // 专业技能输入框
                    var SpecialtyTextBox = new TextBox
                    {
                        Header = "专业技能",
                        Text = StaffInfo.Specialty,
                        Width = 200
                    };

                    // 住址输入框
                    var AddressTextBox = new TextBox
                    {
                        Header = "住址",
                        Text = StaffInfo.Address,
                        Width = 200
                    };

                    // 电话输入框
                    var TelTextBox = new TextBox
                    {
                        Header = "电话",
                        Text = StaffInfo.Telephone,
                        Width = 200
                    };

                    // 邮箱输入框
                    var EmailTextBox = new TextBox
                    {
                        Header = "邮箱",
                        Text = StaffInfo.Email,
                        Width = 200
                    };

                    stackPanel2.Children.Add(SpecialtyTextBox);
                    stackPanel2.Children.Add(AddressTextBox);
                    stackPanel2.Children.Add(TelTextBox);
                    stackPanel2.Children.Add(EmailTextBox);

                    NextDialog.Content = stackPanel;

                    // 检查是否更新了信息
                    void CheckChanged()
                    {
                        string name = NameTextBox.Text;
                        string birthday = BirthdayDatePicker.Date.Value.ToString("yyyy-MM-dd");
                        string specialty = SpecialtyTextBox.Text;
                        string address = AddressTextBox.Text;
                        string tel = TelTextBox.Text;
                        string email = EmailTextBox.Text;
                        bool isChanged = !String.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(specialty) && !string.IsNullOrWhiteSpace(address) && !string.IsNullOrWhiteSpace(tel) 
                                         && !string.IsNullOrWhiteSpace(email) && (name != StaffInfo.Name || GenderComboBox.SelectedItem.ToString() != StaffInfo.Sex 
                                         || birthday != StaffInfo.Birthday || EduComboBox.SelectedItem.ToString() != StaffInfo.Education || specialty != StaffInfo.Specialty || address != StaffInfo.Address
                                         || tel != StaffInfo.Telephone || email != StaffInfo.Email);
                        NextDialog.IsPrimaryButtonEnabled = isChanged;
                    }

                    CheckChanged();

                    NameTextBox.TextChanged += (s, e) => CheckChanged();
                    GenderComboBox.SelectionChanged += (s, e) => CheckChanged();
                    BirthdayDatePicker.DateChanged += (s, e) => CheckChanged();
                    EduComboBox.SelectionChanged += (s, e) => CheckChanged();
                    SpecialtyTextBox.TextChanged += (s, e) => CheckChanged();
                    AddressTextBox.TextChanged += (s, e) => CheckChanged();
                    TelTextBox.TextChanged += (s, e) => CheckChanged();
                    EmailTextBox.TextChanged += (s, e) => CheckChanged();

                    var nextResult = await NextDialog.ShowAsync();

                    if(nextResult == ContentDialogResult.Primary)
                    {
                        StaffInfo.Name = NameTextBox.Text;
                        StaffInfo.Sex = GenderComboBox.SelectedItem?.ToString();
                        StaffInfo.Birthday = BirthdayDatePicker.Date.Value.ToString("yyyy-MM-dd");
                        StaffInfo.Education = EduComboBox.SelectedItem?.ToString();
                        StaffInfo.Specialty = SpecialtyTextBox.Text;
                        StaffInfo.Address = AddressTextBox.Text;
                        StaffInfo.Telephone = TelTextBox.Text;
                        StaffInfo.Email = EmailTextBox.Text;

                        await DatabaseService.EditStaffInfoAsync(StaffInfo);

                        await new ContentDialog
                        {
                            Title = "修改成功",
                            Content = "成功修改员工信息",
                            PrimaryButtonText = "确定",
                            XamlRoot = App.MainWindow.Content.XamlRoot
                        }.ShowAsync();

                        await LoadStaffDataAsync();
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
