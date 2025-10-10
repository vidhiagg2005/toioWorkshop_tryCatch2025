/*
 * ToioTeachingSandbox_StudentPatterns.cs (Simplified)
 *
 * Always runs StudentPatternAsync().
 * Students edit only the marked section to create their own movement pattern.
 * Includes double-tap behavior and motion/sound helpers.
 * Compatible with the Toio Unity SDK (BLE).
 */

using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using toio;

public class ToioTeachingSandbox_StudentPatterns : MonoBehaviour
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
        Debug.Log("[StudentPatterns] Scanning... press cube buttons to wake them.");

        var found = await cm.MultiConnect(numberOfCubes);
        cubes = found.Where(c => c != null && c.isConnected).ToArray();

        if (cubes.Length == 0)
        {
            Debug.LogError("[StudentPatterns] No cubes connected.");
            enabled = false;
            return;
        }

        // Register double-tap listener if available
        foreach (var c in cubes)
        {
            try { c.doubleTapCallback.AddListener("StudentPatterns", OnDoubleTap); }
            catch { Debug.LogWarning("[StudentPatterns] doubleTapCallback not available on this SDK build."); }
        }

        Debug.Log($"[StudentPatterns] Connected {cubes.Length} cube(s).");
        await Task.Delay(500);

        snippetRunning = true;
        if (runOnAllCubes) await RunOnAll();
        else               await RunOnFirst();
        snippetRunning = false;

        Debug.Log("[StudentPatterns] Ready. Double-tap a cube any time to trigger your custom double-tap behavior.");
    }

    void OnDestroy()
    {
        if (cm != null && cubes != null)
        {
            foreach (var c in cubes)
            {
                if (c == null) continue;
                try { c.doubleTapCallback.RemoveListener("StudentPatterns"); } catch { }
                try { cm.Disconnect(c); } catch { }
            }
        }
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
    // Students edit only inside this method.
    // ---------------------------------------------------------
    private async Task StudentPatternAsync(Cube c)
    {
        Debug.Log("[StudentPatterns] Running STUDENT pattern...");

        // --- START: Students edit below this line ---

        // Example 1: Draw a square
        for (int i = 0; i < 4; i++)
        {
            await MoveForwardAsync(c, 80, 600);
            await TurnRightAsync(c, 90);
        }

        // Example 2: Conditional move
        bool spin = Random.value > 0.5f;
        if (spin)
        {
            await TurnRightAsync(c, 360);
        }
        else
        {
            await NudgeAsync(c, 70, 250);
            await BeepAsync(c);
        }

        // Example 3: Simple spiral
        await SpiralAsync(c, turns: 6, startMs: 300, stepMs: 80, speed: 85, turnPerStepDeg: 25);

        // --- END: Students edit above this line ---

        Debug.Log("[StudentPatterns] STUDENT pattern finished.");
    }

    // ---------------------------------------------------------
    // Double-tap behavior (students can customize)
    // ---------------------------------------------------------
    private async void OnDoubleTap(Cube c)
    {
        if (snippetRunning) return;
        await OnDoubleTapStudent(c);
    }

    private async Task OnDoubleTapStudent(Cube c)
    {
        await BlinkMotionAsync(c);
        await TurnRightAsync(c, 180);
        await BeepAsync(c);
    }

    // --- START: Students edit below this line ---
    // Write your own sequence here!
    //private async Task Challenge(Cube c){

    //}
    // --- END: Students edit above this line ---


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

        if (deg >= 0) c.Move( s, -s, ms);
        else          c.Move(-s,  s, ms);
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

    private async Task BlinkMotionAsync(Cube c, int speed = 80, int forwardMs = 180, int backMs = 140)
    {
        speed = Mathf.Clamp(speed, -115, 115);
        forwardMs = Mathf.Clamp(forwardMs, 10, 2000);
        backMs = Mathf.Clamp(backMs, 10, 2000);

        c.Move(speed, speed, forwardMs);
        await Task.Delay(forwardMs + motorSettleMs);
        c.Move(-speed, -speed, backMs);
        await Task.Delay(backMs + motorSettleMs);
    }

    public async Task WaitMs(int ms)
    {
        await Task.Delay(Mathf.Clamp(ms, 0, 10000));
    }

    public void SafeStop(Cube c)
    {
        try { c.Move(0, 0, 0); } catch { }
    }

    public async Task RegularPolygonAsync(Cube c, int sides, int sideMs, int speed)
    {
        sides = Mathf.Clamp(sides, 3, 12);
        sideMs = Mathf.Clamp(sideMs, 80, 4000);
        speed = Mathf.Clamp(speed, 10, 115);

        int interiorTurn = 180 - Mathf.RoundToInt(360f / sides);
        int rightTurnDeg = 180 - interiorTurn;

        for (int i = 0; i < sides; i++)
        {
            await MoveForwardAsync(c, speed, sideMs);
            await TurnRightAsync(c, rightTurnDeg);
        }
    }

    public async Task SpiralAsync(Cube c, int turns, int startMs, int stepMs, int speed, int turnPerStepDeg = 20)
    {
        turns = Mathf.Clamp(turns, 1, 20);
        startMs = Mathf.Clamp(startMs, 100, 1500);
        stepMs = Mathf.Clamp(stepMs, 10, 500);
        speed = Mathf.Clamp(speed, 10, 115);
        turnPerStepDeg = Mathf.Clamp(turnPerStepDeg, 5, 60);

        int ms = startMs;
        for (int i = 0; i < turns * 8; i++)
        {
            await MoveForwardAsync(c, speed, ms);
            await TurnRightAsync(c, turnPerStepDeg);
            ms += stepMs;
        }
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
}
