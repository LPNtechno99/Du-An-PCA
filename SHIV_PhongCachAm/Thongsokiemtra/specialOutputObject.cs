
namespace SHIV_PhongCachAm
{
    public class specialOutputObject : ObservableObject
    {
        private string _name;

        public string UserName
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _info;

        public string Info
        {
            get { return _info; }
            set { _info = value; }
        }

        public specialOutputObject()
        {
            _name = "";
            _info = "";
        }
    }
}
