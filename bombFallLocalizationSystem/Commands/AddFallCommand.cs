using PL.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace bombFallLocalizationSystem.Commands
{
    public class AddFallCommand : ICommand
    {
        public IFallVM CurrentVM { get; }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public event Action<float,float,DateTime> AddFallNeeded;
        public AddFallCommand()
        {

        }
        public AddFallCommand(Action<float,float,DateTime> execute)
        {
            AddFallNeeded = execute;
        }
        public AddFallCommand(IFallVM Vm)
        {
            CurrentVM = Vm;
        }
        public bool CanExecute(object parameter)
        {
            if (parameter != null)
            {
                var Good = parameter.ToString();
                if ((Good.Length > 0))
                    return true;
            }
            return false;
        }

        public void Execute(float x, float y, DateTime date)
        {
            if (AddFallNeeded != null)
                AddFallNeeded(x, y, date);

        }

        public void Execute(object parameter)
        {
            
        }
    }
}
