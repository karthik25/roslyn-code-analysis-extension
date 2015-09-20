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
using Constants = RoslynCodeAnalysis.Lib.Rules;

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
        private readonly IVsActivityLog _log;

        private int _displayMode = 0;

        public RoslynCodeAnalysisHelper(IWpfTextView view, ITextDocument document, IVsTaskList tasks, DTE2 dte, SVsServiceProvider serviceProvider, IVsActivityLog log)
        {
            _view = view;
            _document = document;
            _text = new Adornment();
            _tasks = tasks;
            _serviceProvider = serviceProvider;
            _log = log;
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
            }

            var analysisData = _document.FilePath.AnalyzeFile();
            AdornmentData adornmentData = null;
            if (_displayMode == 0)
            {
                _log.LogEntry((uint)__ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION, "RoslynCodeAnalysisExtension",
                              "Initial adornment data displayed");
                adornmentData = new AdornmentData
                {
                    ClassText = analysisData.Classes.Count.Pluralize("class"),
                    MethodText = analysisData.Classes.SelectMany(a => a.MethodInfos).Count().Pluralize("method"),
                    PropertyText = analysisData.Classes.SelectMany(a => a.PropertyInfos).Count().Pluralize("property"),
                    FieldText = analysisData.Classes.SelectMany(a => a.FieldInfos).Count().Pluralize("field")
                };
                _timer.Interval = 15000;
            }
            else
            {
                var requiredClass = analysisData.Classes[(_displayMode - 1)];
                adornmentData = new AdornmentData
                {
                    ClassText = requiredClass.Name,
                    MethodText = requiredClass.MethodInfos.Count.Pluralize("method"),
                    PropertyText = requiredClass.PropertyInfos.Count.Pluralize("property"),
                    FieldText = requiredClass.FieldInfos.Count.Pluralize("field"),
                    MethodTextTooltip = requiredClass.MethodInfos != null && requiredClass.MethodInfos.Count > 0 && requiredClass.MethodInfos.Any(m => m.LineCount > Constants.Method.IdealLineCount)
                                                ? string.Format("{0} method has {1} lines, consider refactoring!",
                                                        requiredClass.MethodInfos.First(m => m.LineCount > Constants.Method.IdealLineCount).Name,
                                                        requiredClass.MethodInfos.First(m => m.LineCount > Constants.Method.IdealLineCount).LineCount)
                                                : string.Empty,
                    ClassTextTooltip = requiredClass.MethodInfos != null && requiredClass.MethodInfos.Count > Constants.Class.IdealMethodCount 
                                                ? string.Format("{0} has {1} methods, consider splitting this class up", requiredClass.Name, requiredClass.MethodInfos.Count) 
                                                : string.Empty
                };
            }

            _text.SetValues(errors, adornmentData);
            var maxCount = 1 + analysisData.Classes.Count;
            _displayMode = _displayMode.Cycle(maxCount);

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