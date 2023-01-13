// depricated


//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class LinkedTreeSystemNode
//{
//    public Vector3 pos;
//    public Vector3 dir;
//    //LinkedTreeSystemNode previousNode;
//}

//public struct LinkedTreeSystemNodeLink
//{
//    public LinkedTreeSystemNode current;
//    public LinkedTreeSystemNode prev;
//}

//[System.Serializable]
//public struct rule
//{
//    public char replace;
//    public string newrule;
//}

//    public class GenerateTree : MonoBehaviour
//{

//    public int iterations = 3;
//    public string startString = "+TT+F";
//    public List<rule> ruleset = new List<rule>();

//    private int iterationsPrev = 3;
//    private string startStringPrev = "+TT+F";
//    private List<rule> rulesetPrev = new List<rule>();

//    private L_system lsys;


//    List<LinkedTreeSystemNodeLink> treelink = new List<LinkedTreeSystemNodeLink>();
//    Stack<LinkedTreeSystemNode> positionstack = new Stack<LinkedTreeSystemNode>();

//    private void OnDrawGizmos()
//    {
//        foreach(LinkedTreeSystemNodeLink tmp in treelink)
//        {
//            if (tmp.current == null) continue;
//            Gizmos.DrawSphere(tmp.current.pos, .03f);
//            if (tmp.prev == null) continue;
//            Gizmos.DrawLine(tmp.current.pos, tmp.prev.pos);
//        }
//    }
//    private void Awake()
//    {
//        rulesetPrev.Clear();
//        rulesetPrev.AddRange(ruleset);
//        startStringPrev = startString;
//        iterationsPrev = iterations;

//        //string startString = "+TT+R";
//        lsys = new L_system();
//        //lsys.AddRule('F', "F[Fz[zFZXFZYF]Z[ZFxzFyzF]C+]");
//        //lsys.AddRule('F', "TF[[XF][F]xF]");
//        lsys.ClearRuleset();
//        foreach (rule r in ruleset)
//        {
//            //lsys.AddRule('F', "F[Fz[zFZXFZYF]Z[ZFxzFyzF]C+]");
//            //lsys.AddRule(r.replace, r.newrule);
//        }
//        //lsys.AddRule('R', "FFF[FXYZ[FxRxF[zFRzXFC]R[ZFZyFC]]yFRyF]");

//        var watch = System.Diagnostics.Stopwatch.StartNew();
//        string res = lsys.ApplyAxioms(startString, iterations);
//        Debug.LogWarning(watch.ElapsedMilliseconds);
//        BuildTreeNodeSystem(res);
//        watch.Stop();
//        Debug.LogWarning(watch.ElapsedMilliseconds);
//    }

//    private void FixedUpdate()
//    {
//        if(iterations != iterationsPrev || startString != startStringPrev)
//        {
//            startStringPrev = startString;
//            iterationsPrev = iterations;
//            string res = lsys.ApplyAxioms(startString, iterations);
//            BuildTreeNodeSystem(res);
//        }

//        if(bCeckListEquivalence(ruleset, rulesetPrev))
//        {
//            lsys.ClearRuleset();
//            foreach (rule r in ruleset)
//            {
//                //lsys.AddRule('F', "F[Fz[zFZXFZYF]Z[ZFxzFyzF]C+]");
//                //if(lsys.CheckIfRulesExist('F'))
//                //lsys.AddRule(r.replace, r.newrule);
//                //else continue;
            
//                string res = lsys.ApplyAxioms(startString, iterations);
//                BuildTreeNodeSystem(res);
//            }
//        }


//    }

//    bool bCeckListEquivalence(List<rule> first, List<rule> second)
//    {
//        bool areEqual = false;
//        for(int i = 0; i < first.Count; i++)
//        {
//            if (second.Exists(r2 => first[i].replace == r2.replace && first[i].newrule == r2.newrule))
//            {
//                rule r2 = second.Find(r2 => first[i].replace == r2.replace && first[i].newrule == r2.newrule);
//                second.Remove(r2);
//            }
//            else areEqual = true;
//        }
//        rulesetPrev.Clear();
//        rulesetPrev.AddRange(ruleset);
//        return areEqual;
//    }

