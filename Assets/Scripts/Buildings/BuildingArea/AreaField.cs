using System.Collections;
using UnityEngine;

namespace Buildings.BuildingArea
{
    public enum AreaType
    {
        None, Residential, Commercial, Industrial
    }
    
    public class AreaField
    {
        public int Index { get; }
        private readonly Vector3 _position;
        public BuildingArea Parent { get; }
        private Building _building;

        private AreaType _type;
        public AreaType Type
        {
            get => _type;
            set
            {
                if (_type == value) return;

                if (_building != null)
                {
                    //TODO muss unter anderem dem Populationmanager mitgeteilt werden
                    Object.Destroy(_building.gameObject);
                    _building = null;
                }

                _type = value;
                PopulationManager.Instance.AddAreaField(this);
            } 
        }

        public AreaField(Vector3 position, int index, BuildingArea parent)
        {
            _position = position;
            Index = index;
            Parent = parent;
        }

        public Building BuildHouse()
        {
            GameObject go;
            switch (Type)
            {
                case AreaType.Residential:
                    go = Object.Instantiate(GameManager.Instance.residentialPrefab, Parent.transform);
                    break;
                case AreaType.Industrial:
                    go = Object.Instantiate(GameManager.Instance.industrialPrefab, Parent.transform);
                    break;
                case AreaType.Commercial:
                    go = Object.Instantiate(GameManager.Instance.commercialPrefab, Parent.transform);
                    break;
                default:
                    return null;
            }
            
            go.transform.localPosition = _position;
            go.transform.localRotation = Parent.IsRightSide ? Quaternion.AngleAxis(-90f, Vector3.up) : Quaternion.AngleAxis(90f, Vector3.up);
            _building = go.GetComponent<Building>();
            _building.Field = this;
            return _building;
        }
    }
}