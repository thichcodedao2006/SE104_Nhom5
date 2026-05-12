using QLTB.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace QLTB.ViewModel
{
    public class Employee
    {
        public string Name { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Department { get; set; }
        public string Status { get; set; }
        public Brush StatusColor => Status == "Active" ? Brushes.Green : Brushes.Gray;
    }

    public class EmployeeViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Employee> Employees { get; set; }
        public ObservableCollection<Employee> FilteredEmployees { get; set; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                FilterEmployees();
                OnPropertyChanged(nameof(SearchText));
            }
        }

        public int TotalEmployees => Employees.Count;
        public int ActiveEmployees => Employees.Count(e => e.Status == "Active");
        public int DepartmentsCount => Employees.Select(e => e.Department).Distinct().Count();
        public int RolesCount => Employees.Select(e => e.Position).Distinct().Count();

        public ICommand AddEmployeeCommand { get; set; }
        public ICommand EditEmployeeCommand { get; set; }
        public ICommand DeleteEmployeeCommand { get; set; }

        public EmployeeViewModel()
        {
            Employees = new ObservableCollection<Employee>
            {
                new Employee { Name="John Doe", Position="Senior Technician", Email="john.doe@company.com", Phone="+1 555 123-4567", Department="Maintenance", Status="Active" },
                new Employee { Name="Jane Smith", Position="Maintenance Engineer", Email="jane.smith@company.com", Phone="+1 555 234-5678", Department="Engineering", Status="Active" },
                new Employee { Name="Bob Johnson", Position="Technician", Email="bob.johnson@company.com", Phone="+1 555 345-6789", Department="Maintenance", Status="Active" },
                new Employee { Name="Alice Brown", Position="Supervisor", Email="alice.brown@company.com", Phone="+1 555 456-7890", Department="Operations", Status="Active" },
                new Employee { Name="Charlie Wilson", Position="Technician", Email="charlie.wilson@company.com", Phone="+1 555 567-8901", Department="Maintenance", Status="Inactive" },
            };

            FilteredEmployees = new ObservableCollection<Employee>(Employees);

            // Thêm lệnh mẫu, bạn implement logic riêng sau
            AddEmployeeCommand = new RelayCommand(o => { /* implement add */ });
            EditEmployeeCommand = new RelayCommand(o => { /* implement edit */ });
            DeleteEmployeeCommand = new RelayCommand(o => { /* implement delete */ });
        }

        private void FilterEmployees()
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                FilteredEmployees = new ObservableCollection<Employee>(Employees);
            }
            else
            {
                FilteredEmployees = new ObservableCollection<Employee>(
                    Employees.Where(e => e.Name.Contains(SearchText)
                                        || e.Email.Contains(SearchText)
                                        || e.Position.Contains(SearchText)));
            }
            OnPropertyChanged(nameof(FilteredEmployees));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
