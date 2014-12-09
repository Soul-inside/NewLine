// Last Change: 2014 12 09 15:03

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
		/// <summary>
		///     Ломаная зеленая линия
		/// </summary>
		private readonly Polyline Pl_green = new Polyline {Stroke = Brushes.Green, StrokeThickness = 2, FillRule = FillRule.EvenOdd};

		/// <summary>
		///     Ломаная красная линия
		/// </summary>
		private readonly Polyline Pl_red = new Polyline {Stroke = Brushes.Red, StrokeThickness = 2, FillRule = FillRule.EvenOdd};

		/// <summary>
		///     Временная зеленая линия
		/// </summary>
		private readonly SquareLine SLineGreen = new SquareLine {Stroke = Brushes.SpringGreen, StrokeThickness = 2, StrokeDashArray = new DoubleCollection {2}};

		/// <summary>
		///     Временная красная линия
		/// </summary>
		private readonly SquareLine SLineRed = new SquareLine {Stroke = Brushes.DarkRed, StrokeThickness = 2, StrokeDashArray = new DoubleCollection {2}};

		/// <summary>
		///     Первая зеленая линия
		/// </summary>
		private readonly Line tmpLineGreen = new Line {Stroke = Brushes.SpringGreen, StrokeThickness = 2, StrokeDashArray = new DoubleCollection {2}};

		/// <summary>
		///     Первая красная линия
		/// </summary>
		private readonly Line tmpLineRed = new Line {Stroke = Brushes.DarkRed, StrokeThickness = 2, StrokeDashArray = new DoubleCollection {2}};

		/// <summary>
		///     Тип линии
		/// </summary>
		public int Type;

		/// <summary>
		///     Список всех нарисованных линий
		/// </summary>
		private readonly List<SquareLine> _plWrite = new List<SquareLine>();

		/// <summary>
		///     Первая точка на экране, с которой начинается выделение прямоугольника (Левая верхняя)
		/// </summary>
		private static Point _firstPoint;

		/// <summary>
		///     Вторая точка на экране, с которой продолжается выделение прямоугольника (Правая нижняя)
		/// </summary>
		private static Point _secondPoint;

		/// <summary>
		///     Прямоугольник для ручного выделения области
		/// </summary>
		private static Rectangle _rect = new Rectangle {Stroke = Brushes.Black, StrokeThickness = 2, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top};

		/// <summary>
		///     Описывает ширину, высоту и расположение прямоугольника целого числа
		/// </summary>
		private static Int32Rect _rectForDraw;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void ResizeImage()
		{
			ImgWell.MaxHeight = W.Height;
			ImgWell.MaxWidth = W.Width;
			CnvDraw.Height = ImgWell.Source.Height;
			CnvDraw.Width = ImgWell.Source.Width;
		}

		private void CnvDraw_MouseMove(object sender, MouseEventArgs e)
		{
			if (Type == 1)
			{
				if (Pl_green.Points.Count() > 1)
				{
					SLineGreen.EndPoint = new Point(e.GetPosition(CnvDraw).X, e.GetPosition(CnvDraw).Y);
				}
				else if (Pl_green.Points.Any())
				{
					tmpLineGreen.X2 = tmpLineGreen.X1;
					tmpLineGreen.Y2 = e.GetPosition(CnvDraw).Y;
				}
			}
			else if (Type == 2)
			{
				if (Pl_red.Points.Count() > 1)
				{
					SLineRed.EndPoint = new Point(e.GetPosition(CnvDraw).X, e.GetPosition(CnvDraw).Y);
				}
				else if (Pl_red.Points.Any())
				{
					tmpLineRed.X2 = tmpLineRed.X1;
					tmpLineRed.Y2 = e.GetPosition(CnvDraw).Y;
				}
			}
			else
			{
				if (e.LeftButton == MouseButtonState.Pressed)
				{
					_secondPoint = e.GetPosition(CnvDraw);
					MoveDrawRectangle(_firstPoint, _secondPoint);
				}
			}
		}

		private void CnvDraw_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (Type == 1)
			{
				if (CnvDraw.Children.Contains(SLineGreen))
				{
					if (!Pl_green.Points.Any())
					{
						Pl_green.Points.Add(new Point(e.GetPosition(CnvDraw).X, _firstPoint.Y));
						tmpLineGreen.X1 = e.GetPosition(CnvDraw).X;
						tmpLineGreen.Y1 = _firstPoint.Y;
					}
					else if (Pl_green.Points.Count() < 2)
					{
						Pl_green.Points.Add(new Point(tmpLineGreen.X1, e.GetPosition(CnvDraw).Y));
						SLineGreen.StartPoint = new Point(tmpLineGreen.X1, e.GetPosition(CnvDraw).Y);
						SLineGreen.EndPoint = new Point(tmpLineGreen.X1, e.GetPosition(CnvDraw).Y);
						CnvDraw.Children.Remove(tmpLineGreen);
					}
					else
					{
						Pl_green.Points.Add(new Point(e.GetPosition(CnvDraw).X, SLineGreen.StartPoint.Y));
						Pl_green.Points.Add(new Point(e.GetPosition(CnvDraw).X, e.GetPosition(CnvDraw).Y));
						SLineGreen.StartPoint = SLineGreen.EndPoint;
					}
				}
			}

			else if (Type == 2)
			{
				if (CnvDraw.Children.Contains(SLineRed))
				{
					if (!Pl_red.Points.Any())
					{
						Pl_red.Points.Add(new Point(e.GetPosition(CnvDraw).X, _firstPoint.Y));
						tmpLineRed.X1 = e.GetPosition(CnvDraw).X;
						tmpLineRed.Y1 = _firstPoint.Y;
					}
					else if (Pl_red.Points.Count() < 2)
					{
						Pl_red.Points.Add(new Point(tmpLineRed.X1, e.GetPosition(CnvDraw).Y));
						SLineRed.StartPoint = new Point(tmpLineRed.X1, e.GetPosition(CnvDraw).Y);
						SLineRed.EndPoint = new Point(tmpLineRed.X1, e.GetPosition(CnvDraw).Y);
						CnvDraw.Children.Remove(tmpLineRed);
					}
					else
					{
						Pl_red.Points.Add(new Point(e.GetPosition(CnvDraw).X, SLineRed.StartPoint.Y));
						Pl_red.Points.Add(new Point(e.GetPosition(CnvDraw).X, e.GetPosition(CnvDraw).Y));
						SLineRed.StartPoint = SLineRed.EndPoint;
					}
				}
			}
			else
			{
				_firstPoint = e.GetPosition(CnvDraw);
				MoveDrawRectangle(_firstPoint, _firstPoint);
			}
		}

		private void CnvDraw_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			CnvDraw.Children.Remove(SLineGreen);
			CnvDraw.Children.Remove(SLineRed);
		}

		/// <summary>
		///     Загрузка изображения
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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
			Type = 1;
			CnvDraw.Children.Add(Pl_green);
			CnvDraw.Children.Add(SLineGreen);
			CnvDraw.Children.Add(tmpLineGreen);
			SLineGreen.EndPoint = new Point();
			SLineGreen.StartPoint = new Point();
			tmpLineGreen.X1 = new double();
			tmpLineGreen.X2 = new double();
		}

		private void BtRed_Click(object sender, RoutedEventArgs e)
		{
			Type = 2;
			CnvDraw.Children.Add(Pl_red);
			CnvDraw.Children.Add(SLineRed);
			CnvDraw.Children.Add(tmpLineRed);
			SLineRed.EndPoint = new Point();
			SLineRed.StartPoint = new Point();
			tmpLineGreen.X1 = new double();
			tmpLineGreen.X2 = new double();
		}

		private void TgBtOrange_Checked(object sender, RoutedEventArgs e)
		{
			Type = 3;
			_rect = new Rectangle {Stroke = Brushes.OrangeRed, StrokeThickness = 2, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Fill = Brushes.OrangeRed};
			Canvas.SetTop(_rect, _firstPoint.Y);
			Canvas.SetLeft(_rect, _firstPoint.X);
			CnvDraw.Children.Add(_rect);
		}

		private void BtClear_Click(object sender, RoutedEventArgs e)
		{
			CnvDraw.Children.Remove(SLineGreen);
			CnvDraw.Children.Remove(Pl_green);
			CnvDraw.Children.Remove(tmpLineGreen);
			Pl_green.Points.Clear();
			SLineGreen.EndPoint = new Point();
			SLineGreen.StartPoint = new Point();
			tmpLineGreen.X1 = new double();
			tmpLineGreen.X2 = new double();

			CnvDraw.Children.Remove(SLineRed);
			CnvDraw.Children.Remove(Pl_red);
			CnvDraw.Children.Remove(tmpLineRed);
			Pl_red.Points.Clear();
			SLineRed.EndPoint = new Point();
			SLineRed.StartPoint = new Point();
			tmpLineRed.X1 = new double();
			tmpLineRed.X2 = new double();

			CnvDraw.Children.Remove(_rect);
		}

		private void BtWriteToFile_Click(object sender, RoutedEventArgs e)
		{
			//var fs = new FileStream("output.txt", FileMode.Append);
			//var sw = new StreamWriter(fs);
			//foreach (var item in _linesWrite)
			//{
			//	if ((item.Type == 1 || item.Type == 2) && item.GradientEnd == item.GradientStart)
			//	{
			//		sw.WriteLine(item.ToString());
			//	}
			//	else if (item.Type == 3)
			//	{
			//		sw.WriteLine(item.ToString());
			//	}
			//}
			//sw.Close();
			//fs.Close();
		}

		/// <summary>
		///     Удаляет последнюю линию
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CnvDraw_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				if (Type == 1)
				{
					if (Pl_green.Points.Count > 2)
					{
						Pl_green.Points.Remove(Pl_green.Points.Last());
						Pl_green.Points.Remove(Pl_green.Points.Last());
						SLineGreen.StartPoint = Pl_green.Points.Last();
					}
					else if (Pl_green.Points.Count > 0)
					{
						Pl_green.Points.Remove(Pl_green.Points.Last());
						Pl_green.Points.Remove(Pl_green.Points.Last());
						SLineGreen.StartPoint = new Point(0, 0);
						SLineGreen.EndPoint = new Point(0, 0);
						tmpLineGreen.X1 = 0;
						tmpLineGreen.Y1 = 0;
						tmpLineGreen.X2 = 0;
						tmpLineGreen.Y2 = 0;
						CnvDraw.Children.Add(tmpLineGreen);
						try
						{
							CnvDraw.Children.Add(SLineGreen);
						}
						catch
						{
						}
						;
					}
				}
				else if (Type == 2)
				{
					if (Pl_red.Points.Count > 2)
					{
						Pl_red.Points.Remove(Pl_red.Points.Last());
						Pl_red.Points.Remove(Pl_red.Points.Last());
						SLineRed.StartPoint = Pl_red.Points.Last();
					}
					else if (Pl_red.Points.Count > 0)
					{
						Pl_red.Points.Remove(Pl_red.Points.Last());
						Pl_red.Points.Remove(Pl_red.Points.Last());
						SLineRed.StartPoint = new Point(0, 0);
						SLineRed.EndPoint = new Point(0, 0);
						tmpLineRed.X1 = 0;
						tmpLineRed.Y1 = 0;
						tmpLineRed.X2 = 0;
						tmpLineRed.Y2 = 0;
						CnvDraw.Children.Add(tmpLineRed);
						try
						{
							CnvDraw.Children.Add(SLineRed);
						}
						catch
						{
						}
						;
					}
				}
				else
				{
					CnvDraw.Children.Remove(_rect);
				}
			}
		}

		/// <summary>
		///     Задание области определения функции
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtSelectArea_Click(object sender, RoutedEventArgs e)
		{
			CnvDraw.Children.Remove(_rect);
			_rect = new Rectangle {Stroke = Brushes.Red, StrokeThickness = 2, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top};
			Canvas.SetTop(_rect, _firstPoint.Y);
			Canvas.SetLeft(_rect, _firstPoint.X);
			CnvDraw.Children.Add(_rect);
		}

		private void CnvDraw_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var rectangle = new Int32Rect();
			// рисуем влево
			if (_firstPoint.X > _secondPoint.X)
			{
				rectangle.X = (int) (_secondPoint.X + _rect.StrokeThickness);
				rectangle.Width = (int) (_firstPoint.X - _secondPoint.X - _rect.StrokeThickness*2);
				if (rectangle.Width < 2)
				{
					rectangle.Width = 0;
					_rect.Stroke = Brushes.White;
				}
			}
			// рисуем вправо
			else
			{
				rectangle.X = (int) (_firstPoint.X);
				rectangle.Width = (int) (_secondPoint.X - _firstPoint.X);
				if (rectangle.Width < 2)
				{
					rectangle.Width = 0;
					_rect.Stroke = Brushes.White;
				}
			}
			// рисуем вверх
			if (_firstPoint.Y > _secondPoint.Y)
			{
				rectangle.Y = (int) (_secondPoint.Y + _rect.StrokeThickness);
				rectangle.Height = (int) (_firstPoint.Y - _secondPoint.Y - _rect.StrokeThickness*2);
				if (rectangle.Height < 2)
				{
					rectangle.Height = 0;
					_rect.Stroke = Brushes.White;
				}
			}
			// рисуем вниз
			else
			{
				rectangle.Y = (int) (_firstPoint.Y + _rect.StrokeThickness);
				rectangle.Height = (int) (_secondPoint.Y - _firstPoint.Y - _rect.StrokeThickness*2);
				if (rectangle.Height < 2)
				{
					rectangle.Height = 0;
					_rect.Stroke = Brushes.White;
				}
			}
		}

		/// <summary>
		///     Растянуть/Переместить прямоугольник на указанные координаты. Построение временного квадрата.
		/// </summary>
		/// <param name="pointLeftTop">Левый верхний угол</param>
		/// <param name="pointRightBottom">Правый нижний угол</param>
		private void MoveDrawRectangle(Point pointLeftTop, Point pointRightBottom)
		{
			_rectForDraw = new Int32Rect();
			if (pointLeftTop.X > pointRightBottom.X)
			{
				Canvas.SetLeft(_rect, pointRightBottom.X);
				_rect.Width = pointLeftTop.X - pointRightBottom.X;
				_rectForDraw.X = (int) pointRightBottom.X;
			}
			else
			{
				Canvas.SetLeft(_rect, pointLeftTop.X);
				_rect.Width = pointRightBottom.X - pointLeftTop.X;
				_rectForDraw.X = (int) pointLeftTop.X;
			}
			if (pointLeftTop.Y > pointRightBottom.Y)
			{
				Canvas.SetTop(_rect, pointRightBottom.Y);
				_rect.Height = pointLeftTop.Y - pointRightBottom.Y;
				_rectForDraw.Y = (int) pointRightBottom.Y;
			}
			else
			{
				Canvas.SetTop(_rect, pointLeftTop.Y);
				_rect.Height = pointRightBottom.Y - pointLeftTop.Y;
				_rectForDraw.Y = (int) pointLeftTop.Y;
			}
			_rectForDraw.Width = (int) _rect.Width;
			_rectForDraw.Height = (int) _rect.Height;
		}
	}
}