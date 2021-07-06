# Deprecated
**This extension is deprecated. Visual Studio 2019+ now has a better XAML Binding Failure pane as part of the product.**

# XAML Binding Debug Output
[Download the Visual Studio 2019 extension](https://marketplace.visualstudio.com/items?itemName=PeterSpa.XamlBinding)

This extension provides a tool window that shows XAML binding failures while debugging in Visual Studio 2019. The tool window makes it much easier to detect and understand the binding failures in your XAML that would normally be hidden in the output window.

This is currently in the prototyping phase as I figure out what features are most useful to users. Feedback is appreciated!

Currently supported frameworks:
* WPF for .NET Framework
* WPF for .NET Core 3
* UWP
* Xamarin
    * Android and iOS are supported.
    * UWP is supported in Xamarin version 4.5.0.266-pre3 and newer.
    * UWP must target at least Windows version 16299.

**Location of the command in the Debug|Windows menu:**

![Command Location](https://raw.githubusercontent.com/spadapet/xaml-binding-tool/master/XamlBinding/Resources/CommandLocation.png)

**Sample output while debugging a WPF app:**

![XAML Binding Failures](https://raw.githubusercontent.com/spadapet/xaml-binding-tool/master/XamlBinding/Resources/Sample.png)

![XAML Binding Failures](https://raw.githubusercontent.com/spadapet/xaml-binding-tool/master/XamlBinding/Resources/SampleDark.png)
