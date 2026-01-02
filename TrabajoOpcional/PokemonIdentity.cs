using UnityEngine;

public class PokemonIdentity : MonoBehaviour
{
    [Range(1, 10)] public int pokemonId;  
    public string pokemonName;           
    public PokemonType primaryType;
}
