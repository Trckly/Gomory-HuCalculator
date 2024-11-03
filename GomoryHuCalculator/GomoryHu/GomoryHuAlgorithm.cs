using System.Net;

namespace Gomory_HuCalculator.GomoryHu;

public class GomoryHuAlgorithm
{
    private int [,] _weightMatrix;
    private List<List<int>> _treeNodes;
    private List<List<List<int>>> _treePaths;
    private List<int> _treeFlow;
    
    public GomoryHuAlgorithm(int[,] weightMatrix)
    {
        var nodeCount = weightMatrix.GetLength(0);
        
        _weightMatrix = weightMatrix;

        _treeNodes = [];
        var tempList = new List<int>();
        for (var i = 0; i < nodeCount; i++)
        {
            tempList.Add(i);
        }
        _treeNodes.Add(tempList);

        _treePaths = [];
        _treeFlow = [];
    }
    
    public void Solve()
    {
        while (true)
        {
           var stop = _treeNodes.All(sublist => sublist.Count == 1);
           if (stop)
               break;

           var sublist = _treeNodes.First(sublist => sublist.Count >= 2);
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
        var tempNodeList = nodeList; // This list hold initial node list before ChangeNodeForCut
        var last = false;
        do
        {
            var currentFlowValue = CalculateFlowValue(nodeList, allNodeCount);
            if (currentFlowValue < minFlowValue)
            {
                minFlowValue = currentFlowValue;
                minNodeList = nodeList.ToList();
            }

            if (ChangeNodeForCut(nodeList, allNodeCount, t) == -1)
            {
                last = AddNodeForCut(ref nodeList, allNodeCount, t, ref tempNodeList);
            }
        } while(nodeList.Count <= allNodeCount - 1 && !last);

        // Opposite to minNodeList
        var otherMinNodeList = new List<int>();
        for (var i = 0; i < allNodeCount; i++)
        {
            if(minNodeList.Contains(i)) continue;
            otherMinNodeList.Add(i);
        }
        
        a = [];
        b = [];
        sum = minFlowValue;
        
        if (minNodeList.Count == allNodeCount - 1 || otherMinNodeList.All(node => node == t || 
                _treeNodes.Any(list => list.SequenceEqual([node]))))
        {
            b = minNodeList;
            a = otherMinNodeList;
        }
        else
        {
            a = minNodeList;
            b = otherMinNodeList;
        }
    }
    
    private void FillOtherCut(List<int> initialNodeList, List<int> chosenNodeList, List<int> otherNodeList)
    {
        var initialNodeListEnumerator = initialNodeList.GetEnumerator();
        while (initialNodeListEnumerator.MoveNext())
        {
            if(!chosenNodeList.Contains(initialNodeListEnumerator.Current))
                otherNodeList.Add(initialNodeListEnumerator.Current);
        }
    }
    
    private void UpdateTree(int sublistIndex, List<int> a, List<int> b, int sum)
    {
        var sublist = _treeNodes[sublistIndex].ToList();
        
        _treeNodes = _treeNodes
            .Where(nodeList => !nodeList.SequenceEqual(sublist))
            .ToList();
        
        _treeNodes.AddRange(new List<List<int>>{a.Intersect(sublist).ToList(), b.Intersect(sublist).ToList()});
        
        PathUpdate(sublist, a, b);
        _treePaths.Add([a.Intersect(sublist).ToList(), b.Intersect(sublist).ToList()]);
    }

    private void PathUpdate(List<int> sublist, List<int> a, List<int> b)
    {
        for (var i = 0; i < _treeNodes.Count; i++)
        {
            var sublistTreeNode = new List<List<int>>{sublist, _treeNodes[i]};
            var pathToChangeIndex = _treePaths
                .FindIndex(treePath => CompareTreePath(treePath, sublistTreeNode));
            
            if (pathToChangeIndex == -1) continue;

            if (_treeNodes[i].All(a.Contains))
            {
                _treePaths[pathToChangeIndex] = [a.Intersect(sublist).ToList(), _treeNodes[i]];
            }

            if (_treeNodes[i].All(b.Contains))
            {
                _treePaths[pathToChangeIndex] = [b.Intersect(sublist).ToList(), _treeNodes[i]];
            }
        }
    }

    private static bool CompareTreePath(List<List<int>> treePath, List<List<int>> sublistTreeNode)
    {
        if (treePath.Count != sublistTreeNode.Count) return false;

        var swap = false;
        for (var i = 0; i < treePath.Count; i++)
        {
            if (treePath[i].Count != sublistTreeNode[i].Count)
            {
                swap = true;
                break;
            }

            for (var j = 0; j < treePath[i].Count; j++)
            {
                if (treePath[i][j] != sublistTreeNode[i][j]) return false;
            }
        }

        // Try to swap
        if (!swap) return true;

        var swappingList = new List<List<int>>();
        for (var i = 0; i < treePath.Count; i++)
        {
            swappingList.Add(sublistTreeNode[i]);
        }

        swappingList.Reverse();

        for (var i = 0; i < treePath.Count; i++)
        {
            if (treePath[i].Count != swappingList[i].Count) return false;

            for (var j = 0; j < treePath[i].Count; j++)
            {
                if (treePath[i][j] != swappingList[i][j]) return false;
            }
        }


        return true;
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

    // private static int ChangeNodeForCut(List<int> nodeList, int allNodeCount, int t)
    // {
    //     if (nodeList.Count < 2) return allNodeCount;
    //
    //     for (var i = nodeList.Count - 1; i > 0; --i)
    //     {
    //         if (nodeList[i] == allNodeCount - 1 || nodeList[i] + 1 == t)
    //         {
    //             if (i == 1)
    //                 return -1;
    //
    //             nodeList[i] = 0;
    //         }
    //         else
    //         {
    //             return ++nodeList[i];
    //         }
    //     }
    //
    //     return -1;
    // }
    
    private static int ChangeNodeForCut(List<int> nodeList, int allNodeCount, int t)
    {
        if (nodeList.Count < 2) return -1;

        // Iterate from the last element to the first (excluding the first element at index 0)
        for (var i = nodeList.Count - 1; i > 0; --i)
        {
            // Try to increment the current node's value
            while (++nodeList[i] < allNodeCount)
            {
                // Ensure the new value is not equal to `t` and is not already in the list
                if (nodeList[i] != t && !nodeList.Take(i).Contains(nodeList[i]))
                {
                    // Reset all elements to the right of `i` to their initial valid minimum values
                    for (var j = i + 1; j < nodeList.Count; j++)
                    {
                        nodeList[j] = 0; // Start with 0 to find the next valid increment
                        while (nodeList[j] < allNodeCount)
                        {
                            if (nodeList[j] != t && !nodeList.Take(j).Contains(nodeList[j]))
                                break;
                            nodeList[j]++;
                        }

                        // If no valid value was found for nodeList[j], return -1
                        if (nodeList[j] >= allNodeCount) return -1;
                    }

                    return nodeList[i]; // Return the updated value at `i` to indicate a successful change
                }
            }
            
            // Reset the current element to 0 when it can't be incremented anymore
            nodeList[i] = 0;
        }

        // If all combinations have been exhausted, return -1
        return -1;
    }

    
    private static bool AddNodeForCut(ref List<int> nodeList, int allNodeCount, int t, ref List<int> tempNodeList)
    {
        nodeList = tempNodeList.ToList();
        
        if (nodeList.Count == allNodeCount - 1)
            return true;
        
        for (var i = 0; i < allNodeCount; ++i)
        {
            if (nodeList.Contains(i) || i == t) continue;
            nodeList.Add(i);
            tempNodeList = nodeList.ToList();
            break;
        }
        
        return false;
    }

   
}