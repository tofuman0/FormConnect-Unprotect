using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FormConnect_Unprotect
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private fmc FMC = new fmc();

        public MainWindow()
        {
            InitializeComponent();
        }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "FormConnect Form|*.fmc|All Files|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    TbStatus.Text = $"Loading...\r\n{openFileDialog.FileName}";
                    FMC.LoadFMC(openFileDialog.FileName);
                    SetStatusString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Loading FMC File", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "FormConnect Form|*.fmc|All Files|*.*";
            if(saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    FMC.SaveFMC(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Saving FMC File", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void About_Click(object sender, RoutedEventArgs e)
        {
            About aboutWindow = new About();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }

        private void Unprotect_Click(object sender, RoutedEventArgs e)
        {
            if (FMC.Loaded == true)
            {
                FMC.SetProtection1(false);
                FMC.SetProtection2(false);
            }
            SetStatusString();
        }
        private void SetStatusString()
        {
            if (FMC.Loaded == true)
            {
                TbStatus.Text = $"Form name: {FMC.GetFormName()}\r\nFile Size: {FMC.GetFMCSize()}bytes\r\nForm Page Size: {FMC.GetPaperSize()}\r\nForm Language: {FMC.GetLanguage()}\r\nFile Type: {FMC.GetDescription()}\r\nForm type: {FMC.GetFormat()}\r\nForm Compatibility: {FMC.GetCompatibility()}\r\nForm element count: {FMC.GetElementCount()}\r\nProtected: {((FMC.GetProtection1() || FMC.GetProtection2()) ? "YES" : "NO")}";
                MiSave.IsEnabled = true;
                BtnUnprotect.IsEnabled = true;
            }
            else
            {
                TbStatus.Text = "Form Not Loaded...";
                MiSave.IsEnabled = false;
                BtnUnprotect.IsEnabled = false;
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            try
            {
                var dropped = (string[])e.Data.GetData(DataFormats.FileDrop);
                var files = dropped.ToList();

                if (!files.Any())
                    return;

                TbStatus.Text = $"Loading...\r\n{files.First()}";
                FMC.LoadFMC(files.First());
                SetStatusString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Loading FMC File", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
