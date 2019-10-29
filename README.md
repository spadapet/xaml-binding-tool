# XAML Binding Debug Output
This extension provides a tool window that shows XAML binding errors while debugging in Visual Studio 2019. The tool window makes it much easier to detect and understand the binding errors in your XAML that would normally be hidden in the output window.

This is currently in the prototyping phase as I figure out what features are most useful to users. Feedback is appreciated!

Currently supported frameworks:
* WPF for .NET Framework
* WPF for .NET Core 3

Right now a small set of errors are fully parsed and transformed into pretty columns, the rest have their full ugly text shown in the Description column. More and more errors will look pretty in the list over time.

**Location of command in the Debug|Windows menu:**

![Command Location](https://raw.githubusercontent.com/spadapet/xaml-binding-tool/master/XamlBinding/Resources/CommandLocation.png)

**Sample output while debugging a WPF app:**

![XAML Binding Errors](https://raw.githubusercontent.com/spadapet/xaml-binding-tool/master/XamlBinding/Resources/Sample.png)
