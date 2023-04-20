using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
namespace MailClient
{
    public partial class CircularProgress : Shape
    {
        static CircularProgress()
        {
            Brush myGreenBrush = new SolidColorBrush(Color.FromArgb(255, 6, 176, 37));
            myGreenBrush.Freeze();

            StrokeProperty.OverrideMetadata(
                typeof(CircularProgress),
                new FrameworkPropertyMetadata(myGreenBrush));
            FillProperty.OverrideMetadata(
                typeof(CircularProgress),
                new FrameworkPropertyMetadata(Brushes.Transparent));

            StrokeThicknessProperty.OverrideMetadata(
                typeof(CircularProgress),
                new FrameworkPropertyMetadata(10.0));
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        private static FrameworkPropertyMetadata valueMetadata = new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, null, new CoerceValueCallback(CoerceValue));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(CircularProgress), valueMetadata);
        private static object CoerceValue(DependencyObject depObj, object baseVal)
        {
            double val = (double)baseVal;
            val = Math.Min(val, 100199.999);
            val = Math.Max(val, 0.0);
            return val;
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                TimeValue time = new TimeValue();
                double startAngle = 90.0;
                double endAngle = 90.0 - ((Value / time.MyTime) * 360.0);

                double maxWidth = Math.Max(0.0, RenderSize.Width - StrokeThickness);
                double maxHeight = Math.Max(0.0, RenderSize.Height - StrokeThickness);

                double xStart = maxWidth / 2.0 * Math.Cos(startAngle * Math.PI / 180.0);
                double yStart = maxHeight / 2.0 * Math.Sin(startAngle * Math.PI / 180.0);

                double xEnd = maxWidth / 2.0 * Math.Cos(endAngle * Math.PI / 180.0);
                double yEnd = maxHeight / 2.0 * Math.Sin(endAngle * Math.PI / 180.0);

                StreamGeometry geom = new StreamGeometry();
                using (StreamGeometryContext ctx = geom.Open())
                {
                    ctx.BeginFigure(new Point((RenderSize.Width / 2.0) + xStart, (RenderSize.Height / 2.0) - yStart), true, false);
                    ctx.ArcTo(new Point((RenderSize.Width / 2.0) + xEnd, (RenderSize.Height / 2.0) - yEnd), new Size(maxWidth / 2.0, maxHeight / 2), 0.0, (startAngle - endAngle) > 180, SweepDirection.Clockwise, true, false);
                }
                return geom;
            }
        }
    }
}