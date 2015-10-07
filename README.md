# Roslyn Code Analysis Extension

Looking at the name of the extension, you probably already guessed what this does and also wondering why I didn't come up with a more interesting name :) Just in case, this extension uses various roslyn (assemblies like **Microsoft.CodeAnalysis**, **Microsoft.CodeAnalysis.CSharp**) in order to analyze the active tab and shows some helpful information. Like this one for instance!

![Initial view](screenshots/initial.png)

To begin with, you immediately get to see the number of classes in the current tab. Followed by this, every 15 seconds more information about every class is displayed, like this, for example from the same file:

![Initial view](screenshots/class_info.png)

In the above screenshot, you will notice that there is a **(*)** next to the name. This implies that there is more information if you mouseover. In this case the additional information is:

![Initial view](screenshots/class_info_mouseover.png)

The rules to identify if the number of lines of a method is below a certain value, or if the number of methods in a class is below a certain value is defined in various classes under the `RoslynCodeAnalysis.Lib.Rules` namespace (you can look at them [here](https://github.com/karthik25/roslyn-code-analysis-extension/tree/master/RoslynCodeAnalysis.Lib/Rules)). Again, it's a work in progress and so not all the rules defined here have been used yet! Here is another screenshot of a class that has more than the recommended number of methods (10).

![Initial view](screenshots/class_info_mouseover_method_count.png)

Also, if the tab you are viewing also has interfaces, after the number of classes are displayed, you will also see the number of interfaces in the file, like this:

![Initial view](screenshots/interface.png)

It was mentioned earlier that the information displayed changes every 15 seconds after the first time. During this time the contents of the current tab are parsed using roslyn. Before this is carried out there are some checks in place so that if there are errors in the current file, the parsing process does not happen (credits - Error Highlighter extension [[^](https://github.com/madskristensen/ErrorHighlighter)]).

## Installation

To install the extension, download the project, build it and open the RoslynCodeAnalysisExtension\bin\Debug folder, and then double-click on the RoslynCodeAnalysisExtension.vsix file! 

## Supported Versions

I have tested this extension with Visual Studio 2012 Professional, Visual Studio 2012 Ultimate and Visual Studio Community 2015 RC. Soon I will test it with Visual Studio 2013 too and update here. When you build and click on the .vsix file generated the dialog would list all the available version to which the extension should be installed including Visual Studio 2013. But please remember that I haven't had the chance to verify it (but I believe it should work). When I try it out for VS 2013, if I notice that it does not work, I will be updating the **vsixmanifest** file to exclude it.

## Issues

Let me keep it short :) Please use the [issues](https://github.com/karthik25/roslyn-code-analysis-extension/issues) section to report bugs!
