using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamlPearls;

namespace Nuart.Modbus.TestApp.ViewModels
{
    public class FuncCodeMenuViewModel
    {
        private readonly IRegionManager _regionManager;

        public FuncCodeMenuViewModel(IRegionManager regionManager)
        {
            FuncCodeMenus = new int[]
            {
                1,2,3,4,5,6,15,16,23
            }.Select(x => x);

            this._regionManager = regionManager;
        }

        public IEnumerable<int> FuncCodeMenus { get; }

        public void RequestNavigation(int funcCode)
        {
            string navigationTarget = $"Func{funcCode}";
            string lastTarget = _regionManager.Regions["ContentRegion"].NavigationService.Journal.CurrentEntry == null ?
                string.Empty : _regionManager.Regions["ContentRegion"].NavigationService.Journal.CurrentEntry.Uri.ToString();
            if (!string.Equals(lastTarget, navigationTarget, StringComparison.OrdinalIgnoreCase))
            {
                _regionManager.Regions["ContentRegion"].RequestNavigate(navigationTarget);
            }
        }
    }
}