using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Skill : MonoBehaviour
{
    public enum SkillName { 
        Carry2,
        Carry3,
        Carry5,
        RareItemDrop,
        PowerUpAction,
        ChestBumpAction,
        TwoHandClubComboAction,
        TwoHandSwordComboAction,
    }
    public SkillName skillName;
    public int cost;
    public bool isUnlocked = false;
    public Skill[] prerequisites; // Other Skill GameObjects
    public Button skillButton; 
    public Image skillIcon;

    [SerializeField]private SkillTreeManager skillTreeManager;

    //Event For SkillUnlock
    public static event Action<SkillName> OnSkillUnlocked;

    void Start()
    {
        skillButton = transform.GetComponent<Button>(); 
        skillButton.onClick.AddListener(UnlockSkill);
        skillIcon = transform.GetChild(0).GetComponent<Image>();
        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    public void UnlockSkill()
    {
        if (skillTreeManager.CanUnlock(this))
        {
            
            isUnlocked = true;
            skillTreeManager.SpendSkillPoints(cost);

            //Trigger the even when the skill is unlocked

            OnSkillUnlocked?.Invoke(skillName);
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        
        skillButton.interactable = skillTreeManager.CanUnlock(this) && !isUnlocked;
        skillIcon.color = isUnlocked ? Color.green : Color.gray;
    }
}
