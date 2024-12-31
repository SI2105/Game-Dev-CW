using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skills
{
    // Start is called before the first frame update

    public enum SkillType { 
        PowerUpAction,
        WeaponDropBoost,
        ChestBumpAction,
        ConsumableCarry2,
        ConsumableCarry3,
        ConsumableCarry5,
    }

    private List<SkillType> unlockedSkillTypeList;
    public Skills()
    {
        unlockedSkillTypeList = new List<SkillType>();
    }


    public void UnlockSkill(SkillType skillType)
    {

        if (!IsSkillUnlocked(skillType)) {
            unlockedSkillTypeList.Add(skillType);
        }
        

    }
    
    public bool IsSkillUnlocked(SkillType skillType)
    {
        
        return unlockedSkillTypeList.Contains(skillType);
    }
}
