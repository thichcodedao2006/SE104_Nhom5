using QLTB.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace QLTB.ViewModel
{
    // Material Model Class
    public class Material
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Unit { get; set; }
        public int CurrentStock { get; set; }
        public string MinMaxText { get; set; }
        public decimal Price { get; set; }
        public string Supplier { get; set; }
        public string StockStatus { get; set; }
        public string UpdatedDate { get; set; }
    }

    public class MaterialViewModel : BaseViewModel
    {
        public ObservableCollection<Material> Materials { get; set; }

        private Material _selectedMaterial;
        public Material SelectedMaterial
        {
            get => _selectedMaterial;
            set
            {
                _selectedMaterial = value;
                OnPropertyChanged(nameof(SelectedMaterial));
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }

        private bool _isFormOpen;
        public bool IsFormOpen
        {
            get => _isFormOpen;
            set
            {
                _isFormOpen = value;
                OnPropertyChanged(nameof(IsFormOpen));
            }
        }

        // Form properties
        private string _materialName;
        public string MaterialName
        {
            get => _materialName;
            set
            {
                _materialName = value;
                OnPropertyChanged(nameof(MaterialName));
            }
        }

        private string _materialCode;
        public string MaterialCode
        {
            get => _materialCode;
            set
            {
                _materialCode = value;
                OnPropertyChanged(nameof(MaterialCode));
            }
        }

        private int _currentStock;
        public int CurrentStock
        {
            get => _currentStock;
            set
            {
                _currentStock = value;
                OnPropertyChanged(nameof(CurrentStock));
            }
        }

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged(nameof(Price));
            }
        }

        private int _minStock;
        public int MinStock
        {
            get => _minStock;
            set
            {
                _minStock = value;
                OnPropertyChanged(nameof(MinStock));
            }
        }

        private int _maxStock;
        public int MaxStock
        {
            get => _maxStock;
            set
            {
                _maxStock = value;
                OnPropertyChanged(nameof(MaxStock));
            }
        }

        private string _supplier;
        public string Supplier
        {
            get => _supplier;
            set
            {
                _supplier = value;
                OnPropertyChanged(nameof(Supplier));
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public ICommand OpenAddMaterialFormCommand { get; set; }
        public ICommand CloseFormCommand { get; set; }
        public ICommand SaveMaterialCommand { get; set; }

        public MaterialViewModel()
        {
            // Khởi tạo dữ liệu mẫu
            //Materials = new ObservableCollection<Material>
            //{
            //    new Material
            //    {
            //        Name = "RAM DDR4 8GB",
            //        Code = "RAM-DDR4-8GB-001",
            //        Unit = "Cái",
            //        CurrentStock = 45,
            //        MinMaxText = "20 / 100",
            //        Price = 850000,
            //        Supplier = "Kingston Technology",
            //        StockStatus = "Còn hàng",
            //        UpdatedDate = "12/05/2026"
            //    } 
            //};
            //    new Material 
            //    { 
            //        Name = "SSD 256GB SATA", 
            //        Code = "SSD-256GB-002", 
            //        Unit = "Cái", 
            //        CurrentStock = 28, 
            //        MinMaxText = "15 / 80", 
            //        Price = 1200000, 
            //        Supplier = "Samsung Electronics", 
            //        StockStatus = "Còn hàng", 
            //        UpdatedDate = "11/05/2026" 
            //    },
            //    new Material 
            //    { 
            //        Name = "CPU Intel Core i5", 
            //        Code = "CPU-I5-12400-003", 
            //        Unit = "Cái", 
            //        CurrentStock = 12, 
            //        MinMaxText = "10 / 50", 
            //        Price = 4500000, 
            //        Supplier = "Intel Corporation", 
            //        StockStatus = "Còn hàng", 
            //        UpdatedDate = "10/05/2026" 
            //    },
            //    new Material 
            //    { 
            //        Name = "Mainboard B660M", 
            //        Code = "MB-B660M-004", 
            //        Unit = "Cái", 
            //        CurrentStock = 8, 
            //        MinMaxText = "10 / 40", 
            //        Price = 2800000, 
            //        Supplier = "ASUS", 
            //        StockStatus = "Sắp hết", 
            //        UpdatedDate = "09/05/2026" 
            //    },
            //    new Material 
            //    { 
            //        Name = "PSU 650W 80+ Bronze", 
            //        Code = "PSU-650W-005", 
            //        Unit = "Cái", 
            //        CurrentStock = 35, 
            //        MinMaxText = "15 / 60", 
            //        Price = 1500000, 
            //        Supplier = "Corsair", 
            //        StockStatus = "Còn hàng", 
            //        UpdatedDate = "08/05/2026" 
            //    },
            //    new Material 
            //    { 
            //        Name = "Case ATX Mid Tower", 
            //        Code = "CASE-ATX-006", 
            //        Unit = "Cái", 
            //        CurrentStock = 22, 
            //        MinMaxText = "10 / 50", 
            //        Price = 950000, 
            //        Supplier = "NZXT", 
            //        StockStatus = "Còn hàng", 
            //        UpdatedDate = "07/05/2026" 
            //    },
            //    new Material 
            //    { 
            //        Name = "GPU RTX 3060", 
            //        Code = "GPU-RTX3060-007", 
            //        Unit = "Cái", 
            //        CurrentStock = 5, 
            //        MinMaxText = "8 / 30", 
            //        Price = 8500000, 
            //        Supplier = "NVIDIA", 
            //        StockStatus = "Sắp hết", 
            //        UpdatedDate = "06/05/2026" 
            //    },
            //    new Material 
            //    { 
            //        Name = "HDD 1TB 7200RPM", 
            //        Code = "HDD-1TB-008", 
            //        Unit = "Cái", 
            //        CurrentStock = 52, 
            //        MinMaxText = "25 / 100", 
            //        Price = 950000, 
            //        Supplier = "Western Digital", 
            //        StockStatus = "Còn hàng", 
            //        UpdatedDate = "05/05/2026" 
            //    },
            //    new Material 
            //    { 
            //        Name = "Cooling Fan 120mm", 
            //        Code = "FAN-120MM-009", 
            //        Unit = "Cái", 
            //        CurrentStock = 68, 
            //        MinMaxText = "30 / 150", 
            //        Price = 250000, 
            //        Supplier = "Noctua", 
            //        StockStatus = "Còn hàng", 
            //        UpdatedDate = "04/05/2026" 
            //    },
            //    new Material 
            //    { 
            //        Name = "Thermal Paste", 
            //        Code = "PASTE-THERMAL-010", 
            //        Unit = "Tuýp", 
            //        CurrentStock = 15, 
            //        MinMaxText = "20 / 80", 
            //        Price = 150000, 
            //        Supplier = "Arctic", 
            //        StockStatus = "Sắp hết", 
            //        UpdatedDate = "03/05/2026" 
            //    },
            //    new Material 
            //    { 
            //        Name = "SATA Cable", 
            //        Code = "CABLE-SATA-011", 
            //        Unit = "Cái", 
            //        CurrentStock = 95, 
            //        MinMaxText = "40 / 200", 
            //        Price = 35000, 
            //        Supplier = "Generic", 
            //        StockStatus = "Còn hàng", 
            //        UpdatedDate = "02/05/2026" 
            //    },
            //    new Material 
            //    { 
            //        Name = "Monitor 24\" Full HD", 
            //        Code = "MON-24FHD-012", 
            //        Unit = "Cái", 
            //        CurrentStock = 18, 
            //        MinMaxText = "10 / 40", 
            //        Price = 3200000, 
            //        Supplier = "Dell", 
            //        StockStatus = "Còn hàng", 
            //        UpdatedDate = "01/05/2026" 
            //    }
            //};

            OpenAddMaterialFormCommand = new RelayCommand(o =>
            {
                IsFormOpen = true;
            });

            CloseFormCommand = new RelayCommand(o =>
            {
                IsFormOpen = false;
            });

            SaveMaterialCommand = new RelayCommand(o =>
            {
                IsFormOpen = false;
            });
        }
    }
}
