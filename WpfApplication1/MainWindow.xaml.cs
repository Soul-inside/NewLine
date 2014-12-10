// Last Change: 2014 12 10 13:52

using System;
using System.Collections.Generic;
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

		/// <summary>
		///     Сравнение точек прямоугольника для выделения области
		/// </summary>
		/// <param name="X">Первая точка</param>
		/// <param name="Y">Вторая точка</param>
		/// <returns></returns>
		private bool checkRect(Point X, Point Y)
		{
			return X.X > Y.X;
		}

		/// <summary>
		///     координаты прямоугольника для выделения области
		/// </summary>
		private static Rectangle defRectangle = new Rectangle();

		/// <summary>
		///     координаты первой точки прямоугольника для выделения области
		/// </summary>
		private static Point defRectangleFirst;

		/// <summary>
		///     Координаты второй точки прямоугольника для выделения области
		/// </summary>
		private static Point defRectangleSecond;

		/// <summary>
		///     Список всех нарисованных прямоугольников
		/// </summary>
		private static readonly List<Rectangle> _sqRect = new List<Rectangle>();

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
		public int Type = -1;

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

		/// <summary>
		///     Режим рисавания области определения
		/// </summary>
		private bool _drawRectangle;

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
			CnvDraw.Height = ImgWell.Source.Height;
			CnvDraw.Width = ImgWell.Source.Width;
		}

		/// <summary>
		///     Движение мыши по канвасу
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CnvDraw_MouseMove(object sender, MouseEventArgs e)
		{
			// отрисовка линий строго в выделенной области
			double CX = e.GetPosition(CnvDraw).X;
			double CY = e.GetPosition(CnvDraw).Y;
			if (CX > defRectangleSecond.X)
			{
				CX = defRectangleSecond.X;
			}
			if (CY > defRectangleSecond.Y)
			{
				CY = defRectangleSecond.Y;
			}
			if (CX < defRectangleFirst.X)
			{
				CX = defRectangleFirst.X;
			}
			if (CY < defRectangleFirst.Y)
			{
				CY = defRectangleFirst.Y;
			}
			// для рисования зеленой линии
			if (Type == 1)
			{
				if (Pl_green.Points.Count() > 1)
				{
					SLineGreen.EndPoint = new Point(CX, CY);
				}
				else if (Pl_green.Points.Any())
				{
					tmpLineGreen.X2 = tmpLineGreen.X1;
					tmpLineGreen.Y2 = CY;
				}
			}
				//для рисования красной линии
			else if (Type == 2)
			{
				if (Pl_red.Points.Count() > 1)
				{
					SLineRed.EndPoint = new Point(CX, CY);
				}
				else if (Pl_red.Points.Any())
				{
					tmpLineRed.X2 = tmpLineRed.X1;
					tmpLineRed.Y2 = CY;
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
						defRectangleSecond = _secondPoint;
					}
					// для попадания квадратиков в область определения
					if (Type == 3)
					{
						if (defRectangleSecond.X < _secondPoint.X)
						{
							_secondPoint.X = defRectangleSecond.X;
						}
						if (defRectangleSecond.Y < _secondPoint.Y)
						{
							_secondPoint.Y = defRectangleSecond.Y;
						}
						if (defRectangleFirst.X > _secondPoint.X)
						{
							_secondPoint.X = defRectangleFirst.X;
						}
						if (defRectangleFirst.Y > _secondPoint.Y)
						{
							_secondPoint.Y = defRectangleFirst.Y;
						}
					}
					MoveDrawRectangle(_firstPoint, _secondPoint);
				}
			}
		}

		/// <summary>
		///     Нажатие левой кнопки мыши на канвасе содержащем изображение
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CnvDraw_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (_drawRectangle)
			{
				// отрисовка линий строго в выделенной области
				double CX = e.GetPosition(CnvDraw).X;
				double CY = e.GetPosition(CnvDraw).Y;
				if (CX > defRectangleSecond.X)
				{
					CX = defRectangleSecond.X;
				}
				if (CY > defRectangleSecond.Y)
				{
					CY = defRectangleSecond.Y;
				}
				if (CX < defRectangleFirst.X)
				{
					CX = defRectangleFirst.X;
				}
				if (CY < defRectangleFirst.Y)
				{
					CY = defRectangleFirst.Y;
				}
				// для рисования зеленой линии
				if (Type == 1)
				{
					if (CnvDraw.Children.Contains(SLineGreen))
					{
						if (!Pl_green.Points.Any())
						{
							Pl_green.Points.Add(new Point(CX, defRectangleFirst.Y));
							tmpLineGreen.X1 = CX;
							tmpLineGreen.Y1 = defRectangleFirst.Y;
						}
						else if (Pl_green.Points.Count() < 2)
						{
							Pl_green.Points.Add(new Point(tmpLineGreen.X1, CY));
							SLineGreen.StartPoint = new Point(tmpLineGreen.X1, CY);
							SLineGreen.EndPoint = new Point(tmpLineGreen.X1, CY);
							CnvDraw.Children.Remove(tmpLineGreen);
						}
						else
						{
							Pl_green.Points.Add(new Point(CX, SLineGreen.StartPoint.Y));
							Pl_green.Points.Add(new Point(CX, CY));
							SLineGreen.StartPoint = SLineGreen.EndPoint;
						}
					}
				}
					// для рисования красной линии
				else if (Type == 2)
				{
					if (CnvDraw.Children.Contains(SLineRed))
					{
						if (!Pl_red.Points.Any())
						{
							Pl_red.Points.Add(new Point(CX, defRectangleFirst.Y));
							tmpLineRed.X1 = CX;
							tmpLineRed.Y1 = defRectangleFirst.Y;
						}
						else if (Pl_red.Points.Count() < 2)
						{
							Pl_red.Points.Add(new Point(tmpLineRed.X1, CY));
							SLineRed.StartPoint = new Point(tmpLineRed.X1, CY);
							SLineRed.EndPoint = new Point(tmpLineRed.X1, CY);
							CnvDraw.Children.Remove(tmpLineRed);
						}
						else
						{
							Pl_red.Points.Add(new Point(CX, SLineRed.StartPoint.Y));
							Pl_red.Points.Add(new Point(CX, CY));
							SLineRed.StartPoint = SLineRed.EndPoint;
						}
					}
				}
					// в случае рисавания квадратиков
				else
				{
					_firstPoint = e.GetPosition(CnvDraw);
					// для прямоугольной области определения
					if (Type == 0)
					{
						defRectangleFirst = _firstPoint;
					}
					// для попадания квадратиков в область определения
					if (Type == 3)
					{
						if (defRectangleFirst.X > _firstPoint.X)
						{
							_firstPoint.X = defRectangleFirst.X;
						}
						if (defRectangleFirst.Y > _firstPoint.Y)
						{
							_firstPoint.Y = defRectangleFirst.Y;
						}
						if (defRectangleSecond.X< _firstPoint.X)
						{
							_firstPoint.X=defRectangleFirst.X;
						}
						if (defRectangleSecond.Y < _firstPoint.Y)
						{
							_firstPoint.Y = defRectangleFirst.Y;
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
		///     Нажатие правой кнопки мыши на канвасе содержащем изображение
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CnvDraw_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			// удаление временных линий
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

		/// <summary>
		/// нажатие кнопки для построения зеленой линии
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtGreen_Click(object sender, RoutedEventArgs e)
		{
			Type = 1;
			try
			{
				CnvDraw.Children.Add(Pl_green);
				CnvDraw.Children.Add(SLineGreen);
				CnvDraw.Children.Add(tmpLineGreen);
				SLineGreen.EndPoint = new Point();
				SLineGreen.StartPoint = new Point();
				tmpLineGreen.X1 = new double();
				tmpLineGreen.X2 = new double();
			}
			catch (Exception)
			{
			}
		}

		/// <summary>
		/// нажатие кнопки для построения красной линии
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtRed_Click(object sender, RoutedEventArgs e)
		{
			Type = 2;
			try
			{
				CnvDraw.Children.Add(Pl_red);
				CnvDraw.Children.Add(SLineRed);
				CnvDraw.Children.Add(tmpLineRed);
				SLineRed.EndPoint = new Point();
				SLineRed.StartPoint = new Point();
				tmpLineRed.X1 = new double();
				tmpLineRed.X2 = new double();
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
			_rect = new Rectangle {Stroke = Brushes.Black, StrokeThickness = 1, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Fill = Brushes.OrangeRed};
			Canvas.SetTop(_rect, _firstPoint.Y);
			Canvas.SetLeft(_rect, _firstPoint.X);
			CnvDraw.Children.Add(_rect);
			// добавление прямокгольника в список
			_sqRect.Add(_rect);
			_drawRectangle = true;
		}

		/// <summary>
		///     Нажатие кнопки для очистки канваса
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtClear_Click(object sender, RoutedEventArgs e)
		{
			// удаление зеленых линий
			CnvDraw.Children.Remove(SLineGreen);
			CnvDraw.Children.Remove(Pl_green);
			CnvDraw.Children.Remove(tmpLineGreen);
			Pl_green.Points.Clear();
			SLineGreen.EndPoint = new Point();
			SLineGreen.StartPoint = new Point();
			tmpLineGreen.X1 = new double();
			tmpLineGreen.X2 = new double();

			// удаление красных линий
			CnvDraw.Children.Remove(SLineRed);
			CnvDraw.Children.Remove(Pl_red);
			CnvDraw.Children.Remove(tmpLineRed);
			Pl_red.Points.Clear();
			SLineRed.EndPoint = new Point();
			SLineRed.StartPoint = new Point();
			tmpLineRed.X1 = new double();
			tmpLineRed.X2 = new double();

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
				CnvDraw.Children.Remove(defRectangle);
				_drawRectangle = false;
			}
			catch (Exception)
			{
			}
		}

		/// <summary>
		///     Нажатие кнопки записи в файл
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtWriteToFile_Click(object sender, RoutedEventArgs e)
		{
			var fs = new FileStream("output.txt", FileMode.Append);
			var sw = new StreamWriter(fs);
			foreach (var item in _sqRect)
			{
				sw.WriteLine(item);
				Title = Convert.ToString(item);
				sw.WriteLine(item);
			}
			sw.Close();
			fs.Close();
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
				// удаление зеленой линии
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
				// удаление красной линии
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
		}

		/// <summary>
		///     Нажатие кнопки задания области определения функции
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
			defRectangle = _rect;
			defRectangleFirst = _firstPoint;
			defRectangleSecond = _secondPoint;
			_drawRectangle = true;
			Type = 0;
		}

		/// <summary>
		///     Отпускание левой кнопки мыши на канвасе
		///     Построение постоянного квадрата
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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
				}
			}
			// в случае рисования квадратиков оранжевых
			if (Type == 3)
			{
				_rect = new Rectangle {Stroke = Brushes.Black, StrokeThickness = 1, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Fill = Brushes.OrangeRed};
				CnvDraw.Children.Add(_rect);
				_sqRect.Add(_rect);
			}
			// переопределение точек области определения, если область определения рисуется снизу вверх
			if (Type == 0)
			{
				if (checkRect(defRectangleFirst, defRectangleSecond))
				{
					var t = defRectangleFirst;
					defRectangleFirst = defRectangleSecond;
					defRectangleSecond = t;
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

		#endregion
	}
}