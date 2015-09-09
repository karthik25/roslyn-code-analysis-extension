using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace RoslynCodeAnalysis
{
    public sealed class Adornment : Border
    {
        private readonly TextBlock _classes = CreateBlocks(Colors.IndianRed);
        private readonly TextBlock _methods = CreateBlocks(Colors.Green);
        private readonly TextBlock _properties = CreateBlocks(Colors.DarkOrange);
        private readonly TextBlock _fields = CreateBlocks(Colors.CornflowerBlue);
        private readonly StackPanel _panel = new StackPanel();

        public Adornment()
        {
            this.BorderThickness = new Thickness(0, 0, 0, 2);
            this.Padding = new Thickness(0, 0, 0, 3);
            this.Child = _panel;

            _panel.Children.Add(_classes);
            _panel.Children.Add(_methods);
            _panel.Children.Add(_properties);
            _panel.Children.Add(_fields);

            this.Cursor = Cursors.Hand;
        }

        public void SetValues(int errors, string classText, string methodText, string propText, string fieldText)
        {
            if (errors != 0) return;
            SetValue(_methods, methodText);
            SetValue(_properties, propText);
            SetValue(_fields, fieldText);
        }

        private static void SetValue(TextBlock block, string text = "---")
        {
            block.Text = text;
        }

        private static TextBlock CreateBlocks(Color color)
        {
            return new TextBlock
            {
                FontSize = 16,
                Foreground = new SolidColorBrush(color),
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Left,
                FontFamily = new FontFamily("Consolas"),
            };
        }

        public async Task Highlight()
        {
            await Dispatcher.CurrentDispatcher.BeginInvoke(new Action(async () =>
            {
                if (Visibility == Visibility.Visible)
                {
                    BorderBrush = new SolidColorBrush(Colors.Red);
                    BorderBrush.Opacity = .5;
                    await Task.Delay(500);
                    BorderBrush = null;
                }

            }), DispatcherPriority.ApplicationIdle, null);
        }
    }
}