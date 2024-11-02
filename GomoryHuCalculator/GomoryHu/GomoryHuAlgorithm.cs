using System.Net;

namespace Gomory_HuCalculator.GomoryHu;

public class GomoryHuAlgorithm
{
    private int [,] _weightMatrix;
    private bool[] _markedNodes;
    private List<List<int>> _treeNodes;
    private List<List<List<int>>> _treePaths;
    
    public GomoryHuAlgorithm(int[,] weightMatrix)
    {
        var nodeCount = weightMatrix.GetLength(0);
        
        _weightMatrix = weightMatrix;
        _markedNodes = new bool[nodeCount];

        _treeNodes = [];
        var tempList = new List<int>();
        for (var i = 0; i < nodeCount; i++)
        {
            tempList.Add(i);
        }
        _treeNodes.Add(tempList);

        _treePaths = [];
    }
    
    public void Solve()
    {
        while (true)
        {
           var stop = _treeNodes.All(sublist => sublist.Count == 1);
           if (stop)
               break;

           var sublist = _treeNodes[0].ToList();
           var sublistIndex = 0;
           for (var i = 1; i < _treeNodes.Count; i++)
           {
               if (_treeNodes[i].Count >= 2 && _treeNodes[i].Count <= sublist.Count)
               {
                   sublist = _treeNodes[i].ToList();
                   sublistIndex = i;
               }
           }
           
           ChooseElementsToCut(sublist, out var s, out var t);
           PerformCut(sublist, s, t, out var a, out var b, out var sum);
           
           UpdateTree(sublistIndex, a, b, sum);
        }

    }
    
    private void PerformCut(List<int> initialNodeList, int s, int t, out List<int> a, out List<int> b, out int sum)
    {
        var allNodeCount = _weightMatrix.GetLength(0);
        
        var minFlowValue = int.MaxValue;
        List <int> minNodeList = [];
        var nodeList = new List<int>{s};
        var last = false;
        do
        {
            var currentFlowValue = CalculateFlowValue(nodeList, allNodeCount);
            if (currentFlowValue < minFlowValue)
            {
                minFlowValue = currentFlowValue;
                minNodeList = nodeList.ToList();
            }

            if (ChangeNodeForCut(nodeList, allNodeCount, t) == allNodeCount)
            {
                last = AddNodeForCut(nodeList, allNodeCount, t);
            }
        } while(nodeList.Count <= allNodeCount - 1 && !last);

        if (minNodeList.Count == allNodeCount - 1)
        {
            minNodeList.Clear();
            minNodeList.Add(t);
        }

        a = minNodeList;
        b = [];
        sum = minFlowValue;
        
        var initialNodeListEnumerator = initialNodeList.GetEnumerator();
        while (initialNodeListEnumerator.MoveNext())
        {
            if(!a.Contains(initialNodeListEnumerator.Current))
                b.Add(initialNodeListEnumerator.Current);
        }
    }
    
    private void UpdateTree(int sublistIndex, List<int> a, List<int> b, int sum)
    {
        _treeNodes.RemoveAt(sublistIndex);
        _treeNodes.Insert(sublistIndex, a);
        _treeNodes.Insert(sublistIndex, b);
        
        _treePaths.Insert(sublistIndex, new List<List<int>>{});
    }
    
    // Utility functions
    private static void ChooseElementsToCut(List<int> nodes, out int s, out int t)
    {
        s = nodes.First();
        t = nodes.Last();
    }
    
    private int CalculateFlowValue(List<int> nodeList, int allNodeCount)
    {
        var sum = 0;
        var nodeListEnumerator = nodeList.GetEnumerator();
        while (nodeListEnumerator.MoveNext())
        {
            for (var j = 0; j < allNodeCount; j++)
            {
                if (nodeList.Contains(j))
                    continue;

                sum += _weightMatrix[nodeListEnumerator.Current, j];
            }
        }

        return sum;
    }

    private static int ChangeNodeForCut(List<int> nodeList, int allNodeCount, int t)
    {
        if (nodeList.Count < 2) return allNodeCount;
        for (var i = 0; i < allNodeCount; ++i)
        {
            if (!nodeList.Contains(i) && nodeList[nodeList.Count - 1] + 1 != t)
            {
                return ++nodeList[nodeList.Count - 1];
            }

        }

        return allNodeCount;
    }
    
    private static bool AddNodeForCut(List<int> nodeList, int allNodeCount, int t)
    {
        if (nodeList.Count == allNodeCount - 1)
            return true;
        
        for (var i = 0; i < allNodeCount; ++i)
        {
            if (nodeList.Contains(i) || i == t) continue;
            nodeList.Add(i);
            break;
        }
        
        return false;
    }
}