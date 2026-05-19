using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QLTB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLTB.ViewModel
{
    public class DashBoardViewModel : BaseViewModel
    {
        #region Property 
        private string totalDevice;
        private string deviceNeedRepair;
        private string deviceOverRepair;

        public string TotalDevice { get => totalDevice; set

            {
                totalDevice = value;
                OnPropertyChanged(nameof(TotalDevice));
            }
            }

        public string DeviceNeedRepair { get => deviceNeedRepair; set
            {
                deviceNeedRepair = value; OnPropertyChanged(nameof(DeviceNeedRepair));
            }
                }

        public string DeviceOverRepair { get => deviceOverRepair; set
            {
                deviceOverRepair = value; OnPropertyChanged( nameof(DeviceOverRepair));
            }
                }



        #endregion

        public DashBoardViewModel()
        {
            Reload();
        }
        #region Function
        public async Task Reload()
        {
            int totaldev = await DataProvider.Instance.DB.ChiTietThietBis.CountAsync();
            TotalDevice = totaldev.ToString();

        }
        #endregion
    }
}
