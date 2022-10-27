using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class L_system
{
    Dictionary<char, string> ruleSet = new Dictionary<char, string>();
    public void AddRule(char rulechar, string newstring) 
    {
        ruleSet.Add(rulechar, newstring);
    }

    public void ClearRuleset()
    {
        ruleSet.Clear();
    }

    public bool CheckIfRulesExist(char replace)
    {
        return !ruleSet.ContainsKey(replace);
    }

    public string ApplyAxioms(string system)
    {
        string applied = "";
        for (int i = 0; i < system.Length; i++)
        {
            bool pass = true;
            foreach (KeyValuePair<char, string> rule in ruleSet)
            {
                if (system[i] == rule.Key)
                {
                    pass = false;
                    applied += rule.Value;
                    break;
                }
            }
            if (pass)
                applied += system[i];
        }
        return applied;
    }

    public string ApplyAxioms(string system, int iterations)
    {
        if (iterations <= 0)
            return system;
        string applied = system;
        for(int i = 0; i < iterations; i++)
        {
            applied = ApplyAxioms(applied);
        }
        return applied;
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