using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Translator
{
    private Dictionary<char, string> dictionary;
    int currentKey;
    const char ROTATION_INDEX = (char)2;
    const char MOVEMENT_INDEX = (char)1;
    const int MAX_KEY_VALUE = 255;
    const int MIN_KEY_VALUE = 3;
    public Translator()
    {
        dictionary = new Dictionary<char, string>();
        currentKey = 3;
    }

    private bool bIsKeyValid()
    {
        return currentKey <= MAX_KEY_VALUE && currentKey >= MIN_KEY_VALUE;
    }

    public bool AddTranslation(string operation, out char key)
    {
        // explore this one with a char function dictionary
        if (!bIsKeyValid())
        {
            key = (char)0;
            return false;
        }
        key = (char)currentKey;
        dictionary.Add(key, operation);
        currentKey++;
        return true;
    }

    public bool AddTranslation(List<SplineNode> tree, out char key)
    {
        if (!bIsKeyValid())
        {
            key = (char)0;
            return false;
        }
        key = (char)currentKey;
        string operation;
        if (Bez2Lsys.Convert(tree, out operation, out _, false))
            dictionary.Add(key, operation);
        else return false;
        currentKey++;
        return true;
    }

    public string FindTranslation(char identifier)
    {
        string result = "";
        if (dictionary.ContainsKey(identifier))
        {
            result = dictionary[identifier];
        }
        return result;
    }

    public string Translate(string sentence)
    {
        // This does not contain an implimentation of commands as of now
        string result = "";
        for(int i = 0; i < sentence.Length; i++)
        {
            result += FindTranslation(sentence[i]);
        }
        return result;
    }
}
