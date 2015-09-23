using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Input;

namespace ImportManager.Logic
{
    class ImportCommand : ICommand
    {
        private ImportViewModel viewModel;
        public ImportCommand(ImportViewModel _viewModel)
        {
            viewModel = _viewModel;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            //TODO: Change to: viewModel.Rows.Count > 0
            //This requires listening though.
            //TODO: Temporarily disable while import is running
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.Import();
        }
    }
}
