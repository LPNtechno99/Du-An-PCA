namespace SHIV_PhongCachAm
{
    public class ChartViewItem
    {
        private float _key;
        private float _value;
        public float Key
        {
            get
            {
                //int temp;
                //if (_key != null) temp = int.Parse(_key);
                //else temp = 0;
                //if (temp % 10 == 0) return _key;
                //else return _key;
                return _key;
            }
            set
            {
                _key = value;
            }
        }
        public float Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }
    }
}
