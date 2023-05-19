using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CountriesWPF.Modelos.Servicos
{
    public class DialogService
    {
        /// <summary>
        /// Displays a message dialog box with the specified title and message.
        /// </summary>
        /// <param name="title">The title of the dialog box.</param>
        /// <param name="message">The message to be displayed.</param>
        public void ShowMessage(string title, string message)
        {
            MessageBox.Show(message, title);
        }
    }
}