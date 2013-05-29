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
		public Boolean _allowclosing = false;
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
		public void showTimes(String teamA, long msA, String teamB, long msB){
			//MessageBox.Show(teamA+": "+msA+"\n"+teamB+": "+msB);
			teamATime.Content = String.Format("{1}: {0:F2}", (double)msA / 1000, teamA);
			teamATime.Foreground = Brushes.Red;
			teamBTime.Content = String.Format("{1}: {0:F2}", (double)msB / 1000, teamB);
			teamBTime.Foreground = Brushes.Red;
		}
		public void updateStopwatch(String teamA, long msA, bool rA, String teamB, long msB, bool rB)
		{
			//MessageBox.Show(teamA+": "+msA+"\n"+teamB+": "+msB);
			long ms = msA > msB ? msA : msB;
			stopwatchLabel.Content = String.Format("{0:N2}", (double)ms / 1000);
			if (!rA)
			{
				teamATime.Content = String.Format("{1}: {0:F2}", (double)msA / 1000, teamA);
				teamATime.Foreground = Brushes.Red;
			}
			else
			{
				teamATime.Content = "Adten!!";
				teamATime.Foreground = Brushes.White;
			}
			if (!rB)
			{
				teamBTime.Content = String.Format("{1}: {0:F2}", (double)msB / 1000, teamB);
				teamBTime.Foreground = Brushes.Red;
			}
			else
			{
				teamBTime.Content = "Adten!!";
				teamBTime.Foreground = Brushes.White;
			}
			
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = !_allowclosing;
		}
	}
}
