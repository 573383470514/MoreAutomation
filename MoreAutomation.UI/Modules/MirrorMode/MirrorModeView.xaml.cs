using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MoreAutomation.UI.Modules.MirrorMode
{
    public partial class MirrorModeView : UserControl
    {
        private const int GridSpacing = 50; // 网格线间距像素
        private const int DotRadius = 5; // 采集点圆点半径

        public MirrorModeView()
        {
            InitializeComponent();
            Loaded += MirrorModeView_Loaded;
        }

        private void MirrorModeView_Loaded(object sender, RoutedEventArgs e)
        {
            DrawGrid();
            CaptureCanvas.SizeChanged += CaptureCanvas_SizeChanged;
        }

        private void CaptureCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Canvas 大小改变时重绘网格
            DrawGrid();
        }

        private void DrawGrid()
        {
            CaptureCanvas.Children.Clear();

            double width = CaptureCanvas.ActualWidth;
            double height = CaptureCanvas.ActualHeight;

            // 绘制竖线
            for (double x = 0; x <= width; x += GridSpacing)
            {
                var line = new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = height,
                    Stroke = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                    StrokeThickness = 0.5,
                    Opacity = 0.5
                };
                CaptureCanvas.Children.Add(line);
            }

            // 绘制横线
            for (double y = 0; y <= height; y += GridSpacing)
            {
                var line = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = width,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                    StrokeThickness = 0.5,
                    Opacity = 0.5
                };
                CaptureCanvas.Children.Add(line);
            }

            // 绘制边框
            var border = new Rectangle
            {
                Width = width,
                Height = height,
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 2
            };
            Canvas.SetLeft(border, 0);
            Canvas.SetTop(border, 0);
            CaptureCanvas.Children.Add(border);
        }

        private void CaptureCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not MirrorModeViewModel vm || !vm.IsCapturing)
                return;

            var position = e.GetPosition(CaptureCanvas);
            double width = CaptureCanvas.ActualWidth;
            double height = CaptureCanvas.ActualHeight;

            // 计算比例坐标（0.0 ~ 1.0）
            double xPercent = position.X / width;
            double yPercent = position.Y / height;

            // 确保在 [0, 1] 范围内
            xPercent = System.Math.Max(0, System.Math.Min(1, xPercent));
            yPercent = System.Math.Max(0, System.Math.Min(1, yPercent));

            // 通知 ViewModel 记录这个坐标
            vm.AddCapturedCoordinate(xPercent, yPercent);

            // 在 Canvas 上绘制采集点
            DrawCapturedPoint(position.X, position.Y, vm.CapturedCount);
        }

        private void DrawCapturedPoint(double x, double y, int pointNumber)
        {
            // 绘制圆点
            var circle = new Ellipse
            {
                Width = DotRadius * 2,
                Height = DotRadius * 2,
                Fill = new SolidColorBrush(Color.FromRgb(255, 200, 0)),
                Opacity = 0.8
            };
            Canvas.SetLeft(circle, x - DotRadius);
            Canvas.SetTop(circle, y - DotRadius);
            CaptureCanvas.Children.Add(circle);

            // 绘制点号标签
            var label = new TextBlock
            {
                Text = pointNumber.ToString(),
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Width = DotRadius * 2,
                Height = DotRadius * 2,
                VerticalAlignment = VerticalAlignment.Center
            };
            Canvas.SetLeft(label, x - DotRadius);
            Canvas.SetTop(label, y - DotRadius);
            CaptureCanvas.Children.Add(label);
        }
    }
}
