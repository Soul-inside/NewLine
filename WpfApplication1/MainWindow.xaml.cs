// Last Change: 2014 12 04 16:49

using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace WpfApplication1
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly Polyline Pl_green = new Polyline {Stroke = Brushes.Green, StrokeThickness = 2, FillRule = FillRule.EvenOdd};
		private readonly SquareLine SLine = new SquareLine {Stroke = Brushes.SpringGreen, StrokeThickness = 2, StrokeDashArray = new DoubleCollection {2}};
		private readonly Line tmpLine = new Line {Stroke = Brushes.SpringGreen, StrokeThickness = 2, StrokeDashArray = new DoubleCollection {2}};

		public MainWindow()
		{
			InitializeComponent();
			CnvDraw.Children.Add(SLine);
			CnvDraw.Children.Add(Pl_green);
			CnvDraw.Children.Add(tmpLine);
		}

		private void ResizeImage()
		{
			ImgWell.MaxHeight = W.Height;
			ImgWell.MaxWidth = W.Width;
			CnvDraw.Height = ImgWell.Height;
			CnvDraw.Width = ImgWell.Width;
		}

		private void CnvDraw_MouseMove(object sender, MouseEventArgs e)
		{
			if (Pl_green.Points.Count() > 1)
			{
				SLine.EndPoint = new Point(e.GetPosition(CnvDraw).X, e.GetPosition(CnvDraw).Y);
			}
			else if (Pl_green.Points.Count() > 0)
			{
				tmpLine.X2 = tmpLine.X1;
				tmpLine.Y2 = e.GetPosition(CnvDraw).Y;
			}
		}

		private void CnvDraw_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (CnvDraw.Children.Contains(SLine))
			{
				if (!Pl_green.Points.Any())
				{
					Pl_green.Points.Add(new Point(e.GetPosition(CnvDraw).X, e.GetPosition(CnvDraw).Y));
					tmpLine.X1 = e.GetPosition(CnvDraw).X;
					tmpLine.Y1 = e.GetPosition(CnvDraw).Y;
				}
				else if (Pl_green.Points.Count() < 2)
				{
					Pl_green.Points.Add(new Point(tmpLine.X1, e.GetPosition(CnvDraw).Y));
					SLine.StartPoint = new Point(tmpLine.X1, e.GetPosition(CnvDraw).Y);
					SLine.EndPoint = new Point(tmpLine.X1, e.GetPosition(CnvDraw).Y);
					CnvDraw.Children.Remove(tmpLine);
				}
				else
				{
					Pl_green.Points.Add(new Point(e.GetPosition(CnvDraw).X, SLine.StartPoint.Y));
					Pl_green.Points.Add(new Point(e.GetPosition(CnvDraw).X, e.GetPosition(CnvDraw).Y));
					SLine.StartPoint = SLine.EndPoint;
				}
			}
		}

		private void CnvDraw_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			CnvDraw.Children.Remove(SLine);
		}

		private void BtImageLoad_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new OpenFileDialog
			{
				FileName = " Image ",
				DefaultExt = ".jpg",
				Filter = " Image (.jpg)|*.jpg"
			};
			bool? result = dlg.ShowDialog();
			if (result == true)
			{
				try
				{
					string filename = dlg.FileName;
					var bi3 = new BitmapImage();
					bi3.BeginInit();
					bi3.UriSource = new Uri(filename, UriKind.Absolute);
					bi3.EndInit();
					ImgWell.Source = bi3;
					ResizeImage();
				}
				catch (NotSupportedException)
				{
					MessageBox.Show("Некорректный тип файла!", "Внимание");
				}
			}
		}

		private void BtGreen_Click(object sender, RoutedEventArgs e)
		{
		}

		private void BtRed_Click(object sender, RoutedEventArgs e)
		{
		}

		private void TgBtOrange_Checked(object sender, RoutedEventArgs e)
		{
		}
	}
}