using System;
using System.Collections;
using System.Collections.Generic;
using Buildings.BuildingArea;
using Buildings.Houses;
using Population;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

public class PopulationManager : Singleton<PopulationManager>
{
    public int Population { get; private set; }
    public int NeededResidents { get; set; } = 100;
    public int CommerceJobs { get; private set; }
    public int NeededCommerceJobs { get; set; }
    public int IndustrialJobs { get; private set; }
    public int NeededIndustrialJobs { get; set; }
    public int UnemployedPeople => _unemployed.Count;

    private readonly Queue<ResidentialBuilding> _residentialBuildingsWithSpace = new Queue<ResidentialBuilding>();
    private readonly Queue<AreaField> _freeResidentialFields = new Queue<AreaField>();

    private readonly Queue<IndustrialBuilding> _industrialBuildingsWithWorkSpace = new Queue<IndustrialBuilding>();
    private readonly Queue<AreaField> _freeIndustrialFields = new Queue<AreaField>();
    
    private readonly Queue<Building> _commercialBuildingsForCustomers = new Queue<Building>();
    private readonly Queue<CommercialBuilding> _commercialBuildingsWithWorkSpace = new Queue<CommercialBuilding>();
    private readonly Queue<AreaField> _freeCommercialFields = new Queue<AreaField>();

    private readonly Queue<Human> _unemployed = new Queue<Human>();

    private readonly Queue<Human> _idleHumans = new Queue<Human>();

    public Action OnPopulationChanged { get; set; }
    
    private void Start()
    {
        TimeManager.Instance?.Register(TimeUpdate);
    }

    private void OnDisable()
    {
        TimeManager.Instance?.Unregister(TimeUpdate);
    }

    private void TimeUpdate()
    {
        if (NeededResidents > 0)
        {
            HandleResidentialBuildings();
        }
        if (NeededIndustrialJobs > 0 && _unemployed.Count > 0)
        {
            HandleIndustrialBuildings();
        }
        if (NeededCommerceJobs > 0 && _unemployed.Count > 0)
        {
            HandleCommercialBuildings();
        }

        bool success;
        do
        {
            if (_idleHumans.Count > 0 && Random.value > 0.96f)
            {
                Human human = _idleHumans.Dequeue();
                human.DriveShopping();
                success = true;
            }
            else success = false;
        } while (success);
    }

    private void HandleResidentialBuildings()
    {
        ResidentialBuilding building = null;
        if (_residentialBuildingsWithSpace.Count > 0)
        {
            building = _residentialBuildingsWithSpace.Dequeue();
        }
        else if (_freeResidentialFields.Count > 0)
        {
            if (Random.value > 0.95f)
            {
                building = (ResidentialBuilding) _freeResidentialFields.Dequeue().BuildHouse();
            }
            else return;
        }
        else return;

        Human human = HumanGenerator.GenerateHuman();
        building.MoveIn(human);
        Population++;
        if (building.HasSpace) _residentialBuildingsWithSpace.Enqueue(building);

        AddUnemployed(human);
        
        OnPopulationChanged?.Invoke();
    }

    private void HandleIndustrialBuildings()
    {
        IndustrialBuilding building = null;
        if (_industrialBuildingsWithWorkSpace.Count > 0)
        {
            building = _industrialBuildingsWithWorkSpace.Dequeue();
        }
        else if (_freeIndustrialFields.Count > 0)
        {
            if (Random.value > 0.95f)
            {
                building = (IndustrialBuilding) _freeIndustrialFields.Dequeue().BuildHouse();
            }
            else return;
        }
        else return;
            
        building.Employ(RemoveUnemployed());
        if (building.HasWork) _industrialBuildingsWithWorkSpace.Enqueue(building);
        IndustrialJobs++;
        NeededResidents++;
        NeededIndustrialJobs--;
        
        OnPopulationChanged?.Invoke();
    }
    
    private void HandleCommercialBuildings()
    {
        CommercialBuilding building = null;
        if (_commercialBuildingsWithWorkSpace.Count > 0)
        {
            building = _commercialBuildingsWithWorkSpace.Dequeue();
        }
        else if (_freeCommercialFields.Count > 0)
        {
            if (Random.value > 0.95f)
            {
                building = (CommercialBuilding) _freeCommercialFields.Dequeue().BuildHouse();
            }
            else return;
        }
        else return;
            
        building.Employ(RemoveUnemployed());
        if (building.HasWork) _commercialBuildingsWithWorkSpace.Enqueue(building);
        CommerceJobs++;
        NeededResidents++;
        NeededCommerceJobs--;
        
        OnPopulationChanged?.Invoke();
    }

    public void AddIdleHuman(Human human)
    {
        _idleHumans.Enqueue(human);
    }

    public void AddUnemployed(Human human)
    {
        human.Workplace = null;
        _unemployed.Enqueue(human);
    }

    public Human RemoveUnemployed()
    {
        return _unemployed.Dequeue();
    }

    public void AddCommercialBuildingForCustomers(Building building)
    {
        _commercialBuildingsForCustomers.Enqueue(building);
    }

    public Building GetRandomCommercialBuilding()
    {
        if (_commercialBuildingsForCustomers.Count == 0) return null;
        Building building = _commercialBuildingsForCustomers.Dequeue();
        _commercialBuildingsForCustomers.Enqueue(building);
        return building;
    }

    public void AddAreaField(AreaField field)
    {
        switch (field.Type)
        {
            case AreaType.None:
                break;
            case AreaType.Residential:
                _freeResidentialFields.Enqueue(field);
                break;
            case AreaType.Commercial:
                _freeCommercialFields.Enqueue(field);
                break;
            case AreaType.Industrial:
                _freeIndustrialFields.Enqueue(field);
                break;
        }
    }
}
