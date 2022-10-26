using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitAddIn_MyProject
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class MainClass : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                //开启事务
                Transaction documentTransaction = new Transaction(commandData.Application.ActiveUIDocument.Document, "Document");
                documentTransaction.Start();

                //创建一个DataManager类实例
                DataManager dataManager = new DataManager(commandData);

                bool? dialogResult = default;

                //创建一个窗体
                using (MainWindow mainWindow = new MainWindow(dataManager))
                {
                    dialogResult = mainWindow.ShowDialog();
                }

                if (dialogResult == true)
                {
                    documentTransaction.Commit();
                    return Result.Succeeded;
                }
                else
                {
                    documentTransaction.RollBack();
                    return Result.Cancelled;
                }
            }
            catch (Exception e)
            {
                //出错处理
                message = e.Message;
                return Result.Failed;
            }
        }
    }
}
