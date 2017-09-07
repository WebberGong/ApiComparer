using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace ApiComparer.UserControl
{
    /// <summary>
    ///     FileSelect.xaml 的交互逻辑
    /// </summary>
    public partial class SelectFile : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register("FilePath",
            typeof(string), typeof(SelectFile));

        private static string _lastOpenedDirectory = Directory.GetCurrentDirectory();

        public SelectFile()
        {
            InitializeComponent();
        }

        public string FilePath
        {
            get { return GetValue(FilePathProperty) as string; }
            set
            {
                SetValue(FilePathProperty, value);
                OnPropertyChanged("FilePath");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void DoSelectFile(Action<string, string> fileSelected)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "文本文件|*.txt",
                Multiselect = false,
                RestoreDirectory = true,
                InitialDirectory = IsFilePathValid(FilePath) ? new FileInfo(FilePath).DirectoryName : _lastOpenedDirectory
            };
            if (dialog.ShowDialog(Window.GetWindow(this)) == true)
            {
                fileSelected?.Invoke(dialog.SafeFileName, dialog.FileName);
            }
        }

        private bool IsFilePathValid(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || filePath == "文本1" || filePath == "文本2")
            {
                return false;
            }
            return true;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void File_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DoSelectFile((fileName, filePath) =>
            {
                FilePath = filePath;
                _lastOpenedDirectory = new FileInfo(FilePath).DirectoryName;
            });
        }
    }
}