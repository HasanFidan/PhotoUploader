using PhotoUploader.Service;
using PhotoUploader.ViewModel;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace PhotoUploader.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class UploaderView : Window
    {
        public UploaderView()
        {
            InitializeComponent();

            //int top = FaktoriyelRecursive(5);
            
            //MessageBox.Show(top.ToString());
            //result = 0;
            //top = FaktoriyelByFor(5);


            UploaderViewModel viewmodel = GetViewModel;
            //viewmodel.parentWindow = this;
           
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                UploaderViewModel vmodel = GetViewModel;
                //vmodel.pwdManager.Dispose();
            }
            catch (Exception ex)
            { 
            
            }
            
        }


        public UploaderViewModel GetViewModel 
        {
            get {

              return (UploaderViewModel)  this.DataContext;
            
            }
                    
        }


        int result = 0;

        public int FaktoriyelRecursive(int numbertobefactoriel)
        {
            
            if (numbertobefactoriel <= 1)
                return 1;

            return numbertobefactoriel * FaktoriyelRecursive(numbertobefactoriel - 1);
            
        }


        public int FaktoriyelByFor(int numbertobefactoriel)
        {
            for (int number = 1; number <= numbertobefactoriel; number++)
                if (result == 0)
                    result = number;
                else
                    result = result * number;

            return result;
        }

        private void LoadingAnimation_Loaded(object sender, RoutedEventArgs e)
        {

        }

      
        

      
    }
}