//    void BuildTreeNodeSystem(string tree)
//    {

//        float turnAngle = 10;
//        float pitchAngle = 15;
//        float rollAngle = 10;
//        float branchLength = .3f;
//        Debug.Log(tree);
//        LinkedTreeSystemNode currentState = new LinkedTreeSystemNode();
//        LinkedTreeSystemNode prevousNode = null;
//        LinkedTreeSystemNode currentNode = null;
//        //LinkedTreeSystemNode previousNode;
//        //LinkedTreeSystemNode currentNode;
//        Vector3 startDir = Vector3.up;
//        Vector3 pos = Vector3.zero;
//        treelink.Clear();
//        int posIdx = 0;

//        for (int i = 0; i < tree.Length; i++)
//        {
//            currentState = new LinkedTreeSystemNode();
//            currentState.dir = startDir;
//            currentState.pos = pos;
//            switch (tree[i])
//            {
//                case 't':
//                    pos += (-startDir * branchLength);
//                    break;
//                case 'T':
//                    pos += (startDir * branchLength);
//                    break;
//                case 's':
//                    // decrease a certain thickness
//                    break;
//                case 'S':
//                    // increase a certain thickness
//                    break;
//                case 'x':
//                    startDir = Quaternion.AngleAxis(-pitchAngle, Vector3.right) * startDir;
//                    break;
//                case 'X':
//                    startDir = Quaternion.AngleAxis(pitchAngle, Vector3.right) * startDir;
//                    break;
//                case 'y':
//                    startDir = Quaternion.AngleAxis(-pitchAngle, Vector3.up) * startDir;
//                    break;
//                case 'Y':
//                    startDir = Quaternion.AngleAxis(pitchAngle, Vector3.up) * startDir;
//                    break;
//                case 'z':
//                    startDir = Quaternion.AngleAxis(-pitchAngle, Vector3.forward) * startDir;
//                    break;
//                case 'Z':
//                    startDir = Quaternion.AngleAxis(pitchAngle, Vector3.forward) * startDir;
//                    break;
//                case '[':
//                    currentState.dir = startDir;
//                    currentState.pos = pos;
//                    positionstack.Push(currentState);
//                    break;
//                case ']':
//                    // get the first element i dont really know where to save it 
//                    if(positionstack.Count > 0)
//                    currentState = positionstack.Pop();
//                    startDir = currentState.dir;
//                    pos = currentState.pos; // TODO fix object referencing
//                    prevousNode = currentState;
//                    break;
//                case 'c':
//                    prevousNode = null;
//                    break;
//                case '+':
//                    // Adds a center point for mesh Generation
//                    currentNode = new LinkedTreeSystemNode();
//                    currentNode.pos = pos;
//                    currentNode.dir = startDir;
//                    {
//                        LinkedTreeSystemNodeLink tempLink = new LinkedTreeSystemNodeLink();
//                        tempLink.current = currentNode;
//                        tempLink.prev = prevousNode;
//                        treelink.Add(tempLink);
//                    }
//                    prevousNode = currentNode;
//                    break;
//                case 'F':
//                    pos += (startDir * branchLength);
//                    currentNode = new LinkedTreeSystemNode();
//                    currentNode.pos = pos;
//                    currentNode.dir = startDir;
//                    {
//                        LinkedTreeSystemNodeLink tempLink = new LinkedTreeSystemNodeLink();
//                        tempLink.current = currentNode;
//                        tempLink.prev = prevousNode;
//                        treelink.Add(tempLink);
//                    }
//                    prevousNode = currentNode;
//                    //Combines 'T' and '+' for continuity with L-System
//                    break;
//                default:
//                    break;
//            }
//        }
//    }



//}
