namespace SHIV_PhongCachAm
{
    public class valueParse : ObservableObject
    {
        private bool value;

        public bool Value
        {
            get { return value; }
            set
            {
                this.value = value;
                OnPropertyChanged("Value");
            }
        }

        public valueParse()
        {
            value = false;
        }

    }
}
