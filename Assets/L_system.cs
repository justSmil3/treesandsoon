using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class L_system
{
    Dictionary<string, string> ruleSet = new Dictionary<string, string>();
    public void AddRule(string rulechar, string newstring) 
    {
        ruleSet.Add(rulechar, newstring);
    }

    public void ClearRuleset()
    {
        ruleSet.Clear();
    }

    public bool CheckIfRulesExist(string replace)
    {
        return !ruleSet.ContainsKey(replace);
    }

    // this can be way more optimized
    private string ApplyAxioms(string system, bool bTrivialize, float trivializationFactor, int currentiteration)
    {
        string applied = "";
        for (int i = 0; i < system.Length; i++)
        {
            bool pass = true;
            Quaternion systemRotation = ExtractQuaternion(system.Substring(i), out _);
           foreach (KeyValuePair<string, string> rule in ruleSet)
            {
                string _rule = rule.Value;
                float ruleValue, systemValue = 0;
                int k = i;

                // rule adaptation
                string appliedAdd = rule.Value;
                string appliedHelper = rule.Value;
                for (int j = 0; j < rule.Key.Length; j++)
                {
                    // TODO this seems teribly inefficient
                    if (Char.IsDigit(rule.Key[j]))
                    {
                        string tmpRuleValue = "";
                        string tmpSystemValue = "";
                        int tmpK = k;
                        while (k < system.Length)
                        {
                            char systemChar = system[k];
                            if (Char.IsDigit(systemChar) || systemChar == '.' || systemChar == '-')
                            {
                                tmpSystemValue += systemChar;
                                k++;
                            }
                            else break;
                        }
                        systemValue = float.Parse(tmpSystemValue);

                        while (j < rule.Key.Length)
                        {
                            char ruleKeyChar = rule.Key[j];
                            if (Char.IsDigit(ruleKeyChar) || ruleKeyChar == '.' || ruleKeyChar == '-')
                            {
                                tmpRuleValue += ruleKeyChar;
                                j++;
                            }
                            else
                            {
                                j--; // counteract base addition
                                break;
                            }
                        }
                        ruleValue = float.Parse(tmpRuleValue);

                        // compare here with trivialization
                        float dist = Mathf.Abs(systemValue - ruleValue);
                        Debug.Log(systemValue + " : " + ruleValue + " : " + dist + " : " + trivializationFactor);
                        if (dist <= (trivializationFactor / 100.0f))
                        {
                            pass = false;
                        }
                        else
                        {
                            pass = true;
                            break;
                        }

                    }
                    else
                    {
                        if (k < system.Length)
                        {
                            if (system[k] == rule.Key[j])
                            {
                                pass = false;
                                k++;
                            }
                            else
                            {
                                pass = true;
                                break;
                            }
                        }
                    }
                }
                if (!pass)
                {
                    int idxChange = 0;
                    string ruleV = rule.Value;
                    char predecessor = ' ';
                    string tmpValueHolder = "";
                    int insertindex = -1;
                    Quaternion firstQuaternion = Quaternion.identity;
                    for (int j = 0; j < rule.Value.Length; j++)
                    {
                        // TODO start here on monday, gotta handle this quaternion rotation
                        char current = ruleV[j + idxChange];
                        if(current == 'A')
                        {
                            // Extract base rotation: 

                            

                            int idx = j + idxChange + 1;

                            float[] floatQuat = new float[4];
                            int tmpIdxChange = 0;
                            char tmpChar = ' ';
                            //for (int l = 0; l < 4; l++)
                            //{
                            //    tmpChar = ruleV[idx + tmpIdxChange];
                            //    string tmpFloat = "";
                            //    while (Char.IsDigit(tmpChar) || tmpChar == '.' || tmpChar == '-')
                            //    {
                            //        //Debug.Log(appliedAdd[idx]);
                            //        //appliedAdd = appliedAdd.Remove(idx, 1);
                            //        tmpFloat += tmpChar;
                            //        tmpIdxChange++;
                            //        tmpChar = ruleV[idx + tmpIdxChange];
                            //    }
                            //    if (l < 3)
                            //    {
                            //        //Debug.Log(appliedAdd[idx]);
                            //        //appliedAdd = appliedAdd.Remove(idx, 1);
                            //        tmpIdxChange++;
                            //    }
                            //    floatQuat[l] = float.Parse(tmpFloat);
                            //}
                            Quaternion rotationQuaternion = ExtractQuaternion(appliedAdd.Substring(j + idxChange, appliedAdd.Length - (j + idxChange)), out tmpIdxChange);
                            //Quaternion rotationQuaternion = new Quaternion(floatQuat[0], floatQuat[1], floatQuat[2], floatQuat[3]);
                            rotationQuaternion *= systemRotation;
                            appliedAdd = appliedAdd.Remove(idx, tmpIdxChange);
                            string stringQuat = rotationQuaternion.x.ToString() + 'q' +
                                rotationQuaternion.y.ToString() + 'q' +
                                rotationQuaternion.z.ToString() + 'q' +
                                rotationQuaternion.w.ToString();
                            idxChange += stringQuat.Length - tmpIdxChange;
                            j += tmpIdxChange;
                            //idxChange += stringQuat.Length - tmpIdxChange;
                            appliedAdd = appliedAdd.Insert(idx, stringQuat);
                            ruleV = appliedAdd;
                        }
                        else if (Char.IsDigit(current) || current == '.' || current == '-')
                        {
                            tmpValueHolder += current;
                            insertindex = insertindex > -1 ? insertindex : j + idxChange;
                            appliedAdd = appliedAdd.Remove(insertindex, 1);
                            // Debug.Log(appliedAdd);
                        }
                        else if (insertindex > -1)
                        {
                            float v = float.Parse(tmpValueHolder);
                            if (predecessor == 'L')
                            {
                                v /= Mathf.Pow(2, currentiteration);
                            }
                            else
                            {
                                v = v; // make the rotation actually happen
                            }
                            idxChange += (v.ToString().Length - tmpValueHolder.Length);
                            appliedAdd = appliedAdd.Insert(insertindex, v.ToString());
                            tmpValueHolder = "";
                            insertindex = -1;
                            ruleV = appliedAdd;
                            predecessor = current;
                        }
                        else
                        {
                            predecessor = current;
                        }
                    }
                    i = k - 1; 
                    applied += appliedAdd;
                    break;
                }
            }
            if (pass)
            {
                while (i < system.Length)
                {
                    applied += system[i];
                    Debug.Log(system[i]);
                    if (system[i] == ';' || system[i] == ']') break;
                    i++;
                }
            }
        }
        return applied;
    }

    public string ApplyAxioms(string system, int iterations, float trivializationfactor = 0)
    {
        if (iterations <= 0)
            return system;
        string applied = system;
        Debug.Log("---------------------------------------------------------------");
        Debug.Log(system);
        for(int i = 0; i < iterations; i++)
        {
            applied = ApplyAxioms(applied,
                                  trivializationfactor != 0,
                                  trivializationfactor,
                                  i);
            Debug.Log(applied);
        }
        return applied;
    }

    public Quaternion ExtractQuaternion(string extractionString, out int removalCount)
    {
        float[] floatQuat = new float[4];
        int idxChange = 0;
        int idxChange2 = 0;
        char tmpChar = extractionString[idxChange];
        while(tmpChar != 'A' && idxChange < extractionString.Length)
        {
            idxChange2++;
            tmpChar = extractionString[idxChange2];
        }
        idxChange2++;
        idxChange = idxChange2;
        for (int l = 0; l < 4; l++)
        {
            tmpChar = extractionString[idxChange];
            string tmpFloat = "";
            while (Char.IsDigit(tmpChar) || tmpChar == '.' || tmpChar == '-')
            {
                tmpFloat += tmpChar;
                idxChange++;
                tmpChar = extractionString[idxChange];
            }
            idxChange++;
            floatQuat[l] = float.Parse(tmpFloat);
        }
        Quaternion outQuaternion = new Quaternion(floatQuat[0], floatQuat[1], floatQuat[2], floatQuat[3]);
        removalCount = idxChange - 1 - idxChange2;
        return outQuaternion;
    }
}


