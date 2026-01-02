using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PokedexUI : MonoBehaviour
{
    [Header("Panels (mismo canvas)")]
    public GameObject ballsPanel;
    public GameObject pokedexPanel;

    [Header("List")]
    public Transform listContent;
    public GameObject pokemonButtonPrefab;
    public ScrollRect scrollRect;

    [Header("Top progress UI")]
    public TMP_Text progressText;
    public Image progressFill;

    [Header("Entry UI")]
    public TMP_Text numberText;
    public TMP_Text nameText;
    public Image typeIcon;
    public TMP_Text descText;
    public Image pokemonImage;

    [Header("Type icons")]
    public TypeIconMap[] typeIcons;

    [Header("Entries (index 0..n-1)")]
    public PokedexEntry[] entries = new PokedexEntry[10];

    [Header("Navigation")]
    public float deadzone = 0.6f;
    public float repeatDelay = 0.22f;
    public Color normalColor = new Color(1f, 1f, 1f, 0.35f);
    public Color selectedColor = new Color(1f, 1f, 1f, 1f);

    bool isOpen;
    float nextMoveTime;

    readonly List<int> shownPokemonIds = new();
    readonly List<Button> shownButtons = new();
    int currentIndex = 0;

    void Start() => ShowBalls();

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Three)) Toggle();
        if (!isOpen) return;
        HandleStickNavigation();
    }

    public void Toggle()
    {
        if (isOpen) ShowBalls();
        else ShowPokedex();
    }

    public void ShowPokedex()
    {
        isOpen = true;
        if (ballsPanel) ballsPanel.SetActive(false);
        if (pokedexPanel) pokedexPanel.SetActive(true);
        RebuildList();
    }

    public void ShowBalls()
    {
        isOpen = false;
        if (pokedexPanel) pokedexPanel.SetActive(false);
        if (ballsPanel) ballsPanel.SetActive(true);
    }

    void RebuildList()
    {
        if (!listContent || !pokemonButtonPrefab || entries == null || entries.Length == 0)
        {
            Debug.LogWarning("[PokedexUI] Falta listContent / pokemonButtonPrefab / entries.");
            return;
        }

        for (int i = listContent.childCount - 1; i >= 0; i--)
            Destroy(listContent.GetChild(i).gameObject);

        shownPokemonIds.Clear();
        shownButtons.Clear();
        currentIndex = 0;

        UpdateProgressUI();

        int total = entries.Length;

        for (int pokemonId = 1; pokemonId <= total; pokemonId++)
        {
            int entryIdx = pokemonId - 1; 

            bool isCap = (PokedexData.I != null && PokedexData.I.IsCaptured(pokemonId));

            var go = Instantiate(pokemonButtonPrefab, listContent);

            var item = go.GetComponent<PokedexListItem>();
            if (item)
            {
                Sprite t = isCap ? GetTypeIcon(entries[entryIdx].primaryType) : null;
                item.SetCaptured(pokemonId, entries[entryIdx].displayName, isCap, t);
            }
            else
            {
                var txt = go.GetComponentInChildren<TMP_Text>();
                if (txt) txt.text = isCap
                    ? $"{pokemonId:00}  {entries[entryIdx].displayName}"
                    : $"{pokemonId:00}  ???";
            }

            var btn = go.GetComponent<Button>();
            if (btn)
            {
                int pid = pokemonId;
                btn.onClick.AddListener(() => SelectByPokemonId(pid, true));
            }

            shownPokemonIds.Add(pokemonId);
            shownButtons.Add(btn);
        }

        if (scrollRect)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
        }

        // Selecciona el primero
        SelectIndex(0, false);
    }

    void UpdateProgressUI()
    {
        int total = (entries != null) ? entries.Length : 0;
        int cap = (PokedexData.I != null) ? PokedexData.I.CapturedCount() : 0;

        if (progressText) progressText.text = $"Capturados: {cap}/{total}";
        if (progressFill) progressFill.fillAmount = (total > 0) ? (cap / (float)total) : 0f;
    }

    void HandleStickNavigation()
    {
        Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        if (Mathf.Abs(stick.y) < deadzone) return;
        if (Time.time < nextMoveTime) return;

        nextMoveTime = Time.time + repeatDelay;

        if (stick.y > 0f) SelectIndex(currentIndex - 1, true);
        else SelectIndex(currentIndex + 1, true);
    }

    void SelectIndex(int newIndex, bool scrollTo)
    {
        if (shownButtons.Count == 0) return;

        if (newIndex < 0) newIndex = 0;
        if (newIndex >= shownButtons.Count) newIndex = shownButtons.Count - 1;
        currentIndex = newIndex;

        for (int i = 0; i < shownButtons.Count; i++)
        {
            if (!shownButtons[i]) continue;
            var img = shownButtons[i].GetComponent<Image>();
            if (img) img.color = (i == currentIndex) ? selectedColor : normalColor;
        }

        int pokemonId = shownPokemonIds[currentIndex];
        ShowEntry(pokemonId);

        if (scrollTo && scrollRect && shownButtons[currentIndex])
        {
            Canvas.ForceUpdateCanvases();
            var target = shownButtons[currentIndex].GetComponent<RectTransform>();
            ScrollTo(target);
        }
    }

    void SelectByPokemonId(int pokemonId, bool scrollTo)
    {
        int idx = shownPokemonIds.IndexOf(pokemonId);
        if (idx >= 0) SelectIndex(idx, scrollTo);
    }

    void ShowEntry(int pokemonId)
    {
        int idx = pokemonId - 1;
        if (entries == null || idx < 0 || idx >= entries.Length) return;

        bool isCap = (PokedexData.I != null && PokedexData.I.IsCaptured(pokemonId));
        var e = entries[idx];

        if (numberText) numberText.text = $"#{pokemonId:00}";
        if (nameText) nameText.text = isCap ? e.displayName : "???";

        if (typeIcon)
        {
            Sprite icon = isCap ? GetTypeIcon(e.primaryType) : null;
            typeIcon.sprite = icon;
            typeIcon.enabled = (icon != null);
        }

        if (descText) descText.text = isCap ? e.description : "No registrado.";

        if (pokemonImage)
        {
            pokemonImage.sprite = isCap ? e.sprite : null;
            pokemonImage.enabled = isCap && e.sprite != null;
        }
    }

    Sprite GetTypeIcon(PokemonType type)
    {
        if (typeIcons == null) return null;

        for (int i = 0; i < typeIcons.Length; i++)
        {
            if (typeIcons[i] != null && typeIcons[i].pokemonType == type)
                return typeIcons[i].icon;
        }
        return null;
    }

    void ScrollTo(RectTransform target)
    {
        if (!scrollRect || !target) return;

        RectTransform content = scrollRect.content;
        RectTransform viewport = scrollRect.viewport;

        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

        float targetY = Mathf.Abs(target.anchoredPosition.y);

        float normalized = 0f;
        if (contentHeight > viewportHeight)
            normalized = Mathf.Clamp01(targetY / (contentHeight - viewportHeight));

        scrollRect.verticalNormalizedPosition = 1f - normalized;
    }
}

[System.Serializable]
public class PokedexEntry
{
    public string displayName;
    [TextArea(3, 8)] public string description;
    public Sprite sprite;
    public PokemonType primaryType;
}
