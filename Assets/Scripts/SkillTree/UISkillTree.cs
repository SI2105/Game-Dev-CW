using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISkillTree : MonoBehaviour
{
    private Skills playerSkills;
    private void Awake()
    {
       
        transform.Find("SkillBtn").GetComponent<Button>().onClick.AddListener(() => {
           
            playerSkills.UnlockSkill(Skills.SkillType.PowerUpAction);
        });
    }

    public void SetPlayerSkills(Skills playerSkills) {
        
        this.playerSkills = playerSkills;
    }
    
}
