using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Visual;

namespace RevitAPITrainingCreateElementAndAnatations
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;

        public List<WallType> WallTypes { get; set; }= new List<WallType>();
        public DelegateCommand SaveCommand { get; set; }
        public double WallHeight { get; set; }
        public PipingSystemType SelectedPipeSystem { get; set; }
        public WallType SelectedWallType { get; set; }
        public List<Level> Levels { get; set; } = new List<Level>();
        public Level SelectedLevel { get; set; }
        public List<XYZ> Points { get; set; }=new List<XYZ>();

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            WallTypes = WallsUtils.GetWallTypes(commandData);
            Levels = LevelsUtils.GetLevels(commandData);
            SaveCommand=new DelegateCommand(OnSaveCommand);
            WallHeight = 1000;
        }

        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Points = SelectionUtils.GetPoints(_commandData, "Выберите точки", ObjectSnapTypes.Endpoints);

            if(Points.Count < 2||
                SelectedWallType==null||
                SelectedLewel==null)
                return;
            List<Curve> curves = new List<Curve>();
            for (int i = 0; i < Points.Count; i++)
            {
                if (i == 0) continue;
                XYZ prevPoint=Points[i-1];
                XYZ nextPoint=Points[i];
                Curve curve = Line.CreateBound(prevPoint, nextPoint);
                curves.Add(curve);  
            }

            using (Transaction ts = new Transaction(doc,"Create wall"))
            {
                ts.Start();
                foreach (Curve curve in curves)
                {
                    Wall.Create(doc, curve,SelectedWallType.Id,SelectedLewel.Id,
                        UnitUtils.ConvertToInternalUnits(WallHeight,UnitTypeId.Millimeters),
                        0,//смещение 
                        false,//должна ли быть стена повёрнута
                        false//является ли стена конструктивной
                        );
                }
                ts.Commit();
            }
            RaiseCloseRequest();
        }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
