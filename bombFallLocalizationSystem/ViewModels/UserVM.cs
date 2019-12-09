using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using PL.Models;
using DAL;
using System.ComponentModel;

namespace PL.ViewModels
{
    public class UserVM : INotifyPropertyChanged, IUserVM
    {
        public ManageUsersModel CurrentModel { get; set; }
        public ObservableCollection<user> Users { get; set; }

        public UserVM()
        {
            CurrentModel = new ManageUsersModel();
            Users = new ObservableCollection<user>(CurrentModel.db.users);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Users_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                // Insertion
                CurrentModel.db.users.Add(e.NewItems[0] as user);
            }
        }
    }
}
