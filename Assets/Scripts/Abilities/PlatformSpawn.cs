using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlatformSpawn : MonoBehaviour, IAbility
{
    [SerializeField]
    private Transform PlatformPrefab;

    [SerializeField]
    private Transform PlatformPreviewNormalPrefab;

    [SerializeField]
    private Transform PlatformPreviewRedPrefab;

    [SerializeField]
    private const float CooldownTime = 2.0f;

    private float LastTimePlatformSpawned = -CooldownTime;

    private Queue<GameObject> livePlatforms = new Queue<GameObject>(3);

    private float Range = 4.0f;

    [SerializeField]
    private GameObject counter1;

    private GameObject currentPreview;
    private GameObject normalPreview;
    private GameObject redPreview;

    private int counter = 3;

    private void Start()
    {
        normalPreview = Instantiate(PlatformPreviewNormalPrefab).gameObject;
        redPreview = Instantiate(PlatformPreviewRedPrefab).gameObject;

        normalPreview.SetActive(false);
        redPreview.SetActive(false);
    }

    private void Update()
    {
        AbilityManager abilityManager = GetComponent<AbilityManager>();
        if (abilityManager.isActiveAbility(this))
        {
            Vector2 MousePosition = DetermineMousePosition();
            Vector2 PlayerPosition = new Vector2(transform.position.x, transform.position.y);

            if ((PlayerPosition - MousePosition).magnitude <= Range
               && counter >= 1)
            {
                normalPreview.SetActive(true);
                redPreview.SetActive(false);
                currentPreview = normalPreview;
            }
            else
            {
                normalPreview.SetActive(false);
                redPreview.SetActive(true);
                currentPreview = redPreview;
            }

            currentPreview.transform.position = MousePosition;
        }
        else
        {
            normalPreview.SetActive(false);
            redPreview.SetActive(false);
        }
    }

    public void ExecuteAbility()
    {
        SpawnPlatform();
    }

    public void AccumulateAbility()
    {
        // Don't do anything, PlatformSpawn is intended to only be executed once.
    }

    public void ReleaseAbility()
    {
        // Don't do anything, PlatformSpawn is intended to only be executed once.
    }

    public void resetCounter()
    {
        counter = 3;
        counter1.GetComponentInChildren<Text>().text = "" + counter;
    }

    void SpawnPlatform()
    {
        LastTimePlatformSpawned = Time.time;
        Vector2 MousePosition = DetermineMousePosition();
        Vector2 PlayerPosition = new Vector2(transform.position.x, transform.position.y);

        if((PlayerPosition - MousePosition).magnitude <= Range
            && counter >= 1)
        {
            if(livePlatforms.Count == 3)
            {
                Destroy(livePlatforms.Dequeue());
            }
            GameObject platform = Instantiate(PlatformPrefab.gameObject, MousePosition, Quaternion.identity);
            livePlatforms.Enqueue(platform);
            counter1.GetComponentInChildren<Text>().text = "" + --counter;
            StartCoroutine("PlatformDestruction", platform);
        }
    }

    public string getAbilityName()
    {
        return "PlatformSpawn";
    }

    private Vector2 DetermineMousePosition()
    {
        Vector3 Worldpoint = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        return new Vector2(Worldpoint.x, Worldpoint.y);
    }

    private IEnumerator PlatformDestruction(GameObject platform)
    {
        yield return new WaitForSeconds(5);

        if(platform)
        {
            if(livePlatforms.Contains(platform))
            {
                livePlatforms.Dequeue();
            }
            Destroy(platform);
        }
    }
}
