using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Hardcodet.Wpf.TaskbarNotification;

namespace SkinnableApp.Commands
{

    public class ShowMainFormCommand : ICommand
    {
        public void Execute(object parameter)
        {
            MainWindow.mainWindow.InvertWindowVisibility();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }



  
}
