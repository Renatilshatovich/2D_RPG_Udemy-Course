using UnityEngine;

public class CharacterStats : MonoBehaviour
{
     [Header("Major stats")]
     public Stat strength;    // point increase damage by 1 and crit. power by 1%
     public Stat agility;     // 1 point increase evasion by 1% and crit. chance by 1%
     public Stat intelligence;    // 1 point increase magic damage by 1 and magic resistance by 3
     public Stat vitality;    // 1 point increase health by 3 or points
     
     [Header("Offensive stats")]
     public Stat damage;
     public Stat critChance;
     public Stat critPower;
     
     [Header("Defensive stats")]
     public Stat maxHealth;
     public Stat armor;
     public Stat evasion;
     public Stat magicResistance;

     [Header("Magic stats")] 
     public Stat fireDamage;
     public Stat iceDamage;
     public Stat lightingDamage;

     public bool isIgnited;
     public bool isChilled;
     public bool isShocked;
     
     [SerializeField] private int currentHealth;

     protected virtual void Start()
     {
          critPower.SetDefaultValue(150);
          currentHealth = maxHealth.GetValue();
     }

     public virtual void DoDamage(CharacterStats _targetStats)
     {
          if (TargetCanAvoidAttack(_targetStats))
               return;

          int totalDamage = damage.GetValue() + strength.GetValue();

          if (CanCrit())
          {
               totalDamage = CalculatorCriticalDamage(totalDamage);
          }
          
          totalDamage = CheckTargetArmor(_targetStats, totalDamage);
          // _targetStats.TakeDamage(totalDamage);
          DoMagicalDamage(_targetStats);
     }

     public virtual void DoMagicalDamage(CharacterStats _targetStats)
     {
          int _fireDamage = fireDamage.GetValue();
          int _iceDamage = iceDamage.GetValue();
          int _lightingDamage = lightingDamage.GetValue();

          int totalMagicalDamage = _fireDamage + _iceDamage + _lightingDamage + intelligence.GetValue();
          
          totalMagicalDamage = CheckTargetResistance(_targetStats, totalMagicalDamage);
          _targetStats.TakeDamage(totalMagicalDamage);

          if (Mathf.Max(_fireDamage, _iceDamage, _lightingDamage) <= 0)
               return;
          
          bool canApplyIgnite = _fireDamage > _iceDamage && _fireDamage > _lightingDamage;
          bool canApplyChill = _iceDamage > _fireDamage && _iceDamage > _lightingDamage;
          bool canApplyShock = _lightingDamage > _fireDamage && _lightingDamage > _iceDamage;

          while (!canApplyIgnite && !canApplyChill && !canApplyShock)
          {
               if (Random.value < .3f && _fireDamage > 0)
               {
                    canApplyIgnite = true;
                    _targetStats.ApplyAilments(canApplyIgnite, canApplyChill, canApplyShock);
                    Debug.Log("Applied fire");
                    return;
               }
               if (Random.value < .5f && _iceDamage > 0)
               {
                    canApplyChill = true;
                    _targetStats.ApplyAilments(canApplyIgnite, canApplyChill, canApplyShock);
                    Debug.Log("Applied ice");
                    return;
               }
               if (Random.value < .5f && _lightingDamage > 0)
               {
                    canApplyShock = true;
                    Debug.Log("Applied lighting");
                    _targetStats.ApplyAilments(canApplyIgnite, canApplyChill, canApplyShock);
                    return;
               }
          }
          
          _targetStats.ApplyAilments(canApplyIgnite, canApplyChill, canApplyShock);
     }

     private int CheckTargetResistance(CharacterStats _targetStats, int totalMagicalDamage)
     {
          totalMagicalDamage -= _targetStats.magicResistance.GetValue() + (_targetStats.intelligence.GetValue() * 3);
          totalMagicalDamage = Mathf.Clamp(totalMagicalDamage, 0, int.MaxValue);
          return totalMagicalDamage;
     }

     public void ApplyAilments(bool _ignite, bool _chill, bool _shock)
     {
          if (isIgnited || isChilled || isShocked)
               return;

          isIgnited = _ignite;
          isChilled = _chill;
          isShocked = _shock;
     }
     
     public virtual void TakeDamage(int _damage)
     {
          currentHealth -= _damage;
          Debug.Log(_damage);
          if (currentHealth < 0 )
               Die();
     }
     private int CheckTargetArmor(CharacterStats _targetStats, int totalDamage)
     {
          totalDamage -= _targetStats.armor.GetValue();
          totalDamage = Mathf.Clamp(totalDamage, 0, int.MaxValue);
          return totalDamage;
     }

     protected virtual void Die()
     {
          // throw new NotImplementedException();
     }
     private bool TargetCanAvoidAttack(CharacterStats _targetStats)
     {
          int totalEvasion = _targetStats.evasion.GetValue() + _targetStats.agility.GetValue();

          if (Random.Range(0,100 ) < totalEvasion)
          {
               return true;
          }

          return false;
     }

     private bool CanCrit()
     {
          int totalCriticalChance = critChance.GetValue() + agility.GetValue();

          if (Random.Range(0, 100) <=  totalCriticalChance)
          {
               return true;
          }

          return false;
     }

     private int CalculatorCriticalDamage(int _damage)
     {
          float totalCritPower = (critPower.GetValue() + strength.GetValue()) * .01f;
          float critDamage = _damage * totalCritPower;
          
          return Mathf.RoundToInt(critDamage);
     }
}
