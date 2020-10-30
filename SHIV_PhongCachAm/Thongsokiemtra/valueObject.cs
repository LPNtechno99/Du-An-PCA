namespace SHIV_PhongCachAm
{
    public class valueObject : ObservableObject
    {
        private float _value = (float)-0.00001;

        public valueObject()
        {
        }

        public float Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged("Value");
            }
        }

        private int _color = 0;

        public int Color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged("Color");
            }
        }

        public override string ToString()
        {
            return _value.ToString("0.00");
        }
    }

    public class valueIntObject : ObservableObject
    {
        private int _value = 0;

        public valueIntObject()
        {
        }

        public int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged("Value");
            }
        }
    }

    public struct danhgiaStruct
    {
        public bool OK;
        public bool NG;
        public danhgiaStruct(bool gtOK, bool gtNG)
        {
            this.OK = gtOK;
            this.NG = gtNG;
        }
    }

    public struct dulieuStruct
    {
        public float[] thuan;
        public float[] nghich;
        public dulieuStruct(int a, int b)
        {
            this.nghich = new float[b];
            this.thuan = new float[a];
        }
    }

    public struct checkDone
    {
        public bool vongQuay;
        public bool dongDien;
        public bool nhapLuc;
        public bool doRung;
        public bool tiengOn;
        public int countDorung;
        public int countTiengon;
        public checkDone(bool i)
        {
            vongQuay = dongDien = nhapLuc = doRung = tiengOn = false;
            countDorung = countTiengon = 0;
        }
        public bool Sum()
        {
            return (vongQuay && dongDien && nhapLuc && doRung && tiengOn);
        }
    }
}
