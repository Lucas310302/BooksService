using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BookServiceClient.Helpers
{
    public static class WatermarkHelper
    {
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.RegisterAttached("Watermark", typeof(string), typeof(WatermarkHelper),
                new PropertyMetadata(string.Empty, OnWatermarkChanged));

        public static string GetWatermark(DependencyObject obj) => (string)obj.GetValue(WatermarkProperty);

        public static void SetWatermark(DependencyObject obj, string value) => obj.SetValue(WatermarkProperty, value);

        private static void OnWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.Loaded += (sender, _) => UpdateWatermark(textBox);
                textBox.GotFocus += (sender, _) => UpdateWatermark(textBox);
                textBox.LostFocus += (sender, _) => UpdateWatermark(textBox);
            }
        }

        private static void UpdateWatermark(TextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Text) && !textBox.IsFocused)
            {
                textBox.Text = GetWatermark(textBox);
                textBox.Foreground = Brushes.Gray;
            }
            else if (textBox.Text == GetWatermark(textBox) && textBox.IsFocused)
            {
                textBox.Text = string.Empty;
                textBox.Foreground = Brushes.Black;
            }
        }
    }
}
