using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopulationUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI populationText;
    //[SerializeField] private TextMeshProUGUI neededResidentsText;
    [SerializeField] private TextMeshProUGUI commerceJobsText;
    //[SerializeField] private TextMeshProUGUI neededCommerceJobsText;
    [SerializeField] private TextMeshProUGUI industrialJobsText;
    //[SerializeField] private TextMeshProUGUI neededIndustrialJobsText;
    [SerializeField] private TextMeshProUGUI unemployedText;
    
    
    private void Start()
    {
        PopulationManager.Instance.OnPopulationChanged += UpdateUi;
    }

    private void OnDisable()
    {
        PopulationManager.Instance.OnPopulationChanged -= UpdateUi;
    }

    private void UpdateUi()
    {
        populationText.text = "Population: " + PopulationManager.Instance.Population + " (" + PopulationManager.Instance.NeededResidents + " needed)";
        commerceJobsText.text = "Commerce jobs: " + PopulationManager.Instance.CommerceJobs + " (" + PopulationManager.Instance.NeededCommerceJobs + " needed)";
        industrialJobsText.text = "Industrial jobs: " + PopulationManager.Instance.IndustrialJobs + " (" + PopulationManager.Instance.NeededIndustrialJobs + " needed)";
        unemployedText.text = "Unemployed: " + PopulationManager.Instance.UnemployedPeople;
    }
}
