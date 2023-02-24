using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestOperations : MonoBehaviour
{
    // Start is called before the first frame update

    string test = "lkosdf:sldjf:lskdfj:]";
    void Start()
    {
        string[] test2 = test.Split(':');
        foreach(string test3 in test2)
        {
            Debug.Log(test3);
        }
    }

}
