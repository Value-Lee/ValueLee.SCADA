using XamlPearls;

namespace Nuart.Modbus.TestApp.ViewModels
{
    public class AddrValuePairObservableCollection : ObservableCollection<AddrValuePair>
    {
        public AddrValuePairObservableCollection()
        {
            SlaveUnit = -1;
            StartAddr = -1;
            Quantity = -1;
        }

        public int Quantity { get; set; }
        public int SlaveUnit { get; set; }
        public int StartAddr { get; set; }
    }
}