using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renga;

namespace TX_PointAndClick
{
    public class PointAndClick
    {
        public static void Set(ISelection selection, IApplication application, string EM_VoltageId, string EM_NotationId, string EM_InstalledCapacityId, string TX_TaskEMValuesId, string TX_TaskEMId, IUI ui)
        {
            IModel model = application.Project.Model;
            IModelObjectCollection modelObjectCollection = model.GetObjects();

            int[] selectionObjects = (int[])selection.GetSelectedObjects(); // коллекция ID, которые 22046 и т.д.
            string name = string.Empty;
            double voltage = 0;
            double installedCapacity = 0;

            foreach (int index in selectionObjects)  // сбор значений
            {
                IModelObject modelObject = modelObjectCollection.GetById(index);

                if (modelObject.ObjectType == ObjectTypes.Element && modelObject.Name.Contains("ТХ_") 
                    || modelObject.ObjectType == ObjectTypes.AssemblyInstance && modelObject.Name.Contains("ТХ_")
                    || modelObject.ObjectType == ObjectTypes.MechanicalEquipment && modelObject.Name.Contains("ТХ_"))
                {
                    IPropertyContainer propertyContainer = modelObject.GetProperties();                    

                    if (propertyContainer.Get(Guid.Parse(TX_TaskEMId)).GetStringValue() == "Да")
                    {
                        string TX_TaskEM = propertyContainer.GetS(TX_TaskEMValuesId).GetStringValue();
                        ui.ShowMessageBox(MessageIcon.MessageIcon_Info, "", TX_TaskEM); // Вывод на экран строки из свойства, в котором хранятся значения, для контроля правильности его заполнения
                        string[] TX_Task = TX_TaskEM.Split('/');
                        name = TX_Task[0];
                        voltage = double.Parse(TX_Task[1]);
                        installedCapacity = double.Parse(TX_Task[2]);
                    }
                }
            }

            foreach (int index in selectionObjects)  // простановка зачений
            {
                IModelObject modelObject = modelObjectCollection.GetById(index);
                

                if (modelObject.ObjectType == ObjectTypes.RoutePoint)
                {                    
                    IPropertyContainer propertyContainer = modelObject.GetProperties();
                    

                    propertyContainer.Get(Guid.Parse(EM_NotationId)).ResetValue();
                    propertyContainer.Get(Guid.Parse(EM_NotationId)).SetStringValue(name);

                    propertyContainer.Get(Guid.Parse(EM_VoltageId)).ResetValue();
                    propertyContainer.Get(Guid.Parse(EM_VoltageId)).SetDoubleValue(voltage);

                    propertyContainer.Get(Guid.Parse(EM_InstalledCapacityId)).ResetValue();
                    propertyContainer.Get(Guid.Parse(EM_InstalledCapacityId)).SetDoubleValue(installedCapacity);
                }
            }
        }
    }
}
