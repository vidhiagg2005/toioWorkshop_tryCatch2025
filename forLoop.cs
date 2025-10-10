/*
 * ToioTeachingSandbox_ForLoopOnly.cs
 *
 * Focus: Learning how to repeat actions using a for loop.
 * Students edit only the marked section to create their own repeated pattern.
 * Includes helper functions (MoveForwardAsync, TurnRightAsync, WiggleAsync, etc.)
 * Compatible with Toio Unity SDK (BLE).
 */

using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using toio;

public class ToioTeachingSandbox_ForLoopOnly : MonoBehaviour
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
        Debug.Log("[ForLoopSandbox] Scanning... press cube buttons to wake them.");

        var found = await cm.MultiConnect(numberOfCubes);
        cubes = found.Where(c => c != null && c.isConnected).ToArray();

        if (cubes.Length == 0)
        {
            Debug.LogError("[ForLoopSandbox] No cubes connected.");
            enabled = false;
            return;
        }

        Debug.Log($"[ForLoopSandbox] Connected {cubes.Length} cube(s).");
        await Task.Delay(500);

        snippetRunning = true;
        if (runOnAllCubes) await RunOnAll();
        else               await RunOnFirst();
        snippetRunning = false;

        Debug.Log("[ForLoopSandbox] Ready to teach FOR LOOPS!");
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
        Debug.Log("[ForLoopSandbox] Running student for-loop pattern...");

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
         *     → Moves forward for a set time (ms). 
         *       Example: await MoveForwardAsync(c, 80, 700);
         *
         * await TurnRightAsync(c, degrees);
         *     → Turns right by a specific angle.
         *       Example: await TurnRightAsync(c, 90);
         *
         * await TurnLeftAsync(c, degrees);
         *     → Turns left by a specific angle.
         *       Example: await TurnLeftAsync(c, 90);
         *
         * await WiggleAsync(c);
         *     → Wobbles left and right.
         *
         * await NudgeAsync(c);
         *     → Small forward and backward hop.
         *
         * await BeepAsync(c);
         *     → Plays a short sound.
         *
         * await WaitMs(time);
         *     → Pause for a moment before continuing.
         */

        for (int i = 0; i < 4; i++)  // Example: repeat 4 times
        {
            await MoveForwardAsync(c, 80, 600);
            await TurnRightAsync(c, 90);
        }

        await BeepAsync(c); // signal finished
        // --- END: Students edit above this line ---

        Debug.Log("[ForLoopSandbox] Student pattern finished!");
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
