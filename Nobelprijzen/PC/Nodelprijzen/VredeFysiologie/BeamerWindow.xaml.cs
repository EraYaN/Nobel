using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VredeFysiologie
{
	/// <summary>
	/// Interaction logic for BeamerWindow.xaml
	/// </summary>
	public partial class BeamerWindow : Window
	{
		public BeamerWindow()
		{
			InitializeComponent();
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.F11)
			{
				if (WindowState != System.Windows.WindowState.Maximized)
				{
					WindowStyle = System.Windows.WindowStyle.None;
					WindowState = System.Windows.WindowState.Maximized;
					ResizeMode = System.Windows.ResizeMode.NoResize;
				}
				else
				{
					WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
					WindowState = System.Windows.WindowState.Normal;
					ResizeMode = System.Windows.ResizeMode.CanResize;
				}
			}

		}
	}
}
