using SharpMacroPlayer.ClientNew.ViewModels;
using SharpMacroPlayer.ClientNew.Views.Pages;
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
using System.Windows.Shapes;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace SharpMacroPlayer.ClientNew.Views.Windows
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : FluentWindow, INavigationWindow
	{
		public MainWindowViewModel ViewModel { get; init; }

		public MainWindow(MainWindowViewModel vm, IPageService pageService, INavigationService navigationService, IServiceProvider serviceProvider)
		{
			ViewModel = vm;
			DataContext = this;

			InitializeComponent();

			SystemThemeWatcher.Watch(this);
			SetPageService(pageService);
			SetServiceProvider(serviceProvider);
		}

		public void CloseWindow() => Close();

		public INavigationView GetNavigation() => start;

		public bool Navigate(Type pageType) => start.Navigate(pageType);

		public void SetPageService(IPageService pageService) => start.SetPageService(pageService);

		public void SetServiceProvider(IServiceProvider serviceProvider) => start.SetServiceProvider(serviceProvider);

		public void ShowWindow() => Show();

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Make sure that closing this window will begin the process of closing the application.
            Application.Current.Shutdown();
        }
    }
}
