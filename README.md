# XAML Binding Debug Output
Provides a tool window that shows XAML binding errors while debugging in Visual Studio 2019. The tool window makes it much easier to detect and understand the binding errors in your XAML that would normally be hidden in the output window.

This is currently in the prototyping phase I figure out what features are most useful to users. Feedback is appreciated!
* Works when debugging WPF for .NET Framework or .NET Core 3 only
* Right now only "Path Error (40)" is fully parsed and transformed into pretty columns, the rest have their full ugly text shown in the Description column. More and more errors will look pretty in the list over time.

Get to the tool window through a menu command (see the following screenshot) or it'll show up automatically when you debug a WPF app.

![Command Location](https://raw.githubusercontent.com/spadapet/xaml-binding-tool/master/XamlBinding/Resources/CommandLocation.png)

![XAML Binding Errors](https://raw.githubusercontent.com/spadapet/xaml-binding-tool/master/XamlBinding/Resources/Sample.png)
