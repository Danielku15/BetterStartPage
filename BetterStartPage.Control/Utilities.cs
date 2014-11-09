﻿using System.ComponentModel;
using System.Diagnostics;
using EnvDTE;
using EnvDTE80;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace BetterStartPage.Control
{
    class Utilities
    {
        public static DTE2 GetDTE(object dataContext)
        {
            if (dataContext == null)
            {
                return null;
            }

            var typeDescriptor = dataContext as ICustomTypeDescriptor;
            if (typeDescriptor != null)
            {
                PropertyDescriptorCollection propertyCollection = typeDescriptor.GetProperties();
                return propertyCollection.Find("DTE", false).GetValue(dataContext) as DTE2;
            }
            
            var dataSource = dataContext as DataSource;
            if (dataSource != null)
            {
                return dataSource.GetValue("DTE") as DTE2;
            }

            Debug.Assert(false, "Could not get DTE instance, was " + (dataContext == null ? "null" : dataContext.GetType().ToString()));
            return null;
        }

        public static ServiceProvider GetServiceProvider(DTE2 dte)
        {
            return new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)dte);
        }
    }
}