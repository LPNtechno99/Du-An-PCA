using System.ComponentModel;

namespace SHIV_PhongCachAm
{
    public class valueNguoidanhgiaObject
    {
        private int _max, _min;
        public valueDanhgiaOKNG giatriDanhgia;
        private danhgiaStruct _danhGia = new danhgiaStruct(false, false);
        public valueIntObject giatriThuan, giatriNghich, giatriMax;

        public valueNguoidanhgiaObject()
        {
            _max = 10;
            _min = 0;
            giatriThuan = new valueIntObject();
            giatriNghich = new valueIntObject();
            giatriMax = new valueIntObject();
            giatriDanhgia = new valueDanhgiaOKNG();
            giatriThuan.PropertyChanged += Update;
            giatriNghich.PropertyChanged += Update;
        }

        /// <summary>
        /// Cập nhật giá trị Độ lệch 2 chiều, và đánh giá OK/NG
        /// </summary>
        public void Update(object sender, PropertyChangedEventArgs e)
        {
            // Cập nhật giá trị Max
            if (giatriThuan.Value > giatriNghich.Value) giatriMax.Value = giatriThuan.Value;
            else giatriMax.Value = giatriNghich.Value;

            // Cập nhật kiểm tra lỗi OK/NG
            if (CheckAllow())
            {
                if (CheckInRange())
                {
                    _danhGia.OK = true;
                    giatriDanhgia.Value = 1;
                }
                else
                {
                    _danhGia.NG = true; _danhGia.OK = false;
                    giatriDanhgia.Value = 2;
                }
            }
            else giatriDanhgia.Value = 0;
        }

        /// <summary>
        /// Kiểm tra giá trị cả hai chiều khác mặc định thì cho phép cập nhật
        /// </summary>
        /// <returns></returns>
        private bool CheckAllow()
        {
            if ((giatriThuan.Value > 0) && (giatriNghich.Value > 0)) return true;
            return false;
        }

        /// <summary>
        /// Kiểm tra trong khoảng OK, NG
        /// </summary>
        /// <returns></returns>
        private bool CheckInRange()
        {
            if ((giatriNghich.Value > _min) && (giatriNghich.Value < _max) && (giatriThuan.Value > _min) && (giatriThuan.Value < _max)) return true;
            return false;
        }

    }
}
