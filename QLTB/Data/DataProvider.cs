using QLTB.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLTB.Data
{
    public class DataProvider
    {
        private static DataProvider instance;

        public static DataProvider Instance
        {
                get
            {
                if (instance == null)
                {
                    instance = new DataProvider();
                };
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        public QuanLyVatTuContext DB { get; set; }  

        private DataProvider()
        {
            DB = new QuanLyVatTuContext();
        }
    }
}
