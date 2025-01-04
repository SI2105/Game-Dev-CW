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
        //Check if the skill can be unlocked, and Unlocks it
        if (skillTreeManager.CanUnlock(this))
        {
            //Triggers completion of First Skill Objective
            ObjectiveManager.Instance.SetEventComplete("Get your First Skill");
            isUnlocked = true;
            skillTreeManager.SpendSkillPoints(cost);

            //Trigger the even when the skill is unlocked, used on the player end to make it take effect

            OnSkillUnlocked?.Invoke(skillName);
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        //Disables the button if the skill can't be unlocked and if theres prerequisites that are not unlocked.
        skillButton.interactable = skillTreeManager.CanUnlock(this) && !isUnlocked;
        skillIcon.color = isUnlocked ? Color.green : Color.gray;
    }
}
