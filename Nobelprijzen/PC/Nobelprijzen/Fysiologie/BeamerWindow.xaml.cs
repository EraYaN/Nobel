using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Nobel
{
	/// <summary>
	/// Interaction logic for BeamerWindow.xaml
	/// </summary>
	public partial class BeamerWindow : Window
	{
		public Boolean _allowclosing = false;
		public long lastCount = 0;
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
		public void showTimes(String teamA, long msA, String teamB, long msB)
		{
			//MessageBox.Show(teamA+": "+msA+"\n"+teamB+": "+msB);
            stopwatchLabel.Content = String.Format("{0} wins!", (msA < msB ? teamA : teamB));
			teamATime.Content = String.Format("{1}: {0:F2}", (double)msA / 1000, teamA);
			teamATime.Foreground = Brushes.White;
			teamBTime.Content = String.Format("{1}: {0:F2}", (double)msB / 1000, teamB);
			teamBTime.Foreground = Brushes.White;
		}
		public void clearTimes()
		{
			teamATime.Content = "";
			teamATime.Foreground = Brushes.White;
			teamBTime.Content = "";
			teamBTime.Foreground = Brushes.White;
			stopwatchLabel.Content = "0.0";
		}
		public void updatePrematch(int sPM, String teamA, String teamB)
		{
			stopwatchLabel.Content = String.Format("{0:N0}", sPM);

            teamATime.Content = String.Format("{0} klaar?", teamA);
			teamATime.Foreground = Brushes.White;

            teamBTime.Content = String.Format("{0} klaar?", teamB);
			teamBTime.Foreground = Brushes.White;

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
				teamATime.Content = String.Format("{0} adten!!", teamA);
				teamATime.Foreground = Brushes.White;
			}
			if (!rB)
			{
				teamBTime.Content = String.Format("{1}: {0:F2}", (double)msB / 1000, teamB);
				teamBTime.Foreground = Brushes.Red;
			}
			else
			{
                teamBTime.Content = String.Format("{0} adten!!", teamB);
				teamBTime.Foreground = Brushes.White;
			}

		}
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = !_allowclosing;
		}
	}
}
