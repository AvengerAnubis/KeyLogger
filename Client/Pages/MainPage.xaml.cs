using SharpMacroPlayer.Macros;
using SharpMacroPlayer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
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
using static SharpMacroPlayer.Utils.Constants;

namespace SharpMacroPlayer.Pages
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            this.DataContext = MainWindow.Instance;
            MainWindow.Instance.InterceptKeys.KeyInput += (o, args) =>
            {
                KBDLLHOOKSTRUCT data = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(args.lParam);

                if (data.VkCode == (ushort)VK.VK_F1 && args.wParam == (IntPtr)WM.KEYDOWN)
                {
                    MainWindow.Instance.StartStopRecording(recordButton, null);
                }
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.PlayStop(sender, e);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.StartStopRecording(sender, e);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.CreateBindingProfile();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.DeleteBindingProfile();
        }
    }
}
