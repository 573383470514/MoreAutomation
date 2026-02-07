using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Windows.Media.Imaging;

namespace MoreAutomation.Vision
{
    public static class VisionConverter
    {
        public static Mat ToMat(BitmapSource source) => source.ToMat();
        public static BitmapSource ToBitmapSource(Mat mat) => mat.ToBitmapSource();
    }
}