/// L and L3D files typically work with an OFF file for input/output. Off file is a textual format containing vertices and faces. On the other hand, L++ files work on Off+ files, which on top of everything that Off has, contain texture coordinates for vertices, and a flag telling if a face is a leaf or not.


#region fist successfull
/// Instructions for L(or L3D) files are as follows (for generation of rules):
/// + = turn right
/// - = turn left
/// & = pitch down
/// ^ = pitch up
/// < or \ = roll left
/// > or / = roll right
/// | = turn 180 degree
/// f or F = draw branch (and go forward)
/// g = go forward
/// [ = save state
/// ] = restore state
/// 
/// Any of those that require a number, work on the default number by default. You can provide another number by providing it in paranthesis, for example:
/// 
/// +(90) & (75.5)
/// for (int i = 0; i < tree.Length; i++)
//{
//    switch (tree[i])
//    {
//        case '+':
//            startDir = Quaternion.AngleAxis(turnAngle, startUp) * startDir;
//            startRight = Quaternion.AngleAxis(turnAngle, startUp) * startDir;
//            break;
//        case '-':
//            startDir = Quaternion.AngleAxis(-turnAngle, startUp) * startDir;
//            startRight = Quaternion.AngleAxis(-turnAngle, startUp) * startDir;
//            break;
//        case '&':
//            startDir = Quaternion.AngleAxis(pitchAngle, startRight) * startDir;
//            startUp = Quaternion.AngleAxis(pitchAngle, startRight) * startDir;
//            break;
//        case '^':
//            startDir = Quaternion.AngleAxis(-pitchAngle, startRight) * startDir;
//            startUp = Quaternion.AngleAxis(-pitchAngle, startRight) * startDir;
//            break;
//        case '<':
//            startRight = Quaternion.AngleAxis(rollAngle, startDir) * startDir;
//            startUp = Quaternion.AngleAxis(rollAngle, startDir) * startDir;
//            break;
//        case '>':
//            startRight = Quaternion.AngleAxis(-rollAngle, startDir) * startDir;
//            startUp = Quaternion.AngleAxis(-rollAngle, startDir) * startDir;
//            break;
//        case '|':
//            startDir = Quaternion.AngleAxis(180, startUp) * startDir;
//            startRight = Quaternion.AngleAxis(180, startUp) * startDir;
//            break;
//        case 'f':
//            pos += (startDir * branchLength);
//            positions.Add(pos);
//            break;
//        case 'g':
//            pos += (startDir * branchLength);
//            break;
//        case '[':
//            currentState.up = startUp;
//            currentState.dir = startDir;
//            currentState.right = startRight;
//            currentState.currentPosition = pos;
//            break;
//        case ']':
//            startUp = currentState.up;
//            startRight = currentState.right;
//            startDir = currentState.dir;
//            pos = currentState.currentPosition;
//            break;
//        default:
//            break;
//    }
//}
/// 
/// 
#endregion

