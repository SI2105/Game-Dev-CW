using System.Collections;
using System.Collections.Generic;
using SG;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeManager : MonoBehaviour
{
    public int availableSkillPoints = 5;
    public SkillTreeManager instance;
    [SerializeField] private Skill[] skills;
    [SerializeField]private GameObject SkillPointsHolder;
    [SerializeField] private GameObject SkillsPanel;
    private bool isSkillTreeActive = false;

    private void Start()
    {
        instance = this;
        SkillsPanel.SetActive(false);
        SetSkillPointText(availableSkillPoints);    
    }
    public bool CanUnlock(Skill skill)
    {
        
        if (skill.isUnlocked) return false;
        if (skill.cost > availableSkillPoints) return false;

        foreach (Skill prereq in skill.prerequisites)
        {
            if (!prereq.isUnlocked) return false;
        }
        return true;
    }

    public void SpendSkillPoints(int cost)
    {
        availableSkillPoints -= cost;
        SetSkillPointText(availableSkillPoints);
       
    }

    public bool IsSkillUnlocked(Skill.SkillName skillName)
    {

        foreach (Skill skill in skills)
        {
            if (skill.skillName == skillName)
            {
                return skill.isUnlocked;
            }
        }
        return false;
    }

    public void AddSkillPoints(int PointsToAdd) {
        availableSkillPoints += PointsToAdd;
        SetSkillPointText(availableSkillPoints);
    }


    public void SetSkillPointText(int number) {
        SkillPointsHolder.GetComponent<TMPro.TextMeshProUGUI>().text = number.ToString();

    }

    public void ToggleSkillTree() {
        isSkillTreeActive = !isSkillTreeActive;
        SkillsPanel.SetActive(isSkillTreeActive);
    }
}
