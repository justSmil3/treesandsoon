using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkedTreeSystemNode
{
    public Vector3 pos;
    public Vector3 dir;
    //LinkedTreeSystemNode previousNode;
}

public struct LinkedTreeSystemNodeLink
{
    public LinkedTreeSystemNode current;
    public LinkedTreeSystemNode prev;
}

    public class GenerateTree : MonoBehaviour
{

    List<LinkedTreeSystemNodeLink> treelink = new List<LinkedTreeSystemNodeLink>();
    Stack<LinkedTreeSystemNode> positionstack = new Stack<LinkedTreeSystemNode>();

    private void OnDrawGizmos()
    {
        foreach(LinkedTreeSystemNodeLink tmp in treelink)
        {
            if (tmp.current == null) continue;
            Gizmos.DrawSphere(tmp.current.pos, .03f);
            if (tmp.prev == null) continue;
            Gizmos.DrawLine(tmp.current.pos, tmp.prev.pos);
        }
    }
    private void Awake()
    {
        string startString = "+TT+F";
        //string startString = "+TT+R";
        L_system lsys = new L_system();
        //lsys.AddRule('F', "TF[[XF][F]xF]");
        lsys.AddRule('F', "F[Fz[zFZXFZYF]Z[ZFxzFyzF]C+]");
        //lsys.AddRule('R', "FFF[FXYZ[FxRxF[zFRzXFC]R[ZFZyFC]]yFRyF]");
        string res = lsys.ApplyAxioms(startString, 5);
        Debug.Log(res);
        BuildTreeNodeSystem(res);
    }

    void BuildTreeNodeSystem(string tree)
    {

        float turnAngle = 10;
        float pitchAngle = 15;
        float rollAngle = 10;
        float branchLength = .3f;
        Debug.Log(tree);
        LinkedTreeSystemNode currentState = new LinkedTreeSystemNode();
        LinkedTreeSystemNode prevousNode = null;
        LinkedTreeSystemNode currentNode = null;
        //LinkedTreeSystemNode previousNode;
        //LinkedTreeSystemNode currentNode;
        Vector3 startDir = Vector3.up;
        Vector3 pos = Vector3.zero;
        treelink.Clear();
        int posIdx = 0;

        for (int i = 0; i < tree.Length; i++)
        {
            currentState = new LinkedTreeSystemNode();
            currentState.dir = startDir;
            currentState.pos = pos;
            switch (tree[i])
            {
                case 't':
                    pos += (-startDir * branchLength);
                    break;
                case 'T':
                    pos += (startDir * branchLength);
                    break;
                case 's':
                    // decrease a certain thickness
                    break;
                case 'S':
                    // increase a certain thickness
                    break;
                case 'x':
                    startDir = Quaternion.AngleAxis(-pitchAngle, Vector3.right) * startDir;
                    break;
                case 'X':
                    startDir = Quaternion.AngleAxis(pitchAngle, Vector3.right) * startDir;
                    break;
                case 'y':
                    startDir = Quaternion.AngleAxis(-pitchAngle, Vector3.up) * startDir;
                    break;
                case 'Y':
                    startDir = Quaternion.AngleAxis(pitchAngle, Vector3.up) * startDir;
                    break;
                case 'z':
                    startDir = Quaternion.AngleAxis(-pitchAngle, Vector3.forward) * startDir;
                    break;
                case 'Z':
                    startDir = Quaternion.AngleAxis(pitchAngle, Vector3.forward) * startDir;
                    break;
                case '[':
                    currentState.dir = startDir;
                    currentState.pos = pos;
                    positionstack.Push(currentState);
                    break;
                case ']':
                    // get the first element i dont really know where to save it 
                    currentState = positionstack.Pop();
                    startDir = currentState.dir;
                    pos = currentState.pos; // TODO fix object referencing
                    prevousNode = currentState;
                    break;
                case 'c':
                    prevousNode = null;
                    break;
                case '+':
                    // Adds a center point for mesh Generation
                    currentNode = new LinkedTreeSystemNode();
                    currentNode.pos = pos;
                    currentNode.dir = startDir;
                    {
                        LinkedTreeSystemNodeLink tempLink = new LinkedTreeSystemNodeLink();
                        tempLink.current = currentNode;
                        tempLink.prev = prevousNode;
                        treelink.Add(tempLink);
                    }
                    prevousNode = currentNode;
                    break;
                case 'F':
                    pos += (startDir * branchLength);
                    currentNode = new LinkedTreeSystemNode();
                    currentNode.pos = pos;
                    currentNode.dir = startDir;
                    {
                        LinkedTreeSystemNodeLink tempLink = new LinkedTreeSystemNodeLink();
                        tempLink.current = currentNode;
                        tempLink.prev = prevousNode;
                        treelink.Add(tempLink);
                    }
                    prevousNode = currentNode;
                    //Combines 'T' and '+' for continuity with L-System
                    break;
                default:
                    break;
            }
        }
    }



}
