using System.Collections;
using System.Collections.Generic;
using Events;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    // Runtime registry: screen id -> built view/presenter pair.
    [SerializeField] private List<UIPresenterBinding> presenterBindings = new();

    private readonly Dictionary<UIScreenId, UIScreenRegistration> _screens = new();
    private UIScreenId? _currentScreen;

    private void Awake()
    {
        StartCoroutine(AwakeRoutine());
    }

    private IEnumerator AwakeRoutine()
    {
        yield return null; // Wait for other Awake methods to complete before checking for duplicates.
        if (Instance && Instance != this)
        {
            Debug.LogError("[UIManager] Duplicate instance detected. Destroying the new one.");
            Destroy(this.gameObject);
            yield break;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        SubscribeSelfToEvents();
        RegisterScreens();
        SubscribePresenters();
        SetStartupScreen();
    }

    private void OnGameStarted(GameStartedEvent obj)
    {
        SetCurrentScreen(UIScreenId.InGame);
    }

    private void RegisterScreens()
    {
        var registrations = UIFactory.CreateRegistrations(presenterBindings);
        foreach (var registration in registrations)
        {
            if (_screens.TryAdd(registration.ScreenId, registration))
            {
                continue;
            }

            Debug.LogError($"[UIManager] Duplicate screen registration for {registration.ScreenId}.");
        }
    }

    private void SubscribePresenters()
    {
        foreach (var registration in _screens.Values)
        {
            registration.Presenter.SubscribeToEvents();
        }
    }

    private void SetStartupScreen()
    {
        UIScreenId? startupScreen = null;

        foreach (var registration in _screens.Values)
        {
            registration.View.Hide();
            if (!startupScreen.HasValue && registration.ShowOnStartup)
            {
                startupScreen = registration.ScreenId;
            }
        }

        if (!startupScreen.HasValue)
        {
            foreach (var registration in _screens.Values)
            {
                startupScreen = registration.ScreenId;
                break;
            }
        }

        if (startupScreen.HasValue)
        {
            SetCurrentScreen(startupScreen.Value);
        }
    }

    // Primary API for screen navigation.
    public void SetCurrentScreen(UIScreenId screenId)
    {
        if (!_screens.TryGetValue(screenId, out var nextScreen))
        {
            Debug.LogError($"[UIManager] Screen {screenId} is not registered.");
            return;
        }

        if (_currentScreen.HasValue && _currentScreen.Value.Equals(screenId))
        {
            return;
        }

        if (_currentScreen.HasValue && _screens.TryGetValue(_currentScreen.Value, out var currentScreen))
        {
            currentScreen.View.Hide();
        }

        nextScreen.View.Show();
        _currentScreen = screenId;
    }

    // Alias kept for readability at call sites that think in "UI change" terms.
    public void ChangeUI(UIScreenId screenId)
    {
        SetCurrentScreen(screenId);
    }
    
    private void SubscribeSelfToEvents()
    {
        EventBus.Subscribe<GameStartedEvent>(OnGameStarted);
    }
    
    private void OnDestroy()
    {
        UnsubscribeSelf();
        foreach (var registration in _screens.Values)
        {
            registration.Presenter.UnSubscribeToEvents();
        }
    }

    private void UnsubscribeSelf()
    {
        EventBus.Unsubscribe<GameStartedEvent>(OnGameStarted);
    }
}
