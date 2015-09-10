using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Threading;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using RoslynCodeAnalysis.Lib;

namespace RoslynCodeAnalysis
{
    public class RoslynCodeAnalysisHelper
    {
        private readonly Adornment _text;
        private readonly IWpfTextView _view;
        private readonly IAdornmentLayer _adornmentLayer;
        private readonly ITextDocument _document;
        private readonly IVsTaskList _tasks;
        private readonly Dispatcher _dispatcher;
        private bool _processing;
        private readonly Timer _timer;
        private SVsServiceProvider _serviceProvider;

        private int _displayMode = 0;

        public RoslynCodeAnalysisHelper(IWpfTextView view, ITextDocument document, IVsTaskList tasks, DTE2 dte, SVsServiceProvider serviceProvider)
        {
            _view = view;
            _document = document;
            _text = new Adornment();
            _tasks = tasks;
            _serviceProvider = serviceProvider;
            _dispatcher = Dispatcher.CurrentDispatcher;

            _adornmentLayer = view.GetAdornmentLayer(RoslynCodeAnalysisFactory.LayerName);

            _view.ViewportHeightChanged += SetAdornmentLocation;
            _view.ViewportWidthChanged += SetAdornmentLocation;

            _text.MouseUp += (s, e) => dte.ExecuteCommand("View.ErrorList");

            _timer = new Timer(750);
            _timer.Elapsed += (s, e) =>
            {
                _timer.Stop();
                System.Threading.Tasks.Task.Run(() =>
                {
                    _dispatcher.Invoke(new Action(() => Update(false)), DispatcherPriority.ApplicationIdle, null);
                });
            };
            _timer.Start();
        }

        void SetAdornmentLocation(object sender, EventArgs e)
        {
            Canvas.SetLeft(_text, _view.ViewportRight - 230);
            Canvas.SetTop(_text, _view.ViewportTop + 20);
        }

        public void Update(bool highlight)
        {
            if (!highlight && _processing)
                return;

            _processing = true;

            UpdateAdornment(highlight);

            if (_adornmentLayer.IsEmpty)
                _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, _text, null);

            _processing = false;
            _timer.Start();
        }

        private async void UpdateAdornment(bool highlight)
        {
            var errors = 0;
            var warnings = 0;
            var messages = 0;

            foreach (var item in GetErrorListItems())
            {
                string file;
                item.Document(out file);
                if (string.IsNullOrEmpty(file) || file != _document.FilePath)
                    continue;

                var errorItem = item as IVsErrorItem;
                uint errorCategory;
                errorItem.GetCategory(out errorCategory);
                if (errorCategory == (uint)__VSERRORCATEGORY.EC_ERROR) errors++;
                if (errorCategory == (uint)__VSERRORCATEGORY.EC_WARNING) warnings++;
                if (errorCategory == (uint)__VSERRORCATEGORY.EC_MESSAGE) messages++;
            }

            var analysisData = _document.FilePath.AnalyizeFile();
            if (_displayMode == 0)
            {
                var classText = string.Format("{0} classes", analysisData.Count);
                var methodText = string.Format("{0} methods", analysisData.SelectMany(a => a.MethodInfos).Count());
                var propText = string.Format("{0} properties", analysisData.SelectMany(a => a.PropertyInfos).Count());
                var fieldText = string.Format("{0} fields", analysisData.SelectMany(a => a.FieldInfos).Count());
                _text.SetValues(errors, classText, methodText, propText, fieldText);
                _timer.Interval = 15000;
            }
            else if (_displayMode == 1)
            {
                _text.SetValues(errors, "---", "===", "|||", "xxx");                
            }
            else if (_displayMode == 2)
            {
                _text.SetValues(errors, "===", "|||", "xxx", "---");
            }
            else if (_displayMode == 3)
            {
                _text.SetValues(errors, "|||", "xxx", "---", "===");
            }

            if ((_displayMode + 1) < 4)
            {
                _displayMode++;
            }
            else
            {
                _displayMode = 0;
            }

            if (highlight)
                await _text.Highlight();
        }

        public IEnumerable<IVsTaskItem> GetErrorListItems()
        {
            IVsEnumTaskItems itemsEnum;
            _tasks.EnumTaskItems(out itemsEnum);

            var oneItem = new IVsTaskItem[1];
            var items = new List<IVsTaskItem>();
            var result = 0;
            do
            {
                result = itemsEnum.Next(1, oneItem, null);
                if (result == 0)
                {
                    items.Add(oneItem[0]);
                }
            } while (result == 0);

            return items;
        }
    }
}