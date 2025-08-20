using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using Vortex.ViewModels;

namespace Vortex;

public class ViewLocator : IDataTemplate
{

    public Control? Build(object? param)
    {
        if (param is null) return null;

        Debug.WriteLine($"ViewLocator: Build called with param type: {param.GetType().FullName}");

        var viewModelType = param.GetType();
        var assembly = viewModelType.Assembly;
        var viewTypeName = viewModelType.FullName!
            .Replace("ViewModel", "View")
            .Replace("ViewModels", "Views");
        var type = assembly.GetType(viewTypeName);

        if (type != null)
        {
            Debug.WriteLine($"ViewLocator: Found view type {type.FullName}");
            return (Control)Activator.CreateInstance(type)!;
        }

        Debug.WriteLine($"ViewLocator: View not found for {viewTypeName}");
        return new TextBlock { Text = "Not Found: " + viewTypeName };
    }

    public bool Match(object? data)
    {
        if (data == null) return false;

        if (data is string s)
        {
            Debug.WriteLine($"Match: got string value = '{s}'");
            return false; // ï∂éöóÒÇÕViewLocatorÇ≈èàóùÇµÇ»Ç¢
        }

        Debug.WriteLine($"Match: type = {data.GetType().FullName}");
        return data is ObservableObject;
    }
}
