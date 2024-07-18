using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExplorationBehaviorNode : BehaviorNode, IOpenable
{
    private List<GameObject> arrows = new List<GameObject>();
    private List<ExplorationBehaviorNode> children = new List<ExplorationBehaviorNode>();
    private List<ExplorationBehaviorNode> parents = new List<ExplorationBehaviorNode>();

    public void AddChild(ExplorationBehaviorNode child)
    {
        children.Add(child);
    }

    public void AddArrow(GameObject arrow)
    {
        arrows.Add(arrow);
    }

    public void AddParent(ExplorationBehaviorNode parent)
    {
        parents.Add(parent);
    }

    //############################//
    //                            //
    //         IOpenable          //
    //                            //
    //############################//

    private IOpenable.State _currentState = IOpenable.State.Open;

    public IOpenable.State CurrentState
    {
        get => _currentState;
        set => _currentState = value;
    }

    public void Open()
    {
        text.fontStyle = FontStyles.Underline;

        foreach (GameObject arrow in arrows)
        {
            arrow.SetActive(true);
        }

        foreach (ExplorationBehaviorNode child in children)
        {
            child.gameObject.SetActive(true);
        }

        CurrentState = IOpenable.State.Open;
    }

    public void Close()
    {
        CurrentState = IOpenable.State.Closed;

        text.fontStyle = FontStyles.Normal;

        foreach (GameObject arrow in arrows)
        {
            arrow.SetActive(false);
        }

        foreach (ExplorationBehaviorNode child in children)
        {
            if (child.parents.TrueForAll(parent => parent.CurrentState == IOpenable.State.Closed))
            {
                if (child.CurrentState == IOpenable.State.Open)
                {
                    child.Close();
                }
                child.gameObject.SetActive(false);
            }
        }
    }
}
