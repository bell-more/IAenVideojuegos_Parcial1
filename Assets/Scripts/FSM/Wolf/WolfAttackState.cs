using UnityEngine;

public class WolfAttackState : IState
{
    private Wolf wolf;
    private StateMachine fsm;
    private float attackCooldown = 1.5f;
    private float timer = 0f;

    public WolfAttackState(Wolf wolf, StateMachine fsm)
    {
        this.wolf = wolf;
        this.fsm = fsm;
    }

    public void Enter()
    {
        wolf.Agent.Stop(); // Frena para morder
        timer = attackCooldown; // Listo para atacar apenas entra
        Debug.Log($"[FSM] {wolf.gameObject.name} entró en estado ATAQUE.");
    }

    public void Update()
    {
        // Si el cazador se esconde, o si la variable quedó vacía, va a investigar
        if (!wolf.IsSeeingHunter || wolf.CurrentHunter == null)
        {
            fsm.ChangeState(WolfStates.Investigate);
            return;
        }

        // Si el cazador se aleja corriendo, vuelve a perseguirlo
        float distance = Vector3.Distance(wolf.transform.position, wolf.LastKnownHunterPosition);
        if (distance > 2.5f)
        {
            fsm.ChangeState(WolfStates.Pursuit);
            return;
        }

        // Lógica de ataque con cooldown
        timer += Time.deltaTime;
        if (timer >= attackCooldown)
        {
            timer = 0f;
            Debug.Log($"[Wolf] ¡{wolf.gameObject.name} MORDIÓ AL CAZADOR!");

            // Ejecutamos la muerte en el FSM del Cazador
            wolf.CurrentHunter.TakeDamage();

            // Trabajo terminado, el lobo vuelve a su patrulla pacífica
            fsm.ChangeState(WolfStates.Patrol);
        }
    }

    public void Exit() { }
}