/*
 * ToioTeachingSandbox_WhileLoopOnly.cs
 *
 * Focus: Learning how to use while loops to repeat actions until a condition is met.
 * Students edit only the marked section to control repetition dynamically.
 * Includes helper functions (MoveForwardAsync, TurnRightAsync, WiggleAsync, etc.)
 * Compatible with Toio Unity SDK (BLE).
 */

using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using toio;

public class ToioTeachingSandbox_WhileLoopOnly : MonoBehaviour
{
    [Header("Connection")]
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

        cm = new CubeManager(ConnectType.Real);
        Debug.Log("[WhileLoopSandbox] Scanning... press cube buttons to wake them.");

        var found = await cm.MultiConnect(numberOfCubes);
        cubes = found.Where(c => c != null && c.isConnected).ToArray();

        if (cubes.Length == 0)
        {
            Debug.LogError("[WhileLoopSandbox] No cubes connected.");
            enabled = false;
            return;
        }

        Debug.Log($"[WhileLoopSandbox] Connected {cubes.Length} cube(s).");
        await Task.Delay(500);

        snippetRunning = true;
        if (runOnAllCubes) await RunOnAll();
        else               await RunOnFirst();
        snippetRunning = false;

        Debug.Log("[WhileLoopSandbox] Ready to teach WHILE loops!");
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
        Debug.Log("[WhileLoopSandbox] Running student WHILE loop pattern...");

        // --- START: Students edit below this line ---
        /*
         * 🚀 Your Task:
         * Use a WHILE loop to make your robot repeat actions 
         * as long as a condition is TRUE.
         *
         * 💡 Syntax Reminder:
         * int count = 0;
         * while (count < 3)
         * {
         *     // do something
         *     count++;
         * }
         *
         * 🧠 The while loop checks the condition first.
         * If true → runs the block, then checks again.
         * If false → exits the loop.
         *
         * Example:
         * int spins = 0;
         * while (spins < 3)
         * {
         *     await TurnRightAsync(c, 120);
         *     await BeepAsync(c);
         *     spins++;
         * }
         *
         * 🧩 Helper functions you can use inside your while loop:
         * 
         * await MoveForwardAsync(c, speed, ms); → Move forward
         * await TurnRightAsync(c, degrees); → Turn right
         * await TurnLeftAsync(c, degrees); → Turn left
         * await WiggleAsync(c); → Wiggle left-right
         * await NudgeAsync(c); → Small hop forward/back
         * await BeepAsync(c); → Play sound
         * await WaitMs(ms); → Pause for a moment
         */

        // Example: Spin 3 times before stopping
        int spins = 0;
        while (spins < 3)
        {
            await TurnRightAsync(c, 120); // Turn right each time
            await BeepAsync(c);            // Play sound each spin
            spins++;                       // Increase counter
        }

        await WiggleAsync(c); // Celebrate when done
        await BeepAsync(c);
        // --- END: Students edit above this line ---

        Debug.Log("[WhileLoopSandbox] Student pattern finished!");
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
