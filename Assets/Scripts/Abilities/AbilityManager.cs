using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AbilityManager : MonoBehaviour
{
    [SerializeField]
    GameObject Player;

    [SerializeField]
    private GameObject inactive1, inactive2, inactive3, inactive4, inactive5, active1, active2, active3, active4, active5, background1, background2, background3, background4, background5, counter1;

    private GameObject[] inactiveArr;

    private int counter = 3;

    IAbility ActiveAbility;

    Dictionary<IAbility, bool> unlockedAbilities = new Dictionary<IAbility, bool>();

    public bool isActiveAbility(IAbility ability)
    {
        return ActiveAbility.Equals(ability);
    }

    public void UnlockAbility(IAbility ability)
    {
        unlockedAbilities.Remove(ability);
        unlockedAbilities.Add(ability, true);        

        switch(ability.getAbilityName())
        {
            case "PlatformSpawn":
                inactive1.SetActive(true);
                background1.SetActive(false);                
                break;
            case "Telekinesis":
                inactive2.SetActive(true);
                background2.SetActive(false);
                break;
            case "Punch":
                inactive3.SetActive(true);
                background3.SetActive(false);
                break;
            case "Stasis":
                inactive4.SetActive(true);
                background4.SetActive(false);
                break;
            case "Bombthrow":
                inactive5.SetActive(true);
                background5.SetActive(false);
                break;
            default:
                break;
        }        
    }

    void Start()
    {
        ActiveAbility = Player.GetComponent<Punch>();

        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.name == "Level_2")
        {
            unlockedAbilities.Add(Player.GetComponent<Punch>(), false);
            unlockedAbilities.Add(Player.GetComponent<Bombthrow>(), false);
            unlockedAbilities.Add(Player.GetComponent<PlatformSpawn>(), true);
            unlockedAbilities.Add(Player.GetComponent<Telekinesis>(), true);
            unlockedAbilities.Add(Player.GetComponent<Stasis>(), false);

            inactive1.SetActive(true);
            background1.SetActive(false);

            inactive2.SetActive(true);
            background2.SetActive(false);


        } else if (currentScene.name == "Level_3")
        {
            unlockedAbilities.Add(Player.GetComponent<Punch>(), true);
            unlockedAbilities.Add(Player.GetComponent<Bombthrow>(), false);
            unlockedAbilities.Add(Player.GetComponent<PlatformSpawn>(), true);
            unlockedAbilities.Add(Player.GetComponent<Telekinesis>(), true);
            unlockedAbilities.Add(Player.GetComponent<Stasis>(), false);

            inactive1.SetActive(true);
            background1.SetActive(false);

            inactive2.SetActive(true);
            background2.SetActive(false);

            inactive3.SetActive(true);
            background3.SetActive(false);
        } else
        {
            unlockedAbilities.Add(Player.GetComponent<Punch>(), false);
            unlockedAbilities.Add(Player.GetComponent<Bombthrow>(), false);
            unlockedAbilities.Add(Player.GetComponent<PlatformSpawn>(), false);
            unlockedAbilities.Add(Player.GetComponent<Telekinesis>(), false);
            unlockedAbilities.Add(Player.GetComponent<Stasis>(), false);
        }

        inactiveArr = new GameObject[] { inactive1, inactive2, inactive3, inactive4, inactive5 };
        Debug.Log("arrygroesse" + inactiveArr.Length);
    }

    void Update()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        
        QueryChosenAbility();
        QueryAction();
    }

    // this function queries for changes of the chosen ability.
    private void QueryChosenAbility()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ActiveAbility = Player.GetComponent<Punch>();
            inactive3.SetActive(false);
            active3.SetActive(true);
            SwapAbility(2);            
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            bool outBool = false;
            unlockedAbilities.TryGetValue(Player.GetComponent<PlatformSpawn>(), out outBool);
            if (outBool)
            {
                ActiveAbility = Player.GetComponent<PlatformSpawn>();
            }
            inactive1.SetActive(false);
            active1.SetActive(true);
            counter1.SetActive(true);
            SwapAbility(0);            
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            ActiveAbility = Player.GetComponent<Bombthrow>();
            inactive5.SetActive(false);
            active5.SetActive(true);
            SwapAbility(4);            
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ActiveAbility = Player.GetComponent<Stasis>();
            inactive4.SetActive(false);
            active4.SetActive(true);
            SwapAbility(3);            
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ActiveAbility = Player.GetComponent<Telekinesis>();
            inactive2.SetActive(false);
            active2.SetActive(true);
            SwapAbility(1);
        }
    }

    private void SwapAbility(int index)
    {
        for(int i = 0; i < inactiveArr.Length; i++)
        {
            if(!(index == i)) //i Fähigkeit ist aktiv
            {
                inactiveArr[i].SetActive(true);
            }
        }
        if(index != 0)
        {
            counter1.SetActive(false);
        }
    }

    private void QueryAction()
    {
        bool activeAbilityIsUnlocked;
        unlockedAbilities.TryGetValue(ActiveAbility, out activeAbilityIsUnlocked);

        if (Input.GetKey(KeyCode.Mouse0) && activeAbilityIsUnlocked)
        {
            ActiveAbility.AccumulateAbility();
        }
        if (Input.GetKeyDown(KeyCode.Mouse0) && activeAbilityIsUnlocked)
        {
            ActiveAbility.ExecuteAbility();
        }
        if (Input.GetKeyUp(KeyCode.Mouse0) && activeAbilityIsUnlocked)
        {
            ActiveAbility.ReleaseAbility();            
        }
    }
}