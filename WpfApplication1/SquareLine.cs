using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApplication1
{
	public class SquareLine : Shape
	{
		StreamGeometry geometry = new StreamGeometry();

		// Dependency properties
		public static readonly DependencyProperty StartPointProperty =
			LineGeometry.StartPointProperty.AddOwner(
				typeof(SquareLine),
				new FrameworkPropertyMetadata(new Point(0, 0),
					FrameworkPropertyMetadataOptions.AffectsMeasure));

		public static readonly DependencyProperty EndPointProperty =
			LineGeometry.EndPointProperty.AddOwner(
				typeof(SquareLine),
				new FrameworkPropertyMetadata(new Point(0, 0),
					FrameworkPropertyMetadataOptions.AffectsMeasure));

		public Point StartPoint
		{
			set { SetValue(StartPointProperty, value); }
			get { return (Point)GetValue(StartPointProperty); }
		}

		public Point EndPoint
		{
			set { SetValue(EndPointProperty, value); }
			get { return (Point)GetValue(EndPointProperty); }
		}

		protected override Geometry DefiningGeometry
		{
			get
			{
				using (StreamGeometryContext ctx = geometry.Open())
				{
					ctx.BeginFigure(StartPoint, true, false);
					ctx.LineTo(new Point(EndPoint.X, StartPoint.Y), true, false);
					ctx.LineTo(EndPoint, true, false);
				}
				return geometry; 
			}
		}
	}
}
