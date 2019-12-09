using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bombFallLocalizationSystem.Commands;
using DAL;
using PL.Models;

namespace PL.ViewModels
{
    public class FallVM : INotifyPropertyChanged, IFallVM
    {
        public AddFallCommand addFallCommand { get; set; }
        public SearchFallCommand Search { get; set; }
        public ManageFallsModel CurrentModel { get; set; }
        public ObservableCollection<Fall> Falls { get; set; }

        public FallVM()
        {
            Search = new SearchFallCommand();
            CurrentModel = new ManageFallsModel();
            Falls = new ObservableCollection<Fall>(CurrentModel.db.falls);
            addFallCommand = new AddFallCommand();
            addFallCommand.AddFallNeeded += AddFallCommand_AddFallNeeded;
            Falls.CollectionChanged += Falls_CollectionChanged;
        }

        private void AddFallCommand_AddFallNeeded(float x, float y, DateTime date)
        {
            CurrentModel.db = new Model1();
            IEnumerable<Fall> falls = CurrentModel.db.falls;
            int id = falls.OrderBy<Fall, int>(f => f.id).LastOrDefault().id;
            Falls.Add(new Fall { id = id+1, x = x, y = y, date = date });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Falls_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                // Insertion
                CurrentModel.db.falls.Add(e.NewItems[0] as Fall);
                CurrentModel.db.SaveChanges();
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                // Deletion
                Fall toRem = e.OldItems[0] as Fall;
                try
                {
                    CurrentModel.Remove(toRem.id);
                    CurrentModel.db.SaveChanges();
                }
                catch (Exception)
                { } // Just in Case
            }
        }
    }
}
