using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HanziRecipe
{
    public string pinyin; 
    public List<string> requiredCards; 
    
    [Header("Visual Result")]
    [Tooltip("Tarik Prefab INDUK (kotak biru) yang berisi semua line ke sini")]
    public GameObject hanziPrefab; 
    public AudioClip successSound;
}

[CreateAssetMenu(fileName = "NewFreeplayData", menuName = "Zichara/Freeplay Data")]
public class FreeplayData : ScriptableObject
{
    public string dataID;
    public List<HanziRecipe> hanziRecipes;
}