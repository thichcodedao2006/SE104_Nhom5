using QLTB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLTB.ViewModel
{
    public class ForgetPass3VM : BaseViewModel
    {
        private ForgetPass data;
        public ForgetPass3VM(ForgetPass data)
        {
            Data = data;
        }

        public ForgetPass Data { get => data; set => data = value; }
    }
}
