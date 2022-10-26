using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RevitAddIn_MyProject
{
    /// <summary>
    /// DataManager类用来存储、创建和编辑 空间元素集 和 区域元素集
    /// </summary>
    public class DataManager
    {
        public ExternalCommandData CommandData { get; }
        private Document _ActiveDocument;
        private Dictionary<int, List<SpaceZoneNode>> _SpaceDictionary;
        private Dictionary<int, List<ZoneSpacesNode>> _ZoneDictionary;
        private List<Level> _Levels;
        private Phase _DefaultPhase;
        private Level _CurrentLevel;

        //给WPF MainWindow界面暴露的数据源
        public Level CurrentLevel
        {
            get => _CurrentLevel;
            set
            {
                _CurrentLevel = value;
                UpdateObservableCollections();
            }
        }
        public ObservableCollection<SpaceZoneNode> SpaceCollection { get; }
        public ObservableCollection<ZoneSpacesNode> ZoneCollection { get; }
        public ReadOnlyCollection<Level> Levels => new ReadOnlyCollection<Level>(_Levels);

        //给WPF EditZoneWindow界面暴露的数据源
        public ObservableCollection<SpaceZoneNode> AvailableSpaceCollection { get; }
        public ObservableCollection<SpaceZoneNode> CurrentZoneSpaceCollection { get; }

        public DataManager(ExternalCommandData commandData)
        {
            CommandData = commandData;
            AvailableSpaceCollection = new ObservableCollection<SpaceZoneNode>();
            CurrentZoneSpaceCollection = new ObservableCollection<SpaceZoneNode>();
            _ActiveDocument = CommandData.Application.ActiveUIDocument.Document;

            //默认阶段
            Parameter parameter = _ActiveDocument.ActiveView.get_Parameter(BuiltInParameter.VIEW_PHASE);
            ElementId phaseId = parameter.AsElementId();
            _DefaultPhase = _ActiveDocument.GetElement(phaseId) as Phase;

            //标高
            _Levels = new List<Level>();
            FilteredElementCollector levels = new FilteredElementCollector(_ActiveDocument).OfClass(typeof(Level));
            _Levels = levels.Select(element => element as Level).ToList();
            _CurrentLevel = _Levels[0];

            //空间字典
            _SpaceDictionary = new Dictionary<int, List<SpaceZoneNode>>();
            foreach (Level level in _Levels)
            {
                _SpaceDictionary.Add(level.Id.IntegerValue, new List<SpaceZoneNode>());
            }
            UpdateSpaceDictionary();
            SpaceCollection = new ObservableCollection<SpaceZoneNode>(_SpaceDictionary[_CurrentLevel.Id.IntegerValue]);

            //区域字典
            _ZoneDictionary = new Dictionary<int, List<ZoneSpacesNode>>();
            foreach (Level level in _Levels)
            {
                _ZoneDictionary.Add(level.Id.IntegerValue, new List<ZoneSpacesNode>());
            }
            UpdateZoneDictionary();
            ZoneCollection = new ObservableCollection<ZoneSpacesNode>(_ZoneDictionary[_CurrentLevel.Id.IntegerValue]);

        }

        public void UpdateZoneDictionary()
        {
            _ZoneDictionary[_CurrentLevel.Id.IntegerValue].Clear();
            FilteredElementCollector zones = new FilteredElementCollector(_ActiveDocument).OfClass(typeof(Zone));
            foreach (Zone zone in zones)
            {
                if (zone != null && _ActiveDocument.GetElement(zone.LevelId) != null)
                {
                    ZoneSpacesNode zoneSpacesNode = new ZoneSpacesNode(zone);
                    _ZoneDictionary[zone.LevelId.IntegerValue].Add(zoneSpacesNode);
                }
            }
        }

        public void UpdateSpaceDictionary()
        {
            _SpaceDictionary[_CurrentLevel.Id.IntegerValue].Clear();
            FilteredElementCollector spaces = new FilteredElementCollector(_ActiveDocument).WherePasses(new SpaceFilter());
            foreach (Space space in spaces)
            {
                SpaceZoneNode spaceZoneNode = new SpaceZoneNode(space);
                _SpaceDictionary[space.LevelId.IntegerValue].Add(spaceZoneNode);
            }
        }

        public void CreateSpaces()
        {
            Document activeDocument = CommandData.Application.ActiveUIDocument.Document;

            if (_DefaultPhase == null)
            {
                TaskDialog.Show("Revit","The phase of active view is null, you can't create spaces in null phase");
                return;
            }

            try
            {
                if (activeDocument.ActiveView.ViewType == ViewType.FloorPlan)
                {
                    //尝试一下其他的创建方法
                    ICollection<ElementId> newSpaceIDs = activeDocument.Create.NewSpaces2(
                        _CurrentLevel, _DefaultPhase, activeDocument.ActiveView);

                    if (newSpaceIDs == null || newSpaceIDs.Count == 0)
                    {
                        TaskDialog.Show("Revit", "There is no enclosed loop in " + _CurrentLevel.Name);
                        return;
                    }

                    //将创建出来的新空间添加到空间字典中
                    foreach (var newspaceID in newSpaceIDs)
                    {
                        Space newSpace = activeDocument.GetElement(newspaceID) as Space;
                        SpaceZoneNode spaceZoneNode = new SpaceZoneNode(newSpace);
                        _SpaceDictionary[_CurrentLevel.Id.IntegerValue].Add(spaceZoneNode);
                        SpaceCollection.Add(spaceZoneNode);
                    }
                }
                else
                {
                    TaskDialog.Show("Revit", "You can't create spaces in this plan view");
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Revit", ex.Message);
            }
        }

        public void CreateZone()
        {
            Document activeDocument = CommandData.Application.ActiveUIDocument.Document;

            if (_DefaultPhase == null)
            {
                TaskDialog.Show("Revit", "The phase of active view is null, you can't create zone in null phase");
                return;
            }

            try
            {
                Zone newZone = activeDocument.Create.NewZone(_CurrentLevel, _DefaultPhase);
                if (newZone != null)
                {
                    ZoneSpacesNode zoneSpacesNode = new ZoneSpacesNode(newZone);
                    _ZoneDictionary[_CurrentLevel.Id.IntegerValue].Add(zoneSpacesNode);
                    ZoneCollection.Add(zoneSpacesNode);
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Revit", ex.Message);
            }
        }

        public void UpdateObservableCollections()
        {
            SpaceCollection.Clear();
            foreach (SpaceZoneNode item in _SpaceDictionary[_CurrentLevel.Id.IntegerValue])
            {
                SpaceCollection.Add(item);
            }

            ZoneCollection.Clear();
            foreach (ZoneSpacesNode item in _ZoneDictionary[_CurrentLevel.Id.IntegerValue])
            {
                ZoneCollection.Add(item);
            }
        }

        public void UpdateAvailableSpaceCollection(ZoneSpacesNode currentZoneSpacesNode)
        {
            AvailableSpaceCollection.Clear();
            foreach (SpaceZoneNode item in _SpaceDictionary[_CurrentLevel.Id.IntegerValue])
            {
                if (item.ZoneName != currentZoneSpacesNode.ZoneName)
                {
                    AvailableSpaceCollection.Add(item);
                }
            }
        }

        public void UpdateCurrentZoneSpaceCollection(ZoneSpacesNode currentZoneSpacesNode)
        {
            CurrentZoneSpaceCollection.Clear();
            foreach (Space item in currentZoneSpacesNode.SpaceList)
            {
                CurrentZoneSpaceCollection.Add(new SpaceZoneNode(item));
            }
        }

        public void SelectedZoneAddSpaces(IList selectedSpaces, ZoneSpacesNode selectedZone)
        {
            //List<SpaceZoneNode> selectedSpaceList = new List<SpaceZoneNode>();
            SpaceSet spaceSet = new SpaceSet();

            //foreach (SpaceZoneNode item in selectedSpaces)
            //{
            //    selectedSpaceList.Add(item);
            //    _ = spaceSet.Insert(item.Space);
            //}
            //foreach (SpaceZoneNode item in selectedSpaceList)
            //{
            //    _ = AvailableSpaceCollection.Remove(item);
            //    CurrentZoneSpaceCollection.Add(item);
            //}

            for (int i = 0; i < selectedSpaces.Count; i++)
            {
                SpaceZoneNode spaceZoneNode = selectedSpaces[i] as SpaceZoneNode;
                if (spaceZoneNode != null)
                {
                    spaceSet.Insert(spaceZoneNode.Space);
                    AvailableSpaceCollection.Remove(spaceZoneNode);
                    CurrentZoneSpaceCollection.Add(spaceZoneNode);
                }
            }

            _ = selectedZone.Zone.AddSpaces(spaceSet);
        }

        internal void SelectedZoneRemoveSpaces(IList selectedSpaces, ZoneSpacesNode selectedZone)
        {
            List<SpaceZoneNode> selectedSpaceList = new List<SpaceZoneNode>();
            SpaceSet spaceSet = new SpaceSet();

            foreach (SpaceZoneNode item in selectedSpaces)
            {
                selectedSpaceList.Add(item);
                _ = spaceSet.Insert(item.Space);
            }
            foreach (SpaceZoneNode item in selectedSpaceList)
            {
                _ = CurrentZoneSpaceCollection.Remove(item);
                AvailableSpaceCollection.Add(item);
            }

            _ = selectedZone.Zone.RemoveSpaces(spaceSet);
        }
    }
}