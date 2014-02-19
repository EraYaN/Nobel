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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
			teamATime.Content = String.Format("{1}: {0:F2}", (double)msA / 1000, teamA);
			teamATime.Foreground = Brushes.Red;
			teamBTime.Content = String.Format("{1}: {0:F2}", (double)msB / 1000, teamB);
			teamBTime.Foreground = Brushes.Red;
		}
		public void clearTimes()
		{
			teamATime.Content = "";
			teamATime.Foreground = Brushes.White;
			teamBTime.Content = "";
			teamBTime.Foreground = Brushes.White;
			stopwatchLabel.Content = "0.0";
		}
		public void updatePrematch(int sPM)
		{
			stopwatchLabel.Content = String.Format("{0:N0}", sPM);

			teamATime.Content = "Klaar?";
			teamATime.Foreground = Brushes.White;

			teamBTime.Content = "Klaar?";
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

		public void updateTimeslots(ref List<Timeslot> timeslots, DateTime ecoStart, TimeSpan timeslotLen)
		{			
			int timeslot = 0;
			TimeSpan ts = DateTime.Now - ecoStart;
			if (ts > TimeSpan.Zero)
			{
				timeslot = (int)(ts.Ticks / timeslotLen.Ticks);
			}
			if (timeslot >= 0 && timeslot < timeslots.Count && ts > TimeSpan.Zero)
			{
				StringBuilder sb = new StringBuilder();
				bool first = true;
				foreach (KeyValuePair<String, int> kvp in timeslots[timeslot].Items)
				{
					if (!first)
					{
						sb.Append(" & ");
					}
					sb.AppendFormat("{0} ({1})",kvp.Key.Trim('%').UppercaseFirst(),kvp.Value);
					first = false;
				}
				currentTimeslot.Text = "Nu: " + sb.ToString();
				sb = null;
			}
			else
			{
				currentTimeslot.Text = "Nu niets actief.";
			}
			if (ts <= TimeSpan.Zero)
			{
				timeslot = -1;
			}
			if (timeslot+1 >= 0 && timeslot+1 < timeslots.Count)
			{
				StringBuilder sb = new StringBuilder();
				bool first = true;
				foreach (KeyValuePair<String, int> kvp in timeslots[timeslot + 1].Items)
				{
					if (!first)
					{
						sb.Append(" & ");
					}
					sb.AppendFormat("{0} ({1})", kvp.Key.Trim('%').UppercaseFirst(), kvp.Value);
					first = false;
				}
				nextTimeslot.Text = "Straks: " + sb.ToString();
				sb = null;
			}

			else
			{
				nextTimeslot.Text = "";
			}
		}

		public void updateScroller(ref Dictionary<String, long> points)
		{
			upScroller.Children.Clear();
			if (points.Count > 0)
			{
				int i = 1;
				foreach (KeyValuePair<String, long> kvp in points)
				{
					TextBlock tb = new TextBlock();
					tb.Text = i.ToString() + ": " + kvp.Key + " met " + kvp.Value + " punten.";
					upScroller.Children.Add(tb);
					i++;
				}
				upScroller.UpdateLayout();
				
			}
			else
			{
				TextBlock tb = new TextBlock();
				tb.Text = "Er is nog geen data.";
				upScroller.Children.Add(tb);
			}
			if (upScroller.ActualHeight > canUp.ActualHeight && lastCount < points.Count)
			{
				updateAnimation();
			}
			if (upScroller.ActualHeight <= canUp.ActualHeight)
			{
				stopAnimation();
			}
			lastCount = points.Count;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (upScroller.ActualHeight > canUp.ActualHeight)
			{
				updateAnimation();
			}
			if (upScroller.ActualHeight <= canUp.ActualHeight)
			{
				stopAnimation();
			}		
		}
		public void updateAnimation(){
			DoubleAnimation doubleAnimationUp = new DoubleAnimation();
			doubleAnimationUp.From = -upScroller.ActualHeight;
			doubleAnimationUp.To = canUp.ActualHeight;		
			doubleAnimationUp.RepeatBehavior = RepeatBehavior.Forever;
			doubleAnimationUp.Duration = new Duration(TimeSpan.FromSeconds((doubleAnimationUp.To.Value - doubleAnimationUp.From.Value) / 20));
			upScroller.BeginAnimation(Canvas.BottomProperty, doubleAnimationUp);
		}
		public void stopAnimation()
		{			
			upScroller.BeginAnimation(Canvas.BottomProperty, null);
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (upScroller.ActualHeight > canUp.ActualHeight)
			{
				updateAnimation();
			}
			if (upScroller.ActualHeight <= canUp.ActualHeight)
			{
				stopAnimation();
			}
		}
	}
}
