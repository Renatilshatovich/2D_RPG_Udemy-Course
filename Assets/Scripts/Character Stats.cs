using UnityEngine;

public class CharacterStats : MonoBehaviour
{
     private EntityFX fx;
     
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

     [SerializeField] private float ailmentsDuration = 4;
     private float ignitedTimer;
     private float chilledTimer;
     private float shockedTiimer;
     
     private float igniteDamageCooldown = .3f;
     private float igniteDamageTimer;
     private int igniteDamage;
     
     public int currentHealth;

     public System.Action onHealthChanged;

     protected virtual void Start()
     {
          critPower.SetDefaultValue(150);
          currentHealth = GetMaxHealthValue();

          fx = GetComponent<EntityFX>();
     }

     protected virtual void Update()
     {
          ignitedTimer -= Time.deltaTime;
          chilledTimer -= Time.deltaTime;
          shockedTiimer -= Time.deltaTime;
          
          igniteDamageTimer -= Time.deltaTime;

          if (ignitedTimer < 0)
               isIgnited = false;

          if (chilledTimer < 0)
               isChilled = false;
          
          if (shockedTiimer < 0)
               isShocked = false;
          
          if (igniteDamageTimer < 0 && isIgnited)
          {
               Debug.Log("Take burn Damage " + igniteDamage);

               DecreaseHealthBy(igniteDamage);
               
               if (currentHealth < 0)
                    Die();
               
               igniteDamageTimer = igniteDamageCooldown;
          }
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
          
          if (canApplyIgnite)
               _targetStats.SetupIgniteDamage(Mathf.RoundToInt(_fireDamage * .2f));
          
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

          if (_ignite)
          {
               isIgnited = _ignite;
               ignitedTimer = ailmentsDuration;
               
               fx.IgniteFxFor(ailmentsDuration);
          }
          
          if (_chill)
          {
               isChilled = _chill;
               chilledTimer = ailmentsDuration;

               float slowPercentage = .2f;

               GetComponent<Entity>().SlowEntityBy(slowPercentage, ailmentsDuration);
               fx.ChillFxFor(ailmentsDuration);
          }          
          
          if (_shock)
          {
               isShocked = _shock;
               shockedTiimer = ailmentsDuration;
               
               fx.ShockFxFor(ailmentsDuration);
          }
     }

     public void SetupIgniteDamage(int _damage) => igniteDamage = _damage;
     
     public virtual void TakeDamage(int _damage)
     {
          DecreaseHealthBy(_damage);
          
          Debug.Log(_damage);
          
          if (currentHealth < 0 )
               Die();
     }
     private int CheckTargetArmor(CharacterStats _targetStats, int totalDamage)
     {
          if (_targetStats.isChilled)
               totalDamage -= Mathf.RoundToInt(_targetStats.armor.GetValue() * .8f);
          else
               totalDamage -= _targetStats.armor.GetValue();
               
          totalDamage = Mathf.Clamp(totalDamage, 0, int.MaxValue);
          return totalDamage;
     }

     protected virtual void DecreaseHealthBy(int _damage)
     {
          currentHealth -= _damage;

          if (onHealthChanged != null)
               onHealthChanged();
     }
     
     protected virtual void Die()
     {
          // throw new NotImplementedException();
     }
     private bool TargetCanAvoidAttack(CharacterStats _targetStats)
     {
          int totalEvasion = _targetStats.evasion.GetValue() + _targetStats.agility.GetValue();

          if (isShocked)
               totalEvasion += 20;
          
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

     public int GetMaxHealthValue()
     {
          return maxHealth.GetValue() + vitality.GetValue() * 5;
     }
}
