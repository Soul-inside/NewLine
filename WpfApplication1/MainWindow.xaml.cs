// Last Change: 2014 12 12 13:26

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
		#region init

		public static bool isFirstLine = false;

		/// <summary>
		///     координаты прямоугольника для выделения области
		/// </summary>
		private static Rectangle _defRectangle = new Rectangle();

		/// <summary>
		///     координаты первой точки прямоугольника для выделения области
		/// </summary>
		private static Point _defRectangleFirst;

		/// <summary>
		///     Координаты второй точки прямоугольника для выделения области
		/// </summary>
		private static Point _defRectangleSecond;

		/// <summary>
		///     Список всех нарисованных прямоугольников
		/// </summary>
		private static readonly List<Rectangle> _sqRect = new List<Rectangle>();

		/// <summary>
		///     Ломаная зеленая линия
		/// </summary>
		private readonly Polyline _plGreen = new Polyline {Stroke = Brushes.Green, StrokeThickness = 2, FillRule = FillRule.EvenOdd};

		/// <summary>
		///     Ломаная красная линия
		/// </summary>
		private readonly Polyline _plRed = new Polyline {Stroke = Brushes.Red, StrokeThickness = 2, FillRule = FillRule.EvenOdd};

		/// <summary>
		///     Временная зеленая линия
		/// </summary>
		private readonly SquareLine _sLineGreen = new SquareLine {Stroke = Brushes.SpringGreen, StrokeThickness = 2, StrokeDashArray = new DoubleCollection {2}};

		/// <summary>
		///     Временная красная линия
		/// </summary>
		private readonly SquareLine _sLineRed = new SquareLine {Stroke = Brushes.DarkRed, StrokeThickness = 2, StrokeDashArray = new DoubleCollection {2}};

		/// <summary>
		///     Первая зеленая линия
		/// </summary>
		private readonly Line _tmpLineGreen = new Line {Stroke = Brushes.SpringGreen, StrokeThickness = 2, StrokeDashArray = new DoubleCollection {2}};

		/// <summary>
		///     Первая красная линия
		/// </summary>
		private readonly Line _tmpLineRed = new Line {Stroke = Brushes.DarkRed, StrokeThickness = 2, StrokeDashArray = new DoubleCollection {2}};

		/// <summary>
		///     Тип линии
		/// </summary>
		public int Type = -1;

		/// <summary>
		///     Список всех нарисованных квадратов
		/// </summary>
		private readonly List<RectWrite> _rectWrite = new List<RectWrite>();

		/// <summary>
		///     Список всех нарисованных линий
		/// </summary>
		private readonly List<PlWrite> _plWrite = new List<PlWrite>();

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

		/// <summary>
		///     Режим рисавания области определения
		/// </summary>
		private bool _drawRectangle;

		/// <summary>
		/// Глубина от
		/// </summary>
		public double depthStart;

		/// <summary>
		/// Глубина до
		/// </summary>
		public double depthEnd;

		/// <summary>
		/// Градиент от
		/// </summary>
		public double gradientStart;

		/// <summary>
		/// Градиент до
		/// </summary>
		public double gradientEnd;

		/// <summary>
		/// Начальное значение градиента, введенное пользователем
		/// </summary>
		public double xmin;

		/// <summary>
		/// Конечное значение градиента, введенное пользователем
		/// </summary>
		public double xmax;

		/// <summary>
		/// Начальное значение глубины, введенное пользователем
		/// </summary>
		public double ymin;

		/// <summary>
		/// Конечное значение глубины, введенное пользователем
		/// </summary>
		public double ymax;

		#endregion

		#region main_code

		public MainWindow()
		{
			InitializeComponent();
		}

		/// <summary>
		///     Изменение размеров изображения и канваса
		/// </summary>
		private void ResizeImage()
		{
			ImgWell.MaxHeight = W.Height;
			ImgWell.MaxWidth = W.Width;
			CnvDraw.Height = ImgWell.MaxHeight;
			CnvDraw.Width = ImgWell.MaxWidth;
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

		/// <summary>
		///     Нажатие кнопки задания области определения функции
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtSelectArea_Click(object sender, RoutedEventArgs e)
		{
			// в случае повторного выделения области
			BtClear_Click(null, null);
			CnvDraw.Children.Remove(_rect);
			_rect = new Rectangle { Stroke = Brushes.Red, StrokeThickness = 2, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
			Canvas.SetTop(_rect, _firstPoint.Y);
			Canvas.SetLeft(_rect, _firstPoint.X);
			CnvDraw.Children.Add(_rect);
			_defRectangle = _rect;
			_defRectangleFirst = _firstPoint;
			_defRectangleSecond = _secondPoint;
			if (_defRectangleFirst.X == _defRectangleSecond.X && _defRectangleFirst.Y == _defRectangleSecond.Y)
			{
				_defRectangle.Visibility = Visibility.Hidden;
			}
			_drawRectangle = true;
			Type = 0;
		}

		/// <summary>
		///     нажатие кнопки для построения зеленой линии
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtGreen_Click(object sender, RoutedEventArgs e)
		{
			isFirstLine = true;
			Type = 1;
			try
			{
				CnvDraw.Children.Add(_plGreen);
				CnvDraw.Children.Add(_sLineGreen);
				CnvDraw.Children.Add(_tmpLineGreen);
				double x = _defRectangleFirst.X, y = _defRectangleFirst.Y;
				_sLineGreen.StartPoint = _defRectangleFirst;
				_sLineGreen.EndPoint = _defRectangleFirst;
				_tmpLineGreen.X1 = x;
				_tmpLineGreen.Y1 = y;
				_tmpLineGreen.X2 = x;
				_tmpLineGreen.Y2 = y;
			}
			catch (Exception)
			{
			}
		}

		/// <summary>
		///     нажатие кнопки для построения красной линии
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtRed_Click(object sender, RoutedEventArgs e)
		{
			isFirstLine = true;

			Type = 2;
			try
			{
				CnvDraw.Children.Add(_plRed);
				CnvDraw.Children.Add(_sLineRed);
				CnvDraw.Children.Add(_tmpLineRed);
				double x = _defRectangleFirst.X, y = _defRectangleFirst.Y;
				_sLineRed.StartPoint = _defRectangleFirst;
				_sLineRed.EndPoint = _defRectangleFirst;
				_tmpLineRed.X1 = x;
				_tmpLineRed.Y1 = y;
				_tmpLineRed.X2 = x;
				_tmpLineRed.Y2 = y;
			}
			catch (Exception)
			{
			}
		}

		/// <summary>
		///     нажатие кнопки для построения оранжевых квадратиков
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtOrange_Click(object sender, RoutedEventArgs e)
		{
			Type = 3;
			_rect = new Rectangle { Stroke = Brushes.Black, StrokeThickness = 1, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Fill = Brushes.OrangeRed };
			Canvas.SetTop(_rect, _firstPoint.Y);
			Canvas.SetLeft(_rect, _firstPoint.X);
			CnvDraw.Children.Add(_rect);
			// добавление прямокгольника в список
			_sqRect.Add(_rect);
		}

		/// <summary>
		///     Нажатие левой кнопки мыши на канвасе, содержащем изображение
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CnvDraw_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (isFirstLine)
			{
				isFirstLine = false;
				_tmpLineGreen.X1 = e.GetPosition(CnvDraw).X;
				_tmpLineGreen.Y1 = e.GetPosition(CnvDraw).Y;
				_tmpLineRed.X1 = e.GetPosition(CnvDraw).X;
				_tmpLineRed.Y1 = e.GetPosition(CnvDraw).Y;
				_tmpLineGreen.X2 = e.GetPosition(CnvDraw).X;
				_tmpLineGreen.Y2 = e.GetPosition(CnvDraw).Y;
				_tmpLineRed.X2 = e.GetPosition(CnvDraw).X;
				_tmpLineRed.Y2 = e.GetPosition(CnvDraw).Y;
			}
			if (_drawRectangle)
			{
				// отрисовка линий строго в выделенной области
				double cx = e.GetPosition(CnvDraw).X;
				double cy = e.GetPosition(CnvDraw).Y;
				if (cx > _defRectangleSecond.X)
				{
					cx = _defRectangleSecond.X;
				}
				if (cy > _defRectangleSecond.Y)
				{
					cy = _defRectangleSecond.Y;
				}
				if (cx < _defRectangleFirst.X)
				{
					cx = _defRectangleFirst.X;
				}
				if (cy < _defRectangleFirst.Y)
				{
					cy = _defRectangleFirst.Y;
				}
				// для рисования зеленой линии
				if (Type == 1)
				{
					if (CnvDraw.Children.Contains(_sLineGreen))
					{
						if (!_plGreen.Points.Any())
						{
							_plGreen.Points.Add(new Point(cx, _defRectangleFirst.Y));
							_tmpLineGreen.X1 = cx;
							_tmpLineGreen.Y1 = _defRectangleFirst.Y;
						}
						else if (_plGreen.Points.Count() < 2)
						{
							_plGreen.Points.Add(new Point(_tmpLineGreen.X1, cy));
							_sLineGreen.StartPoint = new Point(_tmpLineGreen.X1, cy);
							_sLineGreen.EndPoint = new Point(_tmpLineGreen.X1, cy);
							CnvDraw.Children.Remove(_tmpLineGreen);
							Replace();
							depthStart = getRealCoord(_defRectangleFirst.Y, _defRectangleFirst.Y, _defRectangle.Height, ymax, ymin);
							depthEnd = getRealCoord(cy, _defRectangleFirst.Y, _defRectangle.Height, ymax, ymin);
							gradientStart = getRealCoord(_tmpLineGreen.X1, _defRectangleFirst.X, _defRectangle.Width, xmax, xmin);
							gradientEnd = gradientStart;
							_rectWrite.Add(new RectWrite(Type, depthStart, depthEnd, gradientStart, gradientEnd));
						}
						else
						{
							_plGreen.Points.Add(new Point(cx, _sLineGreen.StartPoint.Y));
							_plGreen.Points.Add(new Point(cx, cy));
							_sLineGreen.StartPoint = _sLineGreen.EndPoint;
							Replace();
							depthStart = getRealCoord(_plGreen.Points[_plGreen.Points.Count - 2].Y, _defRectangleFirst.Y, _defRectangle.Height, ymax, ymin);
							depthEnd = getRealCoord(cy, _defRectangleFirst.Y, _defRectangle.Height, ymax, ymin);
							gradientStart = getRealCoord(cx, _defRectangleFirst.X, _defRectangle.Width, xmax, xmin);
							gradientEnd = gradientStart;
							_rectWrite.Add(new RectWrite(Type, depthStart, depthEnd, gradientStart, gradientEnd));
						}
					}
				}
				// для рисования красной линии
				else if (Type == 2)
				{
					if (CnvDraw.Children.Contains(_sLineRed))
					{
						if (!_plRed.Points.Any())
						{
							_plRed.Points.Add(new Point(cx, _defRectangleFirst.Y));
							_tmpLineRed.X1 = cx;
							_tmpLineRed.Y1 = _defRectangleFirst.Y;
						}
						else if (_plRed.Points.Count() < 2)
						{
							_plRed.Points.Add(new Point(_tmpLineRed.X1, cy));
							_sLineRed.StartPoint = new Point(_tmpLineRed.X1, cy);
							_sLineRed.EndPoint = new Point(_tmpLineRed.X1, cy);
							CnvDraw.Children.Remove(_tmpLineRed);
							Replace();
							depthStart = getRealCoord(_defRectangleFirst.Y, _defRectangleFirst.Y, _defRectangle.Height, ymax, ymin);
							depthEnd = getRealCoord(cy, _defRectangleFirst.Y, _defRectangle.Height, ymax, ymin);
							gradientStart = getRealCoord(_tmpLineRed.X1, _defRectangleFirst.X, _defRectangle.Width, xmax, xmin);
							gradientEnd = gradientStart;
							_rectWrite.Add(new RectWrite(Type, depthStart, depthEnd, gradientStart, gradientEnd));
						}
						else
						{
							_plRed.Points.Add(new Point(cx, _sLineRed.StartPoint.Y));
							_plRed.Points.Add(new Point(cx, cy));
							_sLineRed.StartPoint = _sLineRed.EndPoint;
							Replace();
							depthStart = getRealCoord(_plRed.Points[_plRed.Points.Count - 2].Y, _defRectangleFirst.Y, _defRectangle.Height, ymax, ymin);
							depthEnd = getRealCoord(cy, _defRectangleFirst.Y, _defRectangle.Height, ymax, ymin);
							gradientStart = getRealCoord(cx, _defRectangleFirst.X, _defRectangle.Width, xmax, xmin);
							gradientEnd = gradientStart;
							_rectWrite.Add(new RectWrite(Type, depthStart, depthEnd, gradientStart, gradientEnd));
						}
					}
				}
				// в случае рисования квадратиков
				else
				{
					_firstPoint = e.GetPosition(CnvDraw);
					// для прямоугольной области определения
					if (Type == 0)
					{
						_defRectangleFirst = _firstPoint;
					}
					// для попадания квадратиков в область определения
					if (Type == 3)
					{
						if (_firstPoint.Y > _defRectangleSecond.Y)
						{
							_firstPoint.Y = _defRectangleSecond.Y;
						}
						if (_firstPoint.X > _defRectangleSecond.X)
						{
							_firstPoint.X = _defRectangleSecond.X;
						}
						if (_firstPoint.X < _defRectangleFirst.X)
						{
							_firstPoint.X = _defRectangleFirst.X;
						}
						if (_firstPoint.Y < _defRectangleFirst.Y)
						{
							_firstPoint.Y = _defRectangleFirst.Y;
						}
					}
					MoveDrawRectangle(_firstPoint, _firstPoint);
				}
			}
			else if (Type > 0)
			{
				// если область определения не была задана
				MessageBox.Show("Задайте область определения!", "Внимание");
			}
		}


		/// <summary>
		///     Движение мыши по канвасу
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CnvDraw_MouseMove(object sender, MouseEventArgs e)
		{
			// отрисовка линий строго в выделенной области
			double cx = e.GetPosition(CnvDraw).X;
			double cy = e.GetPosition(CnvDraw).Y;
			if (cx > _defRectangleSecond.X)
			{
				cx = _defRectangleSecond.X;
			}
			if (cy > _defRectangleSecond.Y)
			{
				cy = _defRectangleSecond.Y;
			}
			if (cx < _defRectangleFirst.X)
			{
				cx = _defRectangleFirst.X;
			}
			if (cy < _defRectangleFirst.Y)
			{
				cy = _defRectangleFirst.Y;
			}
			// для рисования зеленой линии
			if (Type == 1)
			{
				if (_plGreen.Points.Count() > 1)
				{
					_sLineGreen.EndPoint = new Point(cx, cy);
				}
				else if (_plGreen.Points.Any())
				{
					_tmpLineGreen.X2 = _tmpLineGreen.X1;
					_tmpLineGreen.Y2 = cy;
				}
			}
				//для рисования красной линии
			else if (Type == 2)
			{
				if (_plRed.Points.Count() > 1)
				{
					_sLineRed.EndPoint = new Point(cx, cy);
				}
				else if (_plRed.Points.Any())
				{
					_tmpLineRed.X2 = _tmpLineRed.X1;
					_tmpLineRed.Y2 = cy;
				}
			}
				// в случае рисования квадратиков
			else
			{
				if (e.LeftButton == MouseButtonState.Pressed)
				{
					_secondPoint = e.GetPosition(CnvDraw);
					if (Type == 0)
					{
						_defRectangleSecond = _secondPoint;
					}
					// для попадания квадратиков в область определения
					if (Type == 3)
					{
						if (_secondPoint.Y > _defRectangleSecond.Y)
						{
							_secondPoint.Y = _defRectangleSecond.Y;
						}
						if (_secondPoint.X > _defRectangleSecond.X)
						{
							_secondPoint.X = _defRectangleSecond.X;
						}
						if (_secondPoint.X < _defRectangleFirst.X)
						{
							_secondPoint.X = _defRectangleFirst.X;
						}
						if (_secondPoint.Y < _defRectangleFirst.Y)
						{
							_secondPoint.Y = _defRectangleFirst.Y;
						}
					}
					MoveDrawRectangle(_firstPoint, _secondPoint);
				}
			}
		}

		/// <summary>
		///     Отпускание левой кнопки мыши на канвасе
		///     Построение квадрата
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CnvDraw_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var rectangle = new Int32Rect();
			// рисуем влево
			if (_firstPoint.X > _secondPoint.X)
			{
				rectangle.X = (int)(_secondPoint.X + _rect.StrokeThickness);
				rectangle.Width = (int)(_firstPoint.X - _secondPoint.X - _rect.StrokeThickness * 2);
				if (rectangle.Width < 1)
				{
					rectangle.Width = 1;
				}
			}
			// рисуем вправо
			else
			{
				rectangle.X = (int)(_firstPoint.X);
				rectangle.Width = (int)(_secondPoint.X - _firstPoint.X);
				if (rectangle.Width < 1)
				{
					rectangle.Width = 1;
				}
			}
			// рисуем вверх
			if (_firstPoint.Y > _secondPoint.Y)
			{
				rectangle.Y = (int)(_secondPoint.Y + _rect.StrokeThickness);
				rectangle.Height = (int)(_firstPoint.Y - _secondPoint.Y - _rect.StrokeThickness * 2);
				if (rectangle.Height < 1)
				{
					rectangle.Height = 1;
				}
			}
			// рисуем вниз
			else
			{
				rectangle.Y = (int)(_firstPoint.Y + _rect.StrokeThickness);
				rectangle.Height = (int)(_secondPoint.Y - _firstPoint.Y - _rect.StrokeThickness * 2);
				if (rectangle.Height < 1)
				{
					rectangle.Height = 1;
				}
			}
			// в случае рисования квадратиков оранжевых
			if (Type == 3)
			{
				if (_rect.Height * _rect.Width > 1.2)
				{
					_rect.Visibility = Visibility.Visible;
				}
				_rect = new Rectangle { Stroke = Brushes.Black, StrokeThickness = 1, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Fill = Brushes.OrangeRed, Visibility = Visibility.Hidden };
				CnvDraw.Children.Add(_rect);
				_sqRect.Add(_rect);
				Replace();
				gradientStart = getRealCoord(_firstPoint.X, _defRectangleFirst.X, _defRectangle.Width, xmax, xmin);
				gradientEnd = getRealCoord(_secondPoint.X, _defRectangleFirst.X, _defRectangle.Width, xmax, xmin);
				depthStart = getRealCoord(_firstPoint.Y, _defRectangleFirst.Y, _defRectangle.Height, ymax, ymin);
				depthEnd = getRealCoord(_secondPoint.Y, _defRectangleFirst.Y, _defRectangle.Height, ymax, ymin);
				_rectWrite.Add(new RectWrite(Type, depthStart, depthEnd, gradientStart, gradientEnd));
			}
			// переопределение точек области определения, если область определения рисуется не сверху вниз
			if (Type == 0)
			{
				OverrideRect();
			}
		}

		/// <summary>
		/// Замена разделителя у чисел, заданных пользователем
		/// </summary>
		private void Replace()
			{
				var ci = new CultureInfo("en-US") { NumberFormat = new NumberFormatInfo { NumberDecimalSeparator = "." } };
				try
				{
					xmin = Convert.ToDouble(TbXmin.Text.Replace(",", ".").Replace("ю", ".").Replace("б", "."), ci);
					xmax = Convert.ToDouble(TbXmax.Text.Replace(",", ".").Replace("ю", ".").Replace("б", "."), ci);
					ymin = Convert.ToDouble(TbYmin.Text.Replace(",", ".").Replace("ю", ".").Replace("б", "."), ci);
					ymax = Convert.ToDouble(TbYmax.Text.Replace(",", ".").Replace("ю", ".").Replace("б", "."), ci);
				}
				catch (Exception)
				{
				}
			}

		/// <summary>
		/// Вычисление координат фигуры в заданной системе координат
		/// </summary>
		/// <param name="coordAxis">Координата по заданной оси</param>
		/// <param name="coordDomain">Начальная координата области определения</param>
		/// <param name="sizeDomain">Ширина/Высота области определения</param>
		/// <param name="finalValue">Конечное значение градиента/глубины</param>
		/// <param name="initValue">Начальное значение градиента/глубины</param>
		/// <returns></returns>
		private double getRealCoord( double coordAxis, double coordDomain, double sizeDomain, double finalValue, double initValue)
		{
			return(Math.Abs((coordAxis-coordDomain)/sizeDomain*(finalValue-initValue)+initValue));
		}

		/// <summary>
		///     Нажатие кнопки для очистки канваса
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtClear_Click(object sender, RoutedEventArgs e)
		{
			// удаление зеленых линий
			CnvDraw.Children.Remove(_sLineGreen);
			CnvDraw.Children.Remove(_plGreen);
			CnvDraw.Children.Remove(_tmpLineGreen);
			_plGreen.Points.Clear();
			_sLineGreen.EndPoint = new Point();
			_sLineGreen.StartPoint = new Point();

			// удаление красных линий
			CnvDraw.Children.Remove(_sLineRed);
			CnvDraw.Children.Remove(_plRed);
			CnvDraw.Children.Remove(_tmpLineRed);
			_plRed.Points.Clear();
			_sLineRed.EndPoint = new Point();
			_sLineRed.StartPoint = new Point();

			// удаление квадратов
			foreach (Rectangle item in _sqRect)
			{
				try
				{
					CnvDraw.Children.Remove(item);
				}
				catch (Exception)
				{
				}
			}
			try
			{
				// удаление выделения области определения
				CnvDraw.Children.Remove(_defRectangle);
				_drawRectangle = false;
			}
			catch (Exception)
			{
			}
		}

		/// <summary>
		///     Удаляет последнюю линию
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CnvDraw_KeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.Key == Key.Escape)
				{
					try
					{
						if (Type == 1)
						{
							if (_plGreen.Points.Count > 2)
							{
								_plGreen.Points.Remove(_plGreen.Points.Last());
								_plGreen.Points.Remove(_plGreen.Points.Last());
								_sLineGreen.StartPoint = _plGreen.Points.Last();
							}
							else if (_plGreen.Points.Count > 0)
							{
								_plGreen.Points.Remove(_plGreen.Points.Last());
								_plGreen.Points.Remove(_plGreen.Points.Last());
								//	CnvDraw.Children.Remove(_tmpLineGreen);
								double x = _defRectangleFirst.X, y = _defRectangleFirst.Y;
								_sLineGreen.StartPoint = _defRectangleFirst;
								_sLineGreen.EndPoint = _defRectangleFirst;
								_tmpLineGreen.X1 = x;
								_tmpLineGreen.Y1 = y;
								_tmpLineGreen.X2 = x;
								_tmpLineGreen.Y2 = y;
								isFirstLine = true;
								CnvDraw.Children.Add(_tmpLineGreen);
								try
								{
									CnvDraw.Children.Add(_sLineGreen);
								}
								catch
								{
								}
								;
							}
						}
						// удаление красной линии
						else if (Type == 2)
						{
							if (_plRed.Points.Count > 2)
							{
								_plRed.Points.Remove(_plRed.Points.Last());
								_plRed.Points.Remove(_plRed.Points.Last());
								_sLineRed.StartPoint = _plRed.Points.Last();
							}
							else if (_plRed.Points.Count > 0)
							{
								_plRed.Points.Remove(_plRed.Points.Last());
								_plRed.Points.Remove(_plRed.Points.Last());
								// CnvDraw.Children.Remove(_tmpLineGreen);

								double x = _defRectangleFirst.X, y = _defRectangleFirst.Y;
								_sLineRed.StartPoint = _defRectangleFirst;
								_sLineRed.EndPoint = _defRectangleFirst;
								_tmpLineRed.X1 = x;
								_tmpLineRed.Y1 = y;
								_tmpLineRed.X2 = x;
								_tmpLineRed.Y2 = y;
								isFirstLine = true;

								CnvDraw.Children.Add(_tmpLineRed);
								try
								{
									CnvDraw.Children.Add(_sLineRed);
								}
								catch
								{
								}
								;
							}
						}
						// удаление квадратиков
						else if (Type == 3)
						{
							try
							{
								CnvDraw.Children.Remove(_sqRect[_sqRect.Count - 2]);
								_sqRect.Remove(_sqRect[_sqRect.Count - 2]);
							}
							catch (Exception)
							{
							}
						}
					}
					catch (Exception)
					{
					}
				}
			}
			catch (Exception)
			{
			}
		}

		/// <summary>
		///     Нажатие правой кнопки мыши на канвасе содержащем изображение
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CnvDraw_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (Type == 1)
			{
				if (_plGreen.Points.Count > 1)
				{
					CnvDraw.Children.Remove(_sLineGreen);
				}
				else if (_plGreen.Points.Count > 0)
				{
					isFirstLine = true;
					_sLineGreen.EndPoint = new Point();
					_sLineGreen.StartPoint = new Point();
				}
			}
			if (Type == 2)
			{
				if (_plRed.Points.Count > 1)
				{
					CnvDraw.Children.Remove(_sLineRed);
				}
				else if (_plRed.Points.Count > 0)
				{
					isFirstLine = true;
					_sLineRed.EndPoint = new Point();
					_sLineRed.StartPoint = new Point();
				}
			}
		}

		/// <summary>
		///     Переопределение точек выделения области определения
		/// </summary>
		private static void OverrideRect()
		{
			var p1 = new Point();
			var p2 = new Point();
			p1.X = Math.Min(_defRectangleFirst.X, _defRectangleSecond.X);
			p1.Y = Math.Min(_defRectangleFirst.Y, _defRectangleSecond.Y);
			p2.X = Math.Max(_defRectangleFirst.X, _defRectangleSecond.X);
			p2.Y = Math.Max(_defRectangleFirst.Y, _defRectangleSecond.Y);
			_defRectangleFirst = p1;
			_defRectangleSecond = p2;
		}

		/// <summary>
		///     Растянуть/Переместить прямоугольник на указанные координаты. Построение временного квадрата.
		/// </summary>
		/// <param name="pointLeftTop">Левый верхний угол</param>
		/// <param name="pointRightBottom">Правый нижний угол</param>
		private void MoveDrawRectangle(Point pointLeftTop, Point pointRightBottom)
		{
			_defRectangle.Visibility = Visibility.Visible;
			if (_rect.Height*_rect.Width > 1.2)
			{
				_rect.Visibility = Visibility.Visible;
			}
			else
			{
				_rect.Visibility = Visibility.Hidden;
			}
			_rectForDraw = new Int32Rect();
			if (pointLeftTop.X > pointRightBottom.X)
			{
				Canvas.SetLeft(_rect, pointRightBottom.X);
				_rect.Width = pointLeftTop.X - pointRightBottom.X;
				_rectForDraw.X = (int) pointRightBottom.X;
				if (_rect.Width < 1)
				{
					_rect.Width = 1;
				}
			}
			else
			{
				Canvas.SetLeft(_rect, pointLeftTop.X);
				_rect.Width = pointRightBottom.X - pointLeftTop.X;
				_rectForDraw.X = (int) pointLeftTop.X;
				if (_rect.Width < 1)
				{
					_rect.Width = 1;
				}
			}
			if (pointLeftTop.Y > pointRightBottom.Y)
			{
				Canvas.SetTop(_rect, pointRightBottom.Y);
				_rect.Height = pointLeftTop.Y - pointRightBottom.Y;
				_rectForDraw.Y = (int) pointRightBottom.Y;
				if (_rect.Height < 1)
				{
					_rect.Height = 1;
				}
			}
			else
			{
				Canvas.SetTop(_rect, pointLeftTop.Y);
				_rect.Height = pointRightBottom.Y - pointLeftTop.Y;
				_rectForDraw.Y = (int) pointLeftTop.Y;
				if (_rect.Height < 1)
				{
					_rect.Height = 1;
				}
			}
			_rectForDraw.Width = (int) _rect.Width;
			_rectForDraw.Height = (int) _rect.Height;
		}

		/// <summary>
		///     Нажатие кнопки записи в файл
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtWriteToFile_Click(object sender, RoutedEventArgs e)
		{
			var sw = new StreamWriter("output.txt");
			sw.WriteLine("Тип Глубина от Глубина до Давление от Давление до");
			foreach (var item in _plWrite)
			{
				sw.WriteLine(item);
			}
			foreach (var item in _rectWrite)
			{
				sw.WriteLine(item);
			}
			sw.Close();
		}

		#endregion
	}
}