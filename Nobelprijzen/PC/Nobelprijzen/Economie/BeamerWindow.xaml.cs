using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

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

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = !_allowclosing;
		}

		public void updateTimeslots(ref List<Timeslot> timeslots, ref Dictionary<string,int> pointsources, DateTime ecoStart, TimeSpan timeslotLen)
		{			
			int timeslot = 0;
			TimeSpan ts = DateTime.Now - ecoStart;
            stackPanel.Children.Clear();
            stackPanel.Children.Add(new Label() { Content = "Altijd", FontWeight = FontWeights.Bold });
            foreach (KeyValuePair<String, int> kvp in pointsources)
			{                   
                stackPanel.Children.Add(new Label() { Content = String.Format("{0} ({1})", kvp.Key.Trim('%').UppercaseFirst(), kvp.Value) });
            }
           
			if (ts > TimeSpan.Zero)
			{
				timeslot = (int)(ts.Ticks / timeslotLen.Ticks);
			}
			if (timeslot >= 0 && timeslot < timeslots.Count && ts > TimeSpan.Zero)
			{	
			    stackPanel.Children.Add(new Label() { Content = "Nu", FontWeight = FontWeights.Bold });
				foreach (KeyValuePair<String, int> kvp in timeslots[timeslot].Items)
				{                   
                    stackPanel.Children.Add(new Label() { Content = String.Format("{0} ({1})", kvp.Key.Trim('%').UppercaseFirst(), kvp.Value) });
                }
			}			
			if (ts <= TimeSpan.Zero)
			{
				timeslot = -1;
			}
            TimeSpan timeToNext = ecoStart.Add(timeslotLen.Multiply(timeslot + 1)).Subtract(DateTime.Now);
			if (timeslot+1 >= 0 && timeslot+1 < timeslots.Count)
			{
                stackPanel.Children.Add(new Label() { Content = String.Format("Straks (nog {0:F0} min)", timeToNext.TotalMinutes), FontWeight = FontWeights.Bold });				
				foreach (KeyValuePair<String, int> kvp in timeslots[timeslot + 1].Items)
				{
                    stackPanel.Children.Add(new Label() { Content = String.Format("{0} ({1})", kvp.Key.Trim('%').UppercaseFirst(), kvp.Value) });
				}
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
                    Grid grid = new Grid() { Width = canUp.ActualWidth};
                    /*TextBlock tb = new TextBlock();
					tb.Text = String.Format("{0,3}: {1,-30} met {2,5} punten.",i,kvp.Key,kvp.Value);
					upScroller.Children.Add(tb);*/
                    Label tbPos = new Label()
                    {
                        Content = i.ToString(),
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right,
                        Width = 30,
                        Style = (Style)(this.Resources["BeamerText"])
                    };
                    Label tbPoints = new Label()
                    {
                        Content = kvp.Value.ToString(),
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                        HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right,
                        Width = 50,
                        Style = (Style)(this.Resources["BeamerText"])
                    };
                    String name = kvp.Key;
                    if (name.Contains(","))
                    {
                        string [] name_strs = name.Split(new char[] { ',' }, 2);
                        Array.Reverse(name_strs);
                        name = String.Join(" ", name_strs);
                    }
                    Label tbName = new Label()
                    {
                        Content = name.Trim(),
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                        Margin = new Thickness(tbPos.Width+5, 0, tbPoints.Width+5, 0),
                        BorderBrush = Brushes.White,
                        BorderThickness = new Thickness(2,0,2,0),
                        Padding = new Thickness(5,0,5,0),
                        Style = (Style)(this.Resources["BeamerText"])
                    };                    
                    grid.Children.Add(tbPos);
                    grid.Children.Add(tbName);
                    grid.Children.Add(tbPoints);
                    upScroller.Children.Add(grid);
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
			/*DoubleAnimation doubleAnimationUp = new DoubleAnimation();
			doubleAnimationUp.From = 0;
			doubleAnimationUp.To = canUp.ActualHeight-upScroller.ActualHeight;
            doubleAnimationUp.AutoReverse = true;

            doubleAnimationUp.EasingFunction = new SineEase();

            doubleAnimationUp.RepeatBehavior = RepeatBehavior.Forever;
			doubleAnimationUp.Duration = new Duration(TimeSpan.FromSeconds(Math.Abs(doubleAnimationUp.To.Value - doubleAnimationUp.From.Value) / 200));
			upScroller.BeginAnimation(Canvas.TopProperty, doubleAnimationUp);*/
            double from = 0.0;
            double to = canUp.ActualHeight-upScroller.ActualHeight;
            DoubleAnimationUsingKeyFrames DA = new DoubleAnimationUsingKeyFrames();
            DA.KeyFrames.Add(new LinearDoubleKeyFrame(from,KeyTime.FromPercent(0)));
            DA.KeyFrames.Add(new LinearDoubleKeyFrame(to, KeyTime.FromPercent(.35)));
            DA.KeyFrames.Add(new LinearDoubleKeyFrame(to, KeyTime.FromPercent(.45)));
            DA.KeyFrames.Add(new LinearDoubleKeyFrame(from, KeyTime.FromPercent(.90)));
            DA.KeyFrames.Add(new LinearDoubleKeyFrame(from, KeyTime.FromPercent(1)));
            DA.RepeatBehavior = RepeatBehavior.Forever;
            DA.Duration = new Duration(TimeSpan.FromSeconds(Math.Abs(to-from) / 10));
            upScroller.BeginAnimation(Canvas.TopProperty, DA);
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
