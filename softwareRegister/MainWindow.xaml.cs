﻿using System;
using System.Diagnostics;
using System.IO;
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
        public MainWindow()
        {
            InitializeComponent();
        }
        
        // todo
        // Improve on the UI
        // Make it so the user can tick boxes for which places they want it to be saved in
        // 


        public static ExecutableObject CurrentExecutable;
        
        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".exe", Filter = "Exe Files (.exe)|*.exe|All Files (*.*)|*.*", FilterIndex = 1
            };

            // Display OpenFileDialog by calling ShowDialog method 
            var result = openFileDialog.ShowDialog();
            
            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                var filename = openFileDialog.FileName;
                FilenameDisplay.Content = filename;
                CurrentExecutable = new ExecutableObject(openFileDialog.FileName,openFileDialog.SafeFileName);
            }
        }

        private void RegisterButton_onClick(object sender, RoutedEventArgs e)
        {
            CurrentExecutable.RegisterInWindows();
        }
        
        private void UnregisterButton_onClick(object sender, RoutedEventArgs e)
        {
            CurrentExecutable.DeregisterInWindows();
        }
    }
}