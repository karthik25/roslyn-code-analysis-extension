﻿using System;
using System.ComponentModel.Composition;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace RoslynCodeAnalysis
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    public class RoslynCodeAnalysisFactory : IWpfTextViewCreationListener
    {
        public const string LayerName = "RoslynCodeAnalysisHighlights";

        [Import]
        public SVsServiceProvider serviceProvider { get; set; }

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        [Export(typeof(AdornmentLayerDefinition))]
        [Name(LayerName)]
        [Order(After = PredefinedAdornmentLayers.Caret)]
        public AdornmentLayerDefinition editorAdornmentLayer = null;

        public void TextViewCreated(IWpfTextView textView)
        {
            var tasks = serviceProvider.GetService(typeof(SVsErrorList)) as IVsTaskList;
            var dte = serviceProvider.GetService(typeof(DTE)) as DTE2;
            var log = serviceProvider.GetService(typeof (SVsActivityLog)) as IVsActivityLog;

            try
            {
                ITextDocument document;
                if (!TextDocumentFactoryService.TryGetTextDocument(textView.TextDataModel.DocumentBuffer, out document))
                    return;

                if (Path.GetExtension(document.FilePath) != ".cs")
                    return;
            
                var highlighter = new RoslynCodeAnalysisHelper(textView, document, tasks, dte, serviceProvider, log);

                // On file save (B)
                document.FileActionOccurred += (s, e) =>
                    {
                        if (e.FileActionType == FileActionTypes.ContentSavedToDisk)
                            highlighter.Update(true);
                    };
            }
            catch (Exception exception)
            {
                if (log != null)
                {
                    log.LogEntry((uint)__ACTIVITYLOG_ENTRYTYPE.ALE_ERROR, "RoslynCodeAnalysisExtension",
                                 "Exception: " + exception);
                }
            }
        }
    }
}