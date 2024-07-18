using UnityEngine;

public class LayoutSetter : MonoBehaviour, IInteractable
{
    [SerializeField] private ClassDiagram classDiagramObject;
    [SerializeField] private ClassDiagram.Layout layoutType;

    //############################//
    //                            //
    //       IInteractable        //
    //                            //
    //############################//

    public void Interact()
    {
        classDiagramObject.Relayout(layoutType);
    }
}