/// The L++ format is slightly different, in the following ways:
/// > and < are not rolling
/// > : decrease thickness(by percent of its own)
/// < : increase thickness(by percent of its own)
/// = : set thickness(to percentage of length)
/// * : draw leaf
/// % : set thickness reduction (for every draw operation)
/// 
/// The major component of L++ is that it supports leafs, so it needs to save its output to OFF+ files to properly reflect leaf textures. Leaves are also affected by gravity, and their tip bends towards ground.
///  
/// 
/// 
/// 
/// anothe rulset by https://www.csh.rit.edu/~aidan/portfolio/3DLSystems.shtml is as follows:  <- currently in use
/// case 't': //translate Center Downard
/// case 'T': //translate Center Upward
/// case 's': //scale radius smaller
/// case 'S': //scale radius larger
/// case 'x': //Counter-Clockwise around X axis
/// case 'X': //Clockwise around X axis
/// case 'y': //Counter-Clockwise around Y axis
/// case 'Y': //Clockwise around Y axis
/// case 'z': //Counter-Clockwise around Z axis
/// case 'Z': //Clockwise around Z axis
/// case '[': //push point onto the stack
/// case ']': //pop point from the stack
/// case 'C': //'Closes' a point to stop it from drawing bad geometry
/// case '+': //Adds a center point for mesh Generation
/// case 'F': //Combines 'T' and '+' for continuity with L-System