using System;
using System.IO;
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
        private readonly TextBlock _classes = CreateBlock(Colors.IndianRed);
        private readonly TextBlock _methods = CreateBlock(Colors.Green);
        private readonly TextBlock _properties = CreateBlock(Colors.DarkOrange);
        private readonly TextBlock _fields = CreateBlock(Colors.CornflowerBlue);
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

        public void SetValues(int errors, AdornmentData adornmentData)
        {
            if (errors != 0) return;
            SetValue(_classes, adornmentData.ClassText, adornmentData.ClassTextTooltip);
            SetValue(_methods, adornmentData.MethodText, adornmentData.MethodTextTooltip);
            SetValue(_properties, adornmentData.PropertyText);
            SetValue(_fields, adornmentData.FieldText);
        }

        public void SetErrors(string errorText)
        {
            SetValue(_classes, errorText);
            SetValue(_methods, string.Empty);
            SetValue(_properties, string.Empty);
            SetValue(_fields, string.Empty);
        }

        private static void SetValue(TextBlock block, string text, string toolTip = null)
        {
            var displayText = text;
            if (!string.IsNullOrEmpty(toolTip))
            {
                displayText += " (*)";
                block.FontWeight = FontWeights.ExtraBold;
            }
            block.Text = displayText;
            block.ToolTip = string.IsNullOrEmpty(toolTip) ? displayText : toolTip;
        }

        private static TextBlock CreateBlock(Color color)
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