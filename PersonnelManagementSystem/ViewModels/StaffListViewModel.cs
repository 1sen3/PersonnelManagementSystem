using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using PersonnelManagementSystem.Models;
using PersonnelManagementSystem.Services;

namespace PersonnelManagementSystem.ViewModels
{
    public class StaffListViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private ObservableCollection<Staff> _staffList;
        private bool _isLoading;
        public event PropertyChangedEventHandler PropertyChanged;
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
            _databaseService = new DatabaseService();
            StaffList = new ObservableCollection<Staff>();
            LoadStaffDataAsync();
        }
        public async Task LoadStaffDataAsync()
        {
            try
            {
                IsLoading = true;
                var staffs = await _databaseService.GetStaffListAsync();
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
    }
}
