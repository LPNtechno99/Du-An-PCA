using System;
using System.ComponentModel;

namespace SHIV_PhongCachAm
{
    public class DataStoreObject : ObservableObject
    {
        private float _max, _min, _doLech;
        private danhgiaStruct _danhGia = new danhgiaStruct(false, false);
        // Khởi tạo giá trị thuận nghịch, mặc định = -1
        private valueObject giatriThuan, giatriNghich;
        private valueDanhgiaOKNG giatriDanhgia;
        public valueObject GiatriThuan
        {
            get { return giatriThuan; }
            set
            {
                giatriThuan = value;
                OnPropertyChanged("GiatriThuan");
            }
        }
        public valueObject GiatriNghich
        {
            get { return giatriNghich; }
            set
            {
                giatriNghich = value;
                OnPropertyChanged("GiatriNghich");
            }
        }
        public valueDanhgiaOKNG GiatriDanhgia
        {
            get { return giatriDanhgia; }
            set
            {
                giatriDanhgia = value;
                OnPropertyChanged("GiatriDanhgia");
            }
        }
        public valueObject giatriLech;
        public bool dataTiengon = false;

        public DataStoreObject()
        {
            _max = (float)(1);
            _min = (float)(0);
            GiatriThuan = new valueObject();
            GiatriNghich = new valueObject();
            giatriLech = new valueObject();
            GiatriDanhgia = new valueDanhgiaOKNG();
            GiatriThuan.PropertyChanged += Update;
            GiatriNghich.PropertyChanged += Update;

            OnPropertyChanged("GiatriNghich");
            OnPropertyChanged("GiatriThuan");
            OnPropertyChanged("GiatriDanhgia");
        }

        public float Max
        {
            get { return _max; }
            set { _max = value; }
        }

        public float Min
        {
            get { return _min; }
            set { _min = value; }
        }

        public float Dolech
        {
            get { return _doLech; }
            set { _doLech = value; }
        }

        public danhgiaStruct Danhgia
        {
            get { return _danhGia; }
            private set { }
        }

        /// <summary>
        /// Cập nhật giá trị Độ lệch 2 chiều, và đánh giá OK/NG
        /// </summary>
        public void Update(object sender, PropertyChangedEventArgs e)
        {
            _doLech = GiatriNghich.Value - GiatriThuan.Value;
			if ((_doLech > 3) || (_doLech < -3))
			{
				giatriLech.Value = (float)Math.Round(_doLech);
			}
			else giatriLech.Value = _doLech;

			if (CheckAllow())
            {
                if (CheckInRange())
                {
                    _danhGia.OK = true;
                    GiatriDanhgia.Value = 1;
                }
                else
                {
                    _danhGia.NG = true; _danhGia.OK = false;
                    GiatriDanhgia.Value = 2;
                }
                if ((Math.Abs(giatriLech.Value) > 3) && dataTiengon)
                {
                    giatriLech.Color = 2;
                    _danhGia.NG = true;
                    _danhGia.OK = false;
                    GiatriDanhgia.Value = 2;
                }
                else giatriLech.Color = 0;
            }
            else
            {
                GiatriDanhgia.Value = 0;
            }
            OnPropertyChanged("GiatriNghich");
            OnPropertyChanged("GiatriThuan");
            OnPropertyChanged("GiatriDanhgia");
        }

        /// <summary>
        /// Kiểm tra trong khoảng Max, Min
        /// </summary>
        /// <returns></returns>
        private bool CheckAllow()
        {
            if ((GiatriThuan.Value > 0.0) && (GiatriNghich.Value > 0.001)) return true;
            return false;
        }

        /// <summary>
        /// Kiểm tra cho phép cập nhật OK/NG
        /// </summary>
        /// <returns></returns>
        private bool CheckInRange()
        {
            if ((GiatriNghich.Value >= _min) && (GiatriNghich.Value <= _max) && (GiatriThuan.Value >= _min) && (GiatriThuan.Value < _max)) return true;
            return false;
        }

        public string GetString()
        {
            string temp = "";
            temp += GiatriThuan.Value + "," + GiatriNghich.Value + "," + _doLech + ",";
            if (GiatriDanhgia.Value == 1) temp += "OK,";
            else temp += "NG,";
            return temp;
        }
    }
}
