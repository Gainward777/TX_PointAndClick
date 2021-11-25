using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Renga;

// Палгин собирает значения из свойства в объектах типа Элемент, Сборка, Вентиляционное оборудование и проставляет их в нужные свойства Точки подключения
// Используется механизм ISelection, так что работа идет только с выбранными объектами. 
//В случае, если выбрано несколько объеков типа Элемент и т.д. значения могут проставлять не корректно.

namespace TX_PointAndClick
{
    class Program : IPlugin
    {
        public ActionEventSource actionEvents;
        public SelectionEventSource selectionEventSource;
        public IApplication application;
        public IAction action;
        IOperation operation;
        IUI ui;       

        public bool Initialize(string pluginFolder)
        {
            application = new Renga.Application();
            ui = application.UI;
            action = ui.CreateAction();

            string imagePath = pluginFolder + @"\Icon.png";      // относительный путь к картинке в папке плагина
            IImage image = ui.CreateImage(); //работает через раз... НЕ БАГУЕТ, ЕСЛИ ЯВНО ОБЪЯВЛЯТЬ ПЕРЕМЕННУЮ ЧЕРЕЗ ИНТЕРФЕЙС!!!
            image.LoadFromFile(imagePath);
            action.Icon = image;

            action.DisplayName = "TX_PointAndClick";
            action.ToolTip = "TX_PointAndClick";


            ISelection selection = application.Selection;
            selectionEventSource = new SelectionEventSource(selection);
            selectionEventSource.ModelSelectionChanged += OnModelSelectionChanged;
            action.Enabled = selection.GetSelectedObjects().Length > 0;

            IUIPanelExtension panelExtension = ui.CreateUIPanelExtension();

            actionEvents = new ActionEventSource(action); 
            actionEvents.Triggered += (sender, args) =>
            {
                try
                {
                    Test(selection);    
                }
                catch (Exception ex)
                {
                    ui.ShowMessageBox(MessageIcon.MessageIcon_Error, "Error", ex.ToString());
                    operation.Apply();
                }
            };
            panelExtension.AddToolButton(action);
            ui.AddExtensionToPrimaryPanel(panelExtension);

            return true;

        }

        private void OnModelSelectionChanged(object sender, EventArgs e)
        {
            ISelection selection = application.Selection;
            action.Enabled = selection.GetSelectedObjects().Length > 0;
        }

        public void Stop()
        {
            actionEvents.Dispose();    
            selectionEventSource.Dispose();           
        }

        public void Test(ISelection selection)
        {
            Renga.IModel model = application.Project.Model;
            operation = model.CreateOperation();

            operation.Start();

            string EM_VoltageId = GetPropertyId.Name("ЭМ_Напряжение", application);    // Свойства, в которые надо передать значения
            string EM_NotationId = GetPropertyId.Name("ЭМ_Примечание", application);
            string EM_InstalledCapacityId = GetPropertyId.Name("ЭМ_Установленная мощность", application);
            string TX_TaskEMValuesId = GetPropertyId.Name("ТХ_Задание ЭМ текст", application);    // Свойство, в котором хранятся, передаваемые значения, разделенные "/"

            string TX_TaskEMId = GetPropertyId.Name("ТХ_ЭМ Задание", application); // фильтрующий параметр           

            PointAndClick.Set(selection, application, EM_VoltageId, EM_NotationId, EM_InstalledCapacityId, TX_TaskEMValuesId, TX_TaskEMId, ui);

            operation.Apply();
        }
    }
}
