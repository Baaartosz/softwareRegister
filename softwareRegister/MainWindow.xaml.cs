using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using static System.Console;

namespace softwareRegister
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        enum Status
        {
            No = 0,
            Yes = 1
        }
    
        public MainWindow()
        {
            InitializeComponent();
        }
        
        // private void button1_Click(object sender, RoutedEventArgs e)
        // {
        //     // Create OpenFileDialog 
        //     Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
        //
        //
        //
        //     // Set filter for file extension and default file extension 
        //     dlg.DefaultExt = ".png";
        //     dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif"; 
        //
        //
        // }

        public static ExecutableObject _currentExecutable;
        
        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.DefaultExt = ".exe";
            openFileDialog.Filter = "Exe Files (.exe)|*.exe|All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            
            // Display OpenFileDialog by calling ShowDialog method 
            var result = openFileDialog.ShowDialog();
            
            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                
                // Open document 
                var filename = openFileDialog.FileName;
                FilenameDisplay.Content = filename;
                _currentExecutable = new ExecutableObject(openFileDialog.FileName,openFileDialog.SafeFileName);
            }
        }

        private void RegisterButton_onClick(object sender, RoutedEventArgs e)
        {
            _currentExecutable.RegisterInWindows();
        }
    }
}