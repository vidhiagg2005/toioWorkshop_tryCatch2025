/*
 * ToioTeachingSandbox_Challenge.cs
 *
 * Focus: Creative challenge — design your own robot movement pattern!
 * Students use what they learned (loops, conditions, timing) to build shapes or dances.
 * Includes helper functions for all basic movements.
 * Compatible with the Toio Unity SDK (BLE).
 */

using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using toio;

public class ToioTeachingSandbox_Challenge : MonoBehaviour
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
        Debug.Log("[ChallengeSandbox] Scanning... press cube buttons to wake them.");

        var found = await cm.MultiConnect(numberOfCubes);
        cubes = found.Where(c => c != null && c.isConnected).ToArray();

        if (cubes.Length == 0)
        {
            Debug.LogError("[ChallengeSandbox] No cubes connected.");
            enabled = false;
            return;
        }

        Debug.Log($"[ChallengeSandbox] Connected {cubes.Length} cube(s).");
        await Task.Delay(500);

        snippetRunning = true;
        if (runOnAllCubes) await RunOnAll();
        else               await RunOnFirst();
        snippetRunning = false;

        Debug.Log("[ChallengeSandbox] Ready for creative mode!");
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
        Debug.Log("[ChallengeSandbox] Running student creative pattern...");

        // --- START: Students edit below this line ---
        /*
         * 🧩 Challenge: "Shape Builders"
         *
         * You’ve learned about loops, if/else, and while —
         * now use them to make your cube draw shapes, spin, or DANCE!
         *
         * Example ideas:
         * ▢ Draw a square using a for loop
         * 🔺 Draw a triangle using turns of 120°
         * 🔄 Spin in place using while
         * 💃 Combine moves into a creative routine or “dance”
         *
         * 💡 Tips:
         * • Use loops to repeat patterns (for or while)
         * • Use angles creatively (90°, 120°, 180°, etc.)
         * • Add BeepAsync() or WiggleAsync() for flair
         * • Experiment and test on the mat!
         *
         * 🧠 Helper functions:
         * await MoveForwardAsync(c, speed, ms);
         * await TurnRightAsync(c, degrees);
         * await TurnLeftAsync(c, degrees);
         * await WiggleAsync(c);
         * await NudgeAsync(c);
         * await BeepAsync(c);
         * await WaitMs(ms);
         */

        // 🔧 Example (you can erase and write your own!)
        for (int i = 0; i < 4; i++)
        {
            await MoveForwardAsync(c, 80, 700);
            await TurnRightAsync(c, 90);
        }

        await BeepAsync(c);
        await WiggleAsync(c);
        Debug.Log("[ChallengeSandbox] Example: Square shape complete!");
        // --- END: Students edit above this line ---

        Debug.Log("[ChallengeSandbox] Student creative pattern finished!");
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
