namespace SHIV_PhongCachAm
{
    public class labelObject : ObservableObject
    {
        private string _label;

        public labelObject()
        {
            _label = "";
        }

        public string Value
        {
            get { return _label; }
            set
            {
                _label = value;
                OnPropertyChanged("Value");
            }
        }

    }
}
