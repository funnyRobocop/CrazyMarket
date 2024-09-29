using UnityEngine;
using System.Collections;

public class Blender : MonoBehaviour
{
    public Slot blenderSlot;
    [SerializeField] private Transform itemSpawnPoint;
    public GameObject blendButton;
    public GameObject appleJuicePrefab;
    public GameObject mangoJuicePrefab;
    public GameObject TomatoJuicePrefab;
    public GameObject PeachJuicePrefab;
    public GameObject strawberryJuicePrefab;
    public GameObject kiwiJuicePrefab;
    public GameObject papayaJuicePrefab;
    public GameObject pomegranateJuicePrefab;
    public GameObject carrotJuicePrefab;
    [SerializeField] private Animator blenderAnimator;
    [SerializeField] private AudioClip NOblendSound;
    [SerializeField] private AudioClip blendFruitSound;
    [SerializeField] private float blendDuration = 1.5f;
    public GameObject NOblend;

    private bool isBlending = false;

    private void Start()
    {
        if (blenderAnimator == null)
        {
            blenderAnimator = GetComponent<Animator>();
        }

        // Инициализируем начальное состояние кнопки
        UpdateBlendButtonState();
    }

    private void OnEnable()
    {
        // Подписываемся на события слота
        if (blenderSlot != null)
        {
            blenderSlot.OnSlotOccupied += UpdateBlendButtonState;
            blenderSlot.OnSlotCleared += UpdateBlendButtonState;
        }
    }

    private void OnDisable()
    {
        // Отписываемся от событий слота
        if (blenderSlot != null)
        {
            blenderSlot.OnSlotOccupied -= UpdateBlendButtonState;
            blenderSlot.OnSlotCleared -= UpdateBlendButtonState;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isBlending && blenderSlot != null && blenderSlot.IsOccupied)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            if (hit.collider != null && hit.collider.gameObject == blendButton)
            {
                StartCoroutine(BlendFruitCoroutine());
            }
        }
    }

    private void UpdateBlendButtonState()
    {
        if (blendButton != null && blenderSlot != null)
        {
            blendButton.SetActive(blenderSlot.IsOccupied && !isBlending);
        }
    }

    private IEnumerator ShowNoblend()
    {
        NOblend.SetActive(true);
        yield return new WaitForSeconds(2);
        NOblend.SetActive(false);
        SoundManager.Instance.PlaySound(NOblendSound);

    }

    private IEnumerator BlendFruitCoroutine()
    {
        isBlending = true;
        UpdateBlendButtonState();

        if (blenderSlot != null)
        {
            DraggableItem item = blenderSlot.GetComponentInChildren<DraggableItem>();
             if (item != null)
        {   
            Collider2D itemCollider = item.GetComponent<Collider2D>();
            if (itemCollider != null)
            {
                StartCoroutine(DisableColliderTemporarily(itemCollider, 3f));
            }
            else
            {
                Debug.LogWarning("No Collider2D found on DraggableItem in blender slot!");
            }

                GameObject juicePrefab = null;
                switch (item.fruitType)
                {
                    case FruitType.Apple:
                        juicePrefab = appleJuicePrefab;
                        break;
                    case FruitType.Mango:
                        juicePrefab = mangoJuicePrefab;
                        break;
                    case FruitType.Peach:
                        juicePrefab = PeachJuicePrefab;
                        break;
                    case FruitType.Strawberry:
                        juicePrefab = strawberryJuicePrefab;
                        break;
                    case FruitType.Kiwi:
                        juicePrefab = kiwiJuicePrefab;
                        break;
                    case FruitType.Papaya:
                        juicePrefab = papayaJuicePrefab;
                        break;
                    case FruitType.Pomegranate:
                        juicePrefab = pomegranateJuicePrefab;
                        break;
                    case FruitType.Carrot:
                        juicePrefab = carrotJuicePrefab;
                        break;
                    case FruitType.Tomato:
                        juicePrefab = TomatoJuicePrefab;
                        break;
                    default:
                        Debug.Log("This fruit can't be blended.");
                        StartCoroutine(ShowNoblend());
                        isBlending = false;
                        UpdateBlendButtonState();
                        yield break;
                }

                PlayBlendAnimation();
                SoundManager.Instance.PlaySound(blendFruitSound);

                yield return new WaitForSeconds(blendDuration);

                ReplaceWithJuice(juicePrefab);
            }
        }
        else
        {
            Debug.Log("Blender slot is null!");
        }

        isBlending = false;
        UpdateBlendButtonState();
    }

    private void ReplaceWithJuice(GameObject juicePrefab)
    {
        if (blenderSlot != null)
        {
            Destroy(blenderSlot.GetComponentInChildren<DraggableItem>().gameObject);
            GameObject juice = Instantiate(juicePrefab, blenderSlot.transform.position, Quaternion.identity);
            juice.transform.SetParent(blenderSlot.transform);
            juice.transform.localPosition = itemSpawnPoint.localPosition;
            blenderSlot.OccupySlot(juice.GetComponent<DraggableItem>());
        }
    }

   private IEnumerator DisableColliderTemporarily(Collider2D collider, float duration)
{
    if (collider != null)
    {
        collider.enabled = false;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            if (collider == null)
            {
                // Объект был уничтожен, завершаем корутину
                yield break;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (collider != null)
        {
            collider.enabled = true;
        }
    }
}

    private void PlayBlendAnimation()
    {
        if (blenderAnimator != null)
        {
            blenderAnimator.SetTrigger("Shake");
        }
        else
        {
            Debug.LogWarning("Blender Animator is not assigned!");
        }
    }
}