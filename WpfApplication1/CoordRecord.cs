using System;

namespace WpfApplication1
{
	class CoordRecord
	{
		/// <summary>
		/// Тип линии
		/// </summary>
		public int Type { get ; set; }

		/// <summary>
		/// Глубина от
		/// </summary>
		public double DepthStart;

		/// <summary>
		/// Глубина до
		/// </summary>
		public double DepthEnd;

		/// <summary>
		/// Градиент от
		/// </summary>
		public double GradientStart;

		/// <summary>
		/// Градиент до
		/// </summary>
		public double GradientEnd;


		/// <summary>
		/// Создает объект для записи информации о линии
		/// </summary>
		/// <param name="Type">Тип линии</param>
		/// <param name="depthStart">Глубина от</param>
		/// <param name="deptEnd">Глубина до</param>
		/// <param name="pressureStart">Градиент от</param>
		/// <param name="pressureEnd">Градиент до</param>
		public CoordRecord( int type,
							double depthStart,
							double depthEnd,
							double gradientStart,
							double gradientEnd)
		{
			Type = type;
			DepthStart = depthStart;
			DepthEnd = depthEnd;
			GradientStart = gradientStart;
			GradientEnd = gradientEnd;
		}

		/// <summary>
		/// Получение строки
		/// </summary>
		/// <returns>Возвращает отформатированную строку</returns>
		public override string ToString()
		{
			return string.Format(" {0} {1} {2} {3} {4}", Type, Math.Round(DepthStart), Math.Round(DepthEnd), Math.Round(GradientStart, 2), Math.Round(GradientEnd, 2));
		}
	}
	}
