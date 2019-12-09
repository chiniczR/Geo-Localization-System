using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using DAL;

namespace PL.ViewModels
{
    public interface IFallVM
    {
        ObservableCollection<Fall> Falls { get; set; }
    }
}
