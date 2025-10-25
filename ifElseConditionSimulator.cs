/*
 * ToioTeachingSandbox_IfElseOnly_Sim.cs
 *
 * Simulator version (no BLE).
 * Focus: IF / ELSE logic with simulated toio cubes.
 */

using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using toio;

public class ToioTeachingSandbox_IfElseOnly_Sim : MonoBehaviour
{
    [Header("Connection (Simulator)")]
    public int numberOfCubes = 1;
    public bool runOnAllCubes = false;

    [Header("Defaults")]
    [Range(10,115)] public int defaultSpeed = 80;
    [Range(50,3000)] public int defaultMoveMs = 1000;
    [Range(5,180)]  public int defaultTurnDeg = 90;

    [Header("Timing / Tuning")]
    public float msPerDeg = 6.0f;
    public int motorSettleMs = 50;

    [Header("Safety / UX")]
    public bool suppressOtherMovementWhileRunning = true;

    private CubeManager cm;
    private Cube[] cubes = new Cube[0];
    private bool snippetRunning = false;

    // ---------------------------------------------------------
    // Unity lifecycle
    // ---------------------------------------------------------
    async void Start()
    {
        Application.targetFrameRate = 30;

        // Simulator instead of Real
        cm = new CubeManager(ConnectType.Simulator);
        Debug.Log("[IfElseSandbox:SIM] Looking for simulated cubes in the scene...");

        var found = await cm.MultiConnect(numberOfCubes);
        cubes = found.Where(c => c != null && c.isConnected).ToArray();

        if (cubes.Length == 0)
        {
            Debug.LogError("[IfElseSandbox:SIM] No simulated cubes found. " +
                           "Make sure the 'Simulator/Playground' prefab with cubes is in the scene.");
            enabled = false;
            return;
        }

        Debug.Log($"[IfElseSandbox:SIM] Connected {cubes.Length} simulated cube(s).");
        await Task.Delay(500);

        snippetRunning = true;
        if (runOnAllCubes) await RunOnAll();
        else               await RunOnFirst();
        snippetRunning = false;

        Debug.Log("[IfElseSandbox:SIM] Ready to teach IF–ELSE logic!");
    }

    void Update()
    {
        if (suppressOtherMovementWhileRunning && snippetRunning)
            return;
    }

    // ---------------------------------------------------------
    // Runner
    // ---------------------------------------------------------
    private async Task RunOnFirst()
    {
        var c = cubes[0];
        await StudentPatternAsync(c);
        SafeStop(c);
    }

    private async Task RunOnAll()
    {
        foreach (var c in cubes)
        {
            if (c == null || !c.isConnected) continue;
            await StudentPatternAsync(c);
            SafeStop(c);
            await Task.Delay(300);
        }
    }

    // ---------------------------------------------------------
    // ✨ STUDENT AREA ✨
    // ---------------------------------------------------------
    private async Task StudentPatternAsync(Cube c)
    {
        Debug.Log("[IfElseSandbox:SIM] Running student IF–ELSE pattern...");

        // --- START: Students edit below this line ---
        /*
         * 🚀 Task: Use IF / ELSE to react differently based on a condition.
         *
         * Example condition using randomness:
         * bool spin = Random.value > 0.5f;
         *
         * Helpers you can use:
         * await MoveForwardAsync(c, speed, ms);
         * await TurnRightAsync(c, degrees);
         * await TurnLeftAsync(c, degrees);
         * await WiggleAsync(c);
         * await NudgeAsync(c);
         * await BeepAsync(c);
         * await WaitMs(ms);
         */

        // Example: Random decision to either spin or wiggle+move
        bool spin = Random.value > 0.5f;

        if (spin)
        {
            await TurnRightAsync(c, 360);     // Spin full circle
            await BeepAsync(c);               // Feedback
        }
        else
        {
            await WiggleAsync(c, 2, 45, 180); // Wiggle twice
            await MoveForwardAsync(c, 80, 600);
        }

        await BeepAsync(c); // Signal done
        // --- END: Students edit above this line ---

        Debug.Log("[IfElseSandbox:SIM] Student pattern finished!");
    }

    // ---------------------------------------------------------
    // Helper Methods (students can call these)
    // ---------------------------------------------------------
    public async Task MoveForwardAsync(Cube c, int speed, int ms)
    {
        speed = Mathf.Clamp(speed, -115, 115);
        ms = Mathf.Clamp(ms, 10, 8000);
        c.Move(speed, speed, ms);
        await Task.Delay(ms + motorSettleMs);
    }

    public async Task TurnRightAsync(Cube c, int deg)
    {
        deg = Mathf.Clamp(deg, -360, 360);
        int ms = Mathf.Clamp(Mathf.RoundToInt(Mathf.Abs(deg) * msPerDeg), 30, 2000);
        int s = 70;
        if (deg >= 0) c.Move(s, -s, ms);
        else          c.Move(-s, s, ms);
        await Task.Delay(ms + motorSettleMs);
    }

    public async Task TurnLeftAsync(Cube c, int deg)
    {
        await TurnRightAsync(c, -Mathf.Abs(deg));
    }

    public async Task WiggleAsync(Cube c, int repeats = 2, int turnDeg = 45, int turnMs = 180)
    {
        for (int i = 0; i < repeats; i++)
        {
            c.Move(70, -70, turnMs); await Task.Delay(turnMs + motorSettleMs);
            c.Move(-70, 70, turnMs); await Task.Delay(turnMs + motorSettleMs);
        }
    }

    public async Task NudgeAsync(Cube c, int speed = 60, int ms = 250)
    {
        c.Move(speed, speed, ms); await Task.Delay(ms + motorSettleMs);
        c.Move(-speed, -speed, ms/2); await Task.Delay(ms/2 + motorSettleMs);
    }

    public async Task WaitMs(int ms)
    {
        await Task.Delay(Mathf.Clamp(ms, 0, 10000));
    }

    public async Task BeepAsync(Cube c)
    {
        // Simulator may not have audio; fall back to a quick spin as feedback.
        bool played = false;
        try
        {
            c.PlayPresetSound(3);
            played = true;
            await Task.Delay(700);
        }
        catch { }

        if (!played)
        {
            int ms = 120;
            c.Move(100, -100, ms);
            await Task.Delay(ms + 50);
        }
    }

    public void SafeStop(Cube c)
    {
        try { c.Move(0, 0, 0); } catch { }
    }
}
