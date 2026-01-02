using UnityEngine;

public class PokedexData : MonoBehaviour
{
    public static PokedexData I;

    [Header("IDs 1..10. El índice 0 se ignora.")]
    public bool[] captured = new bool[11];

    const string PrefKey = "POKEDEX_CAPTURED_MASK"; 

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            ResetAll();
            Debug.Log("[PokedexData] PlayerPrefs borrado y Pokédex reseteada.");
        }
    }
    public void RegisterCapture(int id)
    {
        if (id < 1 || id > 10) return;

        bool was = captured[id];
        captured[id] = true;

        if (!was) Save(); 
    }

    public bool IsCaptured(int id)
    {
        return id >= 1 && id <= 10 && captured[id];
    }

    public int CapturedCount()
    {
        int c = 0;
        for (int id = 1; id <= 10; id++)
            if (captured[id]) c++;
        return c;
    }

    public void Save()
    {
        int mask = 0;
        for (int id = 1; id <= 10; id++)
            if (captured[id]) mask |= (1 << id);

        PlayerPrefs.SetInt(PrefKey, mask);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        int mask = PlayerPrefs.GetInt(PrefKey, 0);

        for (int id = 1; id <= 10; id++)
            captured[id] = (mask & (1 << id)) != 0;
    }

    public void ResetAll()
    {
        for (int id = 1; id <= 10; id++) captured[id] = false;
        PlayerPrefs.DeleteKey("POKEDEX_CAPTURED_MASK");
        PlayerPrefs.Save();
    }
}
