using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionNode : Node
{
    public Node trueNode;
    public Node falseNode;

    public TypeQuest type;

    public override void Execute(Secuaz secuaz)
    {
        switch (type)
        {
            case TypeQuest.LowHP:
                if (secuaz.life < 30)
                    secuaz.fsm.ChangeState("Flight");
                else
                    falseNode.Execute(secuaz);

            break;

            case TypeQuest.InFOVEnemy:
                if (secuaz.InFOVList(secuaz.targetsSecuaz))
                {
                    secuaz.fsm.ChangeState("Attack");
                }
                else
                    falseNode.Execute(secuaz);

                break;

            case TypeQuest.InLineOfSightLeader:
                if(GameManager.Instance.InLineOfSight(secuaz.transform.position, secuaz.myLeaderTarget.transform.position))
                {
                    secuaz.fsm.ChangeState("MoveToLeader");
                }
                else
                {
                    secuaz.fsm.ChangeState("Pathfinding");
                }
                

                break;
        }
    }
}

public enum TypeQuest
{
    LowHP,
    InFOVEnemy,
    InLineOfSightLeader
}
