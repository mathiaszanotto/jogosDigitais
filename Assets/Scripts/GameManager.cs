using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class GameManager : MonoBehaviour
{
    public enum MatchState { Playing, Won, Lost }
    public static GameManager Instance { get; private set; }
    public static bool IsPlaying => Instance != null && Instance.State == MatchState.Playing;
    public MatchState State { get; private set; }
    public bool HasWon => State == MatchState.Won;
    public int CollectedCount => GameController.CollectedCount;
    public int TotalCollectibles => GameController.TotalCollectibles;
    public int Health { get; private set; } = 3;
    public int Score { get; private set; }
    public float Elapsed { get; private set; }
    public float RemainingTime { get; private set; }
    public float timeLimit = 45f;
    public float pickupTimeBonus = 3f;
    public Text counterText, timerText, healthText, scoreText, statusText, victoryText, resultTitle;
    public Slider staminaBar;
    public GameObject victoryPanel;
    public Button restartButton;
    public AudioClip damageSound, victorySound, defeatSound;
    private AudioSource audioSource;
    private PlayerMovement player;
    private float nextDamageTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Instance = null;
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }
    private void Start()
    {
        GameController.Init(FindObjectsByType<Collectible>().Length);
        player = FindAnyObjectByType<PlayerMovement>();
        RemainingTime = timeLimit;
        victoryPanel.SetActive(false);
        RefreshHud();
    }
    private void Update()
    {
        if (IsPlaying) AdvanceClock(Time.deltaTime);
        RefreshHud();
        var keyboard = Keyboard.current;
        var pad = Gamepad.current;
        if ((keyboard != null && keyboard.escapeKey.wasPressedThisFrame) || (pad != null && pad.selectButton.wasPressedThisFrame)) ReturnToMenu();
        if (!IsPlaying && ((keyboard != null && keyboard.rKey.wasPressedThisFrame) || (pad != null && pad.startButton.wasPressedThisFrame))) Restart();
    }
    public void AdvanceClock(float seconds)
    {
        if (!IsPlaying || seconds <= 0) return;
        float delta = Mathf.Min(seconds, RemainingTime);
        Elapsed += delta;
        RemainingTime = Mathf.Max(0, RemainingTime - delta);
        if (RemainingTime <= 0) Finish(false, "CONSUMIDO PELA SEDE");
    }
    public void RegisterCollection()
    {
        if (!IsPlaying || CollectedCount >= TotalCollectibles) return;
        GameController.Collect();
        Score += 100;
        RemainingTime = Mathf.Min(timeLimit, RemainingTime + pickupTimeBonus);
        if (GameController.GameOver) Finish(true, "A NOITE É SUA");
        RefreshHud();
    }
    public bool TakeDamage()
    {
        if (!IsPlaying || Time.time < nextDamageTime) return false;
        nextDamageTime = Time.time + 1.5f;
        Health = Mathf.Max(0, Health - 1);
        Score = Mathf.Max(0, Score - 50);
        PlaySound(damageSound);
        if (Health == 0) Finish(false, "VAMPIRO DERROTADO");
        RefreshHud();
        return true;
    }
    private void Finish(bool won, string title)
    {
        if (!IsPlaying) return;
        State = won ? MatchState.Won : MatchState.Lost;
        if (won) Score += Mathf.FloorToInt(RemainingTime * 10);
        resultTitle.text = title;
        resultTitle.color = won ? new Color(.90f, .76f, .45f) : new Color(1, .4f, .4f);
        victoryText.text = $"FRASCOS  {CollectedCount}/{TotalCollectibles}    •    SCORE  {Score}\nTEMPO FINAL  {Elapsed:0.0}s    •    RESERVA  {RemainingTime:0.0}s\n" + (won ? "Os doze frascos restauraram seu poder." : "Evite os templários e preserve seu sangue.");
        victoryPanel.SetActive(true);
        restartButton.Select();
        PlaySound(won ? victorySound : defeatSound);
    }
    private void RefreshHud()
    {
        if (counterText != null) counterText.text = $"FRASCOS  {CollectedCount:00}/{TotalCollectibles:00}";
        if (timerText != null) { timerText.text = $"SEDE EM {RemainingTime:00.0}s  ·  T {Elapsed:00.0}s"; timerText.color = RemainingTime < 12 ? new Color(1, .45f, .35f) : Color.white; }
        if (healthText != null) healthText.text = "VIDA  " + new string('●', Health) + new string('○', 3 - Health);
        if (scoreText != null) scoreText.text = $"SCORE  {Score:0000}";
        if (staminaBar != null && player != null) staminaBar.value = player.Stamina;
        if (statusText != null) statusText.text = IsPlaying ? "+3s por frasco  •  evite os templários" : "R / START  reiniciar  •  ESC / VIEW  menu";
    }
    private void PlaySound(AudioClip clip) { if (audioSource != null && clip != null) audioSource.PlayOneShot(clip, .7f); }
    private void OnDestroy() { if (Instance == this) Instance = null; }
    public void Restart() { Time.timeScale = 1; UnityEngine.SceneManagement.SceneManager.LoadScene("Jogo"); }
    public void ReturnToMenu() { Time.timeScale = 1; UnityEngine.SceneManagement.SceneManager.LoadScene("MenuInicial"); }
}
