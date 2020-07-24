#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace Addin_Manager
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            string path = Assembly.GetExecutingAssembly().Location;
            String exeConfigPath = @""; //Put the path of the dll of your addin here
            String exeConfigPath2 = Path.GetDirectoryName(path);

            string strCommandName = ""; // Put the Name of your command here 

            byte[] assemblyBytes = File.ReadAllBytes(exeConfigPath);

            Assembly objAssembly = Assembly.Load(assemblyBytes);
            IEnumerable<Type> myIEnumerableType = GetTypesSafely(objAssembly);
            foreach (Type objType in myIEnumerableType)
            {
                if (objType.IsClass)
                {
                    if (objType.Name.ToLower() == strCommandName.ToLower())
                    {
                        object ibaseObject = Activator.CreateInstance(objType);
                        object[] arguments = new object[] { commandData, exeConfigPath2, elements };
                        object result = null;

                        result = objType.InvokeMember("Execute", BindingFlags.Default | BindingFlags.InvokeMethod, null, ibaseObject, arguments);

                        break;
                    }
                }
            }
            return Result.Succeeded;

        }
        private static IEnumerable<Type> GetTypesSafely(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(x => x != null);
            }

        }
    }
}
