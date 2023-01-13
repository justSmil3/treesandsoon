using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestOperations : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        char x = '!';
        for(int i = 0; i < 223; i++)
        Debug.Log((char)((int)x + i));        
    }

}
