
namespace SHIV_PhongCachAm
{
    public class specialOutputInfo : ObservableObject
    {
        private int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _info;

        public string Info
        {
            get { return _info; }
            set { _info = value; }
        }


    }
}
