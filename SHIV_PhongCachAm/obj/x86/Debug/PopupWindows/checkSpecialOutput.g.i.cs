﻿#pragma checksum "..\..\..\..\PopupWindows\checkSpecialOutput.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "C68E65CF655114575C88C069BC32E52F22F83519"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using SHIV_PhongCachAm.PopupWindows;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace SHIV_PhongCachAm.PopupWindows {
    
    
    /// <summary>
    /// checkSpecialOutput
    /// </summary>
    public partial class checkSpecialOutput : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 47 "..\..\..\..\PopupWindows\checkSpecialOutput.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox lblInfo001;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\..\..\PopupWindows\checkSpecialOutput.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox lblInfo002;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\..\..\PopupWindows\checkSpecialOutput.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox lblInfo003;
        
        #line default
        #line hidden
        
        
        #line 50 "..\..\..\..\PopupWindows\checkSpecialOutput.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnConfirm;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/SHIV_PhongCachAm;component/popupwindows/checkspecialoutput.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\PopupWindows\checkSpecialOutput.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.lblInfo001 = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 2:
            this.lblInfo002 = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 3:
            this.lblInfo003 = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 4:
            this.btnConfirm = ((System.Windows.Controls.Button)(target));
            
            #line 50 "..\..\..\..\PopupWindows\checkSpecialOutput.xaml"
            this.btnConfirm.Click += new System.Windows.RoutedEventHandler(this.BtnConfirm_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

