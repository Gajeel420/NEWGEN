using UnityEngine;

/// <summary>
/// Minimal test controller for Phase 2 verification.
/// Attach to a GameObject to test all systems in a simple setup.
/// </summary>
public class Phase2TestController : MonoBehaviour
{
    [SerializeField] private Character player1;
    [SerializeField] private Character player2;
    [SerializeField] private ComboDetector comboDetector1;
    [SerializeField] private AttackManager attackManager1;

    private bool testStarted = false;

    void Update()
    {
        if (!testStarted && Input.GetKeyDown(KeyCode.T))
        {
            StartPhase2Test();
        }
    }

    private void StartPhase2Test()
    {
        testStarted = true;
        Debug.Log("=== PHASE 2 SYSTEM TEST STARTED ===");
        
        // Verify all components exist
        if (player1 == null || player2 == null)
        {
            Debug.LogError("ERROR: Player characters not assigned!");
            return;
        }

        Debug.Log($"✓ Player 1: {player1.name} - Health: {player1.GetHealth()}");
        Debug.Log($"✓ Player 2: {player2.name} - Health: {player2.GetHealth()}");

        // Test input buffering
        if (InputManager.Instance != null)
        {
            Debug.Log("✓ InputManager singleton found");
            Debug.Log("✓ Input buffering active - press Light/Medium/Heavy to test");
        }
        else
        {
            Debug.LogError("ERROR: InputManager not initialized!");
        }

        // Test combo detection
        if (comboDetector1 != null)
        {
            Debug.Log($"✓ ComboDetector found - {comboDetector1.Count} combos loaded");
        }

        // Test attack manager
        if (attackManager1 != null)
        {
            Debug.Log("✓ AttackManager ready for attacks");
        }

        Debug.Log("=== PRESS WASD TO MOVE, SPACE TO JUMP, BUTTONS FOR ATTACKS ===");
        Debug.Log("=== EXECUTE COMBOS: Light > Light > Medium ===");
    }

    public void OnHitReceived(Character attacker, Character defender, float damage)
    {
        Debug.Log($"[TEST] HIT: {attacker.name} dealt {damage} damage to {defender.name}");
        Debug.Log($"[TEST] {defender.name} health: {defender.GetHealth()}");
    }
}
