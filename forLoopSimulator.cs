/*
 * ToioTeachingSandbox_ForLoopOnly_Sim.cs
 *
 * Simulator version (no BLE required)
 * Focus: Learning how to repeat actions using a for loop.
 * Students edit only the marked section to create their own repeated pattern.
 * Works with the Toio SDK Simulator (ConnectType.Simulator).
 */

using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using toio;

public class ToioTeachingSandbox_ForLoopOnly_Sim : MonoBehaviour
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

        // Connect to simulator instead of real BLE cubes
        cm = new CubeManager(ConnectType.Simulator);
        Debug.Log("[ForLoopSandbox:SIM] Looking for simulated cubes in the scene...");

        var found = await cm.MultiConnect(numberOfCubes);
        cubes = found.Where(c => c != null && c.isConnected).ToArray();

        if (cubes.Length == 0)
        {
            Debug.LogError("[ForLoopSandbox:SIM] No simulated cubes found. " +
                           "Make sure 'Simulator/Playground' prefab with cubes is in the scene.");
            enabled = false;
            return;
        }

        Debug.Log($"[ForLoopSandbox:SIM] Connected {cubes.Length} simulated cube(s).");
        await Task.Delay(500);

        snippetRunning = true;
        if (runOnAllCubes) await RunOnAll();
        else               await RunOnFirst();
        snippetRunning = false;

        Debug.Log("[ForLoopSandbox:SIM] Ready to teach FOR LOOPS!");
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
        Debug.Log("[ForLoopSandbox:SIM] Running student for-loop pattern...");

        // --- START: Students edit below this line ---
        /*
         * 🚀 Your Task:
         * Use a for loop to repeat an action several times.
         * Example: draw a shape, make a dance, or repeat sounds!
         *
         * for (int i = 0; i < 4; i++)
         * {
         *     // Repeat this block 4 times
         *     await MoveForwardAsync(c, 80, 600);
         *     await TurnRightAsync(c, 90);
         * }
         *
         * 🧠 Helper functions you can use INSIDE the loop:
         * 
         * await MoveForwardAsync(c, speed, duration);
         * await TurnRightAsync(c, degrees);
         * await TurnLeftAsync(c, degrees);
         * await WiggleAsync(c);
         * await NudgeAsync(c);
         * await BeepAsync(c);
         * await WaitMs(time);
         */

        for (int i = 0; i < 4; i++)  // Example: repeat 4 times (draws a square)
        {
            await MoveForwardAsync(c, 80, 200);
            await TurnRightAsync(c, 90);
        }

        await BeepAsync(c); // signal finished
        // --- END: Students edit above this line ---

        for (int i = 0; i < 2; i++)  // Repeat twice (each iteration draws two sides)
{
    // Move forward for the longer side
    await MoveForwardAsync(c, 80, 800);  // e.g., 800ms for long edge
    await TurnRightAsync(c, 90);

    // Move forward for the shorter side
    await MoveForwardAsync(c, 80, 400);  // e.g., 400ms for short edge
    await TurnRightAsync(c, 90);
}

await BeepAsync(c); // Signal that the rectangle is complete

        Debug.Log("[ForLoopSandbox:SIM] Student pattern finished!");
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
        c.Move(-speed, -speed, ms / 2); await Task.Delay(ms / 2 + motorSettleMs);
    }

    public async Task WaitMs(int ms)
    {
        await Task.Delay(Mathf.Clamp(ms, 0, 10000));
    }

    public async Task BeepAsync(Cube c)
    {
        // Simulator may not have sound, so we use a small spin as feedback
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
