using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using DAL;

namespace PL.ViewModels
{
    interface IUserVM
    {
        ObservableCollection<user> Users { get; set; }
    }
}
