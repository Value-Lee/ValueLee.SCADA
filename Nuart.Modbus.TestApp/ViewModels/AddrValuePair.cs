namespace Nuart.Modbus.TestApp.ViewModels
{
    public class AddrValuePair : BindableBase
    {
        private int _addr;
        private bool _value;

        public int Addr
        {
            get { return _addr; }
            set { SetProperty(ref _addr, value); }
        }

        public bool Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }
    }
}