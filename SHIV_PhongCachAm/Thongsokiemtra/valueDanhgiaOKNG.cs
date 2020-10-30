using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHIV_PhongCachAm
{
    public class valueDanhgiaOKNG : ObservableObject
    {
        private int _value = 0;
        public valueDanhgiaOKNG()
        {
        }
        public int Value
        {
            get { return _value; }
            set { _value = value; OnPropertyChanged("Value"); }
        }
    }
}